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
    }
}
