using Microsoft.Data.SqlClient;

namespace GerenciamentoVeiculos.Data
{
    public class Conexao
    {
        // Instância SQL Server Express local com autenticação do Windows.
        // Se o seu SQL Server usar outra instância, altere apenas o "Server"
        // (ex.: "localhost" para a instância padrão MSSQLSERVER).
        private readonly string stringConexao =
            @"Server=.\SQLEXPRESS;Database=gerenciamento_veiculos;Integrated Security=True;TrustServerCertificate=True;";

        public SqlConnection AbrirConexao()
        {
            return new SqlConnection(stringConexao);
        }
    }
}
