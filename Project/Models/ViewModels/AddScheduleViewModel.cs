using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering; // Pour SelectListItem

namespace Project.ViewModels
{
    public class AddScheduleViewModel
    {
        [Required(ErrorMessage = "Le groupe est requis.")]
        [Display(Name = "Groupe")]
        public int GroupeId { get; set; }

        [Required(ErrorMessage = "Le cours est requis.")]
        [Display(Name = "Cours / Matière")]
        public int CoursId { get; set; }

        [Required(ErrorMessage = "L'enseignant est requis.")]
        [Display(Name = "Enseignant")]
        public int EnseignantId { get; set; }

        [Required(ErrorMessage = "Le jour est requis.")]
        [Display(Name = "Jour de la semaine")]
        public DayOfWeek Jour { get; set; }

        [Required(ErrorMessage = "L'heure de début est requise.")]
        [DataType(DataType.Time)]
        [Display(Name = "Heure de début")]
        public TimeSpan HeureDebut { get; set; }

        [Required(ErrorMessage = "L'heure de fin est requise.")]
        [DataType(DataType.Time)]
        [Display(Name = "Heure de fin")]
        public TimeSpan HeureFin { get; set; }

        // Propriétés utilisées pour la liste déroulante (Dropdowns) dans la vue
        public IEnumerable<SelectListItem>? Groupes { get; set; }
        public IEnumerable<SelectListItem>? CoursList { get; set; }
        public IEnumerable<SelectListItem>? Enseignants { get; set; }
        public IEnumerable<SelectListItem>? Jours { get; set; }
    }
}