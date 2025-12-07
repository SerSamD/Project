using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

// Seuls les utilisateurs avec le rôle "Etudiant" peuvent accéder à ce contrôleur
[Authorize(Roles = "Etudiant")]
public class StudentController : Controller
{
    private readonly SchoolContext _context;

    public StudentController(SchoolContext context)
    {
        _context = context;
    }

    // Vue principale de l'étudiant : Afficher le profil, les cours inscrits et les notes
    public async Task<IActionResult> Index()
    {
        // 1. Récupérer l'ID de l'utilisateur connecté
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return Unauthorized();
        }

        // 2. Récupérer le profil Etudiant et charger toutes les relations nécessaires (Inscriptions, Cours, Notes)
        var etudiantProfile = await _context.Etudiants
            .Include(e => e.Inscriptions)
                .ThenInclude(i => i.Cours) // Inclure les détails du Cours via l'Inscription
            .Include(e => e.Notes)
                .ThenInclude(n => n.Cours) // Inclure les détails du Cours via la Note
            .FirstOrDefaultAsync(e => e.UtilisateurId == userId);

        if (etudiantProfile == null)
        {
            return NotFound("Profil Étudiant introuvable.");
        }

        // Utiliser ViewBag pour les informations du profil
        ViewBag.NomComplet = $"{etudiantProfile.Prenom} {etudiantProfile.Nom}";
        ViewBag.Email = etudiantProfile.Email;

        // Passer le profil complet à la vue
        return View(etudiantProfile);
    }

    // --- Ajoutez ici d'autres actions si nécessaire (Ex: S'inscrire à un nouveau cours) ---
}