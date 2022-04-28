using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Validation.Mediator;
using AddressValidatorGrpc = Validation.BirthDayValidator;

namespace BirthDayValidator.Services;

public class BirthDayValidatonService : AddressValidatorGrpc.BirthDayValidatorBase 
{
    private readonly ILogger<BirthDayValidatonService> _logger;

    public BirthDayValidatonService(ILogger<BirthDayValidatonService> logger, IServiceProvider provider)
    {
        _logger = logger;
    }

    public override async Task<BirthDayValidationReply> Validate(BirthDayValidationRequest request, ServerCallContext context)
    {
        try
        {
            var reply = new TimestampValidationResult
            {
                Value = request.BirthDay
            };


            reply.IsValid = Validate(request.BirthDay.ToDateTime());

            return new BirthDayValidationReply
            {
                BirthDay = reply
            };
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception : {e.Message}");
            throw;
        }

        return null!;
    }

    private bool Validate(DateTime d2)
    {
        var d1 = DateTime.Now;

        return d2 < d1 && (d2.Year - d1.Year) > 18 && (d2.Year - d1.Year) < 99;
    }
}