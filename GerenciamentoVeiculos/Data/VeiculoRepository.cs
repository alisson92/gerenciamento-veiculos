using Microsoft.Data.SqlClient;
using GerenciamentoVeiculos.Models;

namespace GerenciamentoVeiculos.Data
{
    // Todo o acesso ao banco fica nesta classe.
    // Os comandos usam parâmetros (@marca, @placa...) em vez de concatenar texto,
    // o que evita SQL Injection.
    public class VeiculoRepository
    {
        private readonly Conexao conexao = new Conexao();

        // Abre e fecha uma conexão apenas para verificar se o banco está acessível.
        public void TestarConexao()
        {
            using var conn = conexao.AbrirConexao();
            conn.Open();
        }

        // CREATE - insere um veículo e devolve o código gerado pelo banco.
        public int Cadastrar(Veiculo veiculo)
        {
            using var conn = conexao.AbrirConexao();
            conn.Open();

            string sql = @"INSERT INTO veiculo
                           (marca, modelo, placa, ano_fabricacao)
                           OUTPUT INSERTED.codigo
                           VALUES (@marca, @modelo, @placa, @ano_fabricacao)";

            using var comando = new SqlCommand(sql, conn);

            comando.Parameters.AddWithValue("@marca", veiculo.Marca);
            comando.Parameters.AddWithValue("@modelo", veiculo.Modelo);
            comando.Parameters.AddWithValue("@placa", veiculo.Placa);
            comando.Parameters.AddWithValue("@ano_fabricacao", veiculo.AnoFabricacao);

            return (int)comando.ExecuteScalar()!;
        }

        // READ - retorna todos os veículos cadastrados.
        public List<Veiculo> Listar()
        {
            List<Veiculo> veiculos = new List<Veiculo>();

            using var conn = conexao.AbrirConexao();
            conn.Open();

            string sql = @"SELECT codigo, marca, modelo, placa, ano_fabricacao
                           FROM veiculo
                           ORDER BY codigo";

            using var comando = new SqlCommand(sql, conn);
            using var leitor = comando.ExecuteReader();

            while (leitor.Read())
            {
                veiculos.Add(LerVeiculo(leitor));
            }

            return veiculos;
        }

        // READ - busca um veículo pelo código (retorna null se não existir).
        public Veiculo? BuscarPorCodigo(int codigo)
        {
            using var conn = conexao.AbrirConexao();
            conn.Open();

            string sql = @"SELECT codigo, marca, modelo, placa, ano_fabricacao
                           FROM veiculo
                           WHERE codigo = @codigo";

            using var comando = new SqlCommand(sql, conn);
            comando.Parameters.AddWithValue("@codigo", codigo);

            using var leitor = comando.ExecuteReader();

            if (leitor.Read())
            {
                return LerVeiculo(leitor);
            }

            return null;
        }

        // UPDATE - altera os dados de um veículo. Retorna true se algum registro foi alterado.
        public bool Atualizar(Veiculo veiculo)
        {
            using var conn = conexao.AbrirConexao();
            conn.Open();

            string sql = @"UPDATE veiculo
                           SET marca = @marca,
                               modelo = @modelo,
                               placa = @placa,
                               ano_fabricacao = @ano_fabricacao
                           WHERE codigo = @codigo";

            using var comando = new SqlCommand(sql, conn);

            comando.Parameters.AddWithValue("@marca", veiculo.Marca);
            comando.Parameters.AddWithValue("@modelo", veiculo.Modelo);
            comando.Parameters.AddWithValue("@placa", veiculo.Placa);
            comando.Parameters.AddWithValue("@ano_fabricacao", veiculo.AnoFabricacao);
            comando.Parameters.AddWithValue("@codigo", veiculo.Codigo);

            return comando.ExecuteNonQuery() > 0;
        }

        // DELETE - remove um veículo pelo código. Retorna true se algum registro foi removido.
        public bool Excluir(int codigo)
        {
            using var conn = conexao.AbrirConexao();
            conn.Open();

            string sql = "DELETE FROM veiculo WHERE codigo = @codigo";

            using var comando = new SqlCommand(sql, conn);
            comando.Parameters.AddWithValue("@codigo", codigo);

            return comando.ExecuteNonQuery() > 0;
        }

        // Converte a linha atual do resultado da consulta em um objeto Veiculo.
        private static Veiculo LerVeiculo(SqlDataReader leitor)
        {
            Veiculo veiculo = new Veiculo();
            veiculo.Codigo = leitor.GetInt32(0);
            veiculo.Marca = leitor.GetString(1);
            veiculo.Modelo = leitor.GetString(2);
            veiculo.Placa = leitor.GetString(3);
            veiculo.AnoFabricacao = leitor.GetInt32(4);
            return veiculo;
        }
    }
}
