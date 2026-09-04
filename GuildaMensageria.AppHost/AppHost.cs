var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithDataVolume();

var sql = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var dispatchDb = sql.AddDatabase("GuildaDispatch");
var deliveryDb = sql.AddDatabase("GuildaDelivery");
var inboxDb = sql.AddDatabase("GuildaInbox");

var dispatchApi = builder.AddProject<Projects.DispatchService_Host_Api>("dispatchservice-api")
    .WithReference(rabbitmq)
    .WithReference(dispatchDb)
    .WaitFor(rabbitmq)
    .WaitFor(dispatchDb);

var deliveryWorker = builder.AddProject<Projects.DeliveryService_Host_Worker>("deliveryservice-worker")
    .WithReference(rabbitmq)
    .WithReference(deliveryDb)
    .WaitFor(rabbitmq)
    .WaitFor(deliveryDb);

var inboxWorker = builder.AddProject<Projects.InboxService_Host_Worker>("inboxservice-worker")
    .WithReference(rabbitmq)
    .WithReference(inboxDb)
    .WaitFor(rabbitmq)
    .WaitFor(inboxDb);

var inboxApi = builder.AddProject<Projects.InboxService_Host_Api>("inboxservice-api")
    .WithReference(inboxDb)
    .WaitFor(inboxDb);

var notificationWorker = builder.AddProject<Projects.NotificationService_Host_Worker>("notificationservice-worker")
    .WithReference(rabbitmq)
    .WaitFor(rabbitmq);

builder.Build().Run();
