namespace PostIn.Data.Entities;

public class ArticoloCategoria
{
    public int FK_Articolo { get; set; }
    public Articolo Articolo { get; set; } = null!;

    public int FK_Categoria { get; set; }
    public Categoria Categoria { get; set; } = null!;
}