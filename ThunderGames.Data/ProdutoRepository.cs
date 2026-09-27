using System.Data;
using Microsoft.Data.SqlClient;
using ThunderGames.Domain;

namespace ThunderGames.Data
{
    public class ProdutoRepository
    {
        private readonly ConexaoBanco _conexao = new ConexaoBanco();

        public void Inserir(Produto produto)
        {
            using (var conn = _conexao.ObterConexao())
            {
                using (var cmd = new SqlCommand("sp_InserirProduto", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@NomeProduto", produto.NomeProduto);
                    cmd.Parameters.AddWithValue("@Descricao", produto.Descricao ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ValorUnit", produto.ValorUnit);
                    cmd.Parameters.AddWithValue("@QtdEstMinimo", produto.EstMinimo);
                    cmd.Parameters.AddWithValue("@Imagem", produto.Imagem ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@FK_IdCategoria", produto.FK_IdCategoria);
                    cmd.Parameters.AddWithValue("@FK_Pessoa_UserId", produto.Fk_PessoaUserId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}