namespace ChatBot
{
    public class Utilidades
    {
        public static void CargarVariablesDeEntorno()
        {
            foreach (var linea in File.ReadAllLines(".env"))
            {
                var partes = linea.Split('=');
                if (partes.Length == 2)
                {
                    Environment.SetEnvironmentVariable(partes[0], partes[1]);
                }
            }
        }
    }
}
