using System.Net.Mail;
using Grpc.Core;
using RabbitMQReceiver.Interfaces;
using Validation.Mediator;
using AddressValidatorGrpc = Validation.AddressValidator;
using RabbitMQReceiver.RPCReceivers;

namespace AddressValidator.Services;

public class AddressValidationService : AddressValidatorGrpc.AddressValidatorBase
{
    private readonly ILogger<AddressValidationService> _logger;

    public AddressValidationService(ILogger<AddressValidationService> logger)
    {
        _logger = logger;
    }

    public override Task<AddressValidationReplies> Validate(AddressValidationRequests request, ServerCallContext context)
    {
        try
        {
           
            
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception {e.Message}");
            throw;
        }

        return null!;
    }
}