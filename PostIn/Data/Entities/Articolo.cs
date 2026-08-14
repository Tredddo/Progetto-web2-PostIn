using System.Collections.Generic;


namespace PostIn.Data.Entities;

public class Articolo
{
    public int ID_Articolo { get; set; }
    public string Titolo { get; set; } = string.Empty;
    public string CorpoTesto { get; set; } = string.Empty;
    public string? ImmagineCopertina { get; set; }
    //public string DataOraCreazione { get; set; } = string.Empty;
    public DateTime DataOraCreazione { get; set; } = DateTime.UtcNow;

    public int FK_Autore { get; set; }
    public Dipendente Autore { get; set; } = null!;

    public ICollection<ArticoloCategoria> ArticoloCategorie { get; set; } = new List<ArticoloCategoria>();
    public ICollection<Commento> Commenti { get; set; } = new List<Commento>();
    public ICollection<Visualizzazione> Visualizzazioni { get; set; } = new List<Visualizzazione>();
    public ICollection<Dipendente> SalvataggiDaLeggere { get; set; } = new List<Dipendente>();
    public ICollection<Dipendente> Likes { get; set; } = new List<Dipendente>();
}