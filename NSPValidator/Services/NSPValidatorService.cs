using System.Net.Mail;
using Grpc.Core;
using Validation.Mediator;

namespace NSPValidator.Services;

public class NSPValidatorService : Validation.NSPValidator.NSPValidatorBase
{
    private readonly ILogger<NSPValidatorService> _logger;

    public NSPValidatorService(ILogger<NSPValidatorService> logger, NSPValidatorRequestReceiver rpc)
    {
        _logger = logger;
    }

    public override async Task<NSPValidationReply> Validate(NSPValidationRequest request, ServerCallContext context)
    {
        try
        {
            var reply = new NSPValidationResult();

            reply.Name = new StringValidationResult { Value =   request.Nsp.Name.Split().First()};
            reply.Surname = new StringValidationResult { Value =   request.Nsp.Surname.Split().First()};
            reply.Patronymic = new StringValidationResult { Value =  request.Nsp.Patronymic.Split().First()};

            reply.Name.IsValid = ValidateName(reply.Name.Value);
            reply.Surname.IsValid = ValidateName(reply.Surname.Value);
            reply.Patronymic.IsValid = ValidateName(reply.Patronymic.Value);
            
            _logger?.LogInformation($"Reply  with Name {reply.Name.Value} is {reply}");

            return new NSPValidationReply
            {
                Nsp = reply
            };
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception {e.Message}");
            throw;
        }

        return null!;
    }


    bool ValidateName(string value)
    {
        return value.Split().Length == 1;
    }
}