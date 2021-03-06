using Grpc.Net.Client;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using Validation.Mediator;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace AddressValidator.Services;

public class BirthDayValidatorRequestReceiver
{
    private readonly ILogger<BirthDayValidatorRequestReceiver> _logger;

    private readonly Validation.BirthDayValidator.BirthDayValidatorClient _client;

    private readonly IMQRpcReceiver<BirthDayValidationRequest, BirthDayValidationReply> _mqRpcReceiver;

    public BirthDayValidatorRequestReceiver(ILogger<BirthDayValidatorRequestReceiver> logger, 
                                            IMQRpcReceiver<BirthDayValidationRequest, BirthDayValidationReply> mqRpcReceiver,
                                            IServiceProvider provider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqRpcReceiver = mqRpcReceiver;

        var httpHandler = new HttpClientHandler();

        httpHandler.ServerCertificateCustomValidationCallback =
            
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel
            .ForAddress(provider.GetService<IConfiguration>().GetSection("BirthDayValidator").GetSection("gRPC Address").Value
                        ?? throw new ArgumentNullException($"Config doesn't contains gRPC endpoint"),
                new GrpcChannelOptions
                {
                    HttpHandler = httpHandler
                });

        _client = new Validation.BirthDayValidator.BirthDayValidatorClient(channel);
        _mqRpcReceiver.RPC = ValidateAddressAsync;
    }
    
    private async Task<BirthDayValidationReply> ValidateAddressAsync(BirthDayValidationRequest request, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation($"Received to validate from Rabbit - {request}");
            return await _client.ValidateAsync(request, new Grpc.Core.CallOptions(cancellationToken: token));
        }
        catch (Exception ex)
        {
            _logger.LogCritical($" Exception : {ex.Message}");
        }

        return null!;
    }
}