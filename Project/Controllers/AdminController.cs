using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;
using Project.ViewModels;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Project.Data;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly SchoolContext _context;

    public AdminController(SchoolContext context)
    {
        _context = context;
    }

    // ============================================================
    // 🔐 HASH MOT DE PASSE (IDENTIQUE À AccountController)
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
    // 1️⃣ DASHBOARD
    // ============================================================
    // Dans Controllers/AdminController.cs

    public async Task<IActionResult> Index()
    {
        // Calcul des statistiques
        var totalUsers = await _context.Utilisateurs.CountAsync();
        var pendingUsers = await _context.Utilisateurs.CountAsync(u => u.IsApproved == false && u.Role == "Pending");
        var totalStudents = await _context.Utilisateurs.CountAsync(u => u.Role == "Etudiant" && u.IsApproved == true);
        var totalTeachers = await _context.Utilisateurs.CountAsync(u => u.Role == "Enseignant" && u.IsApproved == true);
        var totalSupervisors = await _context.Utilisateurs.CountAsync(u => u.Role == "Surveillant" && u.IsApproved == true);

        var viewModel = new AdminDashboardViewModel
        {
            UtilisateursEnAttente = pendingUsers,
            TotalUtilisateurs = totalUsers,
            TotalEtudiants = totalStudents,
            TotalEnseignants = totalTeachers,
            TotalSurveillants = totalSupervisors
            // Les totaux par mois (utilisateurs, inscriptions) nécessitent une requête plus complexe
        };

        ViewData["Title"] = "Tableau de Bord Administrateur";
        return View(viewModel);
    }

    // ============================================================
    // 2️⃣ APPROBATION DES UTILISATEURS EN ATTENTE (RESTORED)
    // ============================================================

    // GET: Admin/PendingUsers (Routage doit fonctionner maintenant)
    public async Task<IActionResult> PendingUsers()
    {
        var pendingUsers = await _context.Utilisateurs
            .Where(u => u.IsApproved == false && u.Role == "Pending")
            .OrderBy(u => u.NomUtilisateur)
            .ToListAsync();

        // Si le 404 persiste, remplacez par : return View("~/Views/Admin/PendingUsers.cshtml", pendingUsers);
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
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
        ViewBag.ErrorMessage = TempData["ErrorMessage"];
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(AdminCreateUserViewModel model)
    {
        ViewBag.Roles = new List<string> { "Admin", "Enseignant", "Etudiant", "Surveillant" };

        if (!ModelState.IsValid)
        {
            // Affichage des erreurs de validation dans la console pour le débogage
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("MODEL ERROR: " + error.ErrorMessage);
            }
            return View(model);
        }

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

        // DÉBUT DE LA TRANSACTION
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
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = $"Erreur DB : Échec de la création. Détails: {ex.InnerException?.Message ?? ex.Message}";
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
    // 4️⃣ GESTION DES UTILISATEURS ACTIFS (RESTORED)
    // ============================================================

    // GET: Admin/UsersList - Affiche la liste de tous les utilisateurs actifs
    public async Task<IActionResult> UsersList()
    {
        var users = await _context.Utilisateurs
            .Where(u => u.Role != "Admin" && u.IsApproved == true)
            .OrderBy(u => u.Role)
            .ThenBy(u => u.NomUtilisateur)
            .ToListAsync();

        return View(users);
    }

    // POST : Logique de suppression d'un utilisateur ACTIF
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Utilisateurs.FindAsync(id);

        if (user == null || user.Role == "Admin")
        {
            TempData["ErrorMessage"] = "Utilisateur introuvable ou suppression non autorisée (Admin).";
            return RedirectToAction(nameof(UsersList));
        }

        await DeleteSpecificProfile(user, user.Role);

        _context.Utilisateurs.Remove(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"L'utilisateur actif {user.NomUtilisateur} ({user.Role}) a été supprimé définitivement.";
        return RedirectToAction(nameof(UsersList));
    }

    // ============================================================
    // 🧩 HELPERS
    // ============================================================

    private async Task CreateSpecificProfile(Utilisateur user, string role)
    {
        switch (role)
        {
            case "Etudiant":
                _context.Etudiants.Add(new Etudiant
                {
                    UtilisateurId = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email
                });
                break;

            case "Enseignant":
                _context.Enseignants.Add(new Enseignant
                {
                    UtilisateurId = user.Id
                });
                break;

            case "Surveillant":
                _context.Surveillants.Add(new Surveillant
                {
                    UtilisateurId = user.Id
                });
                break;
        }

        await Task.CompletedTask;
    }

    private async Task DeleteSpecificProfile(Utilisateur user, string role)
    {
        switch (role)
        {
            case "Etudiant":
                var etudiant = await _context.Etudiants.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
                if (etudiant != null) _context.Etudiants.Remove(etudiant);
                break;
            case "Enseignant":
                var enseignant = await _context.Enseignants.FirstOrDefaultAsync(e => e.UtilisateurId == user.Id);
                if (enseignant != null) _context.Enseignants.Remove(enseignant);
                break;
            case "Surveillant":
                var surveillant = await _context.Surveillants.FirstOrDefaultAsync(s => s.UtilisateurId == user.Id);
                if (surveillant != null) _context.Surveillants.Remove(surveillant);
                break;
        }
        await Task.CompletedTask;
    }
}