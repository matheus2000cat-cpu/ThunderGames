using System.Data;
using Microsoft.Data.SqlClient;
using ThunderGames.Domain;

namespace ThunderGames.Data
{
    public class CategoriaRepository
    {
        private readonly ConexaoBanco _conexao = new ConexaoBanco();

        public void Inserir(Categoria categoria)
        {
            using (var conn = _conexao.ObterConexao())
            {
                using (var cmd = new SqlCommand("sp_InserirCategoria", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NomeCategoria", categoria.NomeCategoria);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}