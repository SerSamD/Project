using Project.Models;

public class Etudiant
{
    public int Id { get; set; }
    public string Nom { get; set; } // Retirer ces champs si vous utilisez Utilisateur pour l'identité
    public string Prenom { get; set; }
    public string Email { get; set; }

    // Relation 1-à-1 vers Utilisateur (Si vous l'avez implémentée)
    public int UtilisateurId { get; set; }
    public Utilisateur Utilisateur { get; set; }

    public List<Inscription> Inscriptions { get; set; } = new List<Inscription>();
    public List<Note> Notes { get; set; } = new List<Note>();
}