using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocios
{
    public class N_Reportes
    {
        private D_Reportes objDatos = new D_Reportes();

        public List<ReportesVenta> Ventas(string fechainicio, string fechafin, string idtransaccion)
        {
            return objDatos.Ventas(fechainicio, fechafin, idtransaccion);
        }

        public Reportes verReportes()
        {
            return objDatos.verReportes();
        }
    }
}
