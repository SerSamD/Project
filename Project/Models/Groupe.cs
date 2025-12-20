using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Project.Models
{
    public class Groupe
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nom { get; set; }

        // Clé étrangère vers le Surveillant responsable
        public int? SurveillantId { get; set; }
        public Surveillant? Surveillant { get; set; }

        // Navigation : Liste des étudiants dans ce groupe
        public ICollection<Etudiant> Etudiants { get; set; }

        // Navigation : Liste des sessions d'emploi du temps pour ce groupe
       
        public ICollection<EmploiDuTemps> SessionsEmploiDuTemps { get; set; } 
    }
}