using Microsoft.EntityFrameworkCore;
using PostIn.Data.Entities;

namespace PostIn.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Se ci sono già utenti, il database è già stato popolato: usciamo
        if (await context.Dipendenti.AnyAsync())
            return;

        // DIPENDENTI
        var admin = new Dipendente
        {
            Nome = "Mario",
            Cognome = "Rossi",
            Username = "admin",
            PasswordHash = PasswordHelper.HashPassword("password"),
            Ruolo = 1, // 1 = Admin
            StatoAccount = 1,
            UltimoAccesso = DateTime.UtcNow
        };

        var user = new Dipendente
        {
            Nome = "Luigi",
            Cognome = "Verdi",
            Username = "l.verdi",
            PasswordHash = PasswordHelper.HashPassword("password123"),
            Ruolo = 0, // 0 = Utente Base
            StatoAccount = 1,
            UltimoAccesso = DateTime.UtcNow
        };

        context.Dipendenti.AddRange(admin, user);
        await context.SaveChangesAsync(); // Salvo per generare gli ID

        // CATEGORIE
        var catIT = new Categoria { NomeCategoria = "IT & Tech" };
        var catHR = new Categoria { NomeCategoria = "Risorse Umane" };
        var catPolicy = new Categoria { NomeCategoria = "Policy Aziendali" };
        var cat1 = new Categoria { NomeCategoria = "1" };
        var cat2 = new Categoria { NomeCategoria = "2" };
        var cat3 = new Categoria { NomeCategoria = "3" };

        context.Categorie.AddRange(catIT, catHR, catPolicy, cat1, cat2, cat3);
        await context.SaveChangesAsync();

        // ARTICOLI
        var articolo1 = new Articolo
        {
            Titolo = "Nuovo aggiornamento server 2026",
            CorpoTesto = "Il giorno 15 del mese procederemo con l'aggiornamento... // roba lunga",
            ImmagineCopertina = null,
            DataOraCreazione = DateTime.UtcNow,
            FK_Autore = admin.ID_Dipendente // L'admin scrive questo
        };

        var articolo2 = new Articolo
        {
            Titolo = "Guida al nuovo Smart Working",
            CorpoTesto = "Ecco le nuove regole per il lavoro da casa... // roba lunga",
            ImmagineCopertina = "/uploads/covers/smartworking.jpg",
            DataOraCreazione = DateTime.UtcNow,
            FK_Autore = user.ID_Dipendente // L'utente scrive questo
        };

        context.Articoli.AddRange(articolo1, articolo2);
        await context.SaveChangesAsync();

        // TABELLE DI JOIN (Categorie, Preferiti, Follow, Likes)
        
        // Assegno le categorie agli articoli
        context.ArticoloCategorie.AddRange(
            new ArticoloCategoria { FK_Articolo = articolo1.ID_Articolo, FK_Categoria = catIT.ID_Categoria },
            new ArticoloCategoria { FK_Articolo = articolo2.ID_Articolo, FK_Categoria = catHR.ID_Categoria },
            new ArticoloCategoria { FK_Articolo = articolo2.ID_Articolo, FK_Categoria = catPolicy.ID_Categoria }
        );

        // Categorie preferite degli utenti
        context.CategoriePreferite.AddRange(
            new CategoriaPreferita { FK_Dipendente = user.ID_Dipendente, FK_Categoria = catHR.ID_Categoria }
        );

        // Follow: L'utente normale segue l'admin
        context.IscrizioniFollow.Add(new IscrizioniFollow
        {
            FK_Follower = user.ID_Dipendente,
            FK_Followed = admin.ID_Dipendente,
            DataInizioInterazione = DateTime.UtcNow
        });

        // Like: L'admin mette like all'articolo dell'utente
        context.Likes.Add(new Like
        {
            FK_Dipendente = admin.ID_Dipendente,
            FK_Articolo = articolo2.ID_Articolo,
            DataRilascio = DateTime.UtcNow
        });

        // Salvataggio: L'utente salva l'articolo dell'admin per leggerlo dopo
        context.SalvataggiDaLeggere.Add(new SalvataggioDaLeggere
        {
            FK_Dipendente = user.ID_Dipendente,
            FK_Articolo = articolo1.ID_Articolo,
            DataSalvataggio = DateTime.UtcNow
        });

        // COMMENTI & VISUALIZZAZIONI
        context.Commenti.Add(new Commento
        {
            FK_Autore = user.ID_Dipendente,
            FK_Articolo = articolo1.ID_Articolo,
            TestoCommento = "Ottimo aggiornamento, a che ora ci sarà il disservizio? // roba lunga",
            DataPubblicazione = DateTime.UtcNow
        });

        context.Visualizzazioni.Add(new Visualizzazione
        {
            FK_Dipendente = user.ID_Dipendente,
            FK_Articolo = articolo1.ID_Articolo,
            DataOraVisualizzazione = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }
}