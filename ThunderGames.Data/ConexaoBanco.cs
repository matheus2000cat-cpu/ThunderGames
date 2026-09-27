using Microsoft.Data.SqlClient;
namespace ThunderGames.Data
{
    public class ConexaoBanco
    {
        private readonly string _connectionString = @"Server=DESKTOP-ST30F4I\SQLEXPRESS;Database=bd_thunder;Trusted_Connection=True;TrustServerCertificate=True;";
        public SqlConnection ObterConexao()
        {
            return new SqlConnection(_connectionString);
        }
    }
}