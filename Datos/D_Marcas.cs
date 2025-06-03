using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class D_Proveedores
    {
        public List<Proveedor> Listar()
        {
            List<Proveedor> lista = new List<Proveedor>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    string query = "SELECT * FROM catalogo_proveedores";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Proveedor()
                            {
                                idproveedor = Convert.ToInt32(dr["idproveedor"]),
                                nombre = dr["nombre"].ToString(),
                                contacto = dr["contacto"].ToString(),
                                direccion = dr["direccion"].ToString(),
                                correo = dr["correo"].ToString(),
                                telefono = dr["telefono"].ToString(),
                                nit = dr["nit"].ToString(),
                                condicion_pago = dr["condicion_pago"].ToString(),
                                moneda = dr["moneda"].ToString(),
                                pais = dr["pais"].ToString(),
                                estado = Convert.ToBoolean(dr["estado"])
                            });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Proveedor>();
            }

            return lista;
        }
        public int Registrar(Proveedor obj, out string Mensaje)
        {
            int idautogenerado = 0;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("sp_crear_proveedor", oconexion);
                    cmd.Parameters.AddWithValue("nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("contacto", obj.contacto);
                    cmd.Parameters.AddWithValue("direccion", obj.direccion);
                    cmd.Parameters.AddWithValue("correo", obj.correo);
                    cmd.Parameters.AddWithValue("telefono", obj.telefono);
                    cmd.Parameters.AddWithValue("nit", obj.nit);
                    cmd.Parameters.AddWithValue("condicion_pago", obj.condicion_pago);
                    cmd.Parameters.AddWithValue("moneda", obj.moneda);
                    cmd.Parameters.AddWithValue("pais", obj.pais);
                    cmd.Parameters.AddWithValue("estado", obj.estado);

                    // Definir el parámetro de salida
                    SqlParameter outputParam = new SqlParameter("NuevoID", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    // Obtener el valor del parámetro de salida
                    idautogenerado = Convert.ToInt32(outputParam.Value);
                    Mensaje = "Proveedor registrado exitosamente";
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
        public bool Editar(Proveedor obj, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("spu_editar_proveedor", oconexion);
                    cmd.Parameters.AddWithValue("idproveedor", obj.idproveedor);
                    cmd.Parameters.AddWithValue("nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("contacto", obj.contacto);
                    cmd.Parameters.AddWithValue("direccion", obj.direccion);
                    cmd.Parameters.AddWithValue("correo", obj.correo);
                    cmd.Parameters.AddWithValue("telefono", obj.telefono);
                    cmd.Parameters.AddWithValue("nit", obj.nit);
                    cmd.Parameters.AddWithValue("condicion_pago", obj.condicion_pago);
                    cmd.Parameters.AddWithValue("moneda", obj.moneda);
                    cmd.Parameters.AddWithValue("pais", obj.pais);
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

        //eliminar
        public bool Eliminar(int id, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("sp_eliminar_proveedor", oconexion);
                    cmd.Parameters.AddWithValue("idproveedor", id);
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
