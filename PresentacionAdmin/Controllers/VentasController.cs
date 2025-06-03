using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using Entidad;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Negocios;
using System.Linq;
using Datos;

namespace PresentacionAdmin.Controllers
{
    public class VentasController : Controller
    {
        private readonly N_Ventas _nVentas = new N_Ventas();
        private readonly N_Categorias _nCategorias = new N_Categorias();
        private readonly N_Proveedores _nMarcas = new N_Proveedores();
        private readonly N_EmpresaFiscal _nEmpresaFiscal = new N_EmpresaFiscal();

        // GET: /Ventas/Ventas
        public ActionResult Ventas()
        {
            var ventas = _nVentas.Listar();
            // Retorna la vista "Ventas.cshtml" (debe existir en Views/Ventas)
            return View(ventas);
        }

        [HttpGet]
        public JsonResult ListarProductosDisponibles()
        {
            List<Producto> productos = new N_productos().Listar();
            return Json(new { data = productos }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListarCategorias()
        {
            List<Categoria> categorias = _nCategorias.Listar();
            return Json(new { data = categorias }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListarMarcas()
        {
            List<Proveedor> marcas = _nMarcas.Listar();
            return Json(new { data = marcas }, JsonRequestBehavior.AllowGet);
        }

        // VentasController.cs
        public ActionResult RegistrarVenta()
        {
            // Cargar listas para los combos
            ViewBag.ListaProductos = new N_productos().Listar();            // Debe devolver una lista de productos
            ViewBag.ListaLaboratorios = new N_Laboratorio().Listar();         // Debe devolver una lista de laboratorios
            ViewBag.ListaDroguerias = new N_Proveedores().Listar();             // Debe devolver una lista de droguerías

            // Retorna un objeto Venta vacío (la clase Entidad.Venta ya se ha extendido para incluir Detalles)
            return View(new Venta());
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult RegistrarVenta(Venta venta, FormCollection form)
        {
            // Se asume que el binding de Detalles se realiza correctamente gracias a los índices en el nombre de los inputs

            // Ejemplo: recalcular totales a partir de los detalles
            int totalProductos = 0;
            decimal montoTotal = 0m;
            if (venta.Detalles != null)
            {
                foreach (var detalle in venta.Detalles)
                {
                    totalProductos += detalle.Cantidad;
                    montoTotal += detalle.Precio * detalle.Cantidad;
                }
            }
            venta.TotalProducto = totalProductos;
            venta.MontoTotal = montoTotal;

            // Registrar la venta
            string mensaje;
            bool operacionExitosa = new N_Ventas().RegistrarVenta(venta, venta.Detalles, out mensaje);

            if (operacionExitosa)
            {
                // Redirige a la acción que genera el PDF usando el id de la venta registrada.
                // Puedes almacenar el id generado (por ejemplo, en TempData) o modificar el método RegistrarVenta para devolverlo.
                TempData["Exito"] = "Venta registrada correctamente";
                // Supongamos que la venta ya contiene el IdVenta generado; de lo contrario, debes adaptarlo.
                return RedirectToAction("GenerarPDF", new { idVenta = venta.IdVenta });
            }
            else
            {
                ModelState.AddModelError("", mensaje);
                // Recargar las listas en caso de error
                ViewBag.ListaProductos = new N_productos().Listar();
                ViewBag.ListaLaboratorios = new N_Laboratorio().Listar();
                ViewBag.ListaDroguerias = new N_Proveedores().Listar();
                return View(venta);
            }
        }

        public ActionResult ListadoVentas()
        {
            var ventas = _nVentas.Listar();
            return View(ventas); // Vista: Views/Ventas/ListadoVentas.cshtml
        }


        [HttpGet]
        public JsonResult ListarVentas()
        {
            List<Venta> ventas = _nVentas.Listar();
            return Json(new { data = ventas }, JsonRequestBehavior.AllowGet);
        }

        // JSON para los cards de resumen
        public JsonResult ResumenVentas()
        {
            var ventas = _nVentas.Listar();
            var totalVentas = ventas.Count;
            var totalProductos = ventas.Sum(v => v.TotalProducto);
            var totalClientes = ventas.Select(v => v.IdCliente).Distinct().Count();
            return Json(new { totalVentas, totalProductos, totalClientes }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Ventas/RegistrarVentaConsumidor
        public ActionResult RegistrarVentaConsumidor(int? idCliente, string contacto, string idDistrito,
                                                     string telefono, string direccion, string idTransaccion)
        {
            var venta = new Venta
            {
                TipoFactura = 'F',
                IdCliente = idCliente ?? 0,
                Contacto = contacto,
                IdDistrito = idDistrito,
                Telefono = telefono,
                Direccion = direccion,
                IdTransaccion = idTransaccion
            };
            ViewBag.ListaProductos = new N_productos().Listar();
            ViewBag.ListaLaboratorios = new N_Laboratorio().Listar();
            ViewBag.ListaDroguerias = new N_Proveedores().Listar();
            return View("RegistrarVentaConsumidor", venta);
        }

        // POST: /Ventas/RegistrarVentaConsumidor
        [HttpPost]
        public ActionResult RegistrarVentaConsumidor(Venta venta, FormCollection form)
        {
            venta.TipoFactura = 'F';
            venta.TotalProducto = venta.Detalles.Sum(d => d.Cantidad);
            venta.MontoTotal = venta.Detalles.Sum(d => d.Total);

            if (_nVentas.RegistrarVenta(venta, venta.Detalles, out string msg))
                return RedirectToAction("GenerarPDF", new { idVenta = venta.IdVenta });

            ModelState.AddModelError("", msg);
            ViewBag.ListaProductos = new N_productos().Listar();
            ViewBag.ListaLaboratorios = new N_Laboratorio().Listar();
            ViewBag.ListaDroguerias = new N_Proveedores().Listar();
            return View("RegistrarVentaConsumidor", venta);
        }

        // GET: /Ventas/RegistrarVentaCredito
        public ActionResult RegistrarVentaCredito(int? idCliente, string contacto, string idDistrito,
                                                  string telefono, string direccion, string idTransaccion)
        {
            var venta = new Venta
            {
                TipoFactura = 'C',
                IdCliente = idCliente ?? 0,
                Contacto = contacto,
                IdDistrito = idDistrito,
                Telefono = telefono,
                Direccion = direccion,
                IdTransaccion = idTransaccion
            };
            ViewBag.ListaProductos = new N_productos().Listar();
            ViewBag.ListaLaboratorios = new N_Laboratorio().Listar();
            ViewBag.ListaDroguerias = new N_Proveedores().Listar();
            return View("RegistrarVentaCredito", venta);
        }

        // POST: /Ventas/RegistrarVentaCredito
        [HttpPost]
        public ActionResult RegistrarVentaCredito(Venta venta, FormCollection form)
        {
            venta.TipoFactura = 'C';
            venta.TotalProducto = venta.Detalles.Sum(d => d.Cantidad);
            venta.MontoTotal = venta.Detalles.Sum(d => d.Total);

            // Garantizar que no sea null
            if (string.IsNullOrWhiteSpace(venta.Contacto))
                venta.Contacto = "N/A";

            // Garantizar que no sea null
            if (string.IsNullOrWhiteSpace(venta.IdDistrito))
                venta.IdDistrito = "N/A";

            // Garantizar que no sea null
            if (string.IsNullOrWhiteSpace(venta.Telefono))
                venta.Telefono = "969625157";

            // Garantizar que no sea null
            if (string.IsNullOrWhiteSpace(venta.Direccion))
                venta.Direccion = "N/A";

                                // Garantizar que no sea null
            if (string.IsNullOrWhiteSpace(venta.IdTransaccion))
                venta.IdTransaccion = "N/A";

            if (_nVentas.RegistrarVenta(venta, venta.Detalles, out string msg))
                return RedirectToAction("GenerarPDFCredito", new { idVenta = venta.IdVenta });

            ModelState.AddModelError("", msg);
            ViewBag.ListaProductos = new N_productos().Listar();
            ViewBag.ListaLaboratorios = new N_Laboratorio().Listar();
            ViewBag.ListaDroguerias = new N_Proveedores().Listar();
            return View("RegistrarVentaCredito", venta);
        }
        // GET: /Ventas/GenerarPDF/{idVenta}
        [HttpGet]
        public FileResult GenerarPDF(int idVenta)
        {
            var venta = _nVentas.ObtenerVentaPorId(idVenta);
            var detalles = _nVentas.ObtenerDetallesPorVentaId(idVenta);
            InyectarConfigFiscal(venta);

            return CrearPdf(venta, detalles, "Factura de Venta – Consumidor Final");
        }

        // GET: /Ventas/GenerarPDFCredito/{idVenta}
        [HttpGet]
        public FileResult GenerarPDFCredito(int idVenta)
        {
            var venta = _nVentas.ObtenerVentaPorId(idVenta);
            var detalles = _nVentas.ObtenerDetallesPorVentaId(idVenta);
            InyectarConfigFiscal(venta);

            // Añado datos específicos de cliente y vencimiento en el encabezado
            return CrearPdf(venta, detalles, "Factura Crédito Fiscal", includeCliente: true);
        }

        // Método auxiliar para inyectar datos de EmpresaFiscal
        private void InyectarConfigFiscal(Venta venta)
        {
            var cfg = _nEmpresaFiscal.Obtener();
            venta.NRC = cfg.NRC;
            venta.NIT = cfg.NIT;
            venta.NumeroResolucion = cfg.NumeroResolucion;
            venta.CAI = cfg.CAI;
            venta.FechaResolucion = cfg.FechaResolucion;
            venta.FechaLimiteImpresion = cfg.FechaLimiteImpresion;
            venta.DireccionSucursal = cfg.DireccionSucursal;
            venta.TelefonoSucursal = cfg.TelefonoSucursal;
        }

        // Método auxiliar que centraliza la creación del PDF
        private FileResult CrearPdf(Venta venta, List<DetalleVenta> detalles,string titulo, bool includeCliente = false)
        {
            using (var ms = new MemoryStream())
            {
                var doc = new Document(new Rectangle(227f, 800f), 10, 10, 10, 10);
                var writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                var normal = FontFactory.GetFont(FontFactory.HELVETICA, 8);
                var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);

                // Encabezado fiscal
                doc.Add(new Paragraph("Farmacia Santa Isabel", bold));
                doc.Add(new Paragraph($"{titulo}", bold));
                doc.Add(new Paragraph($"Factura: {venta.SerieFactura}-{venta.NumeroFactura:000000}", bold));

                if (includeCliente)
                {
                    doc.Add(new Paragraph($"Cliente: {venta.NombreCliente}", normal));
                    doc.Add(new Paragraph($"NIT: {venta.NITCliente}", normal));
                    doc.Add(new Paragraph($"Dirección: {venta.DireccionCliente}", normal));
                    if (venta.FechaVencimiento != DateTime.MinValue)
                        doc.Add(new Paragraph($"Vence: {venta.FechaVencimiento:dd/MM/yyyy}", normal));
                    doc.Add(new Paragraph(" ", normal));
                }

                doc.Add(new Paragraph($"NRC: {venta.NRC}   NIT Empresa: {venta.NIT}", normal));
                doc.Add(new Paragraph($"Resolución: {venta.NumeroResolucion}", normal));
                doc.Add(new Paragraph($"CAI: {venta.CAI}", normal));
                doc.Add(new Paragraph($"Fecha Res.: {venta.FechaResolucion:dd/MM/yyyy}   Válido hasta: {venta.FechaLimiteImpresion:dd/MM/yyyy}", normal));
                doc.Add(new Paragraph($"Sucursal: {venta.DireccionSucursal}   Tel: {venta.TelefonoSucursal}", normal));
                doc.Add(new Paragraph(" ", normal));

                // Detalles
                doc.Add(new Paragraph($"Fecha: {venta.FechaVenta:dd/MM/yyyy HH:mm:ss}", normal));
                var table = new PdfPTable(4) { WidthPercentage = 100 };
                table.SetWidths(new float[] { 40f, 15f, 20f, 25f });
                table.AddCell(new PdfPCell(new Phrase("Producto", bold)));
                table.AddCell(new PdfPCell(new Phrase("Cant", bold)));
                table.AddCell(new PdfPCell(new Phrase("P.Unit", bold)));
                table.AddCell(new PdfPCell(new Phrase("Total", bold)));

                foreach (var d in detalles)
                {
                    table.AddCell(new PdfPCell(new Phrase(d.NombreProducto, normal)));
                    table.AddCell(new PdfPCell(new Phrase(d.Cantidad.ToString(), normal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase(d.Precio.ToString("F2"), normal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    table.AddCell(new PdfPCell(new Phrase(d.Total.ToString("F2"), normal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                }
                doc.Add(table);

                // Totales
                var sub = detalles.Sum(d => d.Precio * d.Cantidad);
                var iva = detalles.Sum(d => d.Iva);
                var tot = detalles.Sum(d => d.Total);

                doc.Add(new Paragraph($"Subtotal: {sub:F2}", normal));
                doc.Add(new Paragraph($"IVA: {iva:F2}", normal));
                doc.Add(new Paragraph($"Total: {tot:F2}", normal));
                doc.Add(new Paragraph("¡Gracias por su compra!", normal));

                doc.Close();
                writer.Close();

                return File(ms.ToArray(), "application/pdf", $"{titulo}.pdf");
            }
        }




    }
}
