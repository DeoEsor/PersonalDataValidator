using EmailValidator.Services;
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
builder.Services.AddSingleton<IMQRpcReceiver<EmailValidationRequests, EmailValidationReplies>,
    MqRpcReceiver<EmailValidationRequests, EmailValidationReplies>>
(s =>
    new MqRpcReceiver<EmailValidationRequests, EmailValidationReplies>(builder.Configuration.GetSection("EmailValidator")));
builder.Services.AddSingleton<EmailValidatorRequestReciver>();

var app = builder.Build();

var b =app.Services.GetService<EmailValidatorRequestReciver>();
// Configure the HTTP request pipeline.
app.MapGrpcService<EmailValidatorService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();