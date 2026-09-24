using BlazorIA.RAG.Modelos;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;

namespace BlazorIA.RAG.Servicios
{
    public class ServicioRagMemoria : IServicioRag
    {
        private readonly ServicioDocumentosEnMemoria servicioDocumentosEnMemoria;
        private readonly IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator;
        private readonly VectorStoreCollection<Guid, FragmentoDocumentoVector> collection;
        private bool _inicializado;

        public ServicioRagMemoria(ServicioDocumentosEnMemoria servicioDocumentosEnMemoria, 
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            InMemoryVectorStore vectorStore)
        {
            this.servicioDocumentosEnMemoria = servicioDocumentosEnMemoria;
            this.embeddingGenerator = embeddingGenerator;

            collection = vectorStore.GetCollection<Guid, FragmentoDocumentoVector>("documentos");
        }

        public async Task<List<string>> BuscarContextoRelevante(string pregunta, int top = 3, CancellationToken cancellationToken = default)
        {
            await Inicializar(cancellationToken);

            var preguntaEmbedding = await embeddingGenerator.GenerateVectorAsync(pregunta, cancellationToken: cancellationToken);

            var resultados = new List<string>();

            await foreach (var resultado in collection.SearchAsync(preguntaEmbedding, top: top, cancellationToken: cancellationToken))
            {
                resultados.Add($"""
                    Documento: {resultado.Record.TituloDocumento}
                    Contenido: {resultado.Record.Texto}
                    """);
            }

            return resultados;
        }

        public async Task Inicializar(CancellationToken cancellationToken = default)
        {
            if (_inicializado)
            {
                return;
            }

            await collection.EnsureCollectionExistsAsync(cancellationToken);

            var documentos = servicioDocumentosEnMemoria.ObtenerDocumentos();

            foreach (var documento in documentos)
            {
                var fragmentos = documento.Contenido.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                foreach (var fragmento in fragmentos)
                {
                    var vector = await embeddingGenerator.GenerateVectorAsync(fragmento, cancellationToken: cancellationToken);
                    
                    var registro = new FragmentoDocumentoVector
                    {
                        Id = Guid.NewGuid(),
                        TituloDocumento = documento.Titulo,
                        Texto = fragmento,
                        Embedding = vector
                    };

                    await collection.UpsertAsync(registro, cancellationToken);
                }
            }

            _inicializado = true;
        }
    }
}
