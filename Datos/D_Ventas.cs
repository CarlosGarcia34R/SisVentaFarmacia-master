using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class D_Ventas
    {
        /// <summary>
        /// Inserta la venta (cabecera + detalles) y devuelve el Id generado. 
        /// </summary>
        public int RegistrarVenta(Venta venta, List<DetalleVenta> detalles, out string mensaje)
        {
            int idVentaGenerado = 0;
            mensaje = string.Empty;

            using (var conn = new SqlConnection(Conexion.cndb))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 1) Determinar serie y número de factura
                        venta.SerieFactura = venta.TipoFactura == 'C' ? "C000" : "F000";

                        int ultimoNum;
                        using (var cmdSeq = new SqlCommand(
                            "SELECT ISNULL(MAX(numerofactura), 0) FROM ventas WHERE seriefactura = @serie",
                            conn, tran))
                        {
                            cmdSeq.Parameters.AddWithValue("@serie", venta.SerieFactura);
                            ultimoNum = Convert.ToInt32(cmdSeq.ExecuteScalar());
                        }
                        venta.NumeroFactura = ultimoNum + 1;

                        // 2) Insertar cabecera de venta (incluyendo nuevos campos de facturación)
                        var sqlCabecera = @"
                        INSERT INTO ventas (
                            seriefactura,
                            numerofactura,
                            tipoFactura,
                            idcliente,
                            tipoDocumentoCliente,
                            numeroDocumentoCliente,
                            nombreCliente,
                            direccionCliente,
                            telefonoCliente,
                            totalproducto,
                            montototal,
                            contacto,
                            iddistrito,
                            telefono,
                            direccion,
                            idtransaccion,
                            plazoCreditoDias,
                            fechaVencimiento,
                            fechaEmisionDte,
                            codigoGeneracion,
                            jsonDte,
                            estadoFactura,
                            IdEmpresaFiscal,
                            fechaventa
                        )
                        VALUES (
                            @SerieFactura,
                            @NumeroFactura,
                            @TipoFactura,
                            @IdCliente,
                            @TipoDocumentoCliente,
                            @NumeroDocumentoCliente,
                            @NombreCliente,
                            @DireccionCliente,
                            @TelefonoCliente,
                            @TotalProducto,
                            @MontoTotal,
                            @Contacto,
                            @IdDistrito,
                            @Telefono,
                            @Direccion,
                            @IdTransaccion,
                            @PlazoCreditoDias,
                            @FechaVencimiento,
                            @FechaEmisionDte,
                            @CodigoGeneracion,
                            @JsonDte,
                            @EstadoFactura,
                            @IdEmpresaFiscal,
                            GETDATE()
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (var cmdCab = new SqlCommand(sqlCabecera, conn, tran))
                        {
                            cmdCab.Parameters.AddWithValue("@SerieFactura", venta.SerieFactura);
                            cmdCab.Parameters.AddWithValue("@NumeroFactura", venta.NumeroFactura);
                            cmdCab.Parameters.AddWithValue("@TipoFactura", venta.TipoFactura);
                            cmdCab.Parameters.AddWithValue("@IdCliente", venta.IdCliente);

                            // Datos del cliente
                            cmdCab.Parameters.AddWithValue("@TipoDocumentoCliente", venta.TipoDocumentoCliente ?? (object)DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@NumeroDocumentoCliente", venta.NumeroDocumentoCliente ?? (object)DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@NombreCliente", venta.NombreCliente ?? (object)DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@DireccionCliente", venta.DireccionCliente ?? (object)DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@TelefonoCliente", venta.TelefonoCliente ?? (object)DBNull.Value);

                            // Totales
                            cmdCab.Parameters.AddWithValue("@TotalProducto", venta.TotalProducto);
                            cmdCab.Parameters.AddWithValue("@MontoTotal", venta.MontoTotal);

                            // Contacto
                            cmdCab.Parameters.AddWithValue("@Contacto",
                                string.IsNullOrWhiteSpace(venta.Contacto) ? (object)DBNull.Value : venta.Contacto);

                            cmdCab.Parameters.AddWithValue("@IdDistrito",
                                string.IsNullOrWhiteSpace(venta.IdDistrito) ? (object)DBNull.Value : venta.IdDistrito);

                            cmdCab.Parameters.AddWithValue("@Telefono",
                                string.IsNullOrWhiteSpace(venta.Telefono) ? (object)DBNull.Value : venta.Telefono);

                            cmdCab.Parameters.AddWithValue("@Direccion",
                                string.IsNullOrWhiteSpace(venta.Direccion) ? (object)DBNull.Value : venta.Direccion);

                            cmdCab.Parameters.AddWithValue("@IdTransaccion",
                                string.IsNullOrWhiteSpace(venta.IdTransaccion) ? (object)DBNull.Value : venta.IdTransaccion);

                            // Crédito
                            cmdCab.Parameters.AddWithValue("@PlazoCreditoDias", venta.PlazoCreditoDias);
                            cmdCab.Parameters.AddWithValue("@FechaVencimiento",
                                venta.FechaVencimiento != DateTime.MinValue
                                    ? (object)venta.FechaVencimiento
                                    : DBNull.Value);

                            // Campos de facturación electrónica (temporales: se actualizarán luego)
                            cmdCab.Parameters.AddWithValue("@FechaEmisionDte", DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@CodigoGeneracion", DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@JsonDte", DBNull.Value);
                            cmdCab.Parameters.AddWithValue("@EstadoFactura", 0);

                            // FK a EmpresaFiscal (asumimos siempre Id=1)
                            cmdCab.Parameters.AddWithValue("@IdEmpresaFiscal", venta.IdEmpresaFiscal > 0
                                                                                         ? (object)venta.IdEmpresaFiscal
                                                                                         : 1);

                            // Ejecutar insert
                            idVentaGenerado = Convert.ToInt32(cmdCab.ExecuteScalar());
                        }

                        // 3) Insertar detalles
                        foreach (var det in detalles)
                        {
                            var sqlDet = @"
                            INSERT INTO detallesVentas (
                                idventa, idproducto, cantidad, precio, ventaGravada, montoDescuento, iva, total, credito_fiscal
                            )
                            VALUES (
                                @IdVenta, @IdProducto, @Cantidad, @Precio, @VentaGravada, @MontoDescuento, @Iva, @Total, @CreditoFiscal
                            );";

                            using (var cmdDet = new SqlCommand(sqlDet, conn, tran))
                            {
                                // Calcular “ventaGravada” = precio * cantidad – montoDescuento
                                decimal ventaGravada = (det.Precio * det.Cantidad) - det.MontoDescuento;
                                decimal montoDescuento = det.MontoDescuento;
                                decimal ivaPorItem = det.Iva; // ya viene calculado en el DTO
                                decimal totalLinea = ventaGravada + ivaPorItem;

                                cmdDet.Parameters.AddWithValue("@IdVenta", idVentaGenerado);
                                cmdDet.Parameters.AddWithValue("@IdProducto", det.IdProducto);
                                cmdDet.Parameters.AddWithValue("@Cantidad", det.Cantidad);
                                cmdDet.Parameters.AddWithValue("@Precio", det.Precio);
                                cmdDet.Parameters.AddWithValue("@VentaGravada", ventaGravada);
                                cmdDet.Parameters.AddWithValue("@MontoDescuento", montoDescuento);
                                cmdDet.Parameters.AddWithValue("@Iva", ivaPorItem);
                                cmdDet.Parameters.AddWithValue("@Total", totalLinea);
                                cmdDet.Parameters.AddWithValue("@CreditoFiscal", det.CreditoFiscal);

                                cmdDet.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        mensaje = "VENTA REGISTRADA CON ÉXITO";
                        return idVentaGenerado;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        mensaje = $"ERROR AL REGISTRAR VENTA: {ex.Message}";
                        return 0;
                    }
                }
            }
        }

        /// <summary>
        /// Actualiza los campos de facturación (fechaEmisionDte, codigoGeneracion, jsonDte y estadoFactura) 
        /// una vez que se ha generado el JSON externamente.
        /// </summary>
        public void ActualizarFacturaElectronica(int idVenta,
                                                 string codigoGeneracion,
                                                 string jsonDte,
                                                 byte estadoFactura)
        {
            var sql = @"
                UPDATE ventas
                SET 
                    fechaEmisionDte  = GETDATE(),
                    codigoGeneracion = @CodigoGeneracion,
                    jsonDte          = @JsonDte,
                    estadoFactura    = @EstadoFactura
                WHERE idventa = @IdVenta;";

            using (var conn = new SqlConnection(Conexion.cndb))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@CodigoGeneracion", codigoGeneracion);
                cmd.Parameters.AddWithValue("@JsonDte", jsonDte);
                cmd.Parameters.AddWithValue("@EstadoFactura", estadoFactura);
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Devuelve un objeto Venta completo (cabecera + empresa fiscal) para armar JSON/PDF.
        /// </summary>
        public Venta ObtenerVentaPorId(int idVenta)
        {
            Venta venta = null;

            const string sql = @"
                SELECT 
                    v.idventa,
                    v.seriefactura,
                    v.numerofactura,
                    v.tipoFactura,
                    v.idcliente,
                    c.nombres            AS NombreCliente,
                    c.apellidos          AS ApellidoCliente,
                    c.tipoDocumento      AS TipoDocumentoCliente,
                    c.numeroDocumento    AS NumeroDocumentoCliente,
                    c.direccionCliente   AS DireccionCliente,
                    c.telefonoCliente    AS TelefonoCliente,
                    v.totalproducto,
                    v.montototal,
                    v.contacto,
                    v.iddistrito,
                    v.telefono           AS Telefono,
                    v.direccion          AS Direccion,
                    v.idtransaccion,
                    v.plazoCreditoDias,
                    v.fechaVencimiento,
                    v.fechaventa,
                    v.codigoGeneracion,
                    v.jsonDte,
                    v.estadoFactura,
                    ef.Nombre            AS EmpresaNombre,
                    ef.NRC               AS EmpresaNRC,
                    ef.NIT               AS EmpresaNIT,
                    ef.NumeroResolucion  AS EmpresaNumeroResolucion,
                    ef.CAI               AS EmpresaCAI,
                    ef.FechaResolucion   AS EmpresaFechaResolucion,
                    ef.FechaLimiteImpresion AS EmpresaFechaLimite,
                    ef.DireccionSucursal AS EmpresaDireccionSucursal,
                    ef.TelefonoSucursal  AS EmpresaTelefonoSucursal,
                    ef.CorreoEmisor
                FROM ventas v
                LEFT JOIN clientes c ON c.idcliente = v.idcliente
                LEFT JOIN EmpresaFiscal ef ON ef.Id = v.IdEmpresaFiscal
                WHERE v.idventa = @IdVenta;";

            using (var conn = new SqlConnection(Conexion.cndb))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                conn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        venta = new Venta
                        {
                            IdVenta = dr.GetInt32(dr.GetOrdinal("idventa")),
                            SerieFactura = dr.GetString(dr.GetOrdinal("seriefactura")),
                            NumeroFactura = dr.GetInt32(dr.GetOrdinal("numerofactura")),
                            TipoFactura = dr.GetString(dr.GetOrdinal("tipoFactura"))[0],
                            IdCliente = dr.GetInt32(dr.GetOrdinal("idcliente")),
                            NombreCliente = dr["NombreCliente"] as string ?? string.Empty,
                            ApellidoCliente = dr["ApellidoCliente"] as string ?? string.Empty,
                            TipoDocumentoCliente = dr["TipoDocumentoCliente"] as string ?? string.Empty,
                            NumeroDocumentoCliente = dr["NumeroDocumentoCliente"] as string ?? string.Empty,
                            DireccionCliente = dr["DireccionCliente"] as string ?? string.Empty,
                            TelefonoCliente = dr["TelefonoCliente"] as string ?? string.Empty,
                            TotalProducto = dr["totalproducto"] != DBNull.Value
                                                        ? dr.GetInt32(dr.GetOrdinal("totalproducto"))
                                                        : 0,
                            MontoTotal = dr["montototal"] != DBNull.Value
                                                        ? dr.GetDecimal(dr.GetOrdinal("montototal"))
                                                        : 0m,
                            Contacto = dr["contacto"] as string ?? string.Empty,
                            IdDistrito = dr["iddistrito"] as string ?? string.Empty,
                            Telefono = dr["Telefono"] as string ?? string.Empty,
                            Direccion = dr["Direccion"] as string ?? string.Empty,
                            IdTransaccion = dr["idtransaccion"] as string ?? string.Empty,
                            PlazoCreditoDias = dr["plazoCreditoDias"] != DBNull.Value
                                                        ? dr.GetInt32(dr.GetOrdinal("plazoCreditoDias"))
                                                        : 0,
                            FechaVencimiento = dr["fechaVencimiento"] != DBNull.Value
                                                        ? dr.GetDateTime(dr.GetOrdinal("fechaVencimiento"))
                                                        : DateTime.MinValue,
                            FechaVenta = dr["fechaventa"] != DBNull.Value
                                                        ? dr.GetDateTime(dr.GetOrdinal("fechaventa"))
                                                        : DateTime.MinValue,
                            CodigoGeneracion = dr["codigoGeneracion"] as string ?? string.Empty,
                            JsonDte = dr["jsonDte"] as string ?? string.Empty,
                            EstadoFactura = dr["estadoFactura"] != DBNull.Value
                                                        ? Convert.ToByte(dr["estadoFactura"])
                                                        : (byte)0,

                            // Empresa Fiscal
                            EmpresaNombre = dr["EmpresaNombre"] as string ?? string.Empty,
                            EmpresaNRC = dr["EmpresaNRC"] as string ?? string.Empty,
                            EmpresaNIT = dr["EmpresaNIT"] as string ?? string.Empty,
                            EmpresaNumeroResolucion = dr["EmpresaNumeroResolucion"] as string ?? string.Empty,
                            EmpresaCAI = dr["EmpresaCAI"] as string ?? string.Empty,
                            EmpresaFechaResolucion = dr["EmpresaFechaResolucion"] != DBNull.Value
                                                        ? dr.GetDateTime(dr.GetOrdinal("EmpresaFechaResolucion"))
                                                        : DateTime.MinValue,
                            EmpresaFechaLimiteImpresion = dr["EmpresaFechaLimite"] != DBNull.Value
                                                        ? dr.GetDateTime(dr.GetOrdinal("EmpresaFechaLimite"))
                                                        : DateTime.MinValue,
                            EmpresaDireccionSucursal = dr["EmpresaDireccionSucursal"] as string ?? string.Empty,
                            EmpresaTelefonoSucursal = dr["EmpresaTelefonoSucursal"] as string ?? string.Empty,
                            EmpresaCorreoEmisor = dr["CorreoEmisor"] as string ?? string.Empty
                        };
                    }
                }
            }

            return venta;
        }

        /// <summary>
        /// Devuelve todos los detalles de una venta para poder confeccionar el JSON/PDF.
        /// </summary>
        public List<DetalleVenta> ObtenerDetallesPorVentaId(int idVenta)
        {
            var lista = new List<DetalleVenta>();
            const string sql = @"
                SELECT 
                    dv.iddetventa,
                    dv.idventa,
                    dv.idproducto,
                    dv.cantidad,
                    dv.precio        AS PrecioUnitario,
                    dv.ventaGravada  AS Subtotal,
                    dv.montoDescuento,
                    dv.iva           AS IvaPorItem,
                    dv.total         AS TotalLinea,
                    p.nombre         AS NombreProducto,
                    p.codigo         AS CodigoProducto,
                    m.descripcion    AS Marca,
                    c.descripcion    AS Categoria
                FROM detallesVentas dv
                INNER JOIN productos p ON p.idproducto = dv.idproducto
                INNER JOIN marcas m ON m.idmarca = p.idmarca
                INNER JOIN categorias c ON c.idcategoria = p.idcategoria
                WHERE dv.idventa = @IdVenta;";

            using (var conn = new SqlConnection(Conexion.cndb))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                conn.Open();

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new DetalleVenta
                        {
                            IdDetVenta = dr.GetInt32(dr.GetOrdinal("iddetventa")),
                            IdVenta = dr.GetInt32(dr.GetOrdinal("idventa")),
                            IdProducto = dr.GetInt32(dr.GetOrdinal("idproducto")),
                            Cantidad = dr.GetInt32(dr.GetOrdinal("cantidad")),
                            Precio = dr.GetDecimal(dr.GetOrdinal("PrecioUnitario")),
                            Subtotal = dr.GetDecimal(dr.GetOrdinal("Subtotal")),
                            MontoDescuento = dr.GetDecimal(dr.GetOrdinal("montoDescuento")),
                            Iva = dr.GetDecimal(dr.GetOrdinal("IvaPorItem")),
                            Total = dr.GetDecimal(dr.GetOrdinal("TotalLinea")),
                            NombreProducto = dr["NombreProducto"] as string ?? string.Empty,
                            CodigoProducto = dr["CodigoProducto"] as string ?? string.Empty,
                            Marca = dr["Marca"] as string ?? string.Empty,
                            Categoria = dr["Categoria"] as string ?? string.Empty,
                            CreditoFiscal = false // ya no lo almacenamos aquí, pues la lógica DTE usa ventaGravada/Iva
                        });
                    }
                }
            }

            return lista;
        }
    }
}
