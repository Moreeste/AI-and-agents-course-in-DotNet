using System.ComponentModel;

namespace BlazorIA.Servicios
{
    public class ServicioObtenerCorreoFalso
    {
        [Description("Obtiene el correo de una persona")]
        public string ObtenerCorreo([Description("Nombre de la persona")]string nombre) => $"{nombre}@mail.com";
    }
}
