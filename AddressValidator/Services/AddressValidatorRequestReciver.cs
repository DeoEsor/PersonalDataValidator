using Grpc.Net.Client;
using RabbitMQReceiver.Interfaces;
using Validation.Mediator;
using AddressValidatorGrpc = Validation.AddressValidator;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace AddressValidator.Services;

public class AddressValidatorRequestReceiver
{
    private readonly ILogger<AddressValidatorRequestReceiver> _logger;

    private readonly AddressValidatorGrpc.AddressValidatorClient _client;

    private readonly IMQRpcReceiver<AddressValidationRequests, AddressValidationReplies> _mqRpcReceiver;

    public AddressValidatorRequestReceiver(ILogger<AddressValidatorRequestReceiver> logger, 
                                            IMQRpcReceiver<AddressValidationRequests, AddressValidationReplies> mqRpcReceiver)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqRpcReceiver = mqRpcReceiver ?? throw new ArgumentNullException(nameof(mqRpcReceiver));

        var httpHandler = new HttpClientHandler();

        httpHandler.ServerCertificateCustomValidationCallback =
            
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel.ForAddress("https://localhost:5001",
            new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

        _client = new AddressValidatorGrpc.AddressValidatorClient(channel);
        _mqRpcReceiver.RPC = ValidateAddressAsync;
    }
    
    private async Task<AddressValidationReplies> ValidateAddressAsync(AddressValidationRequests request, CancellationToken token = default)
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