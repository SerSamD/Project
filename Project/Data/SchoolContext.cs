using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Data;
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

    public DbSet<Note> Notes { get; set; }
    public DbSet<Surveillant> Surveillants { get; set; }
    public DbSet<Groupe> Groupes { get; set; }

    // ✅ CORRECTION 1 : Le nom exact utilisé dans SurveillantController est 'EmploisDuTemps'
    public DbSet<EmploiDuTemps> EmploisDuTemps { get; set; }


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

     

        // Groupe <--> Surveillant (1-à-N)
        modelBuilder.Entity<Groupe>()
            .HasOne(g => g.Surveillant)
            .WithMany(s => s.Groupes)
            .HasForeignKey(g => g.SurveillantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Etudiant <--> Groupe (N-à-1)
        modelBuilder.Entity<Etudiant>()
            .HasOne(e => e.Groupe)
            .WithMany(g => g.Etudiants)
            .HasForeignKey(e => e.GroupeId)
            .IsRequired(false);

        // SessionEmploiDuTemps <--> Groupe (N-à-1)
        modelBuilder.Entity<EmploiDuTemps>()
            .HasOne(s => s.Groupe)
            .WithMany(g => g.SessionsEmploiDuTemps) 
            .HasForeignKey(s => s.GroupeId)
            .OnDelete(DeleteBehavior.Cascade); ;

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
                IsApproved = true,
                PendingRole = "Admin"
            }
        );
    }
}