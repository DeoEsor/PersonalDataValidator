using AddressValidator.Services;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;
using Validation.Mediator;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.Configuration.AddJsonFile("validatorsConfig.json", false, true);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IMQRpcReceiver<AddressValidationRequests, AddressValidationReplies>,
    MqRpcReceiver<AddressValidationRequests, AddressValidationReplies>>
(s =>
    new MqRpcReceiver<AddressValidationRequests, AddressValidationReplies>(builder.Configuration.GetSection("AddressValidator")));
builder.Services.AddSingleton<AddressValidatorRequestReceiver>();

var app = builder.Build();

var b =app.Services.GetService<AddressValidatorRequestReceiver>();
// Configure the HTTP request pipeline.
app.MapGrpcService<AddressValidationService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client");

app.Run();