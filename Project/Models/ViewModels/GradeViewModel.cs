using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Project.ViewModels
{
    public class StudentGradeItem
    {
        public int EtudiantId { get; set; }
        public string EtudiantNom { get; set; }
        // On utilise double pour le formulaire HTML, on convertira en decimal après
        public double? Note { get; set; }
    }

    public class GradeManagementViewModel
    {
        public int? SelectedGroupId { get; set; }
        public int? SelectedCoursId { get; set; }

        // --- NOUVEAU : Champ obligatoire pour la table Note ---
        [Required(ErrorMessage = "Le type d'évaluation est requis")]
        public string TypeEvaluation { get; set; }

        // Listes pour l'affichage
        public SelectList? Groupes { get; set; }
        public SelectList? Cours { get; set; }

        public List<StudentGradeItem> Students { get; set; } = new List<StudentGradeItem>();
    }
}