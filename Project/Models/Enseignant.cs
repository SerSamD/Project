public class Enseignant
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }

    public List<Cours> Cours { get; set; }
}
