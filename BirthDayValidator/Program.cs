using AddressValidator.Services;
using BirthDayValidator.Services;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;
using Validation.Mediator;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.Configuration.AddJsonFile("validatorsConfig.json", false, true);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<IMQRpcReceiver<BirthDayValidationRequest, BirthDayValidationReply>,
    IMQRpcReceiver<BirthDayValidationRequest, BirthDayValidationReply>>
(s =>
    new MqRpcReceiver<BirthDayValidationRequest, BirthDayValidationReply>(builder.Configuration.GetSection("BirthDayValidator")));
builder.Services.AddSingleton<BirthDayValidatorRequestReceiver>();

var app = builder.Build();

var b =app.Services.GetService<BirthDayValidatorRequestReceiver>();
// Configure the HTTP request pipeline.
app.MapGrpcService<BirthDayValidatonService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();