using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidad
{
    public class EmpresaFiscal
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string NRC { get; set; }
        public string NIT { get; set; }
        public string NumeroResolucion { get; set; }
        public string CAI { get; set; }
        public DateTime FechaResolucion { get; set; }
        public DateTime FechaLimiteImpresion { get; set; }
        public string DireccionSucursal { get; set; }
        public string TelefonoSucursal { get; set; }
    }
}

