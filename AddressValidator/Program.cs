using AddressValidator.Services;
using RabbitMQReceiver.RPCReceivers;
using Validation.Mediator;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddGrpc();
builder.Services.AddSingleton<RpcReceiver<AddressValidationRequest, AddressValidationReply>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AddressValidationService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();