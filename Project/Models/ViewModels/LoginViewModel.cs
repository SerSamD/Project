using System.ComponentModel.DataAnnotations;

namespace Project.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
        [Display(Name = "Nom d'utilisateur")]
        public string NomUtilisateur { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; }
    }
}