using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Nom d'utilisateur")]
        public string NomUtilisateur { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le mot de passe")]
        [Compare("MotDePasse", ErrorMessage = "Le mot de passe et la confirmation ne correspondent pas.")]
        public string ConfirmMotDePasse { get; set; }

        [Required]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}