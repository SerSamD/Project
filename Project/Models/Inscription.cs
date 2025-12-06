public class Inscription
{
    public int Id { get; set; }

    public int EtudiantId { get; set; }
    public Etudiant Etudiant { get; set; }

    public int CoursId { get; set; }
    public Cours Cours { get; set; }

    public DateTime DateInscription { get; set; }
}