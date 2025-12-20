using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Data;
using Project.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Project.Controllers
{
    [Authorize(Roles = "Etudiant")]
    public class EtudiantController : Controller
    {
        private readonly SchoolContext _context;

        public EtudiantController(SchoolContext context)
        {
            _context = context;
        }

        // 1. TABLEAU DE BORD
        public async Task<IActionResult> Index()
        {
            // On récupère les infos de l'étudiant pour l'accueil
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var etudiant = await _context.Etudiants
                .Include(e => e.Groupe)
                .FirstOrDefaultAsync(e => e.UtilisateurId == userId);

            return View(etudiant);
        }

        // 2. MON EMPLOI DU TEMPS
        public async Task<IActionResult> MySchedule()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var etudiant = await _context.Etudiants.FirstOrDefaultAsync(e => e.UtilisateurId == userId);

            if (etudiant == null || etudiant.GroupeId == 0)
            {
                ViewBag.ErrorMessage = "Vous n'êtes assigné à aucun groupe. Contactez l'administration.";
                return View(new List<EmploiDuTemps>());
            }

            // On charge l'emploi du temps DU GROUPE de l'étudiant
            var schedule = await _context.EmploisDuTemps
                .Where(e => e.GroupeId == etudiant.GroupeId)
                .Include(e => e.Cours)
                .Include(e => e.Enseignant)
                .ThenInclude(ens => ens.Utilisateur)
                .OrderBy(e => e.Jour)
                .ThenBy(e => e.HeureDebut)
                .ToListAsync();

            ViewBag.GroupName = _context.Groupes.Find(etudiant.GroupeId)?.Nom;
            return View(schedule);
        }

        // 3. MES NOTES (Avec le filtre de publication !)
        public async Task<IActionResult> MyGrades()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var etudiant = await _context.Etudiants.FirstOrDefaultAsync(e => e.UtilisateurId == userId);

            if (etudiant == null) return Forbid();

            var notes = await _context.Notes
                .Include(n => n.Cours)
                .Where(n => n.EtudiantId == etudiant.Id)
                // =======================================================
                // ⚠️ RAPPEL : C'est ici qu'on filtre les notes publiées
                // =======================================================
                .Where(n => n.IsPublished == true)
                .OrderBy(n => n.Cours.Titre)
                .ToListAsync();

            return View(notes);
        }
    }
}