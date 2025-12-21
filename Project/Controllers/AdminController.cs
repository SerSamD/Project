using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SchoolContext _context;

        public AdminController(SchoolContext context)
        {
            _context = context;
        }

        // --- HELPER HASH ---
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
                return builder.ToString();
            }
        }

        // ============================================================
        // 1️⃣ DASHBOARD
        // ============================================================
        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Utilisateurs.CountAsync();
            var pendingUsers = await _context.Utilisateurs.CountAsync(u => u.IsApproved == false && u.Role == "Pending");
            var totalStudents = await _context.Utilisateurs.CountAsync(u => u.Role == "Etudiant" && u.IsApproved == true);
            var totalTeachers = await _context.Utilisateurs.CountAsync(u => u.Role == "Enseignant" && u.IsApproved == true);
            var totalSurveillants = await _context.Utilisateurs.CountAsync(u => u.Role == "Surveillant" && u.IsApproved == true);

            // Graphe
            var allNotes = await _context.Notes
                .Include(n => n.Etudiant).ThenInclude(e => e.Groupe)
                .Include(n => n.Cours)
                .Where(n => n.IsPublished).ToListAsync();

            var matieres = allNotes.Select(n => n.Cours.Titre).Distinct().OrderBy(t => t).ToList();
            var groupes = allNotes.Select(n => n.Etudiant.Groupe.Nom).Distinct().OrderBy(g => g).ToList();
            var datasets = new List<object>();
            var colors = new[] { "rgba(78, 115, 223, 0.9)", "rgba(28, 200, 138, 0.9)", "rgba(54, 185, 204, 0.9)", "rgba(246, 194, 62, 0.9)" };
            int cIndex = 0;

            foreach (var grp in groupes)
            {
                var moys = new List<decimal>();
                foreach (var mat in matieres)
                {
                    var notes = allNotes.Where(n => n.Etudiant.Groupe.Nom == grp && n.Cours.Titre == mat);
                    moys.Add(notes.Any() ? Math.Round(notes.Average(n => n.Valeur), 2) : 0);
                }
                datasets.Add(new { label = grp, data = moys, backgroundColor = colors[cIndex % colors.Length], borderColor = colors[cIndex % colors.Length], borderWidth = 1 });
                cIndex++;
            }

            ViewBag.ChartLabels = JsonSerializer.Serialize(matieres);
            ViewBag.ChartDatasets = JsonSerializer.Serialize(datasets);

            return View(new AdminDashboardViewModel
            {
                UtilisateursEnAttente = pendingUsers,
                TotalUtilisateurs = totalUsers,
                TotalEtudiants = totalStudents,
                TotalEnseignants = totalTeachers,
                TotalSurveillants = totalSurveillants,
                ChartLabels = new string[] { },
                ChartValues = new int[] { }
            });
        }

        // ============================================================
        // 2️⃣ APPROBATION
        // ============================================================
        public async Task<IActionResult> PendingUsers()
        {
            return View(await _context.Utilisateurs.Where(u => u.IsApproved == false && u.Role == "Pending").OrderBy(u => u.NomUtilisateur).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null) return RedirectToAction(nameof(PendingUsers));

            user.IsApproved = true;
            user.Role = user.PendingRole;
            _context.Utilisateurs.Update(user);
            await _context.SaveChangesAsync();
            await CreateSpecificProfile(user, user.Role);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Utilisateur approuvé.";
            return RedirectToAction(nameof(PendingUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user != null) { _context.Utilisateurs.Remove(user); await _context.SaveChangesAsync(); }
            TempData["SuccessMessage"] = "Utilisateur rejeté.";
            return RedirectToAction(nameof(PendingUsers));
        }

        // ============================================================
        // 3️⃣ CRÉATION (C'est ici que j'ai corrigé la liste vide)
        // ============================================================
        [HttpGet]
        public IActionResult CreateUser()
        {
            // ✅ CORRECTION : On remplit la liste au chargement de la page
            ViewBag.Roles = new List<string> { "Admin", "Enseignant", "Etudiant", "Surveillant" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            // ✅ CORRECTION : On remplit la liste aussi ici en cas d'erreur de validation
            ViewBag.Roles = new List<string> { "Admin", "Enseignant", "Etudiant", "Surveillant" };

            if (!ModelState.IsValid) return View(model);

            if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
            {
                ModelState.AddModelError("NomUtilisateur", "Nom d'utilisateur pris.");
                return View(model);
            }

            var user = new Utilisateur { NomUtilisateur = model.NomUtilisateur, Nom = model.Nom, Prenom = model.Prenom, Email = model.Email, Role = model.Role, IsApproved = true, PendingRole = model.Role, MotDePasseHash = HashPassword(model.MotDePasse) };

            _context.Utilisateurs.Add(user);
            await _context.SaveChangesAsync();
            await CreateSpecificProfile(user, model.Role);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Utilisateur créé avec succès.";
            return RedirectToAction(nameof(CreateUser));
        }

        // ============================================================
        // 4️⃣ LISTE UTILISATEURS
        // ============================================================
        public async Task<IActionResult> ActiveUsers(string role = "All")
        {
            var query = _context.Utilisateurs.Where(u => u.Role != "Admin" && u.IsApproved == true);
            if (!string.IsNullOrEmpty(role) && role != "All") query = query.Where(u => u.Role == role);
            ViewBag.CurrentFilter = role;
            return View("UsersList", await query.OrderBy(u => u.Role).ThenBy(u => u.NomUtilisateur).ToListAsync());
        }

        // --- SUPPRESSION ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null || user.Role == "Admin") { TempData["ErrorMessage"] = "Impossible de supprimer."; return RedirectToAction(nameof(ActiveUsers)); }

            try
            {
                await DeleteSpecificProfile(user, user.Role);
                _context.Utilisateurs.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Utilisateur supprimé.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = "Erreur : " + ex.Message; }
            return RedirectToAction(nameof(ActiveUsers));
        }

        // --- MODIFICATION (GET) ---
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // --- MODIFICATION (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, Utilisateur model, string? nouveauMotDePasse)
        {
            if (id != model.Id) return NotFound();

            // ⚠️ CORRECTION CRITIQUE : Ignorer les champs qui bloquaient la sauvegarde
            ModelState.Remove("MotDePasse");
            ModelState.Remove("MotDePasseHash");
            ModelState.Remove("nouveauMotDePasse");
            ModelState.Remove("PendingRole");
            ModelState.Remove("EnseignantProfil");
            ModelState.Remove("EtudiantProfil");
            ModelState.Remove("SurveillantProfil");
            ModelState.Remove("Notes");
            ModelState.Remove("EmploisDuTemps");
            ModelState.Remove("Cours");

            if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur && u.Id != id))
            {
                ModelState.AddModelError("NomUtilisateur", "Ce nom est déjà pris.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userDb = await _context.Utilisateurs.FindAsync(id);
                    if (userDb == null) return NotFound();

                    userDb.Nom = model.Nom;
                    userDb.Prenom = model.Prenom;
                    userDb.Email = model.Email;
                    userDb.NomUtilisateur = model.NomUtilisateur;

                    if (!string.IsNullOrWhiteSpace(nouveauMotDePasse)) userDb.MotDePasseHash = HashPassword(nouveauMotDePasse);

                    _context.Update(userDb);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Utilisateur modifié avec succès.";
                    return RedirectToAction(nameof(ActiveUsers));
                }
                catch (Exception) { throw; }
            }
            return View(model);
        }

        // ============================================================
        // 5️⃣ COURS & HELPERS
        // ============================================================
        public async Task<IActionResult> ManageCourses()
        {
            return View(await _context.Cours.Include(c => c.Enseignant).ThenInclude(e => e.Utilisateur).OrderBy(c => c.Titre).ToListAsync());
        }

        public async Task<IActionResult> CreateCourse()
        {
            var teachers = await _context.Enseignants.Include(e => e.Utilisateur).Where(e => e.Utilisateur.IsApproved).Select(e => new { e.Id, NomComplet = e.Utilisateur.Nom + " " + e.Utilisateur.Prenom }).ToListAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "NomComplet");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Cours model)
        {
            ModelState.Remove("Enseignant"); ModelState.Remove("EmploisDuTemps"); ModelState.Remove("Notes");
            if (ModelState.IsValid)
            {
                _context.Cours.Add(model); await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cours créé."; return RedirectToAction(nameof(ManageCourses));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var c = await _context.Cours.FindAsync(id);
            if (c != null) { _context.Cours.Remove(c); await _context.SaveChangesAsync(); TempData["SuccessMessage"] = "Cours supprimé."; }
            return RedirectToAction(nameof(ManageCourses));
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var c = await _context.Cours.FindAsync(id);
            if (c == null) return NotFound();
            var teachers = await _context.Enseignants.Include(e => e.Utilisateur).Where(e => e.Utilisateur.IsApproved).Select(e => new { e.Id, NomComplet = e.Utilisateur.Nom + " " + e.Utilisateur.Prenom }).ToListAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "NomComplet", c.EnseignantId);
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Cours model)
        {
            if (id != model.Id) return NotFound();
            ModelState.Remove("Enseignant"); ModelState.Remove("EmploisDuTemps"); ModelState.Remove("Notes");
            if (ModelState.IsValid) { _context.Update(model); await _context.SaveChangesAsync(); return RedirectToAction(nameof(ManageCourses)); }
            return View(model);
        }

        private async Task CreateSpecificProfile(Utilisateur user, string role)
        {
            if (role == "Etudiant" && !_context.Etudiants.Any(x => x.UtilisateurId == user.Id)) _context.Etudiants.Add(new Etudiant { UtilisateurId = user.Id, Nom = user.Nom, Prenom = user.Prenom, Email = user.Email });
            else if (role == "Enseignant" && !_context.Enseignants.Any(x => x.UtilisateurId == user.Id)) _context.Enseignants.Add(new Enseignant { UtilisateurId = user.Id });
            else if (role == "Surveillant" && !_context.Surveillants.Any(x => x.UtilisateurId == user.Id)) _context.Surveillants.Add(new Surveillant { UtilisateurId = user.Id });
        }

        private async Task DeleteSpecificProfile(Utilisateur user, string role)
        {
            if (role == "Etudiant")
            {
                var e = await _context.Etudiants.FirstOrDefaultAsync(x => x.UtilisateurId == user.Id);
                if (e != null) { _context.Notes.RemoveRange(_context.Notes.Where(n => n.EtudiantId == e.Id)); _context.Etudiants.Remove(e); }
            }
            else if (role == "Enseignant")
            {
                var e = await _context.Enseignants.FirstOrDefaultAsync(x => x.UtilisateurId == user.Id);
                if (e != null)
                {
                    var cours = await _context.Cours.Where(c => c.EnseignantId == e.Id).ToListAsync();
                    if (cours.Any()) _context.Cours.RemoveRange(cours);
                    _context.Enseignants.Remove(e);
                }
            }
            else if (role == "Surveillant")
            {
                var s = await _context.Surveillants.FirstOrDefaultAsync(x => x.UtilisateurId == user.Id);
                if (s != null)
                {
                    var grps = await _context.Groupes.Include(g => g.Etudiants).Where(g => g.SurveillantId == s.Id).ToListAsync();
                    foreach (var g in grps) { foreach (var st in g.Etudiants) st.GroupeId = null; }
                    _context.Groupes.RemoveRange(grps);
                    _context.Surveillants.Remove(s);
                }
            }
        }
    }
}