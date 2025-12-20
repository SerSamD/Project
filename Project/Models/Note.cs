using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Project.Models
{
    public class Note
    {
        public int Id { get; set; }

        // ==========================
        // Clés étrangères
        // ==========================
        public int EtudiantId { get; set; }
        public Etudiant Etudiant { get; set; }

        public int CoursId { get; set; }
        public Cours Cours { get; set; }

        // ==========================
        // Données spécifiques
        // ==========================

        // "decimal(5, 2)" permet de stocker des notes comme 18.50 ou 100.00
        [Column(TypeName = "decimal(5, 2)")]
        [Range(0, 20, ErrorMessage = "La note doit être comprise entre 0 et 20")] // Validation utile
        public decimal Valeur { get; set; }

        [Required(ErrorMessage = "Le type d'évaluation est requis (ex: Examen, Devoir)")]
        public string TypeEvaluation { get; set; }

        public DateTime DateNote { get; set; } = DateTime.Now; // Date par défaut

        public bool IsPublished { get; set; } = false; // Par défaut, c'est caché
    }
}