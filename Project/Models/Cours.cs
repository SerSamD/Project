using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

// C'est cette ligne qui manquait !
namespace Project.Models
{
    public class Cours
    {
        public int Id { get; set; }

        [Required]
        public string Titre { get; set; }

        public string? Description { get; set; } // J'ai ajouté le ? pour le rendre optionnel (bonne pratique)

        // Relation 1-à-N vers Enseignant
        public int EnseignantId { get; set; }
        public virtual Enseignant? Enseignant { get; set; } // virtual + ? pour éviter les bugs de chargement

        // Relations 1-à-N vers les autres tables
        public virtual ICollection<EmploiDuTemps>? EmploisDuTemps { get; set; }
        public virtual ICollection<Note>? Notes { get; set; }
    }
}