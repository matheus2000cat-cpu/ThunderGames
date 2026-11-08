using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ThunderGames.Data;
using ThunderGames.Domain;

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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Criar(
            [FromBody] ProdutoRequest request,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return Unauthorized();
            }

            try
            {
                var productId = await _repository.CriarAsync(request.ToProduto(), userId, cancellationToken);
                return StatusCode(StatusCodes.Status201Created, new { idProduto = productId });
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return BadRequest(new { erro = "A categoria selecionada não existe." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Atualizar(
            int id,
            [FromBody] ProdutoRequest request,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest(new { erro = "Identificador de produto inválido." });
            }

            try
            {
                if (!await _repository.AtualizarAsync(id, request.ToProduto(), cancellationToken))
                {
                    return NotFound(new { erro = "Produto não encontrado." });
                }

                return NoContent();
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return BadRequest(new { erro = "A categoria selecionada não existe." });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Excluir(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest(new { erro = "Identificador de produto inválido." });
            }

            try
            {
                if (!await _repository.ExcluirAsync(id, cancellationToken))
                {
                    return NotFound(new { erro = "Produto não encontrado." });
                }

                return NoContent();
            }
            catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return Conflict(new { erro = "Este produto já aparece em pedidos e não pode ser removido." });
            }
        }

        public sealed class ProdutoRequest
        {
            [Required, StringLength(150, MinimumLength = 2)]
            public string NomeProduto { get; set; } = string.Empty;

            [StringLength(2000)]
            public string? Descricao { get; set; }

            [Range(typeof(decimal), "0.01", "999999999")]
            public decimal ValorUnit { get; set; }

            [Range(0, int.MaxValue)]
            public int EstMinimo { get; set; }

            [StringLength(1000)]
            public string? Imagem { get; set; }

            [Range(1, int.MaxValue)]
            public int FkIdCategoria { get; set; }

            public Produto ToProduto() => new()
            {
                NomeProduto = NomeProduto.Trim(),
                Descricao = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
                ValorUnit = ValorUnit,
                EstMinimo = EstMinimo,
                Imagem = string.IsNullOrWhiteSpace(Imagem) ? null : Imagem.Trim(),
                FK_IdCategoria = FkIdCategoria
            };
        }
    }
}