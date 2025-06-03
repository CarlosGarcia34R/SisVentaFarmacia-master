using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocios
{
    public class N_Ventas
    {
        private readonly D_Ventas _dVentas = new D_Ventas();

        /// <summary>
        /// Registra una venta, ya sea Consumidor Final ('F') o Crédito Fiscal ('C').
        /// </summary>
        public bool RegistrarVenta(Venta venta, List<DetalleVenta> detalles, out string Mensaje)
        {
            // Si no se indica Crédito Fiscal, asumimos Consumidor Final
            if (venta.TipoFactura != 'C')
                venta.TipoFactura = 'F';

            int id = _dVentas.RegistrarVenta(venta, detalles, out Mensaje);
            if (id > 0)
            {
                venta.IdVenta = id;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Obtiene todas las ventas (ambos tipos) para listado.
        /// </summary>
        public List<Venta> Listar()
        {
            return _dVentas.ListarVentas();
        }

        /// <summary>
        /// Devuelve la venta con todos sus datos, incluyendo tipo y campos fiscales.
        /// </summary>
        public Venta ObtenerVentaPorId(int idVenta)
        {
            return _dVentas.ObtenerVentaPorId(idVenta);
        }

        /// <summary>
        /// Detalles de una venta específica.
        /// </summary>
        public List<DetalleVenta> ObtenerDetallesPorVentaId(int idVenta)
        {
            return _dVentas.ListarDetalles(idVenta);
        }
    }
}
