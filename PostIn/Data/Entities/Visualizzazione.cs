using System.Collections.Generic;


namespace PostIn.Data.Entities;

public class Visualizzazione
{
    public int ID_Visualizzazione { get; set; }
    public int FK_Dipendente { get; set; }
    public Dipendente Dipendente { get; set; } = null!;

    public int FK_Articolo { get; set; }
    public Articolo Articolo { get; set; } = null!;

    //public string DataOraVisualizzazione { get; set; } = string.Empty;
    public DateTime DataOraVisualizzazione { get; set; } = DateTime.UtcNow;
}