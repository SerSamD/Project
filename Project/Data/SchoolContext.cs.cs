using Microsoft.EntityFrameworkCore;

public class SchoolContext : DbContext
{
    public SchoolContext(DbContextOptions<SchoolContext> options)
        : base(options)
    {
    }

    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Etudiant> Etudiants { get; set; }
    public DbSet<Enseignant> Enseignants { get; set; }
    public DbSet<Cours> Cours { get; set; }
    public DbSet<Inscription> Inscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Relations
        modelBuilder.Entity<Inscription>()
            .HasOne(i => i.Etudiant)
            .WithMany(e => e.Inscriptions)
            .HasForeignKey(i => i.EtudiantId);

        modelBuilder.Entity<Inscription>()
            .HasOne(i => i.Cours)
            .WithMany(c => c.Inscriptions)
            .HasForeignKey(i => i.CoursId);

        // Créer un admin automatiquement
        modelBuilder.Entity<Utilisateur>().HasData(
            new Utilisateur
            {
                Id = 1,
                NomUtilisateur = "admin",
                MotDePasseHash = "admin123", // tu peux mettre hash
                Role = "Admin"
            }
        );
    }
}
