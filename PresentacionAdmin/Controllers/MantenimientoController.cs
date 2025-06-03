using Entidad;
using Negocios;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PresentacionAdmin.Controllers
{
    public class MantenimientoController : Controller
    {

        //ESTAS SON CONEXIONES DE SERVICIOS PARA CADA ENTIDAD

        private readonly N_Categorias _nCategorias = new N_Categorias();
        private readonly N_Proveedores _nProveedores = new N_Proveedores();
        private readonly N_productos _nProductos = new N_productos();
        private readonly N_Laboratorio _nLaboratorios = new N_Laboratorio();

        public ActionResult Mantenimientos()
        {
            return View();
        }

        public ActionResult Categorias()
        {
            return View();
        }

        public ActionResult Marcas()
        {
            return View();
        }

        public ActionResult Productos()
        {
            return View();
        }

        public ActionResult Laboratorio()
        {
            return View();
        }


        //categorias
        #region categoria
        [HttpGet]
        public JsonResult ListarCategorias()
        {
            List<Categoria> oLista = new List<Categoria>();
            oLista = new N_Categorias().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarCategorias(Categoria objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.idcategoria == 0)
            {
                resultado = new N_Categorias().Registrar(objeto, out mensaje);
            }
            else
            {
                resultado = new N_Categorias().Editar(objeto, out mensaje);
            }

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarCategorias(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new N_Categorias().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        // Proveedores
        #region proveedor

        [HttpGet]
        public JsonResult ListarProveedores()
        {
            List<Proveedor> oLista = new List<Proveedor>();
            oLista = new N_Proveedores().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GuardarProveedor(Proveedor objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.idproveedor == 0)
            {
                resultado = new N_Proveedores().Registrar(objeto, out mensaje);
            }
            else
            {
                resultado = new N_Proveedores().Editar(objeto, out mensaje);
            }

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarProveedor(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new N_Proveedores().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //producto
        #region producto
        [HttpGet]
        public JsonResult ListarProductos()
        {
            List<Producto> oLista = new List<Producto>();
            oLista = new N_productos().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        // MantenimientoController.cs
        [HttpPost]

        public JsonResult GuardarProductos(string objeto, HttpPostedFileBase archivoimg)
        {
            string mensaje = string.Empty;
            bool operacionExitosa = true;
            bool guardarImgExitoso = true;

            Producto oProducto = JsonConvert.DeserializeObject<Producto>(objeto);

            // Validación de precios y stocks
            if (oProducto.precio_caja > 0 && oProducto.stock_caja <= 0)
            {
                return Json(new { operacionExitosa = false, mensaje = "Debe ingresar un stock válido para la caja si se asigna un precio." }, JsonRequestBehavior.AllowGet);
            }

            if (oProducto.precio_blister > 0 && oProducto.stock_blister <= 0)
            {
                return Json(new { operacionExitosa = false, mensaje = "Debe ingresar un stock válido para el blister si se asigna un precio." }, JsonRequestBehavior.AllowGet);
            }

            if (oProducto.precio_unidad > 0 && oProducto.stock_unidad <= 0)
            {
                return Json(new { operacionExitosa = false, mensaje = "Debe ingresar un stock válido para la unidad si se asigna un precio." }, JsonRequestBehavior.AllowGet);
            }

            // Si el ID del producto es 0, estamos creando un nuevo producto
            if (oProducto.idproducto == 0)
            {
                int idProdGenerado = new N_productos().Registrar(oProducto, out mensaje);
                if (idProdGenerado != 0)
                {
                    oProducto.idproducto = idProdGenerado;
                }
                else
                {
                    operacionExitosa = false;
                }
            }
            else
            {
                // Si el producto ya existe, se edita
                operacionExitosa = new N_productos().Editar(oProducto, out mensaje);
            }

            // Manejo de la imagen si se subió una
            if (operacionExitosa && archivoimg != null)
            {
                string rutaGuardar = ConfigurationManager.AppSettings["ServidorFotos"];
                string extension = Path.GetExtension(archivoimg.FileName);
                string nombreImg = string.Concat(oProducto.idproducto.ToString(), extension);

                try
                {
                    archivoimg.SaveAs(Path.Combine(rutaGuardar, nombreImg));
                }
                catch (Exception ex)
                {
                    mensaje = $"Error al guardar la imagen: {ex.Message}";
                    guardarImgExitoso = false;
                }

                if (guardarImgExitoso)
                {
                    oProducto.rutaimg = rutaGuardar;
                    oProducto.nombreimg = nombreImg;
                    bool resultadoGuardarImg = new N_productos().GuardarImg(oProducto, out mensaje);
                    if (!resultadoGuardarImg)
                    {
                        mensaje = "Se guardó el producto, pero hubo un problema al guardar la imagen.";
                    }
                }
            }

            return Json(new { operacionExitosa = operacionExitosa, idGenerado = oProducto.idproducto, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult imgProductos(int id)
        {
            bool conversion;
            Producto oProducto = new N_productos().Listar().Where(p => p.idproducto == id).FirstOrDefault();

            string textoBase64 = N_Recursos.ConvertirBase64(Path.Combine(oProducto.rutaimg, oProducto.nombreimg), out conversion);
            return Json(new
            {
                conversion = conversion,
                textobase64 = textoBase64,
                extension = Path.GetExtension(oProducto.nombreimg)
            }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EliminarProductos(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;

            respuesta = new N_productos().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // Laboratorios
        #region laboratorio

        [HttpGet]
        public JsonResult ListarLaboratorios()
        {
            List<Laboratorio> oLista = new List<Laboratorio>();
            oLista = new N_Laboratorio().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarLaboratorio(Laboratorio objeto)
        {
            string mensaje = string.Empty;
            int id = _nLaboratorios.Guardar(objeto, out mensaje);
            return Json(new { id = id, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult EliminarLaboratorio(int id)
        {
            string mensaje = string.Empty;
            bool resultado = _nLaboratorios.Eliminar(id, out mensaje);
            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}