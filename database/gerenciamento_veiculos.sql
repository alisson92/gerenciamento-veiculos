/* ============================================================
   Sistema de Gerenciamento de Veículos - script do banco (SQL Server)
   Cria o banco de dados, a tabela "veiculo" e alguns dados de exemplo.
   Pode ser executado mais de uma vez sem erro (não duplica nada).
   ============================================================ */

IF DB_ID(N'gerenciamento_veiculos') IS NULL
BEGIN
    CREATE DATABASE gerenciamento_veiculos;
END
GO

USE gerenciamento_veiculos;
GO

IF OBJECT_ID(N'dbo.veiculo', N'U') IS NULL
BEGIN
    CREATE TABLE veiculo
    (
        codigo          INT IDENTITY(1,1) NOT NULL,   -- gerado automaticamente pelo banco
        marca           NVARCHAR(50)      NOT NULL,   -- ex.: Toyota
        modelo          NVARCHAR(50)      NOT NULL,   -- ex.: Corolla
        placa           NVARCHAR(10)      NOT NULL,   -- ex.: ABC1D23
        ano_fabricacao  INT               NOT NULL,   -- ex.: 2020

        CONSTRAINT PK_veiculo PRIMARY KEY (codigo),
        CONSTRAINT UQ_veiculo_placa UNIQUE (placa),                               -- não permite placa repetida
        CONSTRAINT CK_veiculo_ano_fabricacao CHECK (ano_fabricacao BETWEEN 1900 AND 2100)
    );
END
GO

/* Dados de exemplo (inseridos somente se a tabela estiver vazia) */
IF NOT EXISTS (SELECT 1 FROM veiculo)
BEGIN
    INSERT INTO veiculo (marca, modelo, placa, ano_fabricacao) VALUES
        (N'Toyota',     N'Corolla', N'ABC1D23', 2020),
        (N'Volkswagen', N'Gol',     N'XYZ9K88', 2018),
        (N'Honda',      N'Civic',   N'QWE4R56', 2022);
END
GO

SELECT * FROM veiculo;
GO
