using Project.Models;

public class Cours
{
    public int Id { get; set; }
    public string Titre { get; set; }
    public string Description { get; set; }

    // Relation 1-à-N vers Enseignant
    public int EnseignantId { get; set; }
    public Enseignant Enseignant { get; set; }

    // Relations 1-à-N vers les tables de jonction
   
   
   

    public ICollection<EmploiDuTemps> EmploisDuTemps { get; set; }
    
    public ICollection<Note> Notes { get; set; }
}