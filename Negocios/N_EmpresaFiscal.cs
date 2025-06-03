using Datos;
using Entidad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocios
{
    public class N_EmpresaFiscal
    {
        private readonly D_EmpresaFiscal _dEmpresaFiscal = new D_EmpresaFiscal();

        public EmpresaFiscal Obtener()
        {
            return _dEmpresaFiscal.Obtener();
        }
    }
}
