using System;
using System.Collections.Generic;

namespace Entidad
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public int TotalProducto { get; set; }
        public decimal MontoTotal { get; set; }
        public string Contacto { get; set; }
        public string IdDistrito { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string IdTransaccion { get; set; }
        public DateTime FechaVenta { get; set; }

        // Nuevos campos para factura
        public string SerieFactura { get; set; }
        public int NumeroFactura { get; set; }
        public string NRC { get; set; }
        public string NIT { get; set; }
        public string NumeroResolucion { get; set; }
        public string CAI { get; set; }
        public DateTime FechaResolucion { get; set; }
        public DateTime FechaLimiteImpresion { get; set; }
        public string DireccionSucursal { get; set; }
        public string TelefonoSucursal { get; set; }

        public string NombreCliente { get; set; }
        public string NITCliente { get; set; }
        public string DireccionCliente { get; set; }
        public int PlazoCreditoDias { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public char TipoFactura { get; set; } // 'F' o 'C'


        // Propiedad para los detalles de la venta
        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
        public string ApellidoCliente { get; set; }
    }

    public class DetalleVenta
    {
        public int IdDetVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Total { get; set; }

        // Nuevas propiedades:
        public decimal Precio { get; set; }
        public bool CreditoFiscal { get; set; }
        public decimal Iva { get; set; }

        // Para la generación de PDF
        public string NombreProducto { get; set; }
        public string NombreMarca { get; set; }
        public string NombreCategoria { get; set; }
    }
}
