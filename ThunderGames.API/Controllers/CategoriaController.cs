using Microsoft.AspNetCore.Mvc;
using ThunderGames.Data;
using ThunderGames.Domain;

namespace ThunderGames.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriaController : ControllerBase
    {
        private readonly CategoriaRepository _repository = new CategoriaRepository();

        [HttpPost]
        public IActionResult Cadastrar([FromBody] Categoria categoria)
        {
            try
            {
                _repository.Inserir(categoria);
                return Ok(new { mensagem = "Categoria cadastrada com sucesso!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}