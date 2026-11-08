namespace ThunderGames.Domain;

public sealed record PedidoResumo(
    long IdPedido,
    DateTime DataPedido,
    decimal Total,
    string Status,
    string TipoEntrega,
    string FormaPagamento,
    string Itens);
