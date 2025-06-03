using Datos;
using System.Collections.Generic;

namespace Negocios
{
    public class N_productos
    {
        private D_productos objDatos = new D_productos();

        public List<Producto> Listar()
        {
            return objDatos.Listar();
        }

        public int Registrar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.ocategoria.idcategoria == 0)
            {
                Mensaje = "Debes seleccionar una categoría 🤨";
            }
            else if (string.IsNullOrEmpty(obj.nombre) || string.IsNullOrWhiteSpace(obj.nombre))
            {
                Mensaje = "Debes colocar un nombre para el producto 🤨";
            }
            else if (string.IsNullOrEmpty(obj.Descripcion) || string.IsNullOrWhiteSpace(obj.Descripcion))
            {
                Mensaje = "Debes colocar una descripción para el producto 🤨";
            }
            else if (obj.oproveedor.idproveedor == 0)
            {
                Mensaje = "Debes seleccionar un proveedor 🤨";
            }
            else if (obj.olaboratorio.id == 0)
            {
                Mensaje = "Debes seleccionar un laboratorio 🤨";
            }
            else if (obj.precio_unidad <= 0 && obj.precio_blister <= 0 && obj.precio_caja <= 0)
            {
                Mensaje = "Debes asignar al menos un precio al producto (unidad, blister o caja) 🤨";
            }
            else if (obj.stock_caja == 0 && obj.stock_blister == 0 && obj.stock_unidad == 0)
            {
                Mensaje = "Debes asignar al menos un stock al producto (unidad, blister o caja) 🤨";
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

        public bool Editar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;

            if (obj.ocategoria.idcategoria == 0)
            {
                Mensaje = "Debes seleccionar una categoría 🤨";
            }
            else if (string.IsNullOrEmpty(obj.nombre) || string.IsNullOrWhiteSpace(obj.nombre))
            {
                Mensaje = "Debes colocar un nombre para el producto 🤨";
            }
            else if (string.IsNullOrEmpty(obj.Descripcion) || string.IsNullOrWhiteSpace(obj.Descripcion))
            {
                Mensaje = "Debes colocar una descripción para el producto 🤨";
            }
            else if (obj.oproveedor.idproveedor == 0)
            {
                Mensaje = "Debes seleccionar un proveedor 🤨";
            }
            else if (obj.olaboratorio.id == 0)
            {
                Mensaje = "Debes seleccionar un laboratorio 🤨";
            }
            else if (obj.precio_unidad <= 0 && obj.precio_blister <= 0 && obj.precio_caja <= 0)
            {
                Mensaje = "Debes asignar al menos un precio al producto (unidad, blister o caja) 🤨";
            }
            else if (obj.stock_caja == 0 && obj.stock_blister == 0 && obj.stock_unidad == 0)
            {
                Mensaje = "Debes asignar al menos un stock al producto (unidad, blister o caja) 🤨";
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

        public bool GuardarImg(Producto obj, out string Mensaje)
        {
            return objDatos.GuardarImg(obj, out Mensaje);
        }

        public bool Eliminar(int id, out string Mensaje)
        {
            return objDatos.Eliminar(id, out Mensaje);
        }
    }
}
