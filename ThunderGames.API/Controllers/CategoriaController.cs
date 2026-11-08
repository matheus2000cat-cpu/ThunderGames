using Microsoft.AspNetCore.Mvc;
using ThunderGames.Data;

namespace ThunderGames.API.Controllers
{
    [ApiController]
    [Route("api/categorias")]
    public class CategoriaController : ControllerBase
    {
        private readonly CategoriaRepository _repository;

        public CategoriaController(CategoriaRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        {
            var categorias = await _repository.ListarAsync(cancellationToken);
            return Ok(categorias);
        }
    }
}