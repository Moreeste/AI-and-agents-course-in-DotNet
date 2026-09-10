using BlazorIA.Datos;
using BlazorIA.Entidades;
using Microsoft.EntityFrameworkCore;

namespace BlazorIA.Servicios
{
    public class ServicioPersonas(IDbContextFactory<ApplicationDbContext> dbContextFactory) : IServicioPersonas
    {
        public async Task<IEnumerable<Persona>> ObtenerTodas()
        {
            using var context = await dbContextFactory.CreateDbContextAsync();
            return await context.Personas.ToListAsync();
        }
    }
}
