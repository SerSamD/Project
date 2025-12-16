using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class Etudiant
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string Email { get; set; }

        public int UtilisateurId { get; set; }
        public Utilisateur Utilisateur { get; set; }

        // Clé étrangère vers le Groupe
        public int? GroupeId { get; set; }
        public Groupe? Groupe { get; set; }

        public ICollection<Note> Notes { get; set; }
    
    }
}