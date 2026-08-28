using Microsoft.EntityFrameworkCore;
using PostIn.Data.Entities;

namespace PostIn.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

    public DbSet<Dipendente> Dipendenti { get; set; } = null!;
    public DbSet<Articolo> Articoli { get; set; } = null!;
    public DbSet<Categoria> Categorie { get; set; } = null!;
    public DbSet<ArticoloCategoria> ArticoloCategorie { get; set; } = null!;
    public DbSet<CategoriaPreferita> CategoriePreferite { get; set; } = null!;
    public DbSet<IscrizioniFollow> IscrizioniFollow { get; set; } = null!;
    public DbSet<SalvataggioDaLeggere> SalvataggiDaLeggere { get; set; } = null!;
    public DbSet<Like> Likes { get; set; } = null!;
    public DbSet<Visualizzazione> Visualizzazioni { get; set; } = null!;
    public DbSet<Commento> Commenti { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // PK: Dipendenti
        modelBuilder.Entity<Dipendente>().HasKey(d => d.ID_Dipendente);

        // PK: Categorie
        modelBuilder.Entity<Categoria>().HasKey(c => c.ID_Categoria);

        // PK: Articoli
        modelBuilder.Entity<Articolo>().HasKey(a => a.ID_Articolo);

        // PK: Visualizzazioni
        modelBuilder.Entity<Visualizzazione>().HasKey(v => v.ID_Visualizzazione);

        // PK: Commenti
        modelBuilder.Entity<Commento>().HasKey(c => c.ID_Commento);

        // PK Composte (Tabelle di Join / Relazioni N:N)
        modelBuilder.Entity<ArticoloCategoria>().HasKey(ac => new { ac.FK_Articolo, ac.FK_Categoria });
        modelBuilder.Entity<IscrizioniFollow>().HasKey(f => new { f.FK_Follower, f.FK_Followed });

        modelBuilder.Entity<Dipendente>()
            .HasMany(d => d.CategoriePreferite)
            .WithMany(c => c.CategoriePreferite)
            .UsingEntity<CategoriaPreferita>(
                j => j.HasOne(cp => cp.Categoria).WithMany().HasForeignKey(cp => cp.FK_Categoria).OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne(cp => cp.Dipendente).WithMany().HasForeignKey(cp => cp.FK_Dipendente).OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey(cp => new { cp.FK_Dipendente, cp.FK_Categoria })
            );

        modelBuilder.Entity<Dipendente>()
            .HasMany(d => d.ArticoliSalvati)
            .WithMany(a => a.SalvataggiDaLeggere)
            .UsingEntity<SalvataggioDaLeggere>(
                j => j.HasOne(s => s.Articolo).WithMany().HasForeignKey(s => s.FK_Articolo).OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne(s => s.Dipendente).WithMany().HasForeignKey(s => s.FK_Dipendente).OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey(s => new { s.FK_Dipendente, s.FK_Articolo })
            );

        modelBuilder.Entity<Dipendente>()
            .HasMany(d => d.Likes)
            .WithMany(a => a.Likes)
            .UsingEntity<Like>(
                j => j.HasOne(l => l.Articolo).WithMany().HasForeignKey(l => l.FK_Articolo).OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne(l => l.Dipendente).WithMany().HasForeignKey(l => l.FK_Dipendente).OnDelete(DeleteBehavior.Cascade),
                j => j.HasKey(l => new { l.FK_Dipendente, l.FK_Articolo })
            );


        // Vincolo SQL per impedire l'autofollow
        modelBuilder.Entity<IscrizioniFollow>()
            .ToTable(t => t.HasCheckConstraint("CK_IscrizioniFollow_NoSelfFollow", "\"FK_Follower\" <> \"FK_Followed\""));

        // Relazione 1:N Dipendente (Autore) -> Articoli
        modelBuilder.Entity<Articolo>()
            .HasOne(a => a.Autore)
            .WithMany(d => d.Articoliscritti)
            .HasForeignKey(a => a.FK_Autore)
            .OnDelete(DeleteBehavior.Restrict);

        // Relazione N:N Articoli <-> Categorie via ArticoloCategoria
        modelBuilder.Entity<ArticoloCategoria>()
            .HasOne(ac => ac.Articolo)
            .WithMany(a => a.ArticoloCategorie)
            .HasForeignKey(ac => ac.FK_Articolo)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArticoloCategoria>()
            .HasOne(ac => ac.Categoria)
            .WithMany(c => c.ArticoloCategorie)
            .HasForeignKey(ac => ac.FK_Categoria)
            .OnDelete(DeleteBehavior.Cascade);

        // Relazione Self-Referencing N:N (Follow tra Dipendenti)
        modelBuilder.Entity<IscrizioniFollow>()
            .HasOne(f => f.Follower)
            .WithMany(d => d.Following)
            .HasForeignKey(f => f.FK_Follower)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<IscrizioniFollow>()
            .HasOne(f => f.Followed)
            .WithMany(d => d.Followers)
            .HasForeignKey(f => f.FK_Followed)
            .OnDelete(DeleteBehavior.Cascade);

        // Relazione 1:N Dipendente & Articolo -> Visualizzazioni
        modelBuilder.Entity<Visualizzazione>()
            .HasOne(v => v.Dipendente)
            .WithMany(d => d.Visualizzazioni)
            .HasForeignKey(v => v.FK_Dipendente)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Visualizzazione>()
            .HasOne(v => v.Articolo)
            .WithMany(a => a.Visualizzazioni)
            .HasForeignKey(v => v.FK_Articolo)
            .OnDelete(DeleteBehavior.Cascade);

        // Relazione 1:N Articolo & Dipendente -> Commenti
        modelBuilder.Entity<Commento>()
            .HasOne(c => c.Articolo)
            .WithMany(a => a.Commenti)
            .HasForeignKey(c => c.FK_Articolo)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Commento>()
            .HasOne(c => c.Autore)
            .WithMany(d => d.Commenti)
            .HasForeignKey(c => c.FK_Autore)
            .OnDelete(DeleteBehavior.Restrict);

        // UNIQUE: Dipendente.Username (Case Insensitive come da script SQL)
        modelBuilder.Entity<Dipendente>()
            .HasIndex(d => d.Username)
            .IsUnique();

        // UNIQUE: Categorie.NomeCategoria
        modelBuilder.Entity<Categoria>()
            .HasIndex(c => c.NomeCategoria)
            .IsUnique();
    }
}