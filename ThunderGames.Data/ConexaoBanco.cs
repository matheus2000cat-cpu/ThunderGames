using Npgsql;

namespace ThunderGames.Data
{
    public class ConexaoBanco
    {
        private readonly string? _connectionString;

        public ConexaoBanco(string? connectionString)
        {
            _connectionString = connectionString;
        }

        public NpgsqlConnection ObterConexao()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException(
                    "Configure a connection string 'ConnectionStrings:Supabase' para acessar o banco PostgreSQL.");
            }

            var builder = new NpgsqlConnectionStringBuilder(_connectionString)
            {
                SslMode = SslMode.Require
            };

            return new NpgsqlConnection(builder.ConnectionString);
        }
    }
}