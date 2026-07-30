using ChatBot.Servicios;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;

namespace ChatBot.Tools
{
    public static class Tools
    {
        public static IEnumerable<AITool> ObtenerTools(this IServiceProvider sp)
        {
            var servicioClima = sp.GetRequiredService<IServicioClima>();

            yield return AIFunctionFactory.Create(
                servicioClima.ObtenerClima,
                new AIFunctionFactoryOptions
                {
                    Name = "obtener_clima",
                    Description ="Obtiene el clima actual de la ciudad indicada"
                });
        }
    }
}
