# Sistema de Gerenciamento de Veículos

Aplicação console em **C#** com banco de dados **SQL Server** que realiza o CRUD completo de veículos
(marca, modelo, placa e ano de fabricação).

Trabalho prático da disciplina de **Programação Orientada a Objetos** — FATEC.

## Funcionalidades

| Opção do menu | Operação | Descrição |
|---------------|----------|-----------|
| `1` | **Cadastrar** (Create) | Inclui um novo veículo; o código é gerado automaticamente pelo banco |
| `2` | **Listar** (Read) | Exibe todos os veículos cadastrados em formato de tabela |
| `3` | **Editar** (Update) | Altera os dados de um veículo; ENTER mantém o valor atual |
| `4` | **Excluir** (Delete) | Remove um veículo após confirmação (`s`/`n`) |
| `0` | Sair | Encerra o programa |

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

## Tecnologias

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| C# / .NET | 10 | Aplicação console |
| Microsoft.Data.SqlClient | 7.1.1 | Acesso ao banco com ADO.NET (sem ORM) |
| SQL Server | 2017 ou superior | Banco de dados (testado com SQL Server Express e 2022) |
| Visual Studio | 2026 | IDE recomendada |

## Estrutura do projeto

```
├── GerenciamentoVeiculos.sln             <- solução (abrir no Visual Studio)
├── GerenciamentoVeiculos/
│   ├── Program.cs                        <- menu, operações do CRUD e validação das entradas
│   ├── Data/
│   │   ├── Conexao.cs                    <- string de conexão com o SQL Server
│   │   └── VeiculoRepository.cs          <- comandos SQL (INSERT, SELECT, UPDATE, DELETE)
│   └── Models/
│       └── Veiculo.cs                    <- classe que representa o veículo
├── database/
│   └── gerenciamento_veiculos.sql        <- cria o banco, a tabela e dados de exemplo
└── docs/
    └── RUNBOOK.md                        <- guia completo de execução e roteiro de testes
```

## Como executar (resumo)

1. Execute o script `database/gerenciamento_veiculos.sql` no SQL Server (SSMS ou `sqlcmd`).
   Ele cria o banco `gerenciamento_veiculos`, a tabela `veiculo` e 3 veículos de exemplo.
2. Confira a string de conexão em `GerenciamentoVeiculos/Data/Conexao.cs`
   (padrão: `.\SQLEXPRESS` com autenticação do Windows — não precisa alterar nesse cenário).
3. Abra `GerenciamentoVeiculos.sln` no Visual Studio e pressione **F5**
   (ou, pela linha de comando: `dotnet run --project GerenciamentoVeiculos`).

O passo a passo detalhado, outras configurações de conexão, o **roteiro de testes do CRUD** e a solução de
problemas comuns estão no **[Runbook](docs/RUNBOOK.md)**.

## Banco de dados

Tabela `veiculo`:

| Coluna | Tipo | Regra |
|--------|------|-------|
| `codigo` | `INT IDENTITY` | Chave primária, gerada automaticamente |
| `marca` | `NVARCHAR(50)` | Obrigatória |
| `modelo` | `NVARCHAR(50)` | Obrigatório |
| `placa` | `NVARCHAR(10)` | Obrigatória e única (`UNIQUE`) |
| `ano_fabricacao` | `INT` | Obrigatório, entre 1900 e 2100 (`CHECK`) |

O script pode ser executado mais de uma vez sem erro: ele não recria o banco nem duplica os dados de exemplo.

## Destaques da implementação

* **Organização em camadas simples:** `Models` (dados), `Data` (acesso ao banco) e `Program.cs` (interface com o usuário).
* **Comandos SQL parametrizados** (`@marca`, `@placa`...) em todas as operações, o que evita SQL Injection.
* **Validação em dois níveis:**
  * no console: campos obrigatórios, tamanho máximo, ano entre 1900 e o próximo ano,
    placa só com letras, números e hífen (convertida para maiúsculas);
  * no banco: `NOT NULL`, `UNIQUE` na placa e `CHECK` no ano.
* **Tratamento de erros:** placa repetida e falhas de conexão exibem mensagens claras, sem encerrar o programa
  de forma inesperada.

## Licença

Distribuído sob a licença MIT. Veja o arquivo [LICENSE](LICENSE).
