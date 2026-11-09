using Npgsql;
using NpgsqlTypes;
using ThunderGames.Domain;

namespace ThunderGames.Data;

public sealed class PedidoRepository
{
    private readonly ConexaoBanco _conexao;

    public PedidoRepository(ConexaoBanco conexao)
    {
        _conexao = conexao;
    }

    public async Task<(long IdPedido, decimal Total, string EnderecoEntrega)> CriarAsync(
        int userId,
        string tipoEntrega,
        string formaPagamento,
        IReadOnlyList<(int IdProduto, int Quantidade)> itens,
        string? enderecoSolicitado,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _conexao.ObterConexao();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var ids = itens.Select(item => item.IdProduto).Distinct().ToArray();
        const string productSql = """
            SELECT id_produto, nome_produto, valor_unit
            FROM public.produtos
            WHERE id_produto = ANY(@ids)
            ORDER BY id_produto
            FOR SHARE;
            """;

        var products = new Dictionary<int, (string Nome, decimal Valor)>();
        await using (var command = new NpgsqlCommand(productSql, connection, transaction))
        {
            command.Parameters.AddWithValue("ids", ids);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                products.Add(reader.GetInt32(0), (reader.GetString(1), reader.GetDecimal(2)));
            }
        }

        if (products.Count != ids.Length)
        {
            throw new InvalidOperationException("Um ou mais produtos não foram encontrados.");
        }

        var total = itens.Sum(item => products[item.IdProduto].Valor * item.Quantidade);
        const string orderSql = """
            INSERT INTO public.pedidos
                (fk_pessoa_user_id, tipo_entrega, endereco_entrega, forma_pagamento, total, status)
            SELECT user_id,
                   @tipo_entrega,
                   CASE
                       WHEN @tipo_entrega = 'entrega'
                           THEN COALESCE(NULLIF(@endereco_entrega, ''), endereco)
                       ELSE ''
                   END,
                   @forma_pagamento,
                   @total,
                   'registrado'
            FROM public.pessoas
            WHERE user_id = @user_id
            RETURNING id_pedido, endereco_entrega;
            """;

        long idPedido;
        string enderecoEntrega;
        await using (var command = new NpgsqlCommand(orderSql, connection, transaction))
        {
            command.Parameters.AddWithValue("user_id", userId);
            command.Parameters.AddWithValue("tipo_entrega", tipoEntrega);
            command.Parameters.AddWithValue("forma_pagamento", formaPagamento);
            command.Parameters.AddWithValue("total", total);
            command.Parameters.Add(new NpgsqlParameter("endereco_entrega", NpgsqlDbType.Text)
            {
                Value = (object?)enderecoSolicitado ?? DBNull.Value
            });
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                throw new InvalidOperationException("A conta autenticada não foi encontrada.");
            }

            idPedido = reader.GetInt64(0);
            enderecoEntrega = reader.GetString(1);
        }

        const string itemSql = """
            INSERT INTO public.itens_pedido
                (fk_id_pedido, fk_id_produto, nome_produto, quantidade, valor_unitario)
            VALUES (@pedido_id, @produto_id, @nome, @quantidade, @valor_unitario);
            """;

        foreach (var item in itens)
        {
            var product = products[item.IdProduto];
            await using var command = new NpgsqlCommand(itemSql, connection, transaction);
            command.Parameters.AddWithValue("pedido_id", idPedido);
            command.Parameters.AddWithValue("produto_id", item.IdProduto);
            command.Parameters.AddWithValue("nome", product.Nome);
            command.Parameters.AddWithValue("quantidade", item.Quantidade);
            command.Parameters.AddWithValue("valor_unitario", product.Valor);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return (idPedido, total, enderecoEntrega);
    }

    public async Task<IReadOnlyList<PedidoResumo>> ListarPorUsuarioAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _conexao.ObterConexao();
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT p.id_pedido,
                   p.data_pedido,
                   p.total,
                   p.status,
                   p.tipo_entrega,
                   p.forma_pagamento,
                   string_agg(i.quantidade::text || 'x ' || i.nome_produto, ', ' ORDER BY i.id_item_pedido)
            FROM public.pedidos AS p
            INNER JOIN public.itens_pedido AS i ON i.fk_id_pedido = p.id_pedido
            WHERE p.fk_pessoa_user_id = @user_id
            GROUP BY p.id_pedido
            ORDER BY p.data_pedido DESC;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("user_id", userId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var orders = new List<PedidoResumo>();
        while (await reader.ReadAsync(cancellationToken))
        {
            orders.Add(new PedidoResumo(
                reader.GetInt64(0),
                reader.GetDateTime(1),
                reader.GetDecimal(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6)));
        }

        return orders;
    }
}
