namespace PostIn.Data.Entities;

public class CategoriaPreferita
{
    public int FK_Dipendente { get; set; }
    public Dipendente Dipendente { get; set; } = null!;

    public int FK_Categoria { get; set; }
    public Categoria Categoria { get; set; } = null!;
}