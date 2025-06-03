using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocios
{
    public class N_Laboratorio
    {
        private D_Laboratorio objDatos = new D_Laboratorio();

        public List<Laboratorio> Listar()
        {
            return objDatos.Listar();
        }

        public int Guardar(Laboratorio obj, out string Mensaje)
        {
            return objDatos.Guardar(obj, out Mensaje);
        }

        public bool Eliminar(int id, out string Mensaje)
        {
            return objDatos.Eliminar(id, out Mensaje);
        }
    }
}
