using Microsoft.AspNetCore.Mvc;
using ThunderGames.Data;
using ThunderGames.Domain;

namespace ThunderGames.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PessoaController : ControllerBase
    {
        private readonly PessoaRepository _repository = new PessoaRepository();

        [HttpPost]
        public IActionResult Cadastrar([FromBody] Pessoa pessoa)
        {
            try
            {
                _repository.Inserir(pessoa);
                return Ok(new { mensagem = "Pessoa cadastrada com sucesso!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}