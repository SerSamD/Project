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

    // --- Helper pour le hachage (Simplifié pour l'exemple) ---
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
                // Hacher le mot de passe pour la comparaison (Utilisateurs non-Admin)
                submittedPasswordHash = HashPassword(model.MotDePasse);
            }

            // Vérification de l'existence et du mot de passe
            if (utilisateur != null && utilisateur.MotDePasseHash == submittedPasswordHash)
            {
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

                // Redirection
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
    // 2️⃣ INSCRIPTION : CHOIX DU RÔLE
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        // Affiche la vue qui donne le choix entre Étudiant et Enseignant
        ViewData["Title"] = "Choisir le type de compte";
        return View();
    }

    // ============================================================
    // 3️⃣ INSCRIPTION : ÉTUDIANT
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterStudent()
    {
        ViewData["Title"] = "Créer un Compte Étudiant";
        // Retourne la vue générique pour le formulaire
        return View("RegisterForm", new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterStudent(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Vérifier si le nom d'utilisateur existe déjà
            if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
            {
                ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà pris.");
                return View("RegisterForm", model);
            }

            try
            {
                // 1. Créer l'objet Utilisateur (Rôle: Etudiant)
                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Etudiant",
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };

                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                // 2. Créer le profil Étudiant (Relation 1-à-1)
                var nouvelEtudiant = new Etudiant
                {
                    UtilisateurId = nouvelUtilisateur.Id,
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };

                _context.Etudiants.Add(nouvelEtudiant);
                await _context.SaveChangesAsync();

                // 3. Connexion de l'utilisateur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, nouvelUtilisateur.Id.ToString()),
                    new Claim(ClaimTypes.Name, nouvelUtilisateur.NomUtilisateur),
                    new Claim(ClaimTypes.Role, nouvelUtilisateur.Role),
                    new Claim("NomComplet", $"{nouvelUtilisateur.Prenom} {nouvelUtilisateur.Nom}")
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError(string.Empty, "Erreur lors de l'enregistrement en base de données. Détails: " + ex.InnerException?.Message);
                return View("RegisterForm", model); // Retourne au formulaire en cas d'erreur
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
    // 4️⃣ INSCRIPTION : ENSEIGNANT (Logique à compléter)
    // ============================================================

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegisterTeacher()
    {
        ViewData["Title"] = "Créer un Compte Enseignant";
        // Retourne la vue générique pour le formulaire
        return View("RegisterForm", new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterTeacher(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Vérifier si le nom d'utilisateur existe déjà
            if (await _context.Utilisateurs.AnyAsync(u => u.NomUtilisateur == model.NomUtilisateur))
            {
                ModelState.AddModelError("NomUtilisateur", "Ce nom d'utilisateur est déjà pris.");
                return View("RegisterForm", model);
            }

            try
            {
                // 1. Créer l'objet Utilisateur (Rôle: Enseignant)
                var nouvelUtilisateur = new Utilisateur
                {
                    NomUtilisateur = model.NomUtilisateur,
                    MotDePasseHash = HashPassword(model.MotDePasse),
                    Role = "Enseignant",
                    Nom = model.Nom,
                    Prenom = model.Prenom,
                    Email = model.Email
                };

                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                // 2. Créer le profil Enseignant (Relation 1-à-1)
                var nouvelEnseignant = new Enseignant
                {
                    UtilisateurId = nouvelUtilisateur.Id,
                    // Si Enseignant a des champs spécifiques, les ajouter ici.
                    // Pour l'instant, seul l'UtilisateurId est requis par votre modèle.
                };

                _context.Enseignants.Add(nouvelEnseignant);
                await _context.SaveChangesAsync();

                // 3. Connexion de l'utilisateur
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, nouvelUtilisateur.Id.ToString()),
                    new Claim(ClaimTypes.Name, nouvelUtilisateur.NomUtilisateur),
                    new Claim(ClaimTypes.Role, nouvelUtilisateur.Role),
                    new Claim("NomComplet", $"{nouvelUtilisateur.Prenom} {nouvelUtilisateur.Nom}")
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

                return RedirectToAction("Index", "Home");
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
    // 5️⃣ DÉCONNEXION & ACCÈS REFUSÉ
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