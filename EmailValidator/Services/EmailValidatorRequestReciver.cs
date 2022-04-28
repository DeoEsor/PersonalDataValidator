using Grpc.Net.Client;
using RabbitMQReceiver.Interfaces;
using Validation.Mediator;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace EmailValidator.Services;

//TODO : Refactor RequestReceivers to Generic
public class AddressValidatorRequestReceiver
{
    private readonly ILogger<AddressValidatorRequestReceiver> _logger;

    private readonly Validation.EmailValidator.EmailValidatorClient _client;

    private readonly IMQRpcReceiver<EmailValidationRequests, EmailValidationReplies> _mqRpcReceiver;

    public AddressValidatorRequestReceiver(ILogger<AddressValidatorRequestReceiver> logger, 
                                            IMQRpcReceiver<EmailValidationRequests, EmailValidationReplies> mqRpcReceiver)
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

        _client = new Validation.EmailValidator.EmailValidatorClient(channel);
        _mqRpcReceiver.RPC = ValidateAddressAsync;
    }
    
    private async Task<EmailValidationReplies> ValidateAddressAsync(EmailValidationRequests request, CancellationToken token = default)
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