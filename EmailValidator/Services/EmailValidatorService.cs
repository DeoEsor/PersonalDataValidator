using System.Net.Mail;
using Grpc.Core;
using Validation.Mediator;

namespace EmailValidator.Services;

public class EmailValidatorService : Validation.EmailValidator.EmailValidatorBase
{
    private readonly ILogger<EmailValidatorService> _logger;

    public EmailValidatorService(ILogger<EmailValidatorService> logger)
    {
        _logger = logger;
    }

    public override async Task<EmailValidationReplies> Validate(EmailValidationRequests request, ServerCallContext context)
    {
        try
        {
            var reply = new EmailValidationReplies();
            foreach (var email in request.Emails)
                reply.Emails.Add(Validate(email));

            return reply;
        }
        catch (Exception e)
        {
            _logger.LogCritical($"Exception {e.Message}");
            throw;
        }

        return null!;
    }

    private EmailValidationReply Validate(EmailValidationRequest email)
    {
        var emailValidationResult = new StringValidationResult();
        var trimmedEmail = email.Email.Trim();

        if (trimmedEmail.EndsWith(".")) 
            emailValidationResult.IsValid = false; // suggested by @TK-421
        try
        {
            var addr = new MailAddress(email.Email);
            emailValidationResult.IsValid = addr.Address == trimmedEmail;
        }
        catch (Exception exception)
        {
            emailValidationResult.IsValid = false;
            emailValidationResult.Comment = exception.Message;
        }
        return new EmailValidationReply
        {
            Email = emailValidationResult
        };
    }
}