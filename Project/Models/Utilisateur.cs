using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

// Assurez-vous que le namespace correspond au nom exact de votre projet !
namespace Project.Models
{
    public class Utilisateur
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nom d'utilisateur")]
        public string NomUtilisateur { get; set; }

        [Required]
        [Display(Name = "Mot de passe hashé")]
        public string MotDePasseHash { get; set; }

        // --- Informations d'identité centralisées ---
        [Required]
        public string Nom { get; set; }

        [Required]
        public string Prenom { get; set; }

        // CORRECTION CRUCIALE : Rendu nullable pour accepter les valeurs NULL de la DB
        public string? Email { get; set; }

        // --- Champs de Rôle et Approbation ---

        [Required]
        [Display(Name = "Rôle Actif")]
        public string Role { get; set; }

        [Display(Name = "Rôle en Attente")]
        // Rendu nullable
        public string? PendingRole { get; set; }

        [Display(Name = "Approuvé")]
        public bool IsApproved { get; set; } = true;

        // --- Relations 1-à-1 (Propriétés de navigation) ---

        public Enseignant EnseignantProfil { get; set; }
        public Etudiant EtudiantProfil { get; set; }

        // Profil pour le Surveillant Général
        public Surveillant SurveillantProfil { get; set; }
    }
}