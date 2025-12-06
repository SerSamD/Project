using System.ComponentModel.DataAnnotations;

public class Utilisateur
{
    public int Id { get; set; }

    [Required]
    public string NomUtilisateur { get; set; } // Utilisé pour le login

    [Required]
    public string MotDePasseHash { get; set; } // Le mot de passe haché

    [Required]
    public string Role { get; set; }    // Admin, Enseignant, Etudiant

    // Informations d'identité centralisées
    [Required]
    public string Nom { get; set; }

    [Required]
    public string Prenom { get; set; }

    public string Email { get; set; }

    // Relations 1-à-1 (Propriétés de navigation)
    // Permet de naviguer de l'utilisateur vers son profil (Enseignant ou Etudiant)
    public Enseignant EnseignantProfil { get; set; }
    public Etudiant EtudiantProfil { get; set; }
}