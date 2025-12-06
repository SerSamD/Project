public class Cours
{
    public int Id { get; set; }
    public string Titre { get; set; }
    public string Description { get; set; }

    // Relation 1-à-N vers Enseignant
    public int EnseignantId { get; set; }
    public Enseignant Enseignant { get; set; }

    // Relations 1-à-N vers les tables de jonction
    public List<Inscription> Inscriptions { get; set; } = new List<Inscription>();
    public List<Note> Notes { get; set; } = new List<Note>(); 
}