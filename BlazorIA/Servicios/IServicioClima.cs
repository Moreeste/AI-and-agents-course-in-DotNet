namespace BlazorIA.Servicios
{
    public interface IServicioClima
    {
        Task<string> ObtenerClima(string ciudad);
    }
}
