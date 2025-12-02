public class Cours
{
    public int Id { get; set; }
    public string Titre { get; set; }
    public string Description { get; set; }

    public int EnseignantId { get; set; }
    public Enseignant Enseignant { get; set; }

    public List<Inscription> Inscriptions { get; set; }
}
