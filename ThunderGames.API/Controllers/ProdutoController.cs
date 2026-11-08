using Microsoft.AspNetCore.Mvc;
using ThunderGames.Data;

namespace ThunderGames.API.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoRepository _repository;

        public ProdutoController(ProdutoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Listar(CancellationToken cancellationToken)
        {
            var produtos = await _repository.ListarAsync(cancellationToken);
            return Ok(produtos);
        }
    }
}