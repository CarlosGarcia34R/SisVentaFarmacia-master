using Entidad;
using System;

public class Producto
{
    public int idproducto { get; set; }
    public int idcategoria { get; set; }
    public Categoria ocategoria { get; set; } // Propiedad relacionada con la entidad Categoria
    public string nombre { get; set; }
    public string Descripcion { get; set; }
    public string rutaimg { get; set; }
    public string nombreimg { get; set; }
    public bool estado { get; set; }
    public DateTime fecharegistro { get; set; }
    public int stock_caja { get; set; }
    public int stock_blister { get; set; }
    public int stock_unidad { get; set; }
    public decimal precio_unidad { get; set; }
    public decimal precio_blister { get; set; }
    public decimal precio_caja { get; set; }
    public int idproveedor { get; set; }
    public Proveedor oproveedor { get; set; } // Propiedad relacionada con la entidad Proveedor
    public int idlaboratorio { get; set; }
    public Laboratorio olaboratorio { get; set; } // Propiedad relacionada con la entidad Laboratorio
    public bool credito_fiscal { get; set; }
    public bool vitrina { get; set; }
    public string presentacion { get; set; }
    public string codigo_barras { get; set; }
    public string absorcion { get; set; }
    public string compuesto { get; set; }
    public DateTime fecha_vencimiento { get; set; }
}