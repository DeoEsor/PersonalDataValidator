using AddressValidator.Services;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using Validation.Mediator;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc();
builder.Services.AddSingleton<AddressValidatorRequestReceiver>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AddressValidationService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client");

app.Run();