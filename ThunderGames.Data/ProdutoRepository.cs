using Npgsql;
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
    }
}
