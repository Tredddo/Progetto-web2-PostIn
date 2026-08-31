using Bogus;
using Microsoft.EntityFrameworkCore;
using PostIn.Data.Entities;
using PostIn.Services;

namespace PostIn.Data;

public static class DbSeeder
{
    private const int MAX_COMMENTS_TO_SEED = 150;

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Dipendenti.AnyAsync())
            return;

        // Imposta un seed fisso per avere sempre gli stessi dati a ogni reset
        Randomizer.Seed = new Random(42);

        // DIPENDENTI (Fissi + Generati con Bogus)
        var dipendenti = new List<Dipendente>
        {
            new Dipendente
            {
                Nome = "Mario",
                Cognome = "Rossi",
                Username = "admin",
                PasswordHash = PasswordHelper.HashPassword("password"),
                Ruolo = 1,
                StatoAccount = 1,
                UltimoAccesso = DateTime.UtcNow
            },
            new Dipendente
            {
                Nome = "Luigi",
                Cognome = "Verdi",
                Username = "l.verdi",
                PasswordHash = PasswordHelper.HashPassword("password123"),
                Ruolo = 0,
                StatoAccount = 1,
                UltimoAccesso = DateTime.UtcNow
            }
        };

        var dipendenteFaker = new Faker<Dipendente>("it")
            .RuleFor(d => d.Nome, f => f.Name.FirstName())
            .RuleFor(d => d.Cognome, f => f.Name.LastName())
            .RuleFor(d => d.Username, (f, d) => f.Internet.UserName(d.Nome, d.Cognome))
            .RuleFor(d => d.PasswordHash, _ => PasswordHelper.HashPassword("Password!123"))
            .RuleFor(d => d.Ruolo, f => f.Random.WeightedRandom(new[] { 0, 1 }, new[] { 0.85f, 0.15f })) // 85% Utenti, 15% Admin
            .RuleFor(d => d.StatoAccount, _ => 1)
            .RuleFor(d => d.UltimoAccesso, f => f.Date.Recent(30));

        dipendenti.AddRange(dipendenteFaker.Generate(50)); // Genera 50 dipendenti

        context.Dipendenti.AddRange(dipendenti);
        await context.SaveChangesAsync(); // Genera ID_Dipendente nel DB

        var dipendentiIds = dipendenti.Select(d => d.ID_Dipendente).ToList();

        // CATEGORIE
        var categorieNomi = new[]
        {
            "IT & Tech", "Risorse Umane", "Policy Aziendali", 
            "Eventi & Social", "Novità Prodotti", "Welfare & Benefit", 
            "Formazione & Corsi", "Sicurezza sul Lavoro"
        };

        var categorie = categorieNomi.Select(c => new Categoria { NomeCategoria = c }).ToList();

        context.Categorie.AddRange(categorie);
        await context.SaveChangesAsync(); // Genera ID_Categoria nel DB

        var categorieIds = categorie.Select(c => c.ID_Categoria).ToList();

        // ARTICOLI
        var articoloFaker = new Faker<Articolo>("it")
            .RuleFor(a => a.Titolo, f => f.Rant.Review("lavoro").Length > 80 ? f.Lorem.Sentence(6) : f.Rant.Review("lavoro"))
            .RuleFor(a => a.CorpoTesto, f => string.Join("\n\n", f.Lorem.Paragraphs(3, 6)))
            .RuleFor(a => a.ImmagineCopertina, _ => null)
            .RuleFor(a => a.DataOraCreazione, f => f.Date.Past(1))
            .RuleFor(a => a.FK_Autore, f => f.PickRandom(dipendentiIds));

        var articoli = articoloFaker.Generate(150); // Genera 150 articoli

        context.Articoli.AddRange(articoli);
        await context.SaveChangesAsync(); // Genera ID_Articolo nel DB

        var articoliIds = articoli.Select(a => a.ID_Articolo).ToList();

        // TABELLE DI JOIN & RELAZIONI
        var random = new Random(42);

        // Assegnazione Categorie agli Articoli (1-3 categorie per articolo, univoche)
        var articoloCategorie = new List<ArticoloCategoria>();
        foreach (var artId in articoliIds)
        {
            var catScelte = categorieIds.OrderBy(_ => random.Next()).Take(random.Next(1, 4));
            foreach (var catId in catScelte)
            {
                articoloCategorie.Add(new ArticoloCategoria { FK_Articolo = artId, FK_Categoria = catId });
            }
        }
        context.ArticoloCategorie.AddRange(articoloCategorie);

        // Categorie preferite per dipendente (0-3 categorie per utente)
        var categoriePreferite = new List<CategoriaPreferita>();
        foreach (var dipId in dipendentiIds)
        {
            var catScelte = categorieIds.OrderBy(_ => random.Next()).Take(random.Next(0, 4));
            foreach (var catId in catScelte)
            {
                categoriePreferite.Add(new CategoriaPreferita { FK_Dipendente = dipId, FK_Categoria = catId });
            }
        }
        context.CategoriePreferite.AddRange(categoriePreferite);

        // Follow tra dipendenti
        var followList = new HashSet<(int Follower, int Followed)>();
        for (int i = 0; i < 150; i++)
        {
            var follower = dipendentiIds[random.Next(dipendentiIds.Count)];
            var followed = dipendentiIds[random.Next(dipendentiIds.Count)];

            if (follower != followed && !followList.Contains((follower, followed)))
            {
                followList.Add((follower, followed));
                context.IscrizioniFollow.Add(new IscrizioniFollow
                {
                    FK_Follower = follower,
                    FK_Followed = followed,
                    DataInizioInterazione = DateTime.UtcNow.AddDays(-random.Next(1, 180))
                });
            }
        }

        // Likes agli Articoli (univoci per coppia dipendente-articolo)
        var likesList = new HashSet<(int Dip, int Art)>();
        for (int i = 0; i < 400; i++)
        {
            var dipId = dipendentiIds[random.Next(dipendentiIds.Count)];
            var artId = articoliIds[random.Next(articoliIds.Count)];

            if (!likesList.Contains((dipId, artId)))
            {
                likesList.Add((dipId, artId));
                context.Likes.Add(new Like
                {
                    FK_Dipendente = dipId,
                    FK_Articolo = artId,
                    DataRilascio = DateTime.UtcNow.AddDays(-random.Next(1, 90))
                });
            }
        }

        // Salvataggi da leggere
        var salvataggiList = new HashSet<(int Dip, int Art)>();
        for (int i = 0; i < 100; i++)
        {
            var dipId = dipendentiIds[random.Next(dipendentiIds.Count)];
            var artId = articoliIds[random.Next(articoliIds.Count)];

            if (!salvataggiList.Contains((dipId, artId)))
            {
                salvataggiList.Add((dipId, artId));
                context.SalvataggiDaLeggere.Add(new SalvataggioDaLeggere
                {
                    FK_Dipendente = dipId,
                    FK_Articolo = artId,
                    DataSalvataggio = DateTime.UtcNow.AddDays(-random.Next(1, 60))
                });
            }
        }

        // COMMENTI DA FILE comments.txt (Limitati a MAX_COMMENTS_TO_SEED)
        string filePath = Path.Combine(AppContext.BaseDirectory, "comments.txt");
        if (!File.Exists(filePath))
        {
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "comments.txt");
        }

        if (File.Exists(filePath))
        {
            var righeCommenti = (await File.ReadAllLinesAsync(filePath))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Trim())
                .Distinct()
                .Take(MAX_COMMENTS_TO_SEED)
                .ToList();

            var commentiDaSalvare = new List<Commento>();
            var randomSeed = new Random(42);

            for (int i = 0; i < righeCommenti.Count; i++)
            {
                int artId = articoliIds[i % articoliIds.Count]; 
                int dipId = dipendentiIds[randomSeed.Next(dipendentiIds.Count)];

                commentiDaSalvare.Add(new Commento
                {
                    FK_Articolo = artId,
                    FK_Autore = dipId,
                    TestoCommento = righeCommenti[i],
                    DataPubblicazione = DateTime.UtcNow.AddMinutes(-randomSeed.Next(1, 43200)),
                    Sentiment = "Da analizzare",
                    PositiveScore = 0.0,
                    NeutralScore = 0.0,
                    NegativeScore = 0.0
                });
            }

            if (commentiDaSalvare.Any())
            {
                context.Commenti.AddRange(commentiDaSalvare);
                await context.SaveChangesAsync();
            }
        }
        else
        {
            // Fallback con Bogus se il file comments.txt non viene trovato
            var commentoFaker = new Faker<Commento>("it")
                .RuleFor(c => c.FK_Autore, f => f.PickRandom(dipendentiIds))
                .RuleFor(c => c.FK_Articolo, f => f.PickRandom(articoliIds))
                .RuleFor(c => c.TestoCommento, f => f.Rant.Review("lavoro") ?? f.Lorem.Sentence(10))
                .RuleFor(c => c.DataPubblicazione, f => f.Date.Recent(60))
                .RuleFor(c => c.Sentiment, _ => "Da analizzare")
                .RuleFor(c => c.PositiveScore, _ => 0.0)
                .RuleFor(c => c.NeutralScore, _ => 0.0)
                .RuleFor(c => c.NegativeScore, _ => 0.0);

            context.Commenti.AddRange(commentoFaker.Generate(MAX_COMMENTS_TO_SEED));
            await context.SaveChangesAsync();
        }

        // VISUALIZZAZIONI
        var visualizzazioneFaker = new Faker<Visualizzazione>("it")
            .RuleFor(v => v.FK_Dipendente, f => f.PickRandom(dipendentiIds))
            .RuleFor(v => v.FK_Articolo, f => f.PickRandom(articoliIds))
            .RuleFor(v => v.DataOraVisualizzazione, f => f.Date.Recent(30));

        context.Visualizzazioni.AddRange(visualizzazioneFaker.Generate(800));

        // Salvataggio finale di tutte le relazioni e visualizzazioni
        await context.SaveChangesAsync();
    }
}