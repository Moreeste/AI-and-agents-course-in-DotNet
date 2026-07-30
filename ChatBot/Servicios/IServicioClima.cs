namespace ChatBot.Servicios
{
    public interface IServicioClima
    {
        Task<string> ObtenerClima(string ciudad);
    }
}
