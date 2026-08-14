namespace PostIn.Data.Entities;

public class IscrizioniFollow
{
    public int FK_Follower { get; set; }
    public Dipendente Follower { get; set; } = null!;

    public int FK_Followed { get; set; }
    public Dipendente Followed { get; set; } = null!;

    //public string DataInizioInterazione { get; set; } = string.Empty;
    public DateTime DataInizioInterazione { get; set; } = DateTime.UtcNow;
}