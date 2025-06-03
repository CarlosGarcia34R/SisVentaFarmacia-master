// PresentacionAdmin/Models/VentaViewModel.cs
using Entidad;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PresentacionAdmin.Models
{
    public class VentaViewModel
    {
        // Datos generales de la venta
        [Display(Name = "Cliente (ID)")]
        public int IdCliente { get; set; }

        [Display(Name = "Contacto")]
        public string Contacto { get; set; }

        [Display(Name = "Distrito")]
        public string IdDistrito { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Display(Name = "ID Transacción (PayPal)")]
        public string IdTransaccion { get; set; }

        // Detalles de venta: una lista para capturar uno o varios productos
        public List<DetalleVentaViewModel> Detalles { get; set; }

        // Lista de productos para llenar el combo
        public List<ProductoViewModel> ListaProductos { get; set; }
    }

    public class DetalleVentaViewModel
    {
        [Display(Name = "Producto")]
        public int IdProducto { get; set; }

        [Display(Name = "Precio Unitario")]
        public decimal PrecioUnitario { get; set; }

        [Display(Name = "Cantidad")]
        public int Cantidad { get; set; }
    }

    public class ProductoViewModel
    {
        public int IdProducto { get; set; }
        public string NombreProducto { get; set; }
        public decimal Precio { get; set; }
    }

    public class VentasIndexViewModel
    {
        public VentaViewModel Venta { get; set; }
        public IEnumerable<Venta> ListaVentas { get; set; }
    }

}
