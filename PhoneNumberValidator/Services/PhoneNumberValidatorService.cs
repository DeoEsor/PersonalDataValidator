using System.Text.RegularExpressions;
using Google.Protobuf.Collections;
using Grpc.Core;
using PhoneNumberValidator;
using Validation.Mediator;

namespace PhoneNumberValidator.Services;

public class PhoneNumberValidatorService : Validation.PhoneNumberValidator.PhoneNumberValidatorBase
{
    private readonly ILogger<PhoneNumberValidatorService> _logger;

    public PhoneNumberValidatorService(ILogger<PhoneNumberValidatorService> logger)
    {
        _logger = logger;
    }

    public override async Task<PhoneNumberValidationReplies> Validate(PhoneNumberValidationRequests request, ServerCallContext context)
    {
        var result = new PhoneNumberValidationReplies();

        var replyArray = new PhoneNumberValidationReply[request.PhoneNumbers.Count];

        Parallel.For(0, request.PhoneNumbers.Count, i => Body(i, request.PhoneNumbers, replyArray));
        
        result.PhoneNumbers.AddRange(replyArray);
        return result;
    }

    private void Body(int obj,RepeatedField<PhoneNumberValidationRequest> requests,PhoneNumberValidationReply[] replies)
    {
        var number = requests[obj].PhoneNumber;

        var isValid = IsPhoneNumber(number);
        
        replies[obj].PhoneNumber.Value = number;
        replies[obj].PhoneNumber.IsValid = isValid;
        if (!isValid)
            replies[obj].PhoneNumber.Comment = "Phone number not match pattern";
    }

    public static bool IsPhoneNumber(string number)
    {
        return Regex.Match(number, @"^(\+[0-9]{9})$").Success;
    }
    
}