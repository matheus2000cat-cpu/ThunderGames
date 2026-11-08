using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using ThunderGames.Data;
using ThunderGames.Domain;

namespace ThunderGames.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PessoaController : ControllerBase
    {
        private readonly PessoaRepository _repository;
        private readonly IPasswordHasher<Pessoa> _passwordHasher;

        public PessoaController(
            PessoaRepository repository,
            IPasswordHasher<Pessoa> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(
            [FromBody] Pessoa pessoa,
            CancellationToken cancellationToken)
        {
            pessoa.Perfil = "Cliente";
            pessoa.Senha = _passwordHasher.HashPassword(pessoa, pessoa.Senha);

            try
            {
                var userId = await _repository.InserirAsync(pessoa, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, new { userId });
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Conflict(new { erro = "Já existe uma conta com este CPF ou e-mail." });
            }
        }
    }
}