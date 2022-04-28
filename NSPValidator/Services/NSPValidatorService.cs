using System.Net.Mail;
using Grpc.Core;
using Validation.Mediator;

namespace NSPValidator.Services;

public class NSPValidatorService : Validation.NSPValidator.NSPValidatorBase
{
    private readonly ILogger<NSPValidatorService> _logger;

    public NSPValidatorService(ILogger<NSPValidatorService> logger)
    {
        _logger = logger;
    }

    public override async Task<NSPValidationReply> Validate(NSPValidationRequest request, ServerCallContext context)
    {
        try
        {
            var reply = new NSPValidationResult();

            reply.Name.Value = request.Nsp.Name;
            reply.Surname.Value = request.Nsp.Surname;
            reply.Patronymic.Value = request.Nsp.Patronymic;

            reply.Name.IsValid = ValidateName(reply.Name.Value);
            reply.Surname.IsValid = ValidateName(reply.Surname.Value);
            reply.Patronymic.IsValid = ValidateName(reply.Patronymic.Value);

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