using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Project.Controllers
{
    public class StudentController : Controller
    {
        private readonly SchoolContext _context;

        public StudentController(SchoolContext context)
        {
            _context = context;
        }

        // Tableau de bord de l'étudiant
        public async Task<IActionResult> Index()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            // On récupère l'étudiant avec ses notes, mais SANS les inscriptions (supprimées)
            var etudiant = await _context.Etudiants
                .Include(e => e.Utilisateur)
                .Include(e => e.Notes)
                    .ThenInclude(n => n.Cours)
                .FirstOrDefaultAsync(e => e.UtilisateurId == userId);

            if (etudiant == null)
            {
                return NotFound("Profil étudiant introuvable.");
            }

            return View(etudiant);
        }

        // Liste des cours disponibles
        public async Task<IActionResult> AvailableCourses()
        {
            // On affiche simplement la liste de tous les cours
            var cours = await _context.Cours.ToListAsync();
            return View(cours);
        }

        // 🚨 J'ai SUPPRIMÉ la méthode Enroll car le modèle Inscription n'existe plus.
        // Si vous voulez gérer les inscriptions plus tard, il faudra recréer la table.
    }
}