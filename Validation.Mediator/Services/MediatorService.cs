using System;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQSender.Interfaces;

namespace Validation.Mediator.Services
{
    public class MediatorService : Validation.Mediator.Mediator.MediatorBase
    {
        private readonly ILogger<MediatorService> _logger;
        private IRPCMQSender<NSPValidationRequest, NSPValidationReply> mqNsp;
        private IRPCMQSender<AddressValidationRequest, AddressValidationReply> mqAddress;
        private IRPCMQSender<EmailValidationRequest, EmailValidationReply> mqEmail;
        private IRPCMQSender<PhoneNumberValidationRequest, PhoneNumberValidationReply> mqPhone;
        private IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply> mqBirthDay;

        public MediatorService(ILogger<MediatorService> logger, IServiceProvider provider)
        {
            _logger = logger;
            mqNsp = provider
                .GetService<IRPCMQSender<NSPValidationRequest, NSPValidationReply>>();
            
            mqAddress = provider
                .GetService<IRPCMQSender<AddressValidationRequest, AddressValidationReply>>();
            
            mqEmail = provider
                .GetService<IRPCMQSender<EmailValidationRequest, EmailValidationReply>>();
            
            mqPhone = provider
                .GetService<IRPCMQSender<PhoneNumberValidationRequest, PhoneNumberValidationReply>>();
            
            mqBirthDay = provider
                .GetService<IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply>>();
        }

        public override Task<RecordsValidationResult> Validate(RecordsValidationRequest request, ServerCallContext context)
        {
            foreach (var recordValidationRequest in request.Records)
            {
                var nspValidationRequest = new NSPValidationRequest
                {
                    Nsp = recordValidationRequest.Nsp
                };
                var nspTask = mqNsp.CallAsync(nspValidationRequest, context.CancellationToken);
                
            }
            
            return Task.FromResult(new RecordsValidationResult ());
        }
    }
}