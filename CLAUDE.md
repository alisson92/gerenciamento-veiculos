# CLAUDE.md — Vehicle Management System

## Contexto
Trabalho prático da FATEC (disciplina de POO): **aplicação console em C# + SQL Server** com CRUD completo
de veículos (marca, modelo, placa, ano de fabricação), adaptada do exemplo feito em aula.

Entrega exigida (um `.zip`):
1. Solução C# completa (`.sln`, `.csproj`, código-fonte) — **não** só o executável.
2. Script `.sql` que cria o banco e as tabelas.
3. Runbook explicando como executar e testar.
4. Todas as operações CRUD testadas antes da entrega.

> O código, as mensagens do app, o script SQL e o runbook estão em **inglês** (o enunciado foi entregue em inglês).
> Mantenha esse padrão nos artefatos da entrega; as conversas comigo seguem em português.

## Stack
- .NET 8 (`net8.0`), console app, `Nullable` e `ImplicitUsings` habilitados
- ADO.NET puro com `Microsoft.Data.SqlClient` 5.2.2 — **sem ORM** (intencional, segue o padrão da aula)
- SQL Server 2017+ (testado com `mcr.microsoft.com/mssql/server:2022-CU15-ubuntu-22.04`)

## Estrutura
```
VehicleManagement.sln
database/Vehicles.sql              # cria DB VehicleManagementDB + dbo.Vehicles + 3 registros de exemplo (idempotente)
docs/RUNBOOK.md                    # passo a passo de execução + plano de testes CRUD (11 casos)
src/VehicleManagement/
  Program.cs                       # lê connection string, testa conexão, inicia o menu
  appsettings.json                 # ConnectionStrings:VehicleDb (copiado para o output no build)
  Models/Vehicle.cs                # entidade (Id, Make, Model, LicensePlate, ManufactureYear)
  Data/VehicleRepository.cs        # todo o SQL (queries parametrizadas)
  UI/VehicleMenu.cs                # menu e fluxos Create/Read/Update/Delete
  UI/ConsoleHelper.cs              # leitura/validação de input do console
```

## Convenções e decisões
- **Camadas:** `Models` (dados) → `Data` (SQL) → `UI` (console). SQL nunca fica na UI.
- **Queries sempre parametrizadas** (`@Param`), nunca concatenação de string.
- **Validação em dois níveis:** console (obrigatórios, tamanho, ano 1900..ano+1, placa só letras/dígitos/hífen)
  e banco (`NOT NULL`, `UNIQUE` na placa, `CHECK` no ano 1900–2100).
- Placa duplicada é tratada capturando `SqlException` números **2601/2627** em `VehicleMenu.Run`.
- **Toda leitura de console passa por `ConsoleHelper.ReadLine()`**, que encerra o app ao receber EOF
  (`Console.ReadLine()` retorna `null`). Não use `Console.ReadLine()` direto — causa loop infinito em EOF.
- Connection string: `appsettings.json` ou variável de ambiente `ConnectionStrings__VehicleDb` (tem prioridade).
- Um `SqlConnection` por operação (`using`), aproveitando o connection pooling do driver.

## Como testar localmente (WSL, sem dotnet instalado — via Docker)
```bash
docker network create vm-test
docker run -d --name vm-sql --network vm-test -e ACCEPT_EULA=Y -e 'MSSQL_SA_PASSWORD=Test!Passw0rd123' \
  mcr.microsoft.com/mssql/server:2022-CU15-ubuntu-22.04
# após ~20s:
docker cp database/Vehicles.sql vm-sql:/tmp/ && \
docker exec vm-sql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P 'Test!Passw0rd123' -C -i /tmp/Vehicles.sql
docker run -it --rm --network vm-test -v "$PWD":/src:ro \
  -e 'ConnectionStrings__VehicleDb=Server=vm-sql,1433;Database=VehicleManagementDB;User Id=sa;Password=Test!Passw0rd123;TrustServerCertificate=True;' \
  mcr.microsoft.com/dotnet/sdk:8.0 bash -c 'cp -r /src /work && cd /work && dotnet run --project src/VehicleManagement'
# limpeza
docker rm -f vm-sql && docker network rm vm-test
```
- Montar o projeto como `:ro` e copiar para `/work` evita poluir o repo com `bin/` e `obj/` criados como root.
- Testes automatizados: alimentar o menu via stdin (`printf '%s\n' 2 "" 0 | docker run -i ...`).
- Senha `Test!Passw0rd123` é **apenas para o container local descartável**.

## Status (2026-11-17)
- Build: 0 warnings / 0 errors.
- Script SQL validado e idempotente (2ª execução não duplica dados).
- Plano de testes do runbook (11 casos) executado com sucesso, incluindo duplicidade de placa,
  ano inválido, Id inexistente e cancelamento de exclusão.
- Corrigido: loop infinito em EOF no stdin.

## Repositório
- GitHub: https://github.com/alisson92/vehicle-management (branch `main`, Conventional Commits)

## Decisões confirmadas pelo usuário
- O professor testa no **Windows** e usou `Integrated Security=True` no projeto da aula → manter esse padrão no `appsettings.json`.
- Senha `sa` em texto puro no runbook é aceitável (contexto acadêmico) — não sinalizar novamente.

## Projeto de referência da aula
- Pode ser colocado em `reference-class-project/` na raiz, apenas para comparação. A pasta está no `.gitignore`
  e **não deve ser commitada nem entrar no `.zip`**.

## Pendências para a entrega
- Não incluir `bin/`, `obj/`, `.vs/`, `.remember/` nem `reference-class-project/` no `.zip`.
