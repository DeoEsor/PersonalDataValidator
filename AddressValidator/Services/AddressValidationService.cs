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

    public override async Task<AddressValidationReplies> Validate(AddressValidationRequests request, ServerCallContext context)
    {
        try
        {
            var res = new AddressValidationReplies();

            foreach (var requestAddress in request.Addresses)
                res.Addresses.Add(
                    new AddressValidationReply
                    {
                        Address = new StringValidationResult
                        {
                            Value = requestAddress.Address,
                            IsValid = true
                        }
                    }
                );

            return res;
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception {e.Message}");
            throw;
        }

        return default!;
    }
}