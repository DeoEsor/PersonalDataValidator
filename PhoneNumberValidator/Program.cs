using PhoneNumberValidator.Services;
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
builder.Services.AddSingleton<IMQRpcReceiver<PhoneNumberValidationRequests, PhoneNumberValidationReplies>,
    MqRpcReceiver<PhoneNumberValidationRequests, PhoneNumberValidationReplies>>
(s =>
    new MqRpcReceiver<PhoneNumberValidationRequests, PhoneNumberValidationReplies>(builder.Configuration.GetSection("PhoneNumberValidator")));
builder.Services.AddSingleton<PhoneNumberValidatorRequestReceiver>();


var app = builder.Build();
var b =app.Services.GetService<PhoneNumberValidatorRequestReceiver>();
// Configure the HTTP request pipeline.
app.MapGrpcService<PhoneNumberValidatorService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();