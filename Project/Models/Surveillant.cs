using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Assurez-vous que le namespace correspond au nom exact de votre projet !
namespace Project.Models
{
    public class Surveillant
    {
        // Clé primaire de l'entité Surveillant
        public int Id { get; set; }

        // Clé étrangère vers la table Utilisateur (relation 1-à-1)
        public int UtilisateurId { get; set; }

        // Propriété de navigation pour accéder aux détails de l'utilisateur
        public Utilisateur Utilisateur { get; set; }

        // Ajoutez ici toute propriété spécifique au Surveillant si nécessaire
        public ICollection<Groupe> Groupes { get; set; } = new List<Groupe>();

    }
}