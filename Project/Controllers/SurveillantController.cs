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

namespace Project.Controllers // Assurez-vous que le namespace est correct
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
        // 3️⃣ ASSIGNATION DES ÉTUDIANTS AUX GROUPES
        // ============================================================

        public async Task<IActionResult> AssignStudents()
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
            return RedirectToAction(nameof(AssignStudents));
        }


        // ============================================================
        // 4️⃣ GESTION DE L'EMPLOI DU TEMPS (CORRIGÉE & SÉCURISÉE)
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
                // Si groupId est null, on met 0, sinon l'ID
                GroupeId = groupId ?? 0,

                // On remplit les listes déroulantes (SelectLists)
                Groupes = new SelectList(groupes, "Id", "Nom", groupId),
                CoursList = new SelectList(cours, "Id", "Titre"), // Attention: Titre vs Nom selon votre modèle
                Enseignants = new SelectList(
                    enseignants.Select(e => new { e.Id, NomComplet = $"{e.Utilisateur.Prenom} {e.Utilisateur.Nom}" }),
                    "Id", "NomComplet"
                ),
                Jours = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                    .Cast<DayOfWeek>()
                    .Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday)
                )
            };

            // 3. Si un groupe est sélectionné, on charge son planning DANS le ViewModel
            if (groupId.HasValue && groupId.Value > 0)
            {
                // Nom du groupe
                viewModel.SelectedGroupName = groupes.FirstOrDefault(g => g.Id == groupId.Value)?.Nom;

                // Liste des sessions
                viewModel.ScheduleList = await _context.EmploisDuTemps
                    .Where(s => s.GroupeId == groupId.Value)
                    .Include(s => s.Cours)
                    .Include(s => s.Enseignant)
                        .ThenInclude(e => e.Utilisateur)
                    .OrderBy(s => s.Jour)
                    .ThenBy(s => s.HeureDebut)
                    .ToListAsync();
            }

            // Messages Flash
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSession(AddScheduleViewModel model)
        {
            // IMPORTANT : On retire les propriétés d'affichage de la validation
            // car elles ne sont pas renvoyées par le formulaire POST.
            ModelState.Remove("Groupes");
            ModelState.Remove("CoursList");
            ModelState.Remove("Enseignants");
            ModelState.Remove("Jours");
            ModelState.Remove("ScheduleList"); // Celle-ci causait souvent des problèmes si non retirée
            ModelState.Remove("SelectedGroupName");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Erreur de validation. Veuillez vérifier tous les champs.";
                // En cas d'erreur, on recharge la page avec le même groupId
                return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
            }

            // 1. Vérifiez les chevauchements d'horaires pour le GROUPE
            var groupOverlapExists = await _context.EmploisDuTemps
                .AnyAsync(s => s.GroupeId == model.GroupeId &&
                               s.Jour == model.Jour &&
                               (model.HeureDebut < s.HeureFin) &&
                               (model.HeureFin > s.HeureDebut));

            if (groupOverlapExists)
            {
                TempData["ErrorMessage"] = "Conflit d'horaire pour le GROUPE! Une session est déjà planifiée à ce moment.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
            }

            // 2. Vérifiez les chevauchements d'horaires pour l'ENSEIGNANT
            var teacherOverlapExists = await _context.EmploisDuTemps
                .AnyAsync(s => s.EnseignantId == model.EnseignantId &&
                               s.Jour == model.Jour &&
                               (model.HeureDebut < s.HeureFin) &&
                               (model.HeureFin > s.HeureDebut));

            if (teacherOverlapExists)
            {
                TempData["ErrorMessage"] = "Conflit d'horaire pour l'ENSEIGNANT! L'enseignant est déjà assigné à une autre session à ce moment.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
            }

            // 3. Créer la nouvelle session
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

            TempData["SuccessMessage"] = $"Session ajoutée le {newSession.Jour} de {newSession.HeureDebut:hh\\:mm} à {newSession.HeureFin:hh\\:mm}.";
            return RedirectToAction(nameof(ManageSchedule), new { groupId = model.GroupeId });
        }
        // ============================================================
        // 5️⃣ SUPPRESSION D'UNE SESSION
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            var session = await _context.EmploisDuTemps.FindAsync(sessionId);

            if (session == null)
            {
                TempData["ErrorMessage"] = "Session introuvable.";
                // On redirige vers la page de gestion sans ID spécifique si on ne trouve pas la session
                return RedirectToAction(nameof(ManageSchedule));
            }

            // On garde l'ID du groupe pour recharger le bon planning après suppression
            int groupId = session.GroupeId;

            _context.EmploisDuTemps.Remove(session);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Session supprimée avec succès.";

            // Redirection vers le planning du groupe concerné
            return RedirectToAction(nameof(ManageSchedule), new { groupId = groupId });
        }
        // ============================================================
        // 6️⃣ RÉINITIALISER TOUT LE PLANNING DU GROUPE
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetSchedule(int groupId)
        {
            // 1. On récupère toutes les sessions de ce groupe
            var sessions = await _context.EmploisDuTemps
                .Where(s => s.GroupeId == groupId)
                .ToListAsync();

            if (sessions == null || !sessions.Any())
            {
                TempData["ErrorMessage"] = "Le planning est déjà vide.";
                return RedirectToAction(nameof(ManageSchedule), new { groupId = groupId });
            }

            // 2. Suppression de masse
            _context.EmploisDuTemps.RemoveRange(sessions);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "L'emploi du temps a été entièrement réinitialisé.";

            // 3. Retour à la page
            return RedirectToAction(nameof(ManageSchedule), new { groupId = groupId });
        }
        // ============================================================
        // 7️⃣ CONSULTATION ET PUBLICATION DES NOTES
        // ============================================================

        [HttpGet]
        public async Task<IActionResult> ConsultGrades(int? groupId, int? coursId)
        {
            // 1. Charger les listes pour les filtres
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

            // 2. Si filtres sélectionnés, charger les notes
            if (groupId.HasValue && coursId.HasValue)
            {
                var notes = await _context.Notes
                    .Include(n => n.Etudiant)
                        .ThenInclude(e => e.Utilisateur)
                    .Where(n => n.Etudiant.GroupeId == groupId.Value && n.CoursId == coursId.Value)
                    .ToListAsync();

                // On vérifie si au moins une note est déjà publiée pour ce groupe/matière
                ViewBag.AreNotesPublished = notes.Any(n => n.IsPublished);

                foreach (var note in notes)
                {
                    viewModel.Students.Add(new StudentGradeItem
                    {
                        EtudiantId = note.EtudiantId,
                        EtudiantNom = $"{note.Etudiant.Utilisateur.Nom} {note.Etudiant.Utilisateur.Prenom}",
                        Note = (double)note.Valeur // Affichage seulement
                    });
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishGrades(int groupId, int coursId)
        {
            // 1. Récupérer toutes les notes de ce groupe pour cette matière
            var notes = await _context.Notes
                .Include(n => n.Etudiant)
                .Where(n => n.Etudiant.GroupeId == groupId && n.CoursId == coursId)
                .ToListAsync();

            if (!notes.Any())
            {
                TempData["ErrorMessage"] = "Aucune note à publier pour ce groupe.";
                return RedirectToAction(nameof(ConsultGrades), new { groupId, coursId });
            }

            // 2. Passer tout le monde à "IsPublished = true"
            foreach (var note in notes)
            {
                note.IsPublished = true;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Les notes ont été publiées et sont maintenant visibles par les étudiants.";
            return RedirectToAction(nameof(ConsultGrades), new { groupId, coursId });
        }
    }
}