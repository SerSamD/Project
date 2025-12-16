using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic; // Assurez-vous d'inclure ceci si nécessaire

namespace Project.Models
{
    // 💡 NOM DE LA CLASSE MODIFIÉ
    public class EmploiDuTemps
    {
        public int Id { get; set; }

        [Required]
        public DayOfWeek Jour { get; set; } // Lundi, Mardi, etc.

        [Required]
        public TimeSpan HeureDebut { get; set; }

        [Required]
        public TimeSpan HeureFin { get; set; }

        // Clé étrangère vers le Groupe concerné
        public int GroupeId { get; set; }
        public Groupe Groupe { get; set; }

        // Clé étrangère vers le Cours (Matière)
        public int CoursId { get; set; }
        public Cours Cours { get; set; }

        // Clé étrangère vers l'Enseignant responsable
        public int EnseignantId { get; set; }
        public Enseignant Enseignant { get; set; }
    }
}