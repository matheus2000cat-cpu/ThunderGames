using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ThunderGames.API;
using ThunderGames.Data;
using ThunderGames.Domain;

namespace ThunderGames.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly PessoaRepository _repository;
    private readonly IPasswordHasher<Pessoa> _passwordHasher;
    private readonly TokenService _tokenService;

    public AuthController(
        PessoaRepository repository,
        IPasswordHasher<Pessoa> passwordHasher,
        TokenService tokenService)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Registrar(
        [FromBody] RegistroRequest request,
        CancellationToken cancellationToken)
    {
        var pessoa = new Pessoa
        {
            Nome = request.Nome.Trim(),
            CPF = request.Cpf.Trim(),
            Email = request.Email.Trim(),
            Telefone = request.Telefone.Trim(),
            Endereco = request.Endereco.Trim(),
            Perfil = "Cliente"
        };
        pessoa.Senha = _passwordHasher.HashPassword(pessoa, request.Senha);

        try
        {
            pessoa.UserId = await _repository.InserirAsync(pessoa, cancellationToken);
            return Ok(CriarResposta(pessoa));
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return Conflict(new { erro = "Já existe uma conta com este CPF ou e-mail." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Entrar(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var pessoa = await _repository.ObterPorEmailAsync(request.Email.Trim(), cancellationToken);
        if (pessoa is null ||
            _passwordHasher.VerifyHashedPassword(pessoa, pessoa.Senha, request.Senha)
                == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { erro = "E-mail ou senha incorretos." });
        }

        return Ok(CriarResposta(pessoa));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> AtualizarPerfil(
        [FromBody] AtualizarPerfilRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var nome = request.Nome.Trim();
        var telefone = request.Telefone.Trim();
        var endereco = request.Endereco.Trim();
        if (!await _repository.AtualizarPerfilAsync(userId, nome, telefone, endereco, cancellationToken))
        {
            return NotFound(new { erro = "Conta não encontrada." });
        }

        return Ok(new
        {
            usuario = new
            {
                userId,
                nome,
                telefone,
                endereco
            }
        });
    }

    private object CriarResposta(Pessoa pessoa) => new
    {
        token = _tokenService.CriarToken(pessoa),
        usuario = new
        {
            userId = pessoa.UserId,
            nome = pessoa.Nome,
            email = pessoa.Email,
            telefone = pessoa.Telefone,
            endereco = pessoa.Endereco
        }
    };

    public sealed class RegistroRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(14, MinimumLength = 11)]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter 11 dígitos.")]
        public string Cpf { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(128, MinimumLength = 8)]
        public string Senha { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Endereco { get; set; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(128, MinimumLength = 8)]
        public string Senha { get; set; } = string.Empty;
    }

    public sealed class AtualizarPerfilRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Telefone { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string Endereco { get; set; } = string.Empty;
    }
}
