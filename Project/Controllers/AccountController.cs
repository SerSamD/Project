using Microsoft.AspNetCore.Mvc;
using Project.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using Project.Models;
using Project.Data;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly SchoolContext _context;

        public AccountController(SchoolContext context)
        {
            _context = context;
        }

        // --- Helper pour le hachage ---
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
        // 1️⃣ CONNEXION
        // ============================================================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.NomUtilisateur == model.NomUtilisateur);
                string submittedPasswordHash;

                // Logique de hachage temporaire pour l'Admin seedé (si Id=1 et MDP non haché)
                if (utilisateur != null && utilisateur.Id == 1 && utilisateur.MotDePasseHash == "admin123")
                {
                    submittedPasswordHash = "admin123";
                }
                else
                {
                    submittedPasswordHash = HashPassword(model.MotDePasse);
                }

                if (utilisateur != null && utilisateur.MotDePasseHash == submittedPasswordHash)
                {
                    // Vérification de l'approbation (Sauf pour l'Admin)
                    if (utilisateur.Role != "Admin" && !utilisateur.IsApproved)
                    {
                        ModelState.AddModelError(string.Empty, "Votre compte est en attente d'approbation par un administrateur.");
                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                        new Claim(ClaimTypes.Name, utilisateur.NomUtilisateur),
                        new Claim(ClaimTypes.Role, utilisateur.Role),
                        new Claim("NomComplet", $"{utilisateur.Prenom} {utilisateur.Nom}")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        new AuthenticationProperties { IsPersistent = true });

                    // =========================================================
                    // 🔄 REDIRECTION AUTOMATIQUE SELON LE RÔLE (CORRIGÉE)
                    // =========================================================

                    if (utilisateur.Role == "Admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    if (utilisateur.Role == "Enseignant")
                    {
                        // Modification ici : "Teacher" -> "Enseignant"
                        return RedirectToAction("Index", "Enseignant");
                    }
                    if (utilisateur.Role == "Etudiant")
                    {
                        // Modification ici : "Student" -> "Etudiant"
                        return RedirectToAction("Index", "Etudiant");
                    }
                    if (utilisateur.Role == "Surveillant")
                    {
                        return RedirectToAction("Index", "Surveillant");
                    }

                    // Si aucun rôle spécifique ou redirection URL demandée
                    if (Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe non valide.");
            }
            return View(model);
        }

        // ============================================================
        // 2️⃣ INSCRIPTION
        // ============================================================

        [HttpGet][AllowAnonymous] public IActionResult Register() { return View(); }

        [HttpGet][AllowAnonymous] public IActionResult RegisterStudent() { return View("RegisterForm", new RegisterViewModel()); }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
                {
                    ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà pris.");
                    return View("RegisterForm", model);
                }

                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Pending",
                    IsApproved = false,
                    PendingRole = "Etudiant",
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };
                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                var nouvelEtudiant = new Etudiant
                { UtilisateurId = nouvelUtilisateur.Id, Nom = model.Nom, Prenom = model.Prenom, Email = model.Email };
                _context.Etudiants.Add(nouvelEtudiant);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Votre compte Étudiant est en attente d'approbation.";
                return RedirectToAction("Login");
            }
            return View("RegisterForm", model);
        }

        [HttpGet][AllowAnonymous] public IActionResult RegisterTeacher() { return View("RegisterForm", new RegisterViewModel()); }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterTeacher(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
                {
                    ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà pris.");
                    return View("RegisterForm", model);
                }

                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Pending",
                    IsApproved = false,
                    PendingRole = "Enseignant",
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };
                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                var nouvelEnseignant = new Enseignant { UtilisateurId = nouvelUtilisateur.Id };
                _context.Enseignants.Add(nouvelEnseignant);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Votre compte Enseignant est en attente d'approbation.";
                return RedirectToAction("Login");
            }
            return View("RegisterForm", model);
        }

        [HttpGet][AllowAnonymous] public IActionResult RegisterSupervisor() { return View("RegisterForm", new RegisterViewModel()); }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterSupervisor(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
                {
                    ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà pris.");
                    return View("RegisterForm", model);
                }

                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Pending",
                    IsApproved = false,
                    PendingRole = "Surveillant",
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };
                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                var nouveauSurveillant = new Surveillant { UtilisateurId = nouvelUtilisateur.Id };
                _context.Surveillants.Add(nouveauSurveillant);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Votre compte Surveillant est en attente d'approbation.";
                return RedirectToAction("Login");
            }
            return View("RegisterForm", model);
        }

        // ============================================================
        // 3️⃣ DÉCONNEXION & ACCÈS REFUSÉ
        // ============================================================

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}