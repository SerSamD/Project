using Microsoft.EntityFrameworkCore;
// TRÈS IMPORTANT : Assurez-vous que ce namespace correspond au nom exact de votre projet !
using Project.Models;

public class SchoolContext : DbContext
{
    public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
    {
    }

    // ============================================================
    // --- DbSets (Tables de la Base de Données) ---
    // ============================================================
    public DbSet<Utilisateur> Utilisateurs { get; set; }
    public DbSet<Enseignant> Enseignants { get; set; }
    public DbSet<Etudiant> Etudiants { get; set; }
    public DbSet<Cours> Cours { get; set; }
    public DbSet<Inscription> Inscriptions { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Surveillant> Surveillants { get; set; } // DbSet pour Surveillant


    // ============================================================
    // --- Configuration des relations (Fluent API) ---
    // ============================================================
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1️⃣ RELATION UN-À-UN ENTRE UTILISATEUR ET ENSEIGNANT
        modelBuilder.Entity<Enseignant>()
            .HasOne(e => e.Utilisateur)
            .WithOne(u => u.EnseignantProfil)
            .HasForeignKey<Enseignant>(e => e.UtilisateurId)
            .IsRequired();

        modelBuilder.Entity<Enseignant>()
            .HasIndex(e => e.UtilisateurId)
            .IsUnique();

        // 2️⃣ RELATION UN-À-UN ENTRE UTILISATEUR ET ETUDIANT
        modelBuilder.Entity<Etudiant>()
            .HasOne(e => e.Utilisateur)
            .WithOne(u => u.EtudiantProfil)
            .HasForeignKey<Etudiant>(e => e.UtilisateurId)
            .IsRequired();

        modelBuilder.Entity<Etudiant>()
            .HasIndex(e => e.UtilisateurId)
            .IsUnique();

        // 3️⃣ RELATION UN-À-UN ENTRE UTILISATEUR ET SURVEILLANT
        modelBuilder.Entity<Surveillant>()
            .HasOne(s => s.Utilisateur)
            // Propriété de navigation dans Utilisateur.cs
            .WithOne(u => u.SurveillantProfil)
            .HasForeignKey<Surveillant>(s => s.UtilisateurId)
            .IsRequired();

        modelBuilder.Entity<Surveillant>()
            .HasIndex(s => s.UtilisateurId)
            .IsUnique();

        // 4️⃣ RELATION NOTE : Etudiant / Cours (N-à-N via Entité Jonction)
        modelBuilder.Entity<Note>()
            .HasOne(n => n.Etudiant)
            .WithMany(e => e.Notes)
            .HasForeignKey(n => n.EtudiantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Note>()
            .HasOne(n => n.Cours)
            .WithMany(c => c.Notes)
            .HasForeignKey(n => n.CoursId)
            .OnDelete(DeleteBehavior.Cascade);

        // 5️⃣ RELATION INSCRIPTION : Etudiant / Cours (N-à-N via Entité Jonction)
        modelBuilder.Entity<Inscription>()
            .HasOne(i => i.Etudiant)
            .WithMany(e => e.Inscriptions)
            .HasForeignKey(i => i.EtudiantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Inscription>()
            .HasOne(i => i.Cours)
            .WithMany(c => c.Inscriptions)
            .HasForeignKey(i => i.CoursId)
            .OnDelete(DeleteBehavior.Cascade);

        // 6️⃣ SEED DATA : Utilisateur Admin
        modelBuilder.Entity<Utilisateur>().HasData(
            new Utilisateur
            {
                Id = 1,
                NomUtilisateur = "admin",
                MotDePasseHash = "admin123",
                Role = "Admin",
                Nom = "Système",
                Prenom = "Administrateur",
                Email = "admin@ecole.com",

                // Champs pour l'approbation (nécessaire pour le seed)
                IsApproved = true,
                PendingRole = "Admin"
            }
        );
    }
}