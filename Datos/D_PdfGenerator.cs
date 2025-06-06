using Entidad;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;

namespace Datos
{
    public class D_PdfGenerator
    {
        /// <summary>
        /// Genera un PDF de la factura en memoria, devolviendo un arreglo de bytes.
        /// Si includeCliente = true, imprime también los datos de cliente (p.ej. para factura crédito).
        /// </summary>
        public byte[] GenerarPdfFactura(Venta venta, List<DetalleVenta> detalles, bool includeCliente = false)
        {
            using (var ms = new MemoryStream())
            {
                // 1) Crear documento en tamaño A4 con márgenes
                var document = new Document(PageSize.A4, 36, 36, 36, 36);
                var writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // 2) Definir fuentes
                var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var fuenteSub = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                // 3) Encabezado de la empresa
                var tablaEncabezado = new PdfPTable(2) { WidthPercentage = 100f };
                tablaEncabezado.SetWidths(new float[] { 60f, 40f });

                //   A) Lado izquierdo: espacio para logo (si existe) o dejar en blanco
                //      Si tienes un logo físico, podrías usar:
                //      string rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "img", "logo.png");
                //      if (File.Exists(rutaLogo))
                //      {
                //          var img = Image.GetInstance(rutaLogo);
                //          img.ScaleToFit(80f, 80f);
                //          tablaEncabezado.AddCell(new PdfPCell(img) { Border = Rectangle.NO_BORDER });
                //      }
                //      else
                //      {
                //          tablaEncabezado.AddCell(new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER });
                //      }
                tablaEncabezado.AddCell(new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER });

                //   B) Lado derecho: datos fiscales de la empresa
                var celdaDatosEmpresa = new PdfPCell { Border = Rectangle.NO_BORDER };
                celdaDatosEmpresa.AddElement(new Paragraph("Farmacia San Nicolás S.A. de C.V.", fuenteTitulo));
                celdaDatosEmpresa.AddElement(new Paragraph($"NIT: {venta.EmpresaNIT}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"NRC: {venta.EmpresaNRC}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"Resolución: {venta.EmpresaNumeroResolucion}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"CAI: {venta.EmpresaCAI}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"Fecha Res.: {venta.EmpresaFechaResolucion:dd/MM/yyyy}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"Válido hasta: {venta.EmpresaFechaLimiteImpresion:dd/MM/yyyy}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"Sucursal: {venta.EmpresaDireccionSucursal}", fuenteNormal));
                celdaDatosEmpresa.AddElement(new Paragraph($"Tel: {venta.EmpresaTelefonoSucursal}", fuenteNormal));
                tablaEncabezado.AddCell(celdaDatosEmpresa);

                document.Add(tablaEncabezado);
                document.Add(new Paragraph(" "));  // Línea en blanco

                // 4) Sección: Datos de la factura (número, fecha, tipo y código de generación)
                var tablaFactura = new PdfPTable(2) { WidthPercentage = 100f };
                tablaFactura.SetWidths(new float[] { 50f, 50f });

                tablaFactura.AddCell(new PdfPCell(new Phrase(
                    $"Factura No: {venta.SerieFactura}-{venta.NumeroFactura:000000}", fuenteSub))
                { Border = Rectangle.NO_BORDER });
                tablaFactura.AddCell(new PdfPCell(new Phrase(
                    $"Fecha Emisión: {venta.FechaEmisionDte:dd/MM/yyyy HH:mm:ss}", fuenteSub))
                { Border = Rectangle.NO_BORDER });

                tablaFactura.AddCell(new PdfPCell(new Phrase(
                    $"Tipo: {(venta.TipoFactura == 'C' ? "Crédito Fiscal" : "Consumidor Final")}", fuenteNormal))
                { Border = Rectangle.NO_BORDER });
                tablaFactura.AddCell(new PdfPCell(new Phrase(
                    $"Cod. Generación: {venta.CodigoGeneracion}", fuenteNormal))
                { Border = Rectangle.NO_BORDER });

                document.Add(tablaFactura);
                document.Add(new Paragraph(" "));

                // 5) Sección: Datos del cliente (solo si includeCliente = true)
                if (includeCliente)
                {
                    var tablaCliente = new PdfPTable(2) { WidthPercentage = 100f };
                    tablaCliente.SetWidths(new float[] { 30f, 70f });

                    tablaCliente.AddCell(new PdfPCell(new Phrase("Cliente:", fuenteSub)) { Border = Rectangle.NO_BORDER });
                    tablaCliente.AddCell(new PdfPCell(new Phrase($"{venta.NombreCliente}", fuenteNormal)) { Border = Rectangle.NO_BORDER });

                    tablaCliente.AddCell(new PdfPCell(new Phrase("NIT/DUI:", fuenteSub)) { Border = Rectangle.NO_BORDER });
                    tablaCliente.AddCell(new PdfPCell(new Phrase($"{venta.NumeroDocumentoCliente}", fuenteNormal)) { Border = Rectangle.NO_BORDER });

                    tablaCliente.AddCell(new PdfPCell(new Phrase("Dirección:", fuenteSub)) { Border = Rectangle.NO_BORDER });
                    tablaCliente.AddCell(new PdfPCell(new Phrase($"{venta.DireccionCliente}", fuenteNormal)) { Border = Rectangle.NO_BORDER });

                    if (venta.FechaVencimiento != DateTime.MinValue)
                    {
                        tablaCliente.AddCell(new PdfPCell(new Phrase("Vence:", fuenteSub)) { Border = Rectangle.NO_BORDER });
                        tablaCliente.AddCell(new PdfPCell(new Phrase($"{venta.FechaVencimiento:dd/MM/yyyy}", fuenteNormal)) { Border = Rectangle.NO_BORDER });
                    }

                    document.Add(tablaCliente);
                    document.Add(new Paragraph(" "));
                }

                // 6) Tabla de detalles de la venta (6 columnas: #, Producto, Cantidad, P.Unit, IVA, Total)
                var tablaDetalles = new PdfPTable(6) { WidthPercentage = 100f };
                tablaDetalles.SetWidths(new float[] { 5f, 40f, 10f, 10f, 10f, 15f });

                tablaDetalles.AddCell(new PdfPCell(new Phrase("#", fuenteSub)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tablaDetalles.AddCell(new PdfPCell(new Phrase("Producto", fuenteSub)) { HorizontalAlignment = Element.ALIGN_LEFT });
                tablaDetalles.AddCell(new PdfPCell(new Phrase("Cant", fuenteSub)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tablaDetalles.AddCell(new PdfPCell(new Phrase("P.Unit", fuenteSub)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tablaDetalles.AddCell(new PdfPCell(new Phrase("IVA", fuenteSub)) { HorizontalAlignment = Element.ALIGN_CENTER });
                tablaDetalles.AddCell(new PdfPCell(new Phrase("Total", fuenteSub)) { HorizontalAlignment = Element.ALIGN_CENTER });

                int contador = 1;
                foreach (var det in detalles)
                {
                    tablaDetalles.AddCell(new PdfPCell(new Phrase(contador++.ToString(), fuenteNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaDetalles.AddCell(new PdfPCell(new Phrase(det.NombreProducto, fuenteNormal)) { HorizontalAlignment = Element.ALIGN_LEFT });
                    tablaDetalles.AddCell(new PdfPCell(new Phrase(det.Cantidad.ToString(), fuenteNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                    tablaDetalles.AddCell(new PdfPCell(new Phrase(det.Precio.ToString("F2"), fuenteNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    tablaDetalles.AddCell(new PdfPCell(new Phrase(det.IvaPorItem.ToString("F2"), fuenteNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                    tablaDetalles.AddCell(new PdfPCell(new Phrase(det.Total.ToString("F2"), fuenteNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                }

                document.Add(tablaDetalles);
                document.Add(new Paragraph(" "));

                // 7) Resumen de totales (Sub-Total, Total IVA, Monto Total)
                decimal sumaGravada = 0m, sumaIva = 0m, sumaTotal = 0m;
                foreach (var d in detalles)
                {
                    sumaGravada += d.Subtotal;
                    sumaIva += d.IvaPorItem;
                    sumaTotal += d.Total;
                }

                var tablaResumen = new PdfPTable(2) { HorizontalAlignment = Element.ALIGN_RIGHT, WidthPercentage = 50f };
                tablaResumen.SetWidths(new float[] { 50f, 50f });

                tablaResumen.AddCell(new PdfPCell(new Phrase("Sub-Total:", fuenteNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                tablaResumen.AddCell(new PdfPCell(new Phrase(sumaGravada.ToString("F2"), fuenteNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                tablaResumen.AddCell(new PdfPCell(new Phrase("Total IVA:", fuenteNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                tablaResumen.AddCell(new PdfPCell(new Phrase(sumaIva.ToString("F2"), fuenteNormal)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                tablaResumen.AddCell(new PdfPCell(new Phrase("Monto Total:", fuenteSub)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
                tablaResumen.AddCell(new PdfPCell(new Phrase(sumaTotal.ToString("F2"), fuenteSub)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

                document.Add(tablaResumen);
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("¡Gracias por su preferencia!", fuenteNormal));

                document.Close();
                writer.Close();

                return ms.ToArray();
            }
        }
    }
}
