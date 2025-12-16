using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Project.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering; // Ajouté pour SelectList dans ManageSchedule
using System; // Ajouté pour DayOfWeek et TimeSpan
using Project.Data;

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
    // 4️⃣ GESTION DE L'EMPLOI DU TEMPS (Intégration finale)
    // ============================================================

    public async Task<IActionResult> ManageSchedule(int? groupId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var currentUserId)) return Forbid();

        var supervisorProfile = await _context.Surveillants
            .FirstOrDefaultAsync(s => s.UtilisateurId == currentUserId);

        if (supervisorProfile == null) return Forbid();

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

        var viewModel = new AddScheduleViewModel
        {
            Groupes = new SelectList(groupes, "Id", "Nom", groupId),
            CoursList = new SelectList(cours, "Id", "Nom"),
            Enseignants = new SelectList(
                enseignants.Select(e => new { e.Id, NomComplet = $"{e.Utilisateur.Prenom} {e.Utilisateur.Nom}" }),
                "Id", "NomComplet"
            ),
            Jours = new SelectList(Enum.GetValues(typeof(DayOfWeek))
                .Cast<DayOfWeek>()
                .Where(d => d != DayOfWeek.Saturday && d != DayOfWeek.Sunday)
            )
        };

        if (groupId.HasValue)
        {
            var schedule = await _context.EmploisDuTemps
                .Where(s => s.GroupeId == groupId.Value)
                .Include(s => s.Cours)
                .Include(s => s.Enseignant)
                    .ThenInclude(e => e.Utilisateur)
                .OrderBy(s => s.Jour)
                .ThenBy(s => s.HeureDebut)
                .ToListAsync();

            ViewBag.Schedule = schedule;
            ViewBag.SelectedGroupId = groupId.Value;
            ViewBag.SelectedGroupName = groupes.FirstOrDefault(g => g.Id == groupId.Value)?.Nom;
        }

        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];

        return View(viewModel);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSession(AddScheduleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Erreur de validation. Veuillez vérifier tous les champs.";
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
}