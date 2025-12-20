using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project.Data // Vérifiez que le namespace correspond bien à votre projet
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly SchoolContext _context;

        public AdminController(SchoolContext context)
        {
            _context = context;
        }

        // ============================================================
        // 🔐 HASH MOT DE PASSE
        // ============================================================
        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
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
            var totalSupervisors = await _context.Utilisateurs.CountAsync(u => u.Role == "Surveillant" && u.IsApproved == true);

            var viewModel = new AdminDashboardViewModel
            {
                UtilisateursEnAttente = pendingUsers,
                TotalUtilisateurs = totalUsers,
                TotalEtudiants = totalStudents,
                TotalEnseignants = totalTeachers,
                TotalSurveillants = totalSupervisors,

                // Données pour le graphique Chart.js
                ChartLabels = new string[] { "Jan", "Fév", "Mar", "Avr", "Mai", "Juin" },
                ChartValues = new int[] { 12, 19, 3, 5, 2, 30 }
            };

            ViewData["Title"] = "Tableau de Bord Administrateur";
            return View(viewModel);
        }

        // ============================================================
        // 2️⃣ APPROBATION DES UTILISATEURS (PENDING)
        // ============================================================
        public async Task<IActionResult> PendingUsers()
        {
            var pendingUsers = await _context.Utilisateurs
                .Where(u => u.IsApproved == false && u.Role == "Pending")
                .OrderBy(u => u.NomUtilisateur)
                .ToListAsync();

            return View(pendingUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null || user.IsApproved || user.Role != "Pending")
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable ou déjà approuvé.";
                return RedirectToAction(nameof(PendingUsers));
            }
            user.IsApproved = true;
            user.Role = user.PendingRole; 
            _context.Utilisateurs.Update(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"L'utilisateur {user.NomUtilisateur} a été approuvé.";
            return RedirectToAction(nameof(PendingUsers));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable.";
                return RedirectToAction(nameof(PendingUsers));
            }
            // On nettoie les liens éventuels avant suppression
            await DeleteSpecificProfile(user, user.PendingRole);
            
            _context.Utilisateurs.Remove(user);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"L'utilisateur {user.NomUtilisateur} a été rejeté et supprimé.";
            return RedirectToAction(nameof(PendingUsers));
        }

        // ============================================================
        // 3️⃣ CRÉATION UTILISATEUR (ADMIN)
        // ============================================================
        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = new List<string> { "Admin", "Enseignant", "Etudiant", "Surveillant" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
        {
            ViewBag.Roles = new List<string> { "Admin", "Enseignant", "Etudiant", "Surveillant" };

            if (!ModelState.IsValid) return View(model);

            if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
            {
                ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà pris.");
                return View(model);
            }

            var newUser = new Utilisateur
            {
                NomUtilisateur = model.NomUtilisateur,
                Nom = model.Nom,
                Prenom = model.Prenom,
                Email = model.Email,
                Role = model.Role,
                IsApproved = true,
                PendingRole = model.Role,
                MotDePasseHash = HashPassword(model.MotDePasse)
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Utilisateurs.Add(newUser);
                    await _context.SaveChangesAsync();
                    await CreateSpecificProfile(newUser, model.Role);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"L'utilisateur '{newUser.NomUtilisateur}' a été créé avec succès.";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = $"Erreur lors de la création : {ex.Message}";
                }
            }
            return RedirectToAction(nameof(CreateUser));
        }

        // ============================================================
        // 4️⃣ GESTION DES UTILISATEURS ACTIFS
        // ============================================================
        public async Task<IActionResult> ActiveUsers()
        {
            var users = await _context.Utilisateurs
                .Where(u => u.Role != "Admin" && u.IsApproved == true)
                .OrderBy(u => u.Role)
                .ThenBy(u => u.NomUtilisateur)
                .ToListAsync();

            // Pointe vers la vue UsersList.cshtml
            return View("UsersList", users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Utilisateurs.FindAsync(id);

            if (user == null || user.Role == "Admin")
            {
                TempData["ErrorMessage"] = "Utilisateur introuvable ou suppression non autorisée.";
                return RedirectToAction(nameof(ActiveUsers));
            }

            try 
            {
                // Appel de la méthode corrigée pour gérer les clés étrangères
                await DeleteSpecificProfile(user, user.Role);
                
                _context.Utilisateurs.Remove(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"L'utilisateur {user.NomUtilisateur} a été supprimé.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erreur critique lors de la suppression : " + ex.Message;
            }

            return RedirectToAction(nameof(ActiveUsers));
        }

        // ============================================================
        // 🧩 HELPERS (Avec correction BUG SUPPRESSION)
        // ============================================================
        private async Task CreateSpecificProfile(Utilisateur user, string role)
        {
            switch (role)
            {
                case "Etudiant":
                    _context.Etudiants.Add(new Etudiant { UtilisateurId = user.Id, Nom = user.Nom, Prenom = user.Prenom, Email = user.Email });
                    break;
                case "Enseignant":
                    _context.Enseignants.Add(new Enseignant { UtilisateurId = user.Id });
                    break;
                case "Surveillant":
                    _context.Surveillants.Add(new Surveillant { UtilisateurId = user.Id });
                    break;
            }
            await Task.CompletedTask;
        }

        // 🔥 C'EST ICI QUE LA CORRECTION A ÉTÉ FAITE
        private async Task DeleteSpecificProfile(Utilisateur user, string role)
        {
            switch (role)
            {
                case "Etudiant":
                    var e = await _context.Etudiants.FirstOrDefaultAsync(x => x.UtilisateurId == user.Id);
                    if (e != null) _context.Etudiants.Remove(e);
                    break;

                case "Enseignant":
                    var ens = await _context.Enseignants.FirstOrDefaultAsync(x => x.UtilisateurId == user.Id);
                    if (ens != null) _context.Enseignants.Remove(ens);
                    break;

                case "Surveillant":
                    var s = await _context.Surveillants.FirstOrDefaultAsync(x => x.UtilisateurId == user.Id);
                    if (s != null)
                    {
                        // Récupérer les groupes
                        var groupes = await _context.Groupes.Where(g => g.SurveillantId == s.Id).ToListAsync();

                        // AU LIEU DE METTRE NULL, ON SUPPRIME LES GROUPES
                        if (groupes.Any())
                        {
                            _context.Groupes.RemoveRange(groupes); // Suppression radicale
                        }

                        await _context.SaveChangesAsync(); // Valider la suppression des groupes

                        // Maintenant on peut supprimer le surveillant
                        _context.Surveillants.Remove(s);
                    }
                    break;
            }
            await Task.CompletedTask;
        }
        // ============================================================
        // 5️⃣ GESTION DES COURS (MATIÈRES)
        // ============================================================

        // AFFICHER LA LISTE DES COURS
        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Cours
                .Include(c => c.Enseignant)
                .ThenInclude(e => e.Utilisateur) // Pour récupérer le Nom/Prénom du prof
                .OrderBy(c => c.Titre)
                .ToListAsync();

            return View(courses);
        }

        // CRÉER UN COURS (GET)
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            // On charge la liste des enseignants VALIDÉS pour le menu déroulant
            var teachers = await _context.Enseignants
                .Include(e => e.Utilisateur)
                .Where(e => e.Utilisateur.IsApproved == true)
                .Select(e => new
                {
                    Id = e.Id,
                    NomComplet = $"{e.Utilisateur.Nom} {e.Utilisateur.Prenom}"
                })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "NomComplet");
            return View();
        }

        // CRÉER UN COURS (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Cours model)
        {
            // On enlève les validations inutiles pour ce formulaire
            ModelState.Remove("Enseignant");
            ModelState.Remove("EmploisDuTemps");
            ModelState.Remove("Notes");

            if (ModelState.IsValid)
            {
                _context.Cours.Add(model);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Le cours '{model.Titre}' a été créé avec succès.";
                return RedirectToAction(nameof(ManageCourses));
            }

            // Si erreur, on recharge la liste des profs pour ne pas casser la vue
            var teachers = await _context.Enseignants
                .Include(e => e.Utilisateur)
                .Where(e => e.Utilisateur.IsApproved == true)
                .Select(e => new
                {
                    Id = e.Id,
                    NomComplet = $"{e.Utilisateur.Nom} {e.Utilisateur.Prenom}"
                })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "NomComplet", model.EnseignantId);
            return View(model);
        }

        // SUPPRIMER UN COURS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Cours.FindAsync(id);
            if (course != null)
            {
                // Attention : Si le cours est utilisé dans un Emploi du temps ou des Notes, 
                // cela peut provoquer une erreur de clé étrangère.
                try
                {
                    _context.Cours.Remove(course);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cours supprimé.";
                }
                catch
                {
                    TempData["ErrorMessage"] = "Impossible de supprimer ce cours car il est lié à des emplois du temps ou des notes.";
                }
            }
            return RedirectToAction(nameof(ManageCourses));
        }
    }
}