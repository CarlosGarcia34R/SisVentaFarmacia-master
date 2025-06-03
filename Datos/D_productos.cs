using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Datos
{
    public class D_productos
    {
        // D_productos.cs
        public List<Producto> Listar()
        {
            List<Producto> lista = new List<Producto>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT p.idproducto, p.nombre, p.Descripcion, c.idcategoria, c.descripcion AS descategoria,");
                    sb.AppendLine("pr.idproveedor, pr.nombre AS nombreproveedor, l.id AS idlaboratorio, l.nombre AS nombrelaboratorio,");
                    sb.AppendLine("p.stock_caja, p.stock_blister, p.stock_unidad, p.precio_unidad, p.precio_blister, p.precio_caja,");
                    sb.AppendLine("p.credito_fiscal, p.vitrina, p.presentacion, p.codigo_barras, p.absorcion, p.compuesto, p.fecha_vencimiento,");
                    sb.AppendLine("p.rutaimg, p.nombreimg, p.estado");
                    sb.AppendLine("FROM productos p");
                    sb.AppendLine("INNER JOIN categorias c ON c.idcategoria = p.idcategoria");
                    sb.AppendLine("INNER JOIN catalogo_proveedores pr ON pr.idproveedor = p.idproveedor");
                    sb.AppendLine("INNER JOIN laboratorio l ON l.id = p.idlaboratorio");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oconexion);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Producto()
                            {
                                idproducto = Convert.ToInt32(dr["idproducto"]),
                                nombre = dr["nombre"].ToString(),
                                Descripcion = dr["Descripcion"].ToString(),
                                ocategoria = new Categoria() { idcategoria = Convert.ToInt32(dr["idcategoria"]), descripcion = dr["descategoria"].ToString() },
                                oproveedor = new Proveedor() { idproveedor = Convert.ToInt32(dr["idproveedor"]), nombre = dr["nombreproveedor"].ToString() },
                                olaboratorio = new Laboratorio() { id = Convert.ToInt32(dr["idlaboratorio"]), nombre = dr["nombrelaboratorio"].ToString() },
                                stock_caja = Convert.ToInt32(dr["stock_caja"]),
                                stock_blister = Convert.ToInt32(dr["stock_blister"]),
                                stock_unidad = Convert.ToInt32(dr["stock_unidad"]),
                                precio_unidad = Convert.ToDecimal(dr["precio_unidad"]),
                                precio_blister = Convert.ToDecimal(dr["precio_blister"]),
                                precio_caja = Convert.ToDecimal(dr["precio_caja"]),
                                credito_fiscal = Convert.ToBoolean(dr["credito_fiscal"]),
                                vitrina = Convert.ToBoolean(dr["vitrina"]),
                                presentacion = dr["presentacion"].ToString(),
                                codigo_barras = dr["codigo_barras"].ToString(),
                                absorcion = dr["absorcion"].ToString(),
                                compuesto = dr["compuesto"].ToString(),
                                fecha_vencimiento = Convert.ToDateTime(dr["fecha_vencimiento"]),
                                rutaimg = dr["rutaimg"].ToString(),
                                nombreimg = dr["nombreimg"].ToString(),
                                estado = Convert.ToBoolean(dr["estado"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error (implement a logging mechanism if not present)
                Console.WriteLine($"Error: {ex.Message}");
                lista = new List<Producto>();
            }

            return lista;
        }


        public int Registrar(Producto obj, out string Mensaje)
        {
            int idautogenerado = 0;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("spu_registrar_productos", oconexion);
                    cmd.Parameters.AddWithValue("idcategoria", obj.ocategoria.idcategoria);
                    cmd.Parameters.AddWithValue("nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("Descripcion", obj.Descripcion);
                    cmd.Parameters.AddWithValue("stock_caja", obj.stock_caja);
                    cmd.Parameters.AddWithValue("stock_blister", obj.stock_blister);
                    cmd.Parameters.AddWithValue("stock_unidad", obj.stock_unidad);
                    cmd.Parameters.AddWithValue("precio_unidad", obj.precio_unidad);
                    cmd.Parameters.AddWithValue("precio_blister", obj.precio_blister);
                    cmd.Parameters.AddWithValue("precio_caja", obj.precio_caja);
                    cmd.Parameters.AddWithValue("idproveedor", obj.oproveedor.idproveedor);
                    cmd.Parameters.AddWithValue("idlaboratorio", obj.olaboratorio.id);
                    cmd.Parameters.AddWithValue("credito_fiscal", obj.credito_fiscal);
                    cmd.Parameters.AddWithValue("vitrina", obj.vitrina);
                    cmd.Parameters.AddWithValue("presentacion", obj.presentacion);
                    cmd.Parameters.AddWithValue("codigo_barras", obj.codigo_barras);
                    cmd.Parameters.AddWithValue("absorcion", obj.absorcion);
                    cmd.Parameters.AddWithValue("compuesto", obj.compuesto);
                    cmd.Parameters.AddWithValue("fecha_vencimiento", obj.fecha_vencimiento);
                    cmd.Parameters.AddWithValue("estado", obj.estado);
                    cmd.Parameters.Add("resultado", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 60).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    cmd.ExecuteNonQuery();
                    idautogenerado = Convert.ToInt32(cmd.Parameters["resultado"].Value);
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                idautogenerado = 0;
                Mensaje = ex.Message;
            }
            return idautogenerado;
        }


        //editar
        public bool Editar(Producto obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("spu_editar_productos", oconexion);
                    cmd.Parameters.AddWithValue("idproducto", obj.idproducto);
                    cmd.Parameters.AddWithValue("idcategoria", obj.ocategoria.idcategoria);
                    cmd.Parameters.AddWithValue("nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("Descripcion", obj.Descripcion);
                    cmd.Parameters.AddWithValue("stock_caja", obj.stock_caja);
                    cmd.Parameters.AddWithValue("stock_blister", obj.stock_blister);
                    cmd.Parameters.AddWithValue("stock_unidad", obj.stock_unidad);
                    cmd.Parameters.AddWithValue("precio_unidad", obj.precio_unidad);
                    cmd.Parameters.AddWithValue("precio_blister", obj.precio_blister);
                    cmd.Parameters.AddWithValue("precio_caja", obj.precio_caja);
                    cmd.Parameters.AddWithValue("idproveedor", obj.oproveedor.idproveedor);
                    cmd.Parameters.AddWithValue("idlaboratorio", obj.olaboratorio.id);
                    cmd.Parameters.AddWithValue("credito_fiscal", obj.credito_fiscal);
                    cmd.Parameters.AddWithValue("vitrina", obj.vitrina);
                    cmd.Parameters.AddWithValue("presentacion", obj.presentacion);
                    cmd.Parameters.AddWithValue("codigo_barras", obj.codigo_barras);
                    cmd.Parameters.AddWithValue("absorcion", obj.absorcion);
                    cmd.Parameters.AddWithValue("compuesto", obj.compuesto);
                    cmd.Parameters.AddWithValue("fecha_vencimiento", obj.fecha_vencimiento);
                    cmd.Parameters.AddWithValue("estado", obj.estado);
                    cmd.Parameters.Add("resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    cmd.ExecuteNonQuery();
                    resultado = Convert.ToBoolean(cmd.Parameters["resultado"].Value);
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
            }
            return resultado;
        }


        //guardar ruta y nombre de imagen
        public bool GuardarImg(Producto obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    string query = "update productos set rutaimg = @rutaimg, nombreimg = @nombreimg where idproducto = @idproducto";

                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@rutaimg", obj.rutaimg);
                    cmd.Parameters.AddWithValue("@nombreimg", obj.nombreimg);
                    cmd.Parameters.AddWithValue("@idproducto", obj.idproducto);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        resultado = true;
                    }
                    else
                    {
                        Mensaje = "No se Pudo Actualiza la Imagen 🖼️";
                    }
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
            }
            return resultado;
        }

        //eliminar
        public bool Eliminar(int id, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("spu_eliminar_producto", oconexion);
                    cmd.Parameters.AddWithValue("idproducto", id);
                    cmd.Parameters.Add("resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("mensaje", SqlDbType.VarChar, 60).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    cmd.ExecuteNonQuery();
                    resultado = Convert.ToBoolean(cmd.Parameters["resultado"].Value);
                    Mensaje = cmd.Parameters["mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
            }
            return resultado;
        }
    }
}
