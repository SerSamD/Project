using Microsoft.AspNetCore.Mvc;
using Project.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;

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
    // 1️⃣ CONNEXION (MIS À JOUR)
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewBag.Message = TempData["Message"]; // Pour les messages d'attente après inscription
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var utilisateur = await _context.Utilisateurs
                                            .FirstOrDefaultAsync(u => u.NomUtilisateur == model.NomUtilisateur);

            string submittedPasswordHash;

            // Logique de hachage temporaire : Admin (ID 1) utilise le mot de passe en clair "admin123"
            if (utilisateur != null && utilisateur.Id == 1 && utilisateur.MotDePasseHash == "admin123")
            {
                submittedPasswordHash = "admin123";
            }
            else
            {
                submittedPasswordHash = HashPassword(model.MotDePasse);
            }

            // Vérification de l'existence et du mot de passe
            if (utilisateur != null && utilisateur.MotDePasseHash == submittedPasswordHash)
            {
                // NOUVEAU : Vérifier si l'utilisateur est approuvé (sauf pour l'Admin)
                if (utilisateur.Role != "Admin" && !utilisateur.IsApproved)
                {
                    ModelState.AddModelError(string.Empty, "Votre compte est en attente d'approbation par un administrateur.");
                    return View(model);
                }

                // Création du ticket d'authentification
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                    new Claim(ClaimTypes.Name, utilisateur.NomUtilisateur),
                    new Claim(ClaimTypes.Role, utilisateur.Role),
                    new Claim("NomComplet", $"{utilisateur.Prenom} {utilisateur.Nom}")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // NOUVELLE REDIRECTION: Basée sur le rôle
                if (utilisateur.Role == "Admin")
                {
                    return RedirectToAction("PendingUsers", "Admin");
                }
                if (utilisateur.Role == "Enseignant")
                {
                    return RedirectToAction("Index", "Teacher");
                }
                if (utilisateur.Role == "Etudiant")
                {
                    return RedirectToAction("Index", "Student");
                }


                // Redirection par défaut (pour les autres cas ou returnUrl)
                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            // ÉCHEC DE L'AUTHENTIFICATION
            ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe non valide.");
        }

        // Si ModelState est invalide ou authentification a échoué
        return View(model);
    }

    // ============================================================
    // 2️⃣ INSCRIPTION : CHOIX DU RÔLE (Inchangé)
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        ViewData["Title"] = "Choisir le type de compte";
        return View();
    }

    // ============================================================
    // 3️⃣ INSCRIPTION : ÉTUDIANT (MIS À JOUR - Ajout de Pending)
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterStudent()
    {
        ViewData["Title"] = "Créer un Compte Étudiant";
        return View("RegisterForm", new RegisterViewModel());
    }

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

            try
            {
                // 1. Créer l'objet Utilisateur AVEC STATUT PENDING
                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Pending", // Rôle temporaire
                    IsApproved = false, // NON approuvé
                    PendingRole = "Etudiant", // Rôle désiré après approbation
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };

                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                // 2. Créer le profil Étudiant
                var nouvelEtudiant = new Etudiant
                {
                    UtilisateurId = nouvelUtilisateur.Id,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };

                _context.Etudiants.Add(nouvelEtudiant);
                await _context.SaveChangesAsync();

                // 3. Rediriger vers le Login avec un message d'attente
                TempData["Message"] = "Votre compte Étudiant a été créé et est en attente d'approbation. Vous recevrez une notification lorsque votre compte sera activé.";
                return RedirectToAction("Login");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement en base de données. Détails: " + ex.InnerException?.Message);
                return View("RegisterForm", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur inattendue est survenue: " + ex.Message);
                return View("RegisterForm", model);
            }
        }

        // Si le modèle n'est pas valide
        return View("RegisterForm", model);
    }

    // ============================================================
    // 4️⃣ INSCRIPTION : ENSEIGNANT (MIS À JOUR - Ajout de Pending)
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterTeacher()
    {
        ViewData["Title"] = "Créer un Compte Enseignant";
        return View("RegisterForm", new RegisterViewModel());
    }

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

            try
            {
                // 1. Créer l'objet Utilisateur AVEC STATUT PENDING
                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Pending", // Rôle temporaire
                    IsApproved = false, // NON approuvé
                    PendingRole = "Enseignant", // Rôle désiré après approbation
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };

                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                // 2. Créer le profil Enseignant
                var nouvelEnseignant = new Enseignant
                {
                    UtilisateurId = nouvelUtilisateur.Id,
                };

                _context.Enseignants.Add(nouvelEnseignant);
                await _context.SaveChangesAsync();

                // 3. Rediriger vers le Login avec un message d'attente
                TempData["Message"] = "Votre compte Enseignant a été créé et est en attente d'approbation. Vous recevrez une notification lorsque votre compte sera activé.";
                return RedirectToAction("Login");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement en base de données de l'enseignant. Détails: " + ex.InnerException?.Message);
                return View("RegisterForm", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur inattendue est survenue: " + ex.Message);
                return View("RegisterForm", model);
            }
        }

        // Si le modèle n'est pas valide
        return View("RegisterForm", model);
    }

    // ============================================================
    // 5️⃣ DÉCONNEXION & ACCÈS REFUSÉ (Inchangé)
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