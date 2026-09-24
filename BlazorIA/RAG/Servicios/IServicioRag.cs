namespace BlazorIA.RAG.Servicios
{
    public interface IServicioRag
    {
        Task Inicializar(CancellationToken cancellationToken = default);
        Task<List<string>> BuscarContextoRelevante(string pregunta, int top = 3, CancellationToken cancellationToken = default);
    }
}
