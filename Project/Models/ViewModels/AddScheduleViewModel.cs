using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Project.Models; // Nécessaire pour EmploiDuTemps

namespace Project.ViewModels
{
    public class AddScheduleViewModel
    {
        // --- CHAMPS DU FORMULAIRE ---
        [Required(ErrorMessage = "Le groupe est requis.")]
        public int GroupeId { get; set; }

        [Required(ErrorMessage = "Le cours est requis.")]
        public int CoursId { get; set; }

        [Required(ErrorMessage = "L'enseignant est requis.")]
        public int EnseignantId { get; set; }

        [Required(ErrorMessage = "Le jour est requis.")]
        public DayOfWeek Jour { get; set; }

        [Required(ErrorMessage = "L'heure de début est requise.")]
        [DataType(DataType.Time)]
        public TimeSpan HeureDebut { get; set; }

        [Required(ErrorMessage = "L'heure de fin est requise.")]
        [DataType(DataType.Time)]
        public TimeSpan HeureFin { get; set; }

        // --- CHAMPS D'AFFICHAGE (Remplace le ViewBag) ---
        public string? SelectedGroupName { get; set; }

        // Initialisé pour éviter le NullReferenceException si la liste est vide
        public List<EmploiDuTemps> ScheduleList { get; set; } = new List<EmploiDuTemps>();

        // --- LISTES DÉROULANTES ---
        public IEnumerable<SelectListItem>? Groupes { get; set; }
        public IEnumerable<SelectListItem>? CoursList { get; set; }
        public IEnumerable<SelectListItem>? Enseignants { get; set; }
        public IEnumerable<SelectListItem>? Jours { get; set; }
    }
}