namespace Project.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int UtilisateursEnAttente { get; set; }
        public int TotalEtudiants { get; set; }
        public int TotalEnseignants { get; set; }
        public int TotalSurveillants { get; set; }
        public int TotalUtilisateurs { get; set; }
        public string[] ChartLabels { get; set; } // Ex: ["Janvier", "Février"]
        public int[] ChartValues { get; set; }    // Ex: [10, 5, 20]

        // Pour les graphiques, vous auriez besoin d'une structure de données ici (ex: List<MonthlyStats>)
    }
}