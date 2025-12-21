using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Project.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Project.Data;

namespace Project.Controllers
{
    [Authorize(Roles = "Surveillant")]
    public class SurveillantController : Controller
    {
        private readonly SchoolContext _context;

        public SurveillantController(SchoolContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ DASHBOARD
        // ============================================================
        public IActionResult Index()
        {
            ViewData["Title"] = "Tableau de Bord du Surveillant";
            return View();
        }


        // ============================================================
        // 2️⃣ GESTION DES GROUPES (LISTE ET CRÉATION)
        // ============================================================

        public async Task<IActionResult> ManageGroups()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out var currentUserId)) return Forbid();

            var supervisorProfile = await _context.Surveillants
             .FirstOrDefaultAsync(s => s.UtilisateurId == currentUserId);

            if (supervisorProfile == null) return Forbid();

            var managedGroups = await _context.Groupes
             .Where(g => g.SurveillantId == supervisorProfile.Id)
             .Include(g => g.Etudiants)
             .ToListAsync();

            ViewData["SupervisorId"] = supervisorProfile.Id;
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(managedGroups);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGroup(string nom, int surveillantId)
        {
            if (string.IsNullOrWhiteSpace(nom))
            {
                TempData["ErrorMessage"] = "Le nom du groupe est requis.";
                return RedirectToAction(nameof(ManageGroups));
            }

            var newGroup = new Groupe
            {
                Nom = nom,
                SurveillantId = surveillantId
            };

            _context.Groupes.Add(newGroup);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Le groupe '{nom}' a été créé avec succès.";
            return RedirectToAction(nameof(ManageGroups));
        }

        // ============================================================
        // 3️⃣ MODIFICATION DES GROUPES (GET & POST)
        // ============================================================

        // GET : Affiche le formulaire de modification
        [HttpGet]
        public async Task<IActionResult> EditGroup(int id)
        {
            var groupe = await _context.Groupes.FindAsync(id);
            if (groupe == null)
            {
                return NotFound();
            }
            return View(groupe);
        }

        // POST : Enregistre les modifications
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGroup(int id, Groupe model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingGroup = await _context.Groupes.FindAsync(id);
                    if (existingGroup == null) return NotFound();

                    // Mise à jour du nom uniquement
                    existingGroup.Nom = model.Nom;

                    _context.Update(existingGroup);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Le nom du groupe a été modifié avec succès.";
                    return RedirectToAction(nameof(ManageGroups));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Groupes.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
            }
            return View(model);
        }

        // ============================================================
        // 4️⃣ GESTION DES ÉTUDIANTS (ASSIGNATION & LISTE)
        // ============================================================

        // MODIFICATION : Ajout du paramètre groupId pour pré-sélectionner le groupe
        public async Task<IActionResult> AssignStudents(int? groupId)
        {
            var unassignedStudents = await _context.Etudiants
             .Where(e => e.GroupeId == null)
             .Include(e => e.Utilisateur)
             .OrderBy(e => e.Nom)
             .ToListAsync();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out var currentUserId)) return Forbid();

            var supervisorProfile = await _context.Surveillants
             .FirstOrDefaultAsync(s => s.UtilisateurId == currentUserId);

            var groups = await _context.Groupes
             .Where(g => g.SurveillantId == supervisorProfile.Id)
             .ToListAsync();

            ViewBag.Groups = groups;
            // On passe l'ID du groupe pour la présélection dans la vue
            ViewBag.PreselectedGroupId = groupId;
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            return View(unassignedStudents);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(int studentId, int groupId)
        {
            var student = await _context.Etudiants.FindAsync(studentId);
            var group = await _context.Groupes.FindAsync(groupId);

            if (student != null && group != null)
            {
                student.GroupeId = groupId;
                _context.Etudiants.Update(student);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Étudiant assigné au groupe {group.Nom}.";
            }
            // On retourne à la page d'assignation pour en ajouter d'autres
            return RedirectToAction(nameof(AssignStudents), new { groupId = groupId });
        }

        // Action pour voir la liste des étudiants d'un groupe (bouton Oeil)
        public async Task<IActionResult> GroupStudents(int groupId)
        {
            var groupe = await _context.Groupes
                .Include(g => g.Etudiants)
                    .ThenInclude(e => e.Utilisateur)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (groupe == null)
            {
                return NotFound();
            }

            return View(groupe);
        }

        // AJOUT : Retirer un étudiant d'un groupe (Bouton Corbeille)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveStudentFromGroup(int studentId)
        {
            var student = await _context.Etudiants.FindAsync(studentId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Étudiant introuvable.";
                return RedirectToAction(nameof(ManageGroups));
            }

            // On retire l'étudiant du groupe
            student.GroupeId = null;

            _context.Etudiants.Update(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Étudiant retiré du groupe avec succès.";

            // On redirige vers la liste principale des groupes
            return RedirectToAction(nameof(ManageGroups));
        }


        // ============================================================
        // 5️⃣ GESTION DE L'EMPLOI DU TEMPS
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ManageSchedule(int? groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out var currentUserId)) return Forbid();

            var supervisorProfile = await _context.Surveillants
                .FirstOrDefaultAsync(s => s.UtilisateurId == currentUserId);

            if (supervisorProfile == null) return Forbid();

            // 1. Charger les données pour les listes déroulantes
            var groupes = await _context.Groupes
                .Where(g => g.SurveillantId == supervisorProfile.Id)
                .OrderBy(g => g.Nom)
                .ToListAsync();

            var enseignants = await _context.Enseignants
                .Include(e => e.Utilisateur)
                .Where(e => e.Utilisateur.IsApproved == true)
                .OrderBy(e => e.Utilisateur.Nom)
                .ToListAsync();

            var cours = await _context.Cours
                .OrderBy(c => c.Titre)
                .ToListAsync();

            // 2. Initialiser le ViewModel
            var viewModel = new AddScheduleViewModel
            {
                GroupeId = groupId ?? 0,
                Groupes = new SelectList(groupes, "Id", "Nom", groupId),
                CoursList = new SelectList(cours, "Id", "Titre"),
                Enseignants = new SelectList(
                    enseignants.Select(e => new { e.Id, NomComplet = $"{e.Utilisateur.Prenom} {e.Utilisateur.Nom}" }),
                    "Id", "NomComplet"
                ),
                Jours = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday)
                )
            };

            // 3. Charger le planning si un groupe est sélectionné
            if (groupId.HasValue && groupId.Value > 0)
            {
                viewModel.SelectedGroupName = groupes.FirstOrDefault(g => g.Id == groupId.Value)?.Nom;

                viewModel.ScheduleList = await _context.EmploisDuTemps
                    .Where(s => s.GroupeId == groupId.Value)
                    .Include(s => s.Cours)
                    .Include(s => s.Enseignant)
                        .ThenInclude(e => e.Utilisateur)
                    .OrderBy(s => s.Jour)
                    .ThenBy(s => s.HeureDebut)
                    .ToListAsync();
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(viewModel);
        }

        // ============================================================
        // 6️⃣ AJOUTER UNE SESSION (COURS)
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSession(AddScheduleViewModel model)
        {
            ModelState.Remove("Groupes");
            ModelState.Remove("CoursList");
            ModelState.Remove("Enseignants");
            ModelState.Remove("Jours");
            ModelState.Remove("ScheduleList");
            ModelState.Remove("SelectedGroupName");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Erreur de validation. Vérifiez les champs.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
            }

            // Vérification chevauchement GROUPE
            var groupOverlap = await _context.EmploisDuTemps
                .AnyAsync(s => s.GroupeId == model.GroupeId &&
                               s.Jour == model.Jour &&
                               (model.HeureDebut < s.HeureFin) &&
                               (model.HeureFin > s.HeureDebut));

            if (groupOverlap)
            {
                TempData["ErrorMessage"] = "Conflit : Le groupe a déjà cours à cette heure.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
            }

            // Vérification chevauchement ENSEIGNANT
            var teacherOverlap = await _context.EmploisDuTemps
                .AnyAsync(s => s.EnseignantId == model.EnseignantId &&
                               s.Jour == model.Jour &&
                               (model.HeureDebut < s.HeureFin) &&
                               (model.HeureFin > s.HeureDebut));

            if (teacherOverlap)
            {
                TempData["ErrorMessage"] = "Conflit : L'enseignant a déjà cours à cette heure.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
            }

            var newSession = new EmploiDuTemps
            {
                GroupeId = model.GroupeId,
                CoursId = model.CoursId,
                EnseignantId = model.EnseignantId,
                Jour = model.Jour,
                HeureDebut = model.HeureDebut,
                HeureFin = model.HeureFin
            };

            _context.EmploisDuTemps.Add(newSession);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Session ajoutée avec succès.";
            return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
        }

        // ============================================================
        // 7️⃣ MODIFIER UNE SESSION (GET & POST)
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> EditSession(int id)
        {
            var session = await _context.EmploisDuTemps
                .Include(e => e.Cours)
                .Include(e => e.Enseignant).ThenInclude(en => en.Utilisateur)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (session == null) return NotFound();

            // Rechargement des listes pour le formulaire
            ViewBag.CoursId = new SelectList(_context.Cours, "Id", "Titre", session.CoursId);

            var enseignants = _context.Enseignants
                .Include(e => e.Utilisateur)
                .Select(e => new { Id = e.Id, NomComplet = e.Utilisateur.Nom + " " + e.Utilisateur.Prenom });
            ViewBag.EnseignantId = new SelectList(enseignants, "Id", "NomComplet", session.EnseignantId);

            var jours = Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday)
                .Select(d => new { Id = d, Name = d.ToString() });
            ViewBag.Jour = new SelectList(jours, "Id", "Name", session.Jour);

            return View(session);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSession(int id, EmploiDuTemps model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Session modifiée avec succès.";
                    return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.EmploisDuTemps.Any(e => e.Id == model.Id)) return NotFound();
                    else throw;
                }
            }

            // En cas d'erreur, recharger les listes
            ViewBag.CoursId = new SelectList(_context.Cours, "Id", "Titre", model.CoursId);
            var enseignants = _context.Enseignants.Include(e => e.Utilisateur)
                .Select(e => new { Id = e.Id, NomComplet = e.Utilisateur.Nom + " " + e.Utilisateur.Prenom });
            ViewBag.EnseignantId = new SelectList(enseignants, "Id", "NomComplet", model.EnseignantId);

            return View(model);
        }


        // ============================================================
        // 8️⃣ SUPPRIMER UNE SESSION
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            var session = await _context.EmploisDuTemps.FindAsync(sessionId);

            if (session == null)
            {
                TempData["ErrorMessage"] = "Session introuvable.";
                return RedirectToAction(nameof(ManageSchedule));
            }

            int groupId = session.GroupeId;

            _context.EmploisDuTemps.Remove(session);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Session supprimée.";
            return RedirectToAction(nameof(ManageSchedule), new { groupId = groupId });
        }

        // ============================================================
        // 9️⃣ RÉINITIALISER TOUT LE PLANNING
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSchedule(int groupId)
        {
            var sessions = await _context.EmploisDuTemps
                .Where(s => s.GroupeId == groupId)
                .ToListAsync();

            if (sessions == null || !sessions.Any())
            {
                TempData["ErrorMessage"] = "Le planning est déjà vide.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = groupId });
            }

            _context.EmploisDuTemps.RemoveRange(sessions);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Planning réinitialisé.";
            return RedirectToAction(nameof(ManageSchedule), new { groupId = groupId });
        }

        // ============================================================
        // 🔟 CONSULTATION ET PUBLICATION DES NOTES
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ConsultGrades(int? groupId, int? coursId)
        {
            var supervisorUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var supervisor = await _context.Surveillants.FirstOrDefaultAsync(s => s.UtilisateurId == supervisorUserId);

            var groupes = await _context.Groupes
                .Where(g => g.SurveillantId == supervisor.Id)
                .OrderBy(g => g.Nom).ToListAsync();

            var cours = await _context.Cours.OrderBy(c => c.Titre).ToListAsync();

            var viewModel = new GradeManagementViewModel
            {
                SelectedGroupId = groupId,
                SelectedCoursId = coursId,
                Groupes = new SelectList(groupes, "Id", "Nom", groupId),
                Cours = new SelectList(cours, "Id", "Titre", coursId)
            };

            if (groupId.HasValue && coursId.HasValue)
            {
                var notes = await _context.Notes
                    .Include(n => n.Etudiant)
                        .ThenInclude(e => e.Utilisateur)
                    .Where(n => n.Etudiant.GroupeId == groupId.Value && n.CoursId == coursId.Value)
                    .ToListAsync();

                ViewBag.AreNotesPublished = notes.Any(n => n.IsPublished);

                foreach (var note in notes)
                {
                    viewModel.Students.Add(new StudentGradeItem
                    {
                        EtudiantId = note.EtudiantId,
                        EtudiantNom = $"{note.Etudiant.Utilisateur.Nom} {note.Etudiant.Utilisateur.Prenom}",
                        Note = (double)note.Valeur
                    });
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishGrades(int groupId, int coursId)
        {
            var notes = await _context.Notes
                .Include(n => n.Etudiant)
                .Where(n => n.Etudiant.GroupeId == groupId && n.CoursId == coursId)
                .ToListAsync();

            if (!notes.Any())
            {
                TempData["ErrorMessage"] = "Aucune note à publier.";
                return RedirectToAction(nameof(ConsultGrades), new { groupId, coursId });
            }

            foreach (var note in notes)
            {
                note.IsPublished = true;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Notes publiées.";
            return RedirectToAction(nameof(ConsultGrades), new { groupId, coursId });
        }
    }
}