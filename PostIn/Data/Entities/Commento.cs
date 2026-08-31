namespace PostIn.Data.Entities;

public class Commento
{
    public int ID_Commento { get; set; }
    public int FK_Articolo { get; set; }
    public Articolo Articolo { get; set; } = null!;

    public int FK_Autore { get; set; }
    public Dipendente Autore { get; set; } = null!;

    public string TestoCommento { get; set; } = string.Empty;
    public DateTime DataPubblicazione { get; set; } = DateTime.UtcNow;

    // Colonne per Sentiment Analysis con Azure AI
    public string Sentiment { get; set; } = "Neutral";
    public double PositiveScore { get; set; }
    public double NeutralScore { get; set; }
    public double NegativeScore { get; set; }
}