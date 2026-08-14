using System.Collections.Generic;


namespace PostIn.Data.Entities;

public class Dipendente
{
    public int ID_Dipendente { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int Ruolo { get; set; } = 0;
    public int StatoAccount { get; set; } = 0;
    //public string? UltimoAccesso { get; set; }
    public DateTime? UltimoAccesso { get; set; }

    // Relazioni e Collezioni
    public ICollection<Articolo> Articoliscritti { get; set; } = new List<Articolo>();
    public ICollection<Commento> Commenti { get; set; } = new List<Commento>();
    public ICollection<Visualizzazione> Visualizzazioni { get; set; } = new List<Visualizzazione>();
    
    // Tabelle di join / Relazioni N:N
    public ICollection<Categoria> CategoriePreferite { get; set; } = new List<Categoria>();
    public ICollection<Articolo> ArticoliSalvati { get; set; } = new List<Articolo>();
    public ICollection<Articolo> Likes { get; set; } = new List<Articolo>();

    // Self-referencing per il Follow
    public ICollection<IscrizioniFollow> Following { get; set; } = new List<IscrizioniFollow>();
    public ICollection<IscrizioniFollow> Followers { get; set; } = new List<IscrizioniFollow>();
}