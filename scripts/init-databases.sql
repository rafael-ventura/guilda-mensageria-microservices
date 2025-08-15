-- Script para criar bancos de dados para cada microsserviço
-- Executado automaticamente quando o container SQL Server sobe

-- Banco para DispatchService
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GuildaDispatch')
BEGIN
    CREATE DATABASE GuildaDispatch;
    PRINT 'Database GuildaDispatch criado com sucesso!';
END
GO

-- Banco para DeliveryService  
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GuildaDelivery')
BEGIN
    CREATE DATABASE GuildaDelivery;
    PRINT 'Database GuildaDelivery criado com sucesso!';
END
GO

-- Banco para InboxService
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GuildaInbox')
BEGIN
    CREATE DATABASE GuildaInbox;
    PRINT 'Database GuildaInbox criado com sucesso!';
END
GO

-- Banco para NotificationService
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'GuildaNotification')
BEGIN
    CREATE DATABASE GuildaNotification;
    PRINT 'Database GuildaNotification criado com sucesso!';
END
GO

PRINT 'Todos os databases foram criados com sucesso para a Guilda dos Mensageiros!';