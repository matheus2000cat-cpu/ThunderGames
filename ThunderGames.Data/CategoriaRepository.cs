using Npgsql;
using ThunderGames.Domain;

namespace ThunderGames.Data
{
    public class CategoriaRepository
    {
        private readonly ConexaoBanco _conexao;

        public CategoriaRepository(ConexaoBanco conexao)
        {
            _conexao = conexao;
        }

        public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT id_categoria, nome_categoria
                FROM public.categorias
                ORDER BY nome_categoria;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var categorias = new List<Categoria>();

            while (await reader.ReadAsync(cancellationToken))
            {
                categorias.Add(new Categoria
                {
                    IdCategoria = reader.GetInt32(0),
                    NomeCategoria = reader.GetString(1)
                });
            }

            return categorias;
        }
    }
}