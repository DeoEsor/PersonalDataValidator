using Grpc.Net.Client;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using Validation.Mediator;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace PhoneNumberValidator.Services;

//TODO : Refactor RequestReceivers to Generic
public class PhoneNumberValidatorRequestReceiver
{
    private readonly ILogger<PhoneNumberValidatorRequestReceiver> _logger;

    private readonly Validation.PhoneNumberValidator.PhoneNumberValidatorClient _client;

    private readonly IMQRpcReceiver<PhoneNumberValidationRequests, PhoneNumberValidationReplies> _mqRpcReceiver;

    public PhoneNumberValidatorRequestReceiver(ILogger<PhoneNumberValidatorRequestReceiver> logger, 
                                            IMQRpcReceiver<PhoneNumberValidationRequests, PhoneNumberValidationReplies> mqRpcReceiver,
                                            IServiceProvider provider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqRpcReceiver = mqRpcReceiver;

        var httpHandler = new HttpClientHandler();

        httpHandler.ServerCertificateCustomValidationCallback =
            
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel
            .ForAddress(provider.GetService<IConfiguration>().GetSection("PhoneNumberValidator").GetSection("gRPC Address").Value
                        ?? throw new ArgumentNullException($"Config doesn't contains gRPC endpoint"),
                new GrpcChannelOptions
                {
                    HttpHandler = httpHandler
                });

        _client = new Validation.PhoneNumberValidator.PhoneNumberValidatorClient(channel);
        _mqRpcReceiver.RPC = ValidateAddressAsync;
    }
    
    private async Task<PhoneNumberValidationReplies> ValidateAddressAsync(PhoneNumberValidationRequests request, CancellationToken token = default)
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