using System.ComponentModel.DataAnnotations;

public class Utilisateur
{
    public int Id { get; set; }

    [Required]
    public string NomUtilisateur { get; set; }

    [Required]
    public string MotDePasseHash { get; set; }

    [Required]
    public string Role { get; set; }   // Admin, Enseignant, Etudiant
}
