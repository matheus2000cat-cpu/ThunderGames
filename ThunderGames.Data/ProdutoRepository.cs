using Npgsql;
using NpgsqlTypes;
using ThunderGames.Domain;

namespace ThunderGames.Data
{
    public class ProdutoRepository
    {
        private readonly ConexaoBanco _conexao;

        public ProdutoRepository(ConexaoBanco conexao)
        {
            _conexao = conexao;
        }

        public async Task<IReadOnlyList<Produto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT p.id_produto,
                       p.nome_produto,
                       p.descricao,
                       p.valor_unit,
                       p.est_minimo,
                       p.imagem,
                       p.fk_id_categoria,
                       p.fk_pessoa_user_id,
                       c.nome_categoria
                FROM public.produtos AS p
                INNER JOIN public.categorias AS c ON c.id_categoria = p.fk_id_categoria
                ORDER BY p.nome_produto;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var produtos = new List<Produto>();

            while (await reader.ReadAsync(cancellationToken))
            {
                produtos.Add(new Produto
                {
                    IdProduto = reader.GetInt32(0),
                    NomeProduto = reader.GetString(1),
                    Descricao = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ValorUnit = reader.GetDecimal(3),
                    EstMinimo = reader.GetInt32(4),
                    Imagem = reader.IsDBNull(5) ? null : reader.GetString(5),
                    FK_IdCategoria = reader.GetInt32(6),
                    Fk_PessoaUserId = reader.GetInt32(7),
                    NomeCategoria = reader.GetString(8)
                });
            }

            return produtos;
        }

        public async Task<int> CriarAsync(
            Produto produto,
            int userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                INSERT INTO public.produtos (
                    nome_produto,
                    descricao,
                    valor_unit,
                    est_minimo,
                    imagem,
                    fk_id_categoria,
                    fk_pessoa_user_id
                )
                VALUES (
                    @nome_produto,
                    @descricao,
                    @valor_unit,
                    @est_minimo,
                    @imagem,
                    @fk_id_categoria,
                    @fk_pessoa_user_id
                )
                RETURNING id_produto;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nome_produto", produto.NomeProduto);
            command.Parameters.AddWithValue("descricao", NpgsqlDbType.Text, (object?)produto.Descricao ?? DBNull.Value);
            command.Parameters.AddWithValue("valor_unit", produto.ValorUnit);
            command.Parameters.AddWithValue("est_minimo", produto.EstMinimo);
            command.Parameters.AddWithValue("imagem", NpgsqlDbType.Text, (object?)produto.Imagem ?? DBNull.Value);
            command.Parameters.AddWithValue("fk_id_categoria", produto.FK_IdCategoria);
            command.Parameters.AddWithValue("fk_pessoa_user_id", userId);

            return (int)(await command.ExecuteScalarAsync(cancellationToken)
                ?? throw new InvalidOperationException("O banco não retornou o identificador do produto."));
        }

        public async Task<bool> AtualizarAsync(
            int productId,
            Produto produto,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                UPDATE public.produtos
                SET nome_produto = @nome_produto,
                    descricao = @descricao,
                    valor_unit = @valor_unit,
                    est_minimo = @est_minimo,
                    imagem = @imagem,
                    fk_id_categoria = @fk_id_categoria
                WHERE id_produto = @id_produto;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id_produto", productId);
            command.Parameters.AddWithValue("nome_produto", produto.NomeProduto);
            command.Parameters.AddWithValue("descricao", NpgsqlDbType.Text, (object?)produto.Descricao ?? DBNull.Value);
            command.Parameters.AddWithValue("valor_unit", produto.ValorUnit);
            command.Parameters.AddWithValue("est_minimo", produto.EstMinimo);
            command.Parameters.AddWithValue("imagem", NpgsqlDbType.Text, (object?)produto.Imagem ?? DBNull.Value);
            command.Parameters.AddWithValue("fk_id_categoria", produto.FK_IdCategoria);

            return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        }

        public async Task<bool> ExcluirAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = "DELETE FROM public.produtos WHERE id_produto = @id_produto;";
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id_produto", productId);

            return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        }
    }
}
