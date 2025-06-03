using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class D_Ventas
    {
        public int RegistrarVenta(Venta venta, List<DetalleVenta> detalles, out string Mensaje)
        {
            int idautogenerado = 0;
            Mensaje = string.Empty;

            using (var oconexion = new SqlConnection(Conexion.cndb))
            {
                oconexion.Open();
                using (var transaction = oconexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Serie según tipo de factura
                        string serie = venta.TipoFactura == 'C' ? "C000" : "F000";
                        venta.SerieFactura = serie;

                        // 2. Último número para la serie
                        int ultimoNumero;
                        using (var cmdSeq = new SqlCommand(
                                "SELECT ISNULL(MAX(numerofactura), 0) FROM ventas WHERE seriefactura = @serie",
                                oconexion, transaction))
                        {
                            cmdSeq.Parameters.AddWithValue("@serie", serie);
                            ultimoNumero = Convert.ToInt32(cmdSeq.ExecuteScalar());
                        }
                        venta.NumeroFactura = ultimoNumero + 1;

                        // 3. Insertar cabecera de venta con todos los campos nuevos
                        var sql = @"
                        INSERT INTO ventas (
                            seriefactura,
                            numerofactura,
                            tipoFactura,
                            idcliente,
                            nombreCliente,
                            nitCliente,
                            direccionCliente,
                            totalproducto,
                            montototal,
                            contacto,
                            iddistrito,
                            telefono,
                            direccion,
                            idtransaccion,
                            plazoCreditoDias,
                            fechaVencimiento,
                            fechaventa
                        )
                        VALUES (
                            @SerieFactura,
                            @NumeroFactura,
                            @TipoFactura,
                            @IdCliente,
                            @NombreCliente,
                            @NITCliente,
                            @DireccionCliente,
                            @TotalProducto,
                            @MontoTotal,
                            @Contacto,
                            @IdDistrito,
                            @Telefono,
                            @Direccion,
                            @IdTransaccion,
                            @PlazoCreditoDias,
                            @FechaVencimiento,
                            GETDATE()
                        );
                        SELECT SCOPE_IDENTITY();";

                        using (var cmdVenta = new SqlCommand(sql, oconexion, transaction))
                        {
                            // dentro de using(var cmdVenta = new SqlCommand(sql, oconexion, transaction)):

                            cmdVenta.Parameters.AddWithValue("@SerieFactura", venta.SerieFactura);
                            cmdVenta.Parameters.AddWithValue("@NumeroFactura", venta.NumeroFactura);
                            cmdVenta.Parameters.AddWithValue("@TipoFactura", venta.TipoFactura);
                            cmdVenta.Parameters.AddWithValue("@IdCliente", venta.IdCliente);

                            // === aquí ===
                            cmdVenta.Parameters.AddWithValue("@NombreCliente", (object)venta.NombreCliente ?? DBNull.Value);
                            cmdVenta.Parameters.AddWithValue("@NITCliente", (object)venta.NITCliente ?? DBNull.Value);
                            cmdVenta.Parameters.AddWithValue("@DireccionCliente", (object)venta.DireccionCliente ?? DBNull.Value);
                            cmdVenta.Parameters.AddWithValue("@PlazoCreditoDias", venta.PlazoCreditoDias);
                            // Calcula el valor a enviar: o la fecha (si no es MinValue) o DBNull.Value
                            object valorFechaVencimiento = venta.FechaVencimiento != DateTime.MinValue
                                ? (object)venta.FechaVencimiento
                                : DBNull.Value;

                            // Añade el parámetro
                            cmdVenta.Parameters.AddWithValue("@FechaVencimiento", valorFechaVencimiento);

                            // === fin ===

                            cmdVenta.Parameters.AddWithValue("@TotalProducto", venta.TotalProducto);
                            cmdVenta.Parameters.AddWithValue("@MontoTotal", venta.MontoTotal);
                            // En el cmdVenta.Parameters…
                            cmdVenta.Parameters.AddWithValue(
                                "@Contacto",
                                string.IsNullOrWhiteSpace(venta.Contacto)
                                  ? (object)DBNull.Value
                                  : venta.Contacto
                            );

                            cmdVenta.Parameters.AddWithValue("@IdDistrito", string.IsNullOrWhiteSpace(venta.IdDistrito) ? (object)DBNull.Value : venta.IdDistrito);
                            cmdVenta.Parameters.AddWithValue("@Telefono", string.IsNullOrWhiteSpace(venta.Telefono) ? (object)DBNull.Value : venta.Telefono);
                            cmdVenta.Parameters.AddWithValue("@Direccion", string.IsNullOrWhiteSpace(venta.Direccion)?(object)DBNull.Value : venta.Direccion );
                            cmdVenta.Parameters.AddWithValue("@IdTransaccion", string.IsNullOrWhiteSpace(venta.IdTransaccion)?(object)DBNull.Value : venta.IdTransaccion);

                            idautogenerado = Convert.ToInt32(cmdVenta.ExecuteScalar());
                        }

                        // 4. Detalles
                        foreach (var det in detalles)
                        {
                            var sqlDet = @"
                            INSERT INTO detallesVentas (
                                idventa, idproducto, cantidad, precio, subtotal, iva, total, credito_fiscal
                            )
                            VALUES (
                                @IdVenta, @IdProducto, @Cantidad, @Precio, @Subtotal, @Iva, @Total, @CreditoFiscal
                            );";

                            using (var cmdDet = new SqlCommand(sqlDet, oconexion, transaction))
                            {
                                decimal subtotal = det.Precio * det.Cantidad;
                                cmdDet.Parameters.AddWithValue("@IdVenta", idautogenerado);
                                cmdDet.Parameters.AddWithValue("@IdProducto", det.IdProducto);
                                cmdDet.Parameters.AddWithValue("@Cantidad", det.Cantidad);
                                cmdDet.Parameters.AddWithValue("@Precio", det.Precio);
                                cmdDet.Parameters.AddWithValue("@Subtotal", subtotal);
                                cmdDet.Parameters.AddWithValue("@Iva", det.Iva);
                                cmdDet.Parameters.AddWithValue("@Total", det.Total);
                                cmdDet.Parameters.AddWithValue("@CreditoFiscal", det.CreditoFiscal);

                                cmdDet.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        Mensaje = "Venta registrada con éxito";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        idautogenerado = 0;
                        Mensaje = $"Error al registrar la venta: {ex.Message}";
                    }
                }
            }

            return idautogenerado;
        }

        public Venta ObtenerVentaPorId(int idVenta)
        {
            Venta venta = null;

            var sql = "SELECT * FROM ventas WHERE idventa = @IdVenta";
            using (var cn = new SqlConnection(Conexion.cndb))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                cn.Open();
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
                            NombreCliente = dr["nombreCliente"] as string ?? string.Empty,
                            NITCliente = dr["nitCliente"] as string ?? string.Empty,
                            DireccionCliente = dr["direccionCliente"] as string ?? string.Empty,
                            TotalProducto = dr.GetInt32(dr.GetOrdinal("totalproducto")),
                            MontoTotal = dr.GetDecimal(dr.GetOrdinal("montototal")),
                            Contacto = dr["contacto"] as string ?? string.Empty,
                            IdDistrito = dr["iddistrito"] as string ?? string.Empty,
                            Telefono = dr["telefono"] as string ?? string.Empty,
                            Direccion = dr["direccion"] as string ?? string.Empty,
                            IdTransaccion = dr["idtransaccion"] as string ?? string.Empty,
                            PlazoCreditoDias = dr["plazoCreditoDias"] != DBNull.Value
                                ? dr.GetInt32(dr.GetOrdinal("plazoCreditoDias")) : 0,
                            FechaVencimiento = dr["fechaVencimiento"] != DBNull.Value
                                ? dr.GetDateTime(dr.GetOrdinal("fechaVencimiento"))
                                : DateTime.MinValue,
                            FechaVenta = dr.GetDateTime(dr.GetOrdinal("fechaventa")),
                            // resto dejaremos que lo pueble N_EmpresaFiscal
                        };
                    }
                }
            }

            return venta;
        }

        public List<Venta> ListarVentas()
        {
            var lista = new List<Venta>();
            var sql = @"
            SELECT 
              v.idventa,
              v.seriefactura,
              v.numerofactura,
              v.tipoFactura,
              v.idcliente,
              c.nombres AS NombreCliente,
              c.apellidos AS ApellidoCliente,
              v.totalproducto,
              v.montototal,
              v.idtransaccion,
              v.fechaventa
            FROM ventas v
            LEFT JOIN clientes c ON c.idcliente = v.idcliente";

            using (var cn = new SqlConnection(Conexion.cndb))
            using (var cmd = new SqlCommand(sql, cn))
            {
                cn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var v = new Venta
                        {
                            IdVenta = dr.GetInt32(dr.GetOrdinal("idventa")),

                            // SerieFactura puede ser NULL
                            SerieFactura = dr["seriefactura"] as string ?? string.Empty,

                            // NumeroFactura, si fuera NULL (aunque no debería), ponemos 0
                            NumeroFactura = dr["numerofactura"] != DBNull.Value
                                ? dr.GetInt32(dr.GetOrdinal("numerofactura"))
                                : 0,

                            // TipoFactura: leemos el primer char de la cadena o 'F' por defecto
                            TipoFactura = (!dr.IsDBNull(dr.GetOrdinal("tipoFactura"))
                                ? (dr["tipoFactura"] as string)[0]
                                : 'F'),

                            IdCliente = dr["idcliente"] != DBNull.Value
                                ? dr.GetInt32(dr.GetOrdinal("idcliente"))
                                : 0,

                            NombreCliente = dr["NombreCliente"] as string ?? "",
                            ApellidoCliente = dr["ApellidoCliente"] as string ?? "",

                            TotalProducto = dr["totalproducto"] != DBNull.Value
                                ? dr.GetInt32(dr.GetOrdinal("totalproducto"))
                                : 0,

                            MontoTotal = dr["montototal"] != DBNull.Value
                                ? dr.GetDecimal(dr.GetOrdinal("montototal"))
                                : 0m,

                            //Contacto = dr["contacto"] as string ?? string.Empty,

                            FechaVenta = dr["fechaventa"] != DBNull.Value
                                ? dr.GetDateTime(dr.GetOrdinal("fechaventa"))
                                : DateTime.MinValue
                        };

                        lista.Add(v);
                    }
                }
            }

            return lista;
        }


        public List<DetalleVenta> ListarDetalles(int idVenta)
        {
            List<DetalleVenta> lista = new List<DetalleVenta>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    string query = @"
    SELECT dv.*, 
           p.nombre AS NombreProducto, 
           p.precio AS Precio, 
           dv.precio AS PrecioDetalle, -- OJO si quieres diferenciar
           dv.credito_fiscal,
           dv.iva,
           m.descripcion AS NombreMarca, 
           c.descripcion AS NombreCategoria
    FROM detallesVentas dv
    INNER JOIN productos p ON p.idproducto = dv.idproducto
    INNER JOIN marcas m ON m.idmarca = p.idmarca
    INNER JOIN categorias c ON c.idcategoria = p.idcategoria
    WHERE dv.idventa = @IdVenta";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new DetalleVenta()
                            {
                                IdDetVenta = Convert.ToInt32(dr["iddetventa"]),
                                IdVenta = Convert.ToInt32(dr["idventa"]),
                                IdProducto = Convert.ToInt32(dr["idproducto"]),
                                Cantidad = Convert.ToInt32(dr["cantidad"]),
                                Total = Convert.ToDecimal(dr["total"]),
                                NombreProducto = dr["NombreProducto"].ToString(),
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                NombreMarca = dr["NombreMarca"].ToString(),
                                NombreCategoria = dr["NombreCategoria"].ToString(),
                                CreditoFiscal = Convert.ToBoolean(dr["credito_fiscal"]),
                                Iva = Convert.ToDecimal(dr["iva"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lista = new List<DetalleVenta>();
                // Manejo del error
            }

            return lista;
        }

    }
}
