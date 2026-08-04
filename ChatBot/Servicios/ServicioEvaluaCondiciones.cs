namespace ChatBot.Servicios
{
    public class ServicioEvaluaCondiciones
    {
        public string EvaluarCondicion(string condicion)
        {
            condicion = condicion.ToLower();

            if (condicion.Contains("lluvia") || condicion.Contains("llovizna") || condicion.Contains("precipitaciones"))
            {
                return "No es un buen momento para actividades al aire libre";
            }

            if (condicion.Contains("tormenta") || condicion.Contains("tormentoso"))
            {
                return "Evita salir, condiciones climáticas peligrosas";
            }

            if (condicion.Contains("nieve") || condicion.Contains("nevadas") || condicion.Contains("ventisca"))
            {
                return "Condiciones frías y probablemente peligrosas, sal sólo sines necesario";
            }

            if (condicion.Contains("neblina") || condicion.Contains("niebla"))
            {
                return "Precaución al salir, la visibilidad puede estar reducida";
            }

            if (condicion.Contains("soleado"))
            {
                return "Excelente clima para salir";
            }

            if (condicion.Contains("nublado"))
            {
                return "Puedes salir, pero no es el clima ideal";
            }

            return "Condiciones normales";
        }
    }
}
