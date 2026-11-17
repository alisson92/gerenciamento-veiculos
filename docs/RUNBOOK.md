# Runbook – Sistema de Gerenciamento de Veículos (C# + SQL Server)

Aplicação console que realiza as quatro operações de CRUD (Cadastrar, Listar, Editar e Excluir)
de veículos — marca, modelo, placa e ano de fabricação — armazenados no SQL Server.

## 1. Estrutura do projeto

```
vehicle-management/
├── GerenciamentoVeiculos.sln             <- solução (abrir no Visual Studio)
├── GerenciamentoVeiculos/
│   ├── GerenciamentoVeiculos.csproj
│   ├── Program.cs                        <- menu e operações do CRUD
│   ├── Data/
│   │   ├── Conexao.cs                    <- string de conexão com o SQL Server
│   │   └── VeiculoRepository.cs          <- comandos SQL (INSERT, SELECT, UPDATE, DELETE)
│   └── Models/
│       └── Veiculo.cs                    <- classe que representa o veículo
├── database/
│   └── gerenciamento_veiculos.sql        <- cria o banco, a tabela e dados de exemplo
└── docs/
    └── RUNBOOK.md                        <- este arquivo
```

## 2. Pré-requisitos

| Item | Versão | Observação |
|------|--------|------------|
| Visual Studio | 2026 | Workload **"Desenvolvimento para desktop com .NET"** (já inclui o .NET 10 SDK) |
| .NET SDK | 10.0 ou superior | Só é necessário instalar à parte se for usar a linha de comando sem o Visual Studio – conferir com `dotnet --version` |
| SQL Server | 2017 ou superior | Express (recomendado), Developer ou container Docker |
| Cliente SQL | qualquer | SSMS, Azure Data Studio, extensão mssql do VS Code ou `sqlcmd` |
| Internet (só na primeira execução) | – | Para baixar o pacote NuGet `Microsoft.Data.SqlClient` |

## 3. Passo a passo

### Passo 1 – Extrair o projeto
Descompacte o arquivo `.zip` em qualquer pasta.

### Passo 2 – Verificar se o SQL Server está em execução
No Windows, abra `services.msc` e confirme que o serviço **SQL Server (SQLEXPRESS)** está *Em execução*.

> Usa a instância padrão (**SQL Server (MSSQLSERVER)**) ou Docker? Veja o Passo 4.

### Passo 3 – Criar o banco de dados
Execute o script `database/gerenciamento_veiculos.sql`. Ele pode ser executado mais de uma vez sem erro e cria:

* o banco `gerenciamento_veiculos`;
* a tabela `veiculo` (`codigo` gerado automaticamente, `marca`, `modelo`, `placa` sem repetição e `ano_fabricacao` entre 1900 e 2100);
* 3 veículos de exemplo (somente se a tabela estiver vazia).

**Pelo SSMS:** conecte em `.\SQLEXPRESS` (Autenticação do Windows), abra o arquivo (*File → Open → File*) e pressione **Execute (F5)**.

**Pelo `sqlcmd`** (a partir da pasta do projeto):
```bash
sqlcmd -S .\SQLEXPRESS -E -i database\gerenciamento_veiculos.sql
```
* `-S` servidor/instância · `-E` autenticação do Windows · `-i` arquivo de script a executar

### Passo 4 – Conferir a string de conexão
A conexão fica na classe `GerenciamentoVeiculos/Data/Conexao.cs`:

```csharp
private readonly string stringConexao =
    @"Server=.\SQLEXPRESS;Database=gerenciamento_veiculos;Integrated Security=True;TrustServerCertificate=True;";
```

Com SQL Server Express e autenticação do Windows **não é preciso alterar nada**. Em outros cenários, troque
apenas o início da string:

| Cenário | String de conexão |
|---------|-------------------|
| SQL Server Express (padrão) | `Server=.\SQLEXPRESS;Database=gerenciamento_veiculos;Integrated Security=True;TrustServerCertificate=True;` |
| Instância padrão (MSSQLSERVER) | `Server=localhost;Database=gerenciamento_veiculos;Integrated Security=True;TrustServerCertificate=True;` |
| Docker / usuário e senha | `Server=localhost,1433;Database=gerenciamento_veiculos;User Id=sa;Password=SuaSenha!123;TrustServerCertificate=True;` |

### Passo 5 – Executar a aplicação
**Visual Studio:** abra `GerenciamentoVeiculos.sln` e pressione **F5** (ou *Ctrl+F5* para executar sem depuração).

**Linha de comando** (na pasta onde está o `.sln`):
```bash
dotnet run --project GerenciamentoVeiculos
```

Se a conexão estiver correta, o menu principal será exibido:

```
=========================================
   SISTEMA DE GERENCIAMENTO DE VEÍCULOS
=========================================
1 - Cadastrar veículo
2 - Listar veículos
3 - Editar veículo
4 - Excluir veículo
0 - Sair
```

## 4. Roteiro de testes (valida todas as operações do CRUD)

Comece com o banco recém-criado pelo script (3 veículos de exemplo).

| # | Operação | Ação | Resultado esperado |
|---|----------|------|--------------------|
| 1 | **Listar** | Opção `2` | Tabela com os 3 veículos de exemplo e `Total: 3 veículo(s).` |
| 2 | **Cadastrar** | Opção `1` → Marca `Ford`, Modelo `Ka`, Placa `FOR1D99`, Ano `2019` | `Veículo cadastrado com sucesso! Código: 4` |
| 3 | **Listar** | Opção `2` | O Ford Ka aparece na lista (4 veículos) |
| 4 | **Cadastrar (placa repetida)** | Opção `1` → qualquer marca/modelo, Placa `for1d99`, Ano `2015` | `Erro: já existe um veículo cadastrado com esta placa.` (a placa é convertida para maiúsculas) |
| 5 | **Cadastrar (ano inválido)** | Opção `1` → Marca `Fiat`, Modelo `Uno`, Placa `FIA2B33`, Ano `1800`, depois `abc`, depois `2015` | Para `1800` e `abc`: `Informe um ano válido...` e o ano é pedido novamente; com `2015` o veículo é cadastrado |
| 6 | **Editar** | Opção `3` → Código `4` → ENTER em Marca, Modelo e Placa, Ano `2021` | `Veículo atualizado com sucesso!` |
| 7 | **Listar** | Opção `2` | O código 4 agora mostra o ano 2021 |
| 8 | **Editar (inexistente)** | Opção `3` → Código `999` | `Veículo não encontrado.` |
| 9 | **Excluir** | Opção `4` → Código `4` → `s` | `Veículo excluído com sucesso!` |
| 10 | **Excluir (cancelar)** | Opção `4` → Código `1` → `n` | `Operação cancelada.` |
| 11 | **Listar** | Opção `2` | O Ford Ka (código 4) não aparece mais; o Toyota (código 1) continua na lista |

> **Por que o Fiat do teste 5 recebeu o código 6 e não 5?** No SQL Server, a coluna `IDENTITY` consome o próximo
> número mesmo quando o `INSERT` falha (como no teste 4, de placa repetida). Pular números é o comportamento normal.

Também é possível conferir os dados direto no banco:
```sql
USE gerenciamento_veiculos;
SELECT * FROM veiculo;
```

## 5. Solução de problemas

| Sintoma | Causa provável / solução |
|---------|--------------------------|
| `Não foi possível conectar ao banco de dados` ao iniciar | Serviço do SQL Server parado, nome do servidor errado em `Conexao.cs` ou o script do Passo 3 não foi executado |
| `Cannot open database "gerenciamento_veiculos"` | Execute o script `database/gerenciamento_veiculos.sql` (Passo 3) |
| `A network-related or instance-specific error...` | A instância informada em `Server=` não existe ou está parada. Confira o nome em `services.msc` (Passo 4) |
| `Login failed for user` | Usuário/senha incorretos ou autenticação SQL não habilitada no servidor |
| Erro de certificado / SSL | Mantenha `TrustServerCertificate=True` na string de conexão (apenas para ambiente de desenvolvimento) |
| `Erro: já existe um veículo cadastrado com esta placa.` | Comportamento esperado: a coluna `placa` não permite repetição |

## 6. Observações sobre a implementação

* **ADO.NET com `Microsoft.Data.SqlClient`**, seguindo o exemplo da aula: classe `Conexao` com `AbrirConexao()`
  e a classe `VeiculoRepository` com os comandos SQL.
* **Comandos parametrizados** (`@marca`, `@placa`...) em todas as operações, o que evita SQL Injection.
* **Separação em pastas:** `Models` (dados), `Data` (acesso ao banco) e `Program.cs` (menu/console).
* **Validação em dois níveis:** no console (campos obrigatórios, tamanho máximo, ano entre 1900 e o próximo ano,
  placa só com letras, números e hífen) e no banco (`NOT NULL`, `UNIQUE` na placa e `CHECK` no ano).
