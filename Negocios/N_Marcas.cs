using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocios
{
    public class N_Proveedores
    {
        private D_Proveedores objDatos = new D_Proveedores();
        public List<Proveedor> Listar()
        {
            return objDatos.Listar();
        }

        public int Registrar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.nombre) || string.IsNullOrWhiteSpace(obj.nombre))
            {
                Mensaje = "Debes colocar un nombre de proveedor.";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {
                return objDatos.Registrar(obj, out Mensaje);
            }
            else
            {
                return 0;
            }
        }

        public bool Editar(Proveedor obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.nombre) || string.IsNullOrWhiteSpace(obj.nombre))
            {
                Mensaje = "Debes colocar un nombre de proveedor.";
            }

            if (string.IsNullOrEmpty(Mensaje))
            {
                return objDatos.Editar(obj, out Mensaje);
            }
            else
            {
                return false;
            }
        }

        public bool Eliminar(int id, out string Mensaje)
        {
            return objDatos.Eliminar(id, out Mensaje);
        }
    }
}
