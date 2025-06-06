using Entidad;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace Datos
{
    public class D_DteGenerator
    {
        private readonly D_Ventas _dVentas;

        public D_DteGenerator()
        {
            _dVentas = new D_Ventas();
        }

        /// <summary>
        /// Genera el JSON (DTE) para la venta pasada por parámetro y
        /// luego llama a D_Ventas.ActualizarFacturaElectronica para salvarlo en la BD.
        /// Devuelve la cadena JSON generada.
        /// </summary>
        public string GenerarYGuardarJson(Venta venta)
        {
            // 1) Leer la plantilla base desde disco (consumidor o crédito)
            string carpetaPlantillas = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plantillas");
            string nombrePlantilla = venta.TipoFactura == 'C'
                ? "FacturaCredito.json"
                : "FacturaConsumidor.json";

            string rutaPlantilla = Path.Combine(carpetaPlantillas, nombrePlantilla);
            if (!File.Exists(rutaPlantilla))
                throw new FileNotFoundException($"No se encontró la plantilla JSON en: {rutaPlantilla}");

            string jsonBase = File.ReadAllText(rutaPlantilla);
            dynamic doc = JsonConvert.DeserializeObject<ExpandoObject>(jsonBase, new ExpandoObjectConverter());

            // 2) Completar datos del Receptor
            doc.receptor.tipoDocumento = venta.TipoDocumentoCliente ?? "07";   // "07" = Consumidor Final si no viene
            doc.receptor.numDocumento = venta.NumeroDocumentoCliente ?? "0000000-0";
            doc.receptor.nombre = $"{venta.NombreCliente}" ?? "CONSUMIDOR FINAL";
            doc.receptor.direccion.departamento = venta.IdDistrito ?? string.Empty;
            doc.receptor.telefono = venta.TelefonoCliente ?? string.Empty;
            doc.receptor.correo = venta.Contacto ?? string.Empty;

            // 3) Completar datos de Identificación
            DateTime ahora = DateTime.Now;
            doc.identificacion.fecEmi = ahora.ToString("yyyy-MM-dd");
            doc.identificacion.horEmi = ahora.ToString("HH:mm:ss");
            doc.identificacion.tipoOperacion = venta.TipoFactura == 'C' ? 2 : 1; // 1 = Contado, 2 = Crédito
            doc.identificacion.tipoDte = "01";    // 01 = factura electrónica
            doc.identificacion.codigoGeneracion = ""; // lo llenamos luego
            // nota: ambiente, versión, modelo, moneda, etc. vienen en la plantilla.

            // 4) Completar datos del Emisor (EmpresaFiscal)
            doc.emisor.nit = venta.EmpresaNIT ?? string.Empty;
            doc.emisor.nrc = venta.EmpresaNRC ?? string.Empty;
            doc.emisor.numeroResolucion = venta.EmpresaNumeroResolucion ?? string.Empty;
            doc.emisor.cai = venta.EmpresaCAI ?? string.Empty;
            doc.emisor.direccion.departamento = venta.EmpresaDireccionSucursal ?? string.Empty;
            doc.emisor.telefono = venta.EmpresaTelefonoSucursal ?? string.Empty;
            // nombreComercial, codEstablecimiento, codPuntoVenta, etc., ya vienen en la plantilla.

            // 5) Construir cuerpoDocumento con los ítems de detalles
            var listaCuerpo = new List<dynamic>();
            int numItem = 1;
            foreach (var item in venta.Detalles)
            {
                dynamic linea = new ExpandoObject();
                linea.ivaItem = item.IvaPorItem;         // p.ej. 0.79
                linea.ventaGravada = item.Subtotal;           // p.ej. 6.87
                linea.tipoItem = 1;                       // 1 = bien o servicio gravado
                linea.codigo = item.CodigoProducto ?? item.IdProducto.ToString();
                linea.cantidad = item.Cantidad;
                linea.uniMedida = 99;                      // 99 = unidad (se define así en plantilla)
                linea.precioUni = item.Precio;             // p.ej. 8.59
                linea.montoDescu = item.MontoDescuento;     // si no hay, 0
                linea.numItem = numItem++;
                linea.descripcion = item.NombreProducto ?? string.Empty;
                listaCuerpo.Add(linea);
            }
            doc.cuerpoDocumento = listaCuerpo;

            // 6) Completar sección resumen:
            decimal totalGravada = 0m;
            decimal totalIva = 0m;
            foreach (var it in venta.Detalles)
            {
                totalGravada += it.Subtotal;
                totalIva += it.IvaPorItem;
            }
            decimal subTotal = totalGravada;
            decimal totalPagar = totalGravada + totalIva;

            doc.resumen.subTotalVentas = subTotal;
            doc.resumen.totalGravada = totalGravada;
            doc.resumen.totalIva = totalIva;
            doc.resumen.totalPagar = totalPagar;
            doc.resumen.totalLetras = ConvertirNumeroALetras(totalPagar);
            doc.resumen.condicionOperacion = venta.TipoFactura == 'C' ? 2 : 1;

            // 7) Pagos (para contado, mostramos un solo pago; para crédito, se podría dejar vacío o detallar plazos)
            doc.resumen.pagos = new[]
            {
                new
                {
                    codigo     = "01",                // 01 = Efectivo (puedes cambiar según tu lógica)
                    montoPago  = totalPagar,
                    referencia = venta.IdTransaccion  // O bien string.Empty
                }
            };

            // 8) Apéndice (campo libre para datos extra)
            var listaApendice = new List<dynamic>();
            listaApendice.Add(new { campo = "FormaPago", etiqueta = "Forma de Pago", valor = (venta.TipoFactura == 'C' ? "CRÉDITO" : "CONTADO") });
            if (venta.TipoFactura == 'C')
            {
                listaApendice.Add(new { campo = "Plazo", etiqueta = "Plazo Crédito (Días)", valor = venta.PlazoCreditoDias.ToString() });
            }
            listaApendice.Add(new { campo = "Vendedor", etiqueta = "Código Vendedor", valor = venta.IdCliente.ToString() });
            doc.apendice = listaApendice;

            // 9) Serializar a JSON
            string jsonFinal = JsonConvert.SerializeObject(doc, Formatting.Indented);

            // 10) Generar un código único de generación (GUID)
            string codigoGeneracion = Guid.NewGuid().ToString().ToUpper();

            // 11) Guardar en BD los campos DTE
            _dVentas.ActualizarFacturaElectronica(
                venta.IdVenta,
                codigoGeneracion,
                jsonFinal,
                estadoFactura: 1  // 1 = JSON generado
            );

            return jsonFinal;
        }

        /// <summary>
        /// Convierte un monto decimal a texto (p.ej. “CATORCE CON 50/100 DÓLARES”). 
        /// Aquí puedes invocar alguna librería o bien implementarlo de forma básica.
        /// </summary>
        private string ConvertirNumeroALetras(decimal monto)
        {
            // Ejemplo muy simplificado. En producción usa una librería robusta.
            int parteEntera = (int)Math.Truncate(monto);
            int centavos = (int)((monto - parteEntera) * 100);
            return $"{parteEntera} con {centavos:00}/100 DÓLARES";
        }
    }
}
