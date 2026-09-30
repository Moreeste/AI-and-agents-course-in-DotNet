namespace BlazorIA.RAG.Modelos
{
    public record ResultadoBusquedaRag(string TituloDocumento, string Texto)
    {
        public override string ToString()
        {
            return $"""
                    Documento: {TituloDocumento}
                    Contenido: {Texto}
                    """;
        }
    }
}
