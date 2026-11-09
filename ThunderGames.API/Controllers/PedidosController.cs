using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThunderGames.Data;

namespace ThunderGames.API.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize]
public sealed class PedidosController : ControllerBase
{
    private readonly PedidoRepository _repository;

    public PedidosController(PedidoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var orders = await _repository.ListarPorUsuarioAsync(userId, cancellationToken);
        return Ok(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Criar(
        [FromBody] CriarPedidoRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        if (request.Itens.Count is < 1 or > 50 ||
            request.Itens.Any(item => item.IdProduto <= 0 || item.Quantidade is < 1 or > 20) ||
            request.Itens.Select(item => item.IdProduto).Distinct().Count() != request.Itens.Count)
        {
            return BadRequest(new { erro = "Confira os produtos e as quantidades do carrinho." });
        }

        if (request.TipoEntrega is not ("entrega" or "retirar") ||
            request.FormaPagamento is not ("pix" or "debito" or "credito"))
        {
            return BadRequest(new { erro = "Forma de entrega ou pagamento inválida." });
        }

        try
        {
            var order = await _repository.CriarAsync(
                userId,
                request.TipoEntrega,
                request.FormaPagamento,
                request.Itens.Select(item => (item.IdProduto, item.Quantidade)).ToArray(),
                request.EnderecoEntrega,
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, new
            {
                idPedido = order.IdPedido,
                total = order.Total,
                enderecoEntrega = order.EnderecoEntrega,
                status = "registrado"
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { erro = exception.Message });
        }
    }

    public sealed class CriarPedidoRequest
    {
        [Required, MinLength(1)]
        public List<ItemPedidoRequest> Itens { get; set; } = [];

        [Required]
        public string TipoEntrega { get; set; } = string.Empty;

        [Required]
        public string FormaPagamento { get; set; } = string.Empty;

        [StringLength(500)]
        public string? EnderecoEntrega { get; set; }
    }

    public sealed class ItemPedidoRequest
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
    }
}
