using System.Collections.Generic;


namespace PostIn.Data.Entities;

public class Categoria
{
    public int ID_Categoria { get; set; }
    public string NomeCategoria { get; set; } = string.Empty;

    public ICollection<ArticoloCategoria> ArticoloCategorie { get; set; } = new List<ArticoloCategoria>();
    public ICollection<Dipendente> CategoriePreferite { get; set; } = new List<Dipendente>();
}