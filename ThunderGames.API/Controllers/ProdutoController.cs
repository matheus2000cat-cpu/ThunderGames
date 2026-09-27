using Microsoft.AspNetCore.Mvc;
using ThunderGames.Data;
using ThunderGames.Domain;

namespace ThunderGames.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly ProdutoRepository _repository = new ProdutoRepository();

        [HttpPost]
        public IActionResult Cadastrar([FromBody] Produto produto)
        {
            try
            {
                _repository.Inserir(produto);
                return Ok(new { mensagem = "Produto cadastrado com sucesso!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}