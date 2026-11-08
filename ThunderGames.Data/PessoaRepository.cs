using Npgsql;
using ThunderGames.Domain;

namespace ThunderGames.Data
{
    public class PessoaRepository
    {
        private readonly ConexaoBanco _conexao;

        public PessoaRepository(ConexaoBanco conexao)
        {
            _conexao = conexao;
        }

        public async Task<Pessoa?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT user_id, nome, cpf, email, senha, telefone, endereco, perfil
                FROM public.pessoas
                WHERE lower(email) = lower(@email)
                LIMIT 1;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("email", email);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new Pessoa
            {
                UserId = reader.GetInt32(0),
                Nome = reader.GetString(1),
                CPF = reader.GetString(2),
                Email = reader.GetString(3),
                Senha = reader.GetString(4),
                Telefone = reader.GetString(5),
                Endereco = reader.GetString(6),
                Perfil = reader.GetString(7)
            };
        }

        public async Task<int> InserirAsync(Pessoa pessoa, CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                INSERT INTO public.pessoas (nome, cpf, email, senha, telefone, endereco, perfil)
                VALUES (@nome, @cpf, @email, @senha, @telefone, @endereco, @perfil)
                RETURNING user_id;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nome", pessoa.Nome);
            command.Parameters.AddWithValue("cpf", pessoa.CPF);
            command.Parameters.AddWithValue("email", pessoa.Email);
            command.Parameters.AddWithValue("senha", pessoa.Senha);
            command.Parameters.AddWithValue("telefone", pessoa.Telefone);
            command.Parameters.AddWithValue("endereco", pessoa.Endereco);
            command.Parameters.AddWithValue("perfil", pessoa.Perfil);

            return (int)(await command.ExecuteScalarAsync(cancellationToken)
                ?? throw new InvalidOperationException("O banco não retornou o identificador da pessoa cadastrada."));
        }

        public async Task<bool> AtualizarPerfilAsync(
            int userId,
            string nome,
            string telefone,
            string endereco,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                UPDATE public.pessoas
                SET nome = @nome,
                    telefone = @telefone,
                    endereco = @endereco
                WHERE user_id = @user_id;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("user_id", userId);
            command.Parameters.AddWithValue("nome", nome);
            command.Parameters.AddWithValue("telefone", telefone);
            command.Parameters.AddWithValue("endereco", endereco);

            return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        }

        public async Task<Pessoa?> ObterPorIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                SELECT user_id, nome, cpf, email, senha, telefone, endereco, perfil
                FROM public.pessoas
                WHERE user_id = @user_id
                LIMIT 1;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("user_id", userId);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new Pessoa
            {
                UserId = reader.GetInt32(0),
                Nome = reader.GetString(1),
                CPF = reader.GetString(2),
                Email = reader.GetString(3),
                Senha = reader.GetString(4),
                Telefone = reader.GetString(5),
                Endereco = reader.GetString(6),
                Perfil = reader.GetString(7)
            };
        }

        public async Task<bool> AtualizarSenhaAsync(
            int userId,
            string senhaHashAtual,
            string novaSenhaHash,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _conexao.ObterConexao();
            await connection.OpenAsync(cancellationToken);

            const string sql = """
                UPDATE public.pessoas
                SET senha = @nova_senha
                WHERE user_id = @user_id
                  AND senha = @senha_atual;
                """;

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("user_id", userId);
            command.Parameters.AddWithValue("senha_atual", senhaHashAtual);
            command.Parameters.AddWithValue("nova_senha", novaSenhaHash);

            return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
        }
    }
}
