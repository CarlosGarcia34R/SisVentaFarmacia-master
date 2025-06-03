using System.Configuration;

namespace Datos
{
    public class Conexion
    {
        public static string cndb = ConfigurationManager.ConnectionStrings["conexion"].ToString();
    }
}
