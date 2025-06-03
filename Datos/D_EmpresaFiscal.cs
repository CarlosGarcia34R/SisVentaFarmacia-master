using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Datos;
using Entidad;

namespace Datos
{
    public class D_EmpresaFiscal
{
    public EmpresaFiscal Obtener()
    {
        EmpresaFiscal config = null;
        using (var cn = new SqlConnection(Conexion.cndb))
        using (var cmd = new SqlCommand("SELECT TOP 1 * FROM EmpresaFiscal ORDER BY Id", cn))
        {
            cn.Open();
            using (var dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    config = new EmpresaFiscal
                    {
                        Id = dr["Id"] != DBNull.Value ? Convert.ToInt32(dr["Id"]) : 0,
                        Nombre = dr["Nombre"] as string ?? string.Empty,
                        NRC = dr["NRC"] as string ?? string.Empty,
                        NIT = dr["NIT"] as string ?? string.Empty,
                        NumeroResolucion = dr["NumeroResolucion"] as string ?? string.Empty,
                        CAI = dr["CAI"] as string ?? string.Empty,
                        FechaResolucion = dr["FechaResolucion"] != DBNull.Value ? Convert.ToDateTime(dr["FechaResolucion"]) : DateTime.MinValue,
                        FechaLimiteImpresion = dr["FechaLimiteImpresion"] != DBNull.Value ? Convert.ToDateTime(dr["FechaLimiteImpresion"]) : DateTime.MinValue,
                        DireccionSucursal = dr["DireccionSucursal"] as string ?? string.Empty,
                        TelefonoSucursal = dr["TelefonoSucursal"] as string ?? string.Empty
                    };
                }
            }
        }
        return config;
    }
}
}