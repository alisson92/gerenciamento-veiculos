namespace GerenciamentoVeiculos.Models
{
    // Representa um registro da tabela "veiculo".
    public class Veiculo
    {
        public int Codigo { get; set; }
        public string Marca { get; set; } = "";
        public string Modelo { get; set; } = "";
        public string Placa { get; set; } = "";
        public int AnoFabricacao { get; set; }
    }
}
