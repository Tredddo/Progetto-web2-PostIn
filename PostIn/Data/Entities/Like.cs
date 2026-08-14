namespace PostIn.Data.Entities;

public class Like
{
    public int FK_Dipendente { get; set; }
    public Dipendente Dipendente { get; set; } = null!;

    public int FK_Articolo { get; set; }
    public Articolo Articolo { get; set; } = null!;

    //public string DataRilascio { get; set; } = string.Empty;
    public DateTime DataRilascio { get; set; } = DateTime.UtcNow;
}