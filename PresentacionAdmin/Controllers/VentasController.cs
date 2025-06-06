using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Entidad;
using Datos;
using Negocios;

namespace PresentacionAdmin.Controllers
{
    public class VentasController : Controller
    {
        private readonly N_Ventas _nVentas = new N_Ventas();
        private readonly N_productos _nProductos = new N_productos();
        private readonly N_Clientes _nClientes = new N_Clientes();
        private readonly N_EmpresaFiscal _nEmpresaFiscal = new N_EmpresaFiscal();

        // GET: /Ventas/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: /Ventas/ListarVentas
        [HttpGet]
        public JsonResult ListarVentas(DateTime? fechaInicio, DateTime? fechaFin, string idTransaccion)
        {
            var ventas = _nVentas.Listar();

            if (fechaInicio.HasValue)
                ventas = ventas.Where(v => v.FechaVenta.Date >= fechaInicio.Value.Date).ToList();
            if (fechaFin.HasValue)
                ventas = ventas.Where(v => v.FechaVenta.Date <= fechaFin.Value.Date).ToList();
            if (!string.IsNullOrWhiteSpace(idTransaccion))
                ventas = ventas.Where(v => v.IdTransaccion.Contains(idTransaccion)).ToList();

            return Json(new { data = ventas }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Ventas/ResumenVentas
        [HttpGet]
        public JsonResult ResumenVentas()
        {
            var ventas = _nVentas.Listar();
            var totalVentas = ventas.Count;
            var totalProductos = ventas.Sum(v => v.TotalProducto);
            var totalClientes = ventas.Select(v => v.IdCliente).Distinct().Count();
            return Json(new { totalVentas, totalProductos, totalClientes }, JsonRequestBehavior.AllowGet);
        }

        // GET: /Ventas/Create
        [HttpGet]
        public ActionResult Create()
        {
            // Listas para el formulario
            ViewBag.ListaProductos = _nProductos.Listar()
                .Select(p => new
                {
                    IdProducto = p.IdProducto,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Iva = p.Iva,
                    CreditoFiscal = p.CreditoFiscal
                }).ToList();

            ViewBag.ListaClientes = _nClientes.Listar()
                .Select(c => new
                {
                    IdCliente = c.IdCliente,
                    NombreCompleto = c.Nombres + " " + c.Apellidos
                }).ToList();

            return View(new Venta());
        }

        // POST: /Ventas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Venta venta, List<DetalleVenta> detalleItems)
        {
            // Validación mínima
            if (detalleItems == null || !detalleItems.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un producto al detalle.");
            }

            if (!ModelState.IsValid)
            {
                // Recargar listas si hay error
                ViewBag.ListaProductos = _nProductos.Listar()
                    .Select(p => new
                    {
                        IdProducto = p.IdProducto,
                        Nombre = p.Nombre,
                        Precio = p.Precio,
                        Iva = p.Iva,
                        CreditoFiscal = p.CreditoFiscal
                    }).ToList();

                ViewBag.ListaClientes = _nClientes.Listar()
                    .Select(c => new
                    {
                        IdCliente = c.IdCliente,
                        NombreCompleto = c.Nombres + " " + c.Apellidos
                    }).ToList();

                return View(venta);
            }

            // Calcular totales
            venta.TotalProducto = detalleItems.Sum(d => d.Cantidad);
            venta.MontoTotal = detalleItems.Sum(d => d.Total);

            // Registrar cabecera y detalles
            string mensaje;
            int idVentaGenerado = _nVentas.RegistrarVenta(venta, detalleItems, out mensaje);

            if (idVentaGenerado <= 0)
            {
                ModelState.AddModelError("", mensaje);
                ViewBag.ListaProductos = _nProductos.Listar()
                    .Select(p => new
                    {
                        IdProducto = p.IdProducto,
                        Nombre = p.Nombre,
                        Precio = p.Precio,
                        Iva = p.Iva,
                        CreditoFiscal = p.CreditoFiscal
                    }).ToList();

                ViewBag.ListaClientes = _nClientes.Listar()
                    .Select(c => new
                    {
                        IdCliente = c.IdCliente,
                        NombreCompleto = c.Nombres + " " + c.Apellidos
                    }).ToList();

                return View(venta);
            }

            // Obtener venta completa (con datos de EmpresaFiscal y Cliente)
            var ventaCompleta = _nVentas.ObtenerVentaPorId(idVentaGenerado);
            var detalles = _nVentas.ObtenerDetallesPorVentaId(idVentaGenerado);

            // Generar y guardar JSON (DTE)
            var dteGen = new D_DteGenerator();
            string jsonDte = dteGen.GenerarYGuardarJson(ventaCompleta);

            // Generar PDF en memoria
            var pdfGen = new D_PdfGenerator();
            bool includeCliente = ventaCompleta.TipoFactura == 'C';
            byte[] pdfBytes = pdfGen.GenerarPdfFactura(ventaCompleta, detalles, includeCliente);

            // Enviar correo al cliente con PDF + JSON
            try
            {
                var emailSender = new D_EmailSender();
                string asunto = $"Factura {ventaCompleta.SerieFactura}-{ventaCompleta.NumeroFactura:000000}";
                string cuerpoHtml = $@"
                    <p>Estimado(a) {ventaCompleta.NombreCliente},</p>
                    <p>Adjunto su factura electrónica No. <strong>{ventaCompleta.SerieFactura}-{ventaCompleta.NumeroFactura:000000}</strong>.</p>
                    <p>Código de Generación: <strong>{ventaCompleta.CodigoGeneracion}</strong></p>
                    <p>Fecha Emisión: <strong>{ventaCompleta.FechaEmisionDte:dd/MM/yyyy HH:mm:ss}</strong></p>
                    <p>Monto Total: <strong>USD {ventaCompleta.MontoTotal:F2}</strong></p>
                    <p>¡Gracias por su preferencia!</p>";

                emailSender.EnviarCorreo(
                    destinatario: ventaCompleta.Contacto,
                    asunto: asunto,
                    cuerpoHtml: cuerpoHtml,
                    pdfBytes: pdfBytes,
                    nombrePdf: $"Factura_{ventaCompleta.SerieFactura}-{ventaCompleta.NumeroFactura:000000}.pdf",
                    jsonContenido: jsonDte,
                    nombreJson: $"Factura_{ventaCompleta.SerieFactura}-{ventaCompleta.NumeroFactura:000000}.json"
                );
            }
            catch (Exception ex)
            {
                // Si falla el correo, podemos registrar el error y continuar
                // o mostrar un mensaje, según necesites:
                // ModelState.AddModelError("", "La venta se guardó, pero no fue posible enviar el correo: " + ex.Message);
            }

            TempData["Success"] = "Venta registrada y factura enviada correctamente.";
            return RedirectToAction("Index");
        }

        // GET: /Ventas/GenerarPDF/5
        [HttpGet]
        public FileResult GenerarPDF(int idVenta)
        {
            var venta = _nVentas.ObtenerVentaPorId(idVenta);
            var detalles = _nVentas.ObtenerDetallesPorVentaId(idVenta);
            InyectarConfigFiscal(venta);

            var pdfGen = new D_PdfGenerator();
            bool includeCliente = (venta.TipoFactura == 'C');
            byte[] bytes = pdfGen.GenerarPdfFactura(venta, detalles, includeCliente);
            string fileName = $"Factura_{venta.SerieFactura}-{venta.NumeroFactura:000000}.pdf";
            return File(bytes, "application/pdf", fileName);
        }

        // Método auxiliar para inyectar datos fiscales antes de generar PDF
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
    }
}
