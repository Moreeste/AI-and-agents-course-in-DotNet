using Microsoft.AspNetCore.Mvc;
using ServidorMCP.Entidades;
using ServidorMCP.Servicios;

namespace ServidorMCP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonasController(IRepositorioPersonas repositorioPersonas)
    {
        [HttpGet]
        public List<Persona> ObtenerPersonas()
        {
            return repositorioPersonas.ObtenerTodas();
        }
    }
}
