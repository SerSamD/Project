using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Migrations
{
    /// <inheritdoc />
    public partial class Update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Enseignants");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "Enseignants");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Enseignants");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Utilisateurs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "Utilisateurs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "Utilisateurs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "UtilisateurId",
                table: "Etudiants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UtilisateurId",
                table: "Enseignants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EtudiantId = table.Column<int>(type: "int", nullable: false),
                    CoursId = table.Column<int>(type: "int", nullable: false),
                    Valeur = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TypeEvaluation = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateNote = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Cours_CoursId",
                        column: x => x.CoursId,
                        principalTable: "Cours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_Etudiants_EtudiantId",
                        column: x => x.EtudiantId,
                        principalTable: "Etudiants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Utilisateurs",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Email", "Nom", "Prenom" },
                values: new object[] { "admin@ecole.com", "Système", "Administrateur" });

            migrationBuilder.CreateIndex(
                name: "IX_Etudiants_UtilisateurId",
                table: "Etudiants",
                column: "UtilisateurId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Enseignants_UtilisateurId",
                table: "Enseignants",
                column: "UtilisateurId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notes_CoursId",
                table: "Notes",
                column: "CoursId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EtudiantId",
                table: "Notes",
                column: "EtudiantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Enseignants_Utilisateurs_UtilisateurId",
                table: "Enseignants",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Etudiants_Utilisateurs_UtilisateurId",
                table: "Etudiants",
                column: "UtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Enseignants_Utilisateurs_UtilisateurId",
                table: "Enseignants");

            migrationBuilder.DropForeignKey(
                name: "FK_Etudiants_Utilisateurs_UtilisateurId",
                table: "Etudiants");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Etudiants_UtilisateurId",
                table: "Etudiants");

            migrationBuilder.DropIndex(
                name: "IX_Enseignants_UtilisateurId",
                table: "Enseignants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "Nom",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "Prenom",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "UtilisateurId",
                table: "Etudiants");

            migrationBuilder.DropColumn(
                name: "UtilisateurId",
                table: "Enseignants");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Enseignants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Nom",
                table: "Enseignants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Prenom",
                table: "Enseignants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
