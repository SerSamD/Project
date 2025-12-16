using Project.Models;

public class Enseignant
{
    public int Id { get; set; }

    // L'ID pour la relation 1-à-1
    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; } // Propriété de navigation

    public List<Cours> Cours { get; set; } = new List<Cours>();
}