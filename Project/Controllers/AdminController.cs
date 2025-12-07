using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

// Seulement accessible par les utilisateurs ayant le rôle "Admin"
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly SchoolContext _context;

    public AdminController(SchoolContext context)
    {
        _context = context;
    }

    // Affiche la liste des utilisateurs en attente
    public async Task<IActionResult> PendingUsers()
    {
        // Récupérer les utilisateurs non approuvés ou ceux avec le rôle Pending
        var pendingUsers = await _context.Utilisateurs
            .Where(u => u.IsApproved == false && u.Role == "Pending")
            .OrderBy(u => u.NomUtilisateur)
            .ToListAsync();

        return View(pendingUsers);
    }

    // POST : Logique d'approbation
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

        // 1. Approuver l'utilisateur
        user.IsApproved = true;

        // 2. Attribuer le rôle demandé (PendingRole)
        user.Role = user.PendingRole;

        // 3. Mettre à jour la BDD
        _context.Utilisateurs.Update(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"L'utilisateur {user.NomUtilisateur} a été approuvé et le rôle '{user.Role}' a été attribué.";
        return RedirectToAction(nameof(PendingUsers));
    }

    // POST : Logique de rejet
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

        // 1. Supprimer le profil associé (Étudiant ou Enseignant)
        if (user.PendingRole == "Etudiant")
        {
            var etudiant = await _context.Etudiants.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
            if (etudiant != null) _context.Etudiants.Remove(etudiant);
        }
        else if (user.PendingRole == "Enseignant")
        {
            var enseignant = await _context.Enseignants.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
            if (enseignant != null) _context.Enseignants.Remove(enseignant);
        }

        // 2. Supprimer l'utilisateur lui-même
        _context.Utilisateurs.Remove(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"L'utilisateur {user.NomUtilisateur} a été rejeté et supprimé.";
        return RedirectToAction(nameof(PendingUsers));
    }
}