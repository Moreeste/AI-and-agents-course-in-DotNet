using BlazorIA.Servicios;
using Microsoft.Extensions.AI;

namespace BlazorIA.Utilidades
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
                    Description = "Obtiene el clima actual de la ciudad indicada"
                });

            var servicioEvaluaCondiciones = sp.GetRequiredService<ServicioEvaluaCondiciones>();

            yield return AIFunctionFactory.Create(
                servicioEvaluaCondiciones.EvaluarCondicion,
                new AIFunctionFactoryOptions
                {
                    Name = "evaluar_condiciones_clima",
                    Description = "Evalúa una condición climática (por ejemplo: 'lluvia', 'soleado') y determina si es momento para realizar actividades al aire libre "
                });

            var servicioObtenerCorreo = sp.GetRequiredService<ServicioObtenerCorreoFalso>();
            yield return AIFunctionFactory.Create(servicioObtenerCorreo.ObtenerCorreo);

            var servicioCorreos = sp.GetRequiredService<ServicioEnviarCorreoFalso>();
            var functionEnviarCorreos = AIFunctionFactory.Create(servicioCorreos.EnviarCorreo);
            yield return new ApprovalRequiredAIFunction(functionEnviarCorreos);
        }
    }
}
