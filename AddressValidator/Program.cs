using AddressValidator.Services;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;
using Validation.Mediator;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("validatorsConfig.json", false, true);

builder.Services.AddGrpc();
builder.Services.AddTransient<IMQRpcReceiver<AddressValidationRequests, AddressValidationReplies>,
    RpcReceiver<AddressValidationRequests, AddressValidationReplies>>
(s =>
    new RpcReceiver<AddressValidationRequests, AddressValidationReplies>(builder.Configuration.GetSection("AddressValidator")));
builder.Services.AddSingleton<AddressValidatorRequestReceiver>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AddressValidationService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client");

app.Run();