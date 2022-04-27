using Grpc.Core;
using Validation.Mediator;
using AddressValidatorGrpc = Validation.AddressValidator;
using RabbitMQReceiver.RPCReceivers;

namespace AddressValidator.Services;

public class AddressValidationService : AddressValidatorGrpc.AddressValidatorBase
{
    private readonly ILogger<AddressValidationService> _logger;

    public AddressValidationService(ILogger<AddressValidationService> logger, IServiceProvider provider)
    {
        provider.GetService<RpcReceiver<AddressValidationRequest, AddressValidationReply>>();
        _logger = logger;
    }

    public override Task<AddressValidationReply> Validate(AddressValidationRequest request, ServerCallContext context)
    {
        return Task.FromResult(new AddressValidationReply ());
    }
}