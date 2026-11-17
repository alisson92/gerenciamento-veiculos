# CLAUDE.md — Sistema de Gerenciamento de Veículos

## Contexto
Trabalho prático da FATEC (disciplina de POO): **aplicação console em C# + SQL Server** com CRUD completo
de veículos (marca, modelo, placa, ano de fabricação), adaptada do exemplo feito em aula.

Entrega exigida (um `.zip`):
1. Solução C# completa (`.sln`, `.csproj`, código-fonte) — **não** só o executável.
2. Script `.sql` que cria o banco e as tabelas.
3. Runbook explicando como executar e testar.
4. Todas as operações CRUD testadas antes da entrega.

## Diretriz principal: seguir o exemplo da aula
O professor espera algo **próximo do que foi ensinado** (projeto de referência em `reference-class-project/ProjetoBanco/`).
Antes de introduzir qualquer padrão novo (DI, appsettings, ORM, camadas extras, interfaces...), confira se ele existe
no exemplo da aula; se não existir, **não introduza** sem perguntar. Padrões da aula seguidos aqui:
- Tudo em **português** (classes, métodos, variáveis, mensagens, SQL, runbook).
- Classe `Data/Conexao.cs` com a string de conexão **fixa** e o método `AbrirConexao()`.
- `XxxRepository` com `private readonly Conexao conexao = new Conexao();` e métodos com
  `using var conn = conexao.AbrirConexao(); conn.Open();` + `SqlCommand` + `Parameters.AddWithValue`.
- `namespace X { }` em bloco, tipos explícitos (`Veiculo veiculo = new Veiculo();`), propriedades `string X { get; set; } = "";`.
- Chave primária chamada `Codigo`/`codigo`; tabela e colunas em minúsculo (`veiculo`, `ano_fabricacao`).
- `Program.cs` com *top-level statements* (menu + funções locais), sem pasta `UI`.

Diferença intencional da aula: o projeto da aula usa `.slnx`, mas o enunciado pede **`.sln`** → mantido `.sln`.

## Stack
- .NET 10 (`net10.0`), console app, `Nullable` e `ImplicitUsings` habilitados
- ADO.NET puro com `Microsoft.Data.SqlClient` 7.1.1 (mesma versão da aula) — sem ORM
- `System.Security.Cryptography.Pkcs` 10.0.15 fixado no `.csproj`: dependência transitiva do SqlClient 7.1.1
  (que traria a 9.0.18, com CVE-2026-70162/84008). Ao atualizar o SqlClient, reavalie se o pin ainda é necessário.
- SQL Server 2017+ (testado com `mcr.microsoft.com/mssql/server:2022-CU15-ubuntu-22.04`)

## Estrutura
```
GerenciamentoVeiculos.sln
GerenciamentoVeiculos/
  GerenciamentoVeiculos.csproj
  Program.cs                       # menu, operações do CRUD e leitura/validação do console (funções locais)
  Data/Conexao.cs                  # string de conexão fixa: .\SQLEXPRESS + Integrated Security
  Data/VeiculoRepository.cs        # Cadastrar, Listar, BuscarPorCodigo, Atualizar, Excluir, TestarConexao
  Models/Veiculo.cs                # Codigo, Marca, Modelo, Placa, AnoFabricacao
database/gerenciamento_veiculos.sql  # cria DB gerenciamento_veiculos + tabela veiculo + 3 exemplos (idempotente)
docs/RUNBOOK.md                    # passo a passo + roteiro de testes CRUD (11 casos)
```

## Convenções e decisões
- **Comandos SQL sempre parametrizados**, nunca concatenação de string.
- **Validação em dois níveis:** console (obrigatórios, tamanho, ano 1900..ano+1, placa só letras/números/hífen,
  convertida para maiúsculas) e banco (`NOT NULL`, `UNIQUE` na placa, `CHECK` no ano 1900–2100).
- Placa repetida: captura `SqlException` números **2601/2627** no laço do menu.
- **Toda leitura de console passa por `LerLinha()`** (em `Program.cs`), que encerra o app no EOF
  (`Console.ReadLine()` retorna `null`). Não use `Console.ReadLine()` direto — causa loop infinito em EOF.
- Confirmação de exclusão: `s`/`sim`.
- `IDENTITY` pula números após INSERT com erro (ex.: placa repetida) — comportamento normal, explicado no runbook.

## Como testar localmente (WSL, sem dotnet instalado — via Docker)
A string de conexão é fixa em `Conexao.cs` (Windows Auth + SQLEXPRESS), então o teste faz `sed` **apenas na cópia
dentro do container** trocando para o SQL Server do Docker. Nunca altere `Conexao.cs` no repositório para testar.
```bash
docker network create vm-test
docker run -d --name vm-sql --network vm-test -e ACCEPT_EULA=Y -e 'MSSQL_SA_PASSWORD=Test!Passw0rd123' \
  mcr.microsoft.com/mssql/server:2022-CU15-ubuntu-22.04
# após ~20s:
docker cp database/gerenciamento_veiculos.sql vm-sql:/tmp/s.sql && \
docker exec vm-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Test!Passw0rd123' -C -i /tmp/s.sql
docker run -it --rm --network vm-test -v "$PWD":/src:ro mcr.microsoft.com/dotnet/sdk:10.0 bash -c '
  mkdir /work && cp -r /src/GerenciamentoVeiculos /src/GerenciamentoVeiculos.sln /work/ && cd /work &&
  sed -i "s#Server=\.\\\\SQLEXPRESS#Server=vm-sql,1433#; s#Integrated Security=True#User Id=sa;Password=Test!Passw0rd123#" GerenciamentoVeiculos/Data/Conexao.cs &&
  dotnet run --project GerenciamentoVeiculos'
# limpeza
docker rm -f vm-sql && docker network rm vm-test
```
- Montar o projeto `:ro` e copiar só o projeto + `.sln` para `/work` evita `bin/`/`obj/` de root no repo
  e deixa de fora `reference-class-project/` (outro `Program.cs`, quebraria o build).
- Testes automatizados: alimentar o menu via stdin (`printf '%s\n' 2 "" 0 | docker run -i ...`).
- Senha `Test!Passw0rd123` é apenas para o container local descartável.

## Status (2026-11-17)
- Projeto reescrito em português no padrão da aula. Build (.NET SDK 10.0.404): 0 warnings / 0 errors;
  `dotnet list package --vulnerable` limpo; script SQL idempotente; roteiro de 11 testes do runbook OK; EOF OK.

## Repositório
- GitHub: https://github.com/alisson92/vehicle-management (branch `main`, Conventional Commits)

## Decisões confirmadas pelo usuário
- O professor testa no **Windows** com **Visual Studio 2026 / .NET 10** e **SQL Server Express** (`Server=.\SQLEXPRESS`,
  `Integrated Security=True`), conforme o projeto da aula.
- Idioma da entrega: **português** (o professor espera português).
- Senha `sa` em texto puro no runbook é aceitável (contexto acadêmico) — não sinalizar novamente.

## Projeto de referência da aula
- Em `reference-class-project/ProjetoBanco/` (cadastro de `Aluno`, só Create), apenas para comparação.
  A pasta está no `.gitignore` e **não deve ser commitada nem entrar no `.zip`**.

## Pendências para a entrega
- Gerar o `.zip` com `git archive` (só arquivos versionados: sem `bin/`, `obj/`, `.vs/`, `.remember/`, `reference-class-project/`).
