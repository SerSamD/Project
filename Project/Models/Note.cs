public class Note
{
    public int Id { get; set; }

    // Clés étrangères
    public int EtudiantId { get; set; }
    public Etudiant Etudiant { get; set; }

    public int CoursId { get; set; }
    public Cours Cours { get; set; }

    // Données spécifiques à la note
    public decimal Valeur { get; set; } // Ex: 17.5/20
    public string TypeEvaluation { get; set; } // Ex: "Examen Final", "Projet"
    public DateTime DateNote { get; set; }
}