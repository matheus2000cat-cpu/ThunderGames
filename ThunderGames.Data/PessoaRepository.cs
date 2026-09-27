using System.Data;
using Microsoft.Data.SqlClient;
using ThunderGames.Domain;

namespace ThunderGames.Data
{
    public class PessoaRepository
    {
        private readonly ConexaoBanco _Conexao = new ConexaoBanco ();
        public void Inserir (Pessoa pessoa)
        {
            using (var conn = _Conexao.ObterConexao())
            {
                using (var cmd = new SqlCommand("sp_InserirPessoa", conn))

                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nome", pessoa.Nome);
                    cmd.Parameters.AddWithValue("@CPF", pessoa.CPF);
                    cmd.Parameters.AddWithValue("@Email", pessoa.Email);
                    cmd.Parameters.AddWithValue("@Telefone", pessoa.Telefone);
                    cmd.Parameters.AddWithValue("@Endereco", pessoa.Endereco);
                    cmd.Parameters.AddWithValue("@Perfil", pessoa.Perfil);

                    conn.Open();
                    cmd.ExecuteNonQuery();

                }
            }

        }
    }
}