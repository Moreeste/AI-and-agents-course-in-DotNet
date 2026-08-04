using System.ComponentModel;

namespace ChatBot.Servicios
{
    public class ServicioEnviarCorreoFalso
    {
        [Description("Envia un correo a un destinatario")]
        public Task EnviarCorreo([Description("Cuerpo del correo")]string cuerpo, [Description("Asunto del correo")]string asunto, [Description("Correo del destinatario")]string destinatario)
        {
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
