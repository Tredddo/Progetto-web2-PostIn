namespace PostIn.Data.Entities;

public class SalvataggioDaLeggere
{
    public int FK_Dipendente { get; set; }
    public Dipendente Dipendente { get; set; } = null!;

    public int FK_Articolo { get; set; }
    public Articolo Articolo { get; set; } = null!;

    //public string DataSalvataggio { get; set; } = string.Empty;
    public DateTime DataSalvataggio { get; set; } = DateTime.UtcNow;
}