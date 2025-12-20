using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using Project.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace Project.Controllers
{
    [Authorize(Roles = "Enseignant")]
    public class EnseignantController : Controller
    {
        private readonly SchoolContext _context;

        public EnseignantController(SchoolContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ C'EST CETTE MÉTHODE QUI MANQUAIT PEUT-ÊTRE
        // ============================================================
        public IActionResult Index()
        {
            return View(); // Cela va chercher Views/Enseignant/Index.cshtml
        }

        // ============================================================
        // 2️⃣ MON EMPLOI DU TEMPS
        // ============================================================
        public async Task<IActionResult> MySchedule()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int currentUserId)) return Forbid();

            var enseignant = await _context.Enseignants
                .FirstOrDefaultAsync(e => e.UtilisateurId == currentUserId);

            if (enseignant == null) return Forbid();

            var mySchedule = await _context.EmploisDuTemps
                .Where(e => e.EnseignantId == enseignant.Id)
                .Include(e => e.Groupe)
                .Include(e => e.Cours)
                .OrderBy(e => e.Jour)
                .ThenBy(e => e.HeureDebut)
                .ToListAsync();

            return View(mySchedule);
        }

        // ============================================================
        // 3️⃣ GESTION DES NOTES (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ManageGrades(int? groupId, int? coursId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int currentUserId)) return Forbid();

            var enseignant = await _context.Enseignants.FirstOrDefaultAsync(e => e.UtilisateurId == currentUserId);
            if (enseignant == null) return Forbid();

            var groupes = await _context.Groupes.OrderBy(g => g.Nom).ToListAsync();
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
                var students = await _context.Etudiants
                    .Where(e => e.GroupeId == groupId.Value)
                    .Include(e => e.Utilisateur)
                    .OrderBy(e => e.Nom)
                    .ToListAsync();

                var existingNotes = await _context.Notes
                    .Where(n => n.CoursId == coursId.Value && n.Etudiant.GroupeId == groupId.Value)
                    .ToListAsync();

                foreach (var student in students)
                {
                    var existingNote = existingNotes.FirstOrDefault(n => n.EtudiantId == student.Id);
                    viewModel.Students.Add(new StudentGradeItem
                    {
                        EtudiantId = student.Id,
                        EtudiantNom = $"{student.Utilisateur.Nom} {student.Utilisateur.Prenom}",
                        Note = existingNote != null ? (double)existingNote.Valeur : null
                    });
                }
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(viewModel);
        }

        // ============================================================
        // 4️⃣ SAUVEGARDE DES NOTES (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrades(GradeManagementViewModel model)
        {
            if (model.SelectedGroupId == null || model.SelectedCoursId == null)
                return RedirectToAction(nameof(ManageGrades));

            if (string.IsNullOrWhiteSpace(model.TypeEvaluation))
            {
                TempData["ErrorMessage"] = "Le type d'évaluation est obligatoire.";
                return RedirectToAction(nameof(ManageGrades), new { groupId = model.SelectedGroupId, coursId = model.SelectedCoursId });
            }

            foreach (var item in model.Students)
            {
                if (item.Note.HasValue)
                {
                    var existingNote = await _context.Notes
                        .FirstOrDefaultAsync(n => n.EtudiantId == item.EtudiantId
                                               && n.CoursId == model.SelectedCoursId
                                               && n.TypeEvaluation == model.TypeEvaluation);

                    if (existingNote != null)
                    {
                        existingNote.Valeur = (decimal)item.Note.Value;
                        existingNote.DateNote = DateTime.Now;
                        _context.Notes.Update(existingNote);
                    }
                    else
                    {
                        var newNote = new Note
                        {
                            EtudiantId = item.EtudiantId,
                            CoursId = model.SelectedCoursId.Value,
                            Valeur = (decimal)item.Note.Value,
                            TypeEvaluation = model.TypeEvaluation,
                            DateNote = DateTime.Now
                        };
                        _context.Notes.Add(newNote);
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Notes ({model.TypeEvaluation}) enregistrées avec succès.";
            return RedirectToAction(nameof(ManageGrades), new { groupId = model.SelectedGroupId, coursId = model.SelectedCoursId });
        }
    }
}