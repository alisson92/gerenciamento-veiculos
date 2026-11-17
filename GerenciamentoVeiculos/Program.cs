using Microsoft.Data.SqlClient;
using GerenciamentoVeiculos.Data;
using GerenciamentoVeiculos.Models;

VeiculoRepository repositorio = new VeiculoRepository();

// Antes de exibir o menu, verifica se o banco está acessível
// para mostrar uma mensagem clara caso algo esteja errado.
try
{
    repositorio.TestarConexao();
}
catch (SqlException ex)
{
    Console.WriteLine("Não foi possível conectar ao banco de dados.");
    Console.WriteLine($"Detalhes: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("Verifique: 1) se o serviço do SQL Server está em execução;");
    Console.WriteLine("           2) se o script database/gerenciamento_veiculos.sql foi executado;");
    Console.WriteLine("           3) se a string de conexão em Data/Conexao.cs está correta.");
    return;
}

string opcao = "";

while (opcao != "0")
{
    Console.Clear();
    Console.WriteLine("=========================================");
    Console.WriteLine("   SISTEMA DE GERENCIAMENTO DE VEÍCULOS");
    Console.WriteLine("=========================================");
    Console.WriteLine("1 - Cadastrar veículo");
    Console.WriteLine("2 - Listar veículos");
    Console.WriteLine("3 - Editar veículo");
    Console.WriteLine("4 - Excluir veículo");
    Console.WriteLine("0 - Sair");
    Console.Write("\nEscolha uma opção: ");

    opcao = LerLinha().Trim();
    Console.WriteLine();

    try
    {
        switch (opcao)
        {
            case "1": CadastrarVeiculo(); break;
            case "2": ListarVeiculos(); break;
            case "3": EditarVeiculo(); break;
            case "4": ExcluirVeiculo(); break;
            case "0": Console.WriteLine("Programa encerrado."); break;
            default: Console.WriteLine("Opção inválida."); break;
        }
    }
    catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
    {
        // 2601 e 2627 são os códigos do SQL Server para violação de UNIQUE (placa repetida).
        Console.WriteLine("Erro: já existe um veículo cadastrado com esta placa.");
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"Erro no banco de dados: {ex.Message}");
    }

    if (opcao != "0")
    {
        Pausar();
    }
}

// ===================== OPERAÇÕES DO CRUD =====================

void CadastrarVeiculo()
{
    Console.WriteLine("--- Cadastrar veículo ---");

    Veiculo veiculo = new Veiculo();
    veiculo.Marca = LerTexto("Marca (ex.: Toyota)", 50);
    veiculo.Modelo = LerTexto("Modelo (ex.: Corolla)", 50);
    veiculo.Placa = LerPlaca();
    veiculo.AnoFabricacao = LerAno();

    veiculo.Codigo = repositorio.Cadastrar(veiculo);
    Console.WriteLine($"\nVeículo cadastrado com sucesso! Código: {veiculo.Codigo}");
}

void ListarVeiculos()
{
    Console.WriteLine("--- Veículos cadastrados ---");

    List<Veiculo> veiculos = repositorio.Listar();

    if (veiculos.Count == 0)
    {
        Console.WriteLine("Nenhum veículo cadastrado.");
        return;
    }

    Console.WriteLine($"{"Código",-8}{"Marca",-18}{"Modelo",-20}{"Placa",-12}{"Ano",-6}");
    Console.WriteLine(new string('-', 64));

    foreach (Veiculo v in veiculos)
    {
        Console.WriteLine($"{v.Codigo,-8}{v.Marca,-18}{v.Modelo,-20}{v.Placa,-12}{v.AnoFabricacao,-6}");
    }

    Console.WriteLine($"\nTotal: {veiculos.Count} veículo(s).");
}

void EditarVeiculo()
{
    Console.WriteLine("--- Editar veículo ---");

    int? codigo = LerCodigo("Código do veículo a editar: ");
    if (codigo == null)
    {
        Console.WriteLine("Código inválido.");
        return;
    }

    Veiculo? veiculo = repositorio.BuscarPorCodigo(codigo.Value);
    if (veiculo == null)
    {
        Console.WriteLine("Veículo não encontrado.");
        return;
    }

    Console.WriteLine("Pressione ENTER para manter o valor atual mostrado entre [colchetes].\n");
    veiculo.Marca = LerTexto("Marca", 50, veiculo.Marca);
    veiculo.Modelo = LerTexto("Modelo", 50, veiculo.Modelo);
    veiculo.Placa = LerPlaca(veiculo.Placa);
    veiculo.AnoFabricacao = LerAno(veiculo.AnoFabricacao);

    if (repositorio.Atualizar(veiculo))
        Console.WriteLine("\nVeículo atualizado com sucesso!");
    else
        Console.WriteLine("\nNenhuma alteração foi realizada.");
}

void ExcluirVeiculo()
{
    Console.WriteLine("--- Excluir veículo ---");

    int? codigo = LerCodigo("Código do veículo a excluir: ");
    if (codigo == null)
    {
        Console.WriteLine("Código inválido.");
        return;
    }

    Veiculo? veiculo = repositorio.BuscarPorCodigo(codigo.Value);
    if (veiculo == null)
    {
        Console.WriteLine("Veículo não encontrado.");
        return;
    }

    Console.WriteLine($"Selecionado: {veiculo.Marca} {veiculo.Modelo} ({veiculo.Placa}, {veiculo.AnoFabricacao})");
    Console.Write("Tem certeza que deseja excluir este veículo? (s/n): ");
    string resposta = LerLinha().Trim().ToLower();

    if (resposta != "s" && resposta != "sim")
    {
        Console.WriteLine("Operação cancelada.");
        return;
    }

    if (repositorio.Excluir(codigo.Value))
        Console.WriteLine("\nVeículo excluído com sucesso!");
    else
        Console.WriteLine("\nNão foi possível excluir o veículo.");
}

// ===================== LEITURA E VALIDAÇÃO DOS DADOS =====================

// Lê uma linha do teclado. O Console.ReadLine() devolve null quando a entrada termina
// (Ctrl+Z no Windows ou Ctrl+D no Linux); sem esta verificação, os laços de validação
// abaixo pediriam o mesmo campo para sempre. Nesse caso o programa é encerrado.
string LerLinha()
{
    string? linha = Console.ReadLine();
    if (linha == null)
    {
        Console.WriteLine("\nEntrada encerrada. Saindo do programa.");
        Environment.Exit(0);
    }
    return linha;
}

// Lê um texto obrigatório com tamanho máximo.
// Na edição (valorAtual preenchido), ENTER mantém o valor atual.
string LerTexto(string rotulo, int tamanhoMaximo, string? valorAtual = null)
{
    while (true)
    {
        string dica = valorAtual == null ? "" : $" [{valorAtual}]";
        Console.Write($"{rotulo}{dica}: ");
        string texto = LerLinha().Trim();

        if (texto == "" && valorAtual != null)
            return valorAtual;

        if (texto == "")
            Console.WriteLine("  ! Campo obrigatório.");
        else if (texto.Length > tamanhoMaximo)
            Console.WriteLine($"  ! Máximo de {tamanhoMaximo} caracteres.");
        else
            return texto;
    }
}

// Lê a placa, convertendo para maiúsculas e aceitando apenas letras, números e hífen.
string LerPlaca(string? valorAtual = null)
{
    while (true)
    {
        string placa = LerTexto("Placa (ex.: ABC1D23 ou ABC-1234)", 10, valorAtual).ToUpper();

        if (placa.All(c => char.IsLetterOrDigit(c) || c == '-'))
            return placa;

        Console.WriteLine("  ! Use apenas letras, números e hífen.");
    }
}

// Lê o ano de fabricação, aceitando de 1900 até o ano que vem (modelos do próximo ano).
int LerAno(int? valorAtual = null)
{
    int anoMaximo = DateTime.Now.Year + 1;

    while (true)
    {
        string dica = valorAtual == null ? "" : $" [{valorAtual}]";
        Console.Write($"Ano de fabricação (1900-{anoMaximo}){dica}: ");
        string texto = LerLinha().Trim();

        if (texto == "" && valorAtual != null)
            return valorAtual.Value;

        if (int.TryParse(texto, out int ano) && ano >= 1900 && ano <= anoMaximo)
            return ano;

        Console.WriteLine($"  ! Informe um ano válido entre 1900 e {anoMaximo}.");
    }
}

// Lê um código numérico; retorna null se o valor digitado não for um número.
int? LerCodigo(string mensagem)
{
    Console.Write(mensagem);
    if (int.TryParse(LerLinha(), out int codigo))
        return codigo;
    return null;
}

void Pausar()
{
    Console.WriteLine();
    Console.Write("Pressione ENTER para continuar...");
    LerLinha();
}
