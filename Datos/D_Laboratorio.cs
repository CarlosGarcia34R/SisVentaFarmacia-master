using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class D_Laboratorio
    {
        public List<Laboratorio> Listar()
        {
            List<Laboratorio> lista = new List<Laboratorio>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("sp_ListarLaboratorios", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Laboratorio()
                            {
                                id = Convert.ToInt32(dr["id"]),
                                nombre = dr["nombre"].ToString(),
                                estado = Convert.ToBoolean(dr["estado"]) // Cambiado para manejar el valor numérico directamente
                            });
                        }
                    }
                }
            }
            catch
            {
                lista = new List<Laboratorio>();
            }

            return lista;
        }



        public int Guardar(Laboratorio obj, out string Mensaje)
        {
            int idautogenerado = 0;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("sp_GuardarLaboratorio", oconexion);
                    cmd.Parameters.AddWithValue("@id", obj.id);
                    cmd.Parameters.AddWithValue("@nombre", obj.nombre);
                    cmd.Parameters.AddWithValue("@estado", obj.estado); // Aquí se debería manejar como booleano

                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    if (obj.id == 0)
                    {
                        idautogenerado = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    else
                    {
                        cmd.ExecuteNonQuery();
                        idautogenerado = obj.id;
                    }
                }
            }
            catch (Exception ex)
            {
                idautogenerado = 0;
                Mensaje = ex.Message;
            }

            return idautogenerado;
        }


        public bool Eliminar(int id, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cndb))
                {
                    SqlCommand cmd = new SqlCommand("sp_EliminarLaboratorio", oconexion);
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();

                    resultado = cmd.ExecuteNonQuery() > 0;
                    Mensaje = "Laboratorio eliminado con éxito.";
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
