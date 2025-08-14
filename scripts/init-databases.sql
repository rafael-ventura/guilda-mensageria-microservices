-- Script para criar bancos de dados para cada microsserviço
-- Executado automaticamente quando o container PostgreSQL sobe

-- Banco para DispatchService
CREATE DATABASE guilda_dispatch;
GRANT ALL PRIVILEGES ON DATABASE guilda_dispatch TO postgres;

-- Banco para DeliveryService  
CREATE DATABASE guilda_delivery;
GRANT ALL PRIVILEGES ON DATABASE guilda_delivery TO postgres;

-- Banco para InboxService
CREATE DATABASE guilda_inbox;
GRANT ALL PRIVILEGES ON DATABASE guilda_inbox TO postgres;

-- Banco para NotificationService (se precisar de persistência)
CREATE DATABASE guilda_notification;
GRANT ALL PRIVILEGES ON DATABASE guilda_notification TO postgres;

-- Log das criações
\echo 'Databases criados com sucesso para a Guilda dos Mensageiros!'
