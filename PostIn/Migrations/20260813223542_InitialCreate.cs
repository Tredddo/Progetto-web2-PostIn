using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PostIn.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorie",
                columns: table => new
                {
                    ID_Categoria = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NomeCategoria = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorie", x => x.ID_Categoria);
                });

            migrationBuilder.CreateTable(
                name: "Dipendenti",
                columns: table => new
                {
                    ID_Dipendente = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Cognome = table.Column<string>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Ruolo = table.Column<int>(type: "INTEGER", nullable: false),
                    StatoAccount = table.Column<int>(type: "INTEGER", nullable: false),
                    UltimoAccesso = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dipendenti", x => x.ID_Dipendente);
                });

            migrationBuilder.CreateTable(
                name: "Articoli",
                columns: table => new
                {
                    ID_Articolo = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titolo = table.Column<string>(type: "TEXT", nullable: false),
                    CorpoTesto = table.Column<string>(type: "TEXT", nullable: false),
                    ImmagineCopertina = table.Column<string>(type: "TEXT", nullable: true),
                    DataOraCreazione = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FK_Autore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articoli", x => x.ID_Articolo);
                    table.ForeignKey(
                        name: "FK_Articoli_Dipendenti_FK_Autore",
                        column: x => x.FK_Autore,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CategoriePreferite",
                columns: table => new
                {
                    FK_Dipendente = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Categoria = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriePreferite", x => new { x.FK_Dipendente, x.FK_Categoria });
                    table.ForeignKey(
                        name: "FK_CategoriePreferite_Categorie_FK_Categoria",
                        column: x => x.FK_Categoria,
                        principalTable: "Categorie",
                        principalColumn: "ID_Categoria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoriePreferite_Dipendenti_FK_Dipendente",
                        column: x => x.FK_Dipendente,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IscrizioniFollow",
                columns: table => new
                {
                    FK_Follower = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Followed = table.Column<int>(type: "INTEGER", nullable: false),
                    DataInizioInterazione = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IscrizioniFollow", x => new { x.FK_Follower, x.FK_Followed });
                    table.CheckConstraint("CK_IscrizioniFollow_NoSelfFollow", "\"FK_Follower\" <> \"FK_Followed\"");
                    table.ForeignKey(
                        name: "FK_IscrizioniFollow_Dipendenti_FK_Followed",
                        column: x => x.FK_Followed,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IscrizioniFollow_Dipendenti_FK_Follower",
                        column: x => x.FK_Follower,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArticoloCategorie",
                columns: table => new
                {
                    FK_Articolo = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Categoria = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticoloCategorie", x => new { x.FK_Articolo, x.FK_Categoria });
                    table.ForeignKey(
                        name: "FK_ArticoloCategorie_Articoli_FK_Articolo",
                        column: x => x.FK_Articolo,
                        principalTable: "Articoli",
                        principalColumn: "ID_Articolo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArticoloCategorie_Categorie_FK_Categoria",
                        column: x => x.FK_Categoria,
                        principalTable: "Categorie",
                        principalColumn: "ID_Categoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commenti",
                columns: table => new
                {
                    ID_Commento = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FK_Articolo = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Autore = table.Column<int>(type: "INTEGER", nullable: false),
                    TestoCommento = table.Column<string>(type: "TEXT", nullable: false),
                    DataPubblicazione = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commenti", x => x.ID_Commento);
                    table.ForeignKey(
                        name: "FK_Commenti_Articoli_FK_Articolo",
                        column: x => x.FK_Articolo,
                        principalTable: "Articoli",
                        principalColumn: "ID_Articolo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commenti_Dipendenti_FK_Autore",
                        column: x => x.FK_Autore,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    FK_Dipendente = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Articolo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataRilascio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => new { x.FK_Dipendente, x.FK_Articolo });
                    table.ForeignKey(
                        name: "FK_Likes_Articoli_FK_Articolo",
                        column: x => x.FK_Articolo,
                        principalTable: "Articoli",
                        principalColumn: "ID_Articolo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Dipendenti_FK_Dipendente",
                        column: x => x.FK_Dipendente,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalvataggiDaLeggere",
                columns: table => new
                {
                    FK_Dipendente = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Articolo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataSalvataggio = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalvataggiDaLeggere", x => new { x.FK_Dipendente, x.FK_Articolo });
                    table.ForeignKey(
                        name: "FK_SalvataggiDaLeggere_Articoli_FK_Articolo",
                        column: x => x.FK_Articolo,
                        principalTable: "Articoli",
                        principalColumn: "ID_Articolo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalvataggiDaLeggere_Dipendenti_FK_Dipendente",
                        column: x => x.FK_Dipendente,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visualizzazioni",
                columns: table => new
                {
                    ID_Visualizzazione = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FK_Dipendente = table.Column<int>(type: "INTEGER", nullable: false),
                    FK_Articolo = table.Column<int>(type: "INTEGER", nullable: false),
                    DataOraVisualizzazione = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visualizzazioni", x => x.ID_Visualizzazione);
                    table.ForeignKey(
                        name: "FK_Visualizzazioni_Articoli_FK_Articolo",
                        column: x => x.FK_Articolo,
                        principalTable: "Articoli",
                        principalColumn: "ID_Articolo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visualizzazioni_Dipendenti_FK_Dipendente",
                        column: x => x.FK_Dipendente,
                        principalTable: "Dipendenti",
                        principalColumn: "ID_Dipendente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articoli_FK_Autore",
                table: "Articoli",
                column: "FK_Autore");

            migrationBuilder.CreateIndex(
                name: "IX_ArticoloCategorie_FK_Categoria",
                table: "ArticoloCategorie",
                column: "FK_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Categorie_NomeCategoria",
                table: "Categorie",
                column: "NomeCategoria",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriePreferite_FK_Categoria",
                table: "CategoriePreferite",
                column: "FK_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Commenti_FK_Articolo",
                table: "Commenti",
                column: "FK_Articolo");

            migrationBuilder.CreateIndex(
                name: "IX_Commenti_FK_Autore",
                table: "Commenti",
                column: "FK_Autore");

            migrationBuilder.CreateIndex(
                name: "IX_Dipendenti_Username",
                table: "Dipendenti",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IscrizioniFollow_FK_Followed",
                table: "IscrizioniFollow",
                column: "FK_Followed");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_FK_Articolo",
                table: "Likes",
                column: "FK_Articolo");

            migrationBuilder.CreateIndex(
                name: "IX_SalvataggiDaLeggere_FK_Articolo",
                table: "SalvataggiDaLeggere",
                column: "FK_Articolo");

            migrationBuilder.CreateIndex(
                name: "IX_Visualizzazioni_FK_Articolo",
                table: "Visualizzazioni",
                column: "FK_Articolo");

            migrationBuilder.CreateIndex(
                name: "IX_Visualizzazioni_FK_Dipendente",
                table: "Visualizzazioni",
                column: "FK_Dipendente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticoloCategorie");

            migrationBuilder.DropTable(
                name: "CategoriePreferite");

            migrationBuilder.DropTable(
                name: "Commenti");

            migrationBuilder.DropTable(
                name: "IscrizioniFollow");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "SalvataggiDaLeggere");

            migrationBuilder.DropTable(
                name: "Visualizzazioni");

            migrationBuilder.DropTable(
                name: "Categorie");

            migrationBuilder.DropTable(
                name: "Articoli");

            migrationBuilder.DropTable(
                name: "Dipendenti");
        }
    }
}
