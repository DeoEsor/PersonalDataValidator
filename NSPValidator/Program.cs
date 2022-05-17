using NSPValidator.Services;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;
using Validation.Mediator;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.Configuration.AddJsonFile("validatorsConfig.json", false, true);

builder.Services.AddGrpc();
builder.Services.AddSingleton<IMQRpcReceiver<NSPValidationRequest, NSPValidationReply>,
    MqRpcReceiver<NSPValidationRequest, NSPValidationReply>>
        (s => new MqRpcReceiver<NSPValidationRequest, NSPValidationReply>(builder.Configuration.GetSection("NSPValidator")));

builder.Services.AddSingleton<NSPValidatorRequestReceiver>();

var app = builder.Build();
var b =app.Services.GetService<NSPValidatorRequestReceiver>();
// Configure the HTTP request pipeline.
app.MapGrpcService<NSPValidatorService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();