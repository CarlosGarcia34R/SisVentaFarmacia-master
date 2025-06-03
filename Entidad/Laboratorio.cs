namespace Entidad
{
    public class Laboratorio
    {
        public int id { get; set; }  // Este será el identificador único del laboratorio
        public string nombre { get; set; }  // Nombre del laboratorio
        public bool estado { get; set; }  // Estado de si el laboratorio está activo o no (S/N)
    }
}
