using Grpc.Net.Client;
using RabbitMQReceiver.Interfaces;
using RabbitMQReceiver.RPCReceivers;
using Validation.Mediator;
// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace EmailValidator.Services;

//TODO : Refactor RequestReceivers to Generic
public class EmailValidatorRequestReciver
{
    private readonly ILogger<EmailValidatorRequestReciver> _logger;

    private readonly Validation.EmailValidator.EmailValidatorClient _client;

    private readonly IMQRpcReceiver<EmailValidationRequests, EmailValidationReplies> _mqRpcReceiver;

    public EmailValidatorRequestReciver(ILogger<EmailValidatorRequestReciver> logger, 
        IMQRpcReceiver<EmailValidationRequests, EmailValidationReplies> mqRpcReceiver,
        IServiceProvider provider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqRpcReceiver =mqRpcReceiver;

        var httpHandler = new HttpClientHandler();

        httpHandler.ServerCertificateCustomValidationCallback =
            
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel
            .ForAddress(provider.GetService<IConfiguration>().GetSection("EmailValidator").GetSection("gRPC Address").Value
                        ?? throw new ArgumentNullException($"Config doesn't contains gRPC endpoint"),
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