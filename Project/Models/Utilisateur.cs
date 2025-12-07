using System.ComponentModel.DataAnnotations;

public class Utilisateur
{
    public int Id { get; set; }

    [Required]
    public string NomUtilisateur { get; set; }

    [Required]
    public string MotDePasseHash { get; set; }

    [Required]
    public string Role { get; set; } // Rôle ACTUEL (Admin, Enseignant, Etudiant, ou Pending)

    // --- NOUVELLES PROPRIÉTÉS POUR L'APPROBATION ---

    /// <summary>
    /// Indique si l'utilisateur a été approuvé par un administrateur.
    /// Les utilisateurs non approuvés ne peuvent pas se connecter.
    /// </summary>
    public bool IsApproved { get; set; } = false; // Par défaut : NON approuvé

    /// <summary>
    /// Stocke le rôle demandé lors de l'inscription ("Etudiant" ou "Enseignant").
    /// Ce rôle sera copié dans la propriété 'Role' après l'approbation.
    /// </summary>
    public string PendingRole { get; set; } = string.Empty;

    // Informations d'identité centralisées
    [Required]
    public string Nom { get; set; }

    [Required]
    public string Prenom { get; set; }

    public string Email { get; set; }

    // Relations 1-à-1 (Propriétés de navigation)
    public Enseignant EnseignantProfil { get; set; }
    public Etudiant EtudiantProfil { get; set; }
}
// ASSUREZ-VOUS QUE LA CLASSE N'EST PAS DÉFINIE UNE SECONDE FOIS ICI