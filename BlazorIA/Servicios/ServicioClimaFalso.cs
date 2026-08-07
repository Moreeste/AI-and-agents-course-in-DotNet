namespace BlazorIA.Servicios
{
    public class ServicioClimaFalso : IServicioClima
    {
        public async Task<string> ObtenerClima(string ciudad)
        {
            return ciudad.ToLower() switch
            {
                "londres" => "Lluvioso, 15°C.",
                "paris" => "Soleado, 22°C.",
                "nueva york" => "Nublado, 18°C.",
                _ => $"No tengo información del clima para esa ciudad.",
            };
        }
    }
}
