using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic; // Nécessaire si vous utilisez List<T>

// Seuls les utilisateurs avec le rôle "Etudiant" peuvent accéder à ce contrôleur
[Authorize(Roles = "Etudiant")]
public class StudentController : Controller
{
    private readonly SchoolContext _context;

    public StudentController(SchoolContext context)
    {
        _context = context;
    }

    // Fonction utilitaire pour obtenir l'ID de l'utilisateur
    private bool TryGetUserId(out int userId)
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    // =========================================================================
    // TABLEAU DE BORD (INDEX)
    // =========================================================================

    // Vue principale de l'étudiant : Afficher le profil, les cours inscrits et les notes
    public async Task<IActionResult> Index()
    {
        if (!TryGetUserId(out int userId))
        {
            return Unauthorized();
        }

        // 2. CORRECTION : AJOUTER .Include(e => e.Utilisateur) pour éviter l'erreur de référence nulle
        var etudiantProfile = await _context.Etudiants
            .Include(e => e.Utilisateur) // <-- ESSENTIEL pour accéder au Nom, Prénom, Email, etc.
            .Include(e => e.Inscriptions)
                .ThenInclude(i => i.Cours)
            .Include(e => e.Notes)
                .ThenInclude(n => n.Cours)
            .FirstOrDefaultAsync(e => e.UtilisateurId == userId);

        if (etudiantProfile == null)
        {
            return NotFound("Profil Étudiant introuvable.");
        }

        // Utiliser les informations du profil chargé via Utilisateur
        ViewBag.NomComplet = $"{etudiantProfile.Utilisateur.Prenom} {etudiantProfile.Utilisateur.Nom}";
        ViewBag.Email = etudiantProfile.Utilisateur.Email;

        // Passer le profil complet à la vue
        return View(etudiantProfile);
    }

    // =========================================================================
    // COURS DISPONIBLES (AVAILABLECOURSES)
    // =========================================================================

    // GET: /Student/AvailableCourses
    public async Task<IActionResult> AvailableCourses()
    {
        if (!TryGetUserId(out int userId))
        {
            return Unauthorized();
        }

        var etudiantProfile = await _context.Etudiants
            .Include(e => e.Inscriptions)
            .FirstOrDefaultAsync(e => e.UtilisateurId == userId);

        if (etudiantProfile == null)
        {
            return NotFound("Profil Étudiant introuvable.");
        }

        // Récupérer l'ID des cours auxquels l'étudiant est déjà inscrit
        var coursInscritsIds = etudiantProfile.Inscriptions
                                                .Select(i => i.CoursId)
                                                .ToList();

        // Récupérer tous les cours qui ne sont pas inscrits, y compris l'enseignant (pour l'affichage)
        var coursDisponibles = await _context.Cours
            .Include(c => c.Enseignant).ThenInclude(e => e.Utilisateur)
            .Where(c => !coursInscritsIds.Contains(c.Id))
            .OrderBy(c => c.Titre)
            .ToListAsync();

        return View(coursDisponibles);
    }

    // =========================================================================
    // INSCRIPTION (ENROLL)
    // =========================================================================

    // POST: /Student/Enroll
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enroll(int coursId)
    {
        if (!TryGetUserId(out int userId))
        {
            return Unauthorized();
        }

        var etudiant = await _context.Etudiants.FirstOrDefaultAsync(e => e.UtilisateurId == userId);

        if (etudiant == null)
        {
            TempData["Error"] = "Erreur: Profil étudiant non trouvé.";
            return RedirectToAction("AvailableCourses");
        }

        // Vérifier si le cours existe et si l'étudiant n'est pas déjà inscrit
        var coursExiste = await _context.Cours.AnyAsync(c => c.Id == coursId);
        if (!coursExiste)
        {
            TempData["Error"] = "Le cours spécifié n'existe pas.";
            return RedirectToAction("AvailableCourses");
        }

        var inscriptionExistante = await _context.Inscriptions
            .AnyAsync(i => i.EtudiantId == etudiant.Id && i.CoursId == coursId);

        if (inscriptionExistante)
        {
            TempData["Error"] = "Vous êtes déjà inscrit à ce cours.";
            return RedirectToAction("AvailableCourses");
        }

        // Créer la nouvelle inscription
        var nouvelleInscription = new Inscription
        {
            EtudiantId = etudiant.Id,
            CoursId = coursId,
            DateInscription = DateTime.Now
        };

        _context.Inscriptions.Add(nouvelleInscription);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Inscription au cours réussie !";
        return RedirectToAction("Index");
    }
}