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
using System.Text.Json; // ⚠️ Nécessaire pour le graphique
using System.Threading.Tasks;

// Vérifiez bien votre namespace (généralement Project.Controllers)
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
        // 1️⃣ DASHBOARD (MODIFIÉ POUR LE GRAPHE MOYENNE)
        // ============================================================
        public async Task<IActionResult> Index()
        {
            // --- 1. Compteurs classiques ---
            var totalUsers = await _context.Utilisateurs.CountAsync();
            var pendingUsers = await _context.Utilisateurs.CountAsync(u => u.IsApproved == false && u.Role == "Pending");
            var totalStudents = await _context.Utilisateurs.CountAsync(u => u.Role == "Etudiant" && u.IsApproved == true);
            var totalTeachers = await _context.Utilisateurs.CountAsync(u => u.Role == "Enseignant" && u.IsApproved == true);
            var totalSupervisors = await _context.Utilisateurs.CountAsync(u => u.Role == "Surveillant" && u.IsApproved == true);

            // --- 2. LOGIQUE DU GRAPHE : MOYENNE PAR GROUPE / MATIÈRE ---

            // On récupère toutes les notes PUBLIÉES avec les liens nécessaires
            var allNotes = await _context.Notes
                .Include(n => n.Etudiant).ThenInclude(e => e.Groupe)
                .Include(n => n.Cours)
                .Where(n => n.IsPublished) // Uniquement les notes visibles
                .ToListAsync();

            // Axe X : Liste des Matières (ex: C#, Java, UML...)
            var matieres = allNotes.Select(n => n.Cours.Titre).Distinct().OrderBy(t => t).ToList();

            // Les Groupes (ex: G1, G2...)
            var groupes = allNotes.Select(n => n.Etudiant.Groupe.Nom).Distinct().OrderBy(g => g).ToList();

            // Construction des Datasets pour Chart.js
            var datasets = new List<object>();

            // Palette de couleurs pour différencier les groupes
            var colors = new[] {
                "rgba(78, 115, 223, 0.9)",  // Bleu
                "rgba(28, 200, 138, 0.9)",  // Vert
                "rgba(54, 185, 204, 0.9)",  // Cyan
                "rgba(246, 194, 62, 0.9)",  // Jaune
                "rgba(231, 74, 59, 0.9)",   // Rouge
                "rgba(133, 135, 150, 0.9)"  // Gris
            };
            int colorIndex = 0;

            foreach (var grpName in groupes)
            {
                var moyennes = new List<decimal>();

                foreach (var matiereName in matieres)
                {
                    // Notes de ce groupe dans cette matière
                    var notesGroupeMatiere = allNotes
                        .Where(n => n.Etudiant.Groupe.Nom == grpName && n.Cours.Titre == matiereName);

                    // Moyenne (0 si pas de notes)
                    decimal avg = notesGroupeMatiere.Any()
                        ? Math.Round(notesGroupeMatiere.Average(n => n.Valeur), 2)
                        : 0;

                    moyennes.Add(avg);
                }

                datasets.Add(new
                {
                    label = grpName,
                    data = moyennes,
                    backgroundColor = colors[colorIndex % colors.Length],
                    borderColor = colors[colorIndex % colors.Length],
                    borderWidth = 1
                });
                colorIndex++;
            }

            // Envoi des données JSON à la vue via ViewBag (plus flexible que le ViewModel pour les graphes complexes)
            ViewBag.ChartLabels = JsonSerializer.Serialize(matieres);
            ViewBag.ChartDatasets = JsonSerializer.Serialize(datasets);

            // --- 3. Remplissage du ViewModel ---
            var viewModel = new AdminDashboardViewModel
            {
                UtilisateursEnAttente = pendingUsers,
                TotalUtilisateurs = totalUsers,
                TotalEtudiants = totalStudents,
                TotalEnseignants = totalTeachers,
                TotalSurveillants = totalSupervisors,

                // On laisse vide ici car on utilise ViewBag pour le nouveau graphe complexe
                ChartLabels = new string[] { },
                ChartValues = new int[] { }
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
        // 4️⃣ GESTION DES UTILISATEURS ACTIFS (AVEC FILTRE)
        // ============================================================
        public async Task<IActionResult> ActiveUsers(string role = "All")
        {
            // 1. On prépare la requête de base (Tous les actifs sauf Admin)
            var query = _context.Utilisateurs
                .Where(u => u.Role != "Admin" && u.IsApproved == true);

            // 2. On applique le filtre si nécessaire
            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                query = query.Where(u => u.Role == role);
            }

            // 3. On exécute la requête
            var users = await query
                .OrderBy(u => u.Role)
                .ThenBy(u => u.NomUtilisateur)
                .ToListAsync();

            // 4. On stocke le filtre actuel pour colorer le bouton actif dans la vue
            ViewBag.CurrentFilter = role;

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
        // 🧩 HELPERS
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
                        var groupes = await _context.Groupes.Where(g => g.SurveillantId == s.Id).ToListAsync();
                        if (groupes.Any())
                        {
                            _context.Groupes.RemoveRange(groupes);
                        }
                        await _context.SaveChangesAsync();
                        _context.Surveillants.Remove(s);
                    }
                    break;
            }
            await Task.CompletedTask;
        }

        // ============================================================
        // 5️⃣ GESTION DES COURS (MATIÈRES)
        // ============================================================

        public async Task<IActionResult> ManageCourses()
        {
            var courses = await _context.Cours
                .Include(c => c.Enseignant)
                .ThenInclude(e => e.Utilisateur)
                .OrderBy(c => c.Titre)
                .ToListAsync();

            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Cours model)
        {
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Cours.FindAsync(id);
            if (course != null)
            {
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
        // ============================================================
        // ➕ AJOUTER CECI : MODIFICATION D'UN COURS (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Cours.FindAsync(id);
            if (course == null) return NotFound();

            // Liste des enseignants pour le menu déroulant
            var teachers = await _context.Enseignants
                .Include(e => e.Utilisateur)
                .Where(e => e.Utilisateur.IsApproved == true)
                .Select(e => new
                {
                    Id = e.Id,
                    NomComplet = $"{e.Utilisateur.Nom} {e.Utilisateur.Prenom}"
                })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "NomComplet", course.EnseignantId);
            return View(course);
        }

        // ============================================================
        // ➕ AJOUTER CECI : MODIFICATION D'UN COURS (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Cours model)
        {
            if (id != model.Id) return NotFound();

            // Nettoyage validation
            ModelState.Remove("Enseignant");
            ModelState.Remove("EmploisDuTemps");
            ModelState.Remove("Notes");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le cours '{model.Titre}' a été mis à jour.";
                    return RedirectToAction(nameof(ManageCourses));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Cours.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
            }

            // Si erreur, on recharge la liste
            var teachers = await _context.Enseignants
                .Include(e => e.Utilisateur)
                .Where(e => e.Utilisateur.IsApproved == true)
                .Select(e => new { Id = e.Id, NomComplet = $"{e.Utilisateur.Nom} {e.Utilisateur.Prenom}" })
                .ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "NomComplet", model.EnseignantId);
            return View(model);
        }
    }
}