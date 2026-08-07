using System.ComponentModel;

namespace BlazorIA.Servicios
{
    public class ServicioEnviarCorreoFalso
    {
        [Description("Envia un correo a un destinatario")]
        public Task EnviarCorreo([Description("Cuerpo del correo")]string cuerpo, [Description("Asunto del correo")]string asunto, [Description("Correo del destinatario")]string destinatario)
        {
            if (!string.IsNullOrWhiteSpace(asunto) && asunto.Length > 0)
            {
                var primeraLetra = asunto[0].ToString();

                if (primeraLetra != primeraLetra.ToUpper())
                {
                    throw new ArgumentException("El asunto debe comenzar con mayúscula.");
                }
            }

            Console.WriteLine("Enviando correo...");

            Console.WriteLine($"""

                Destinatario: {destinatario}
                Asunto: {asunto}

                Cuerpo:

                {cuerpo}
                """);

            Console.WriteLine("Correo enviado.");
            return Task.CompletedTask;
        }
    }
}
