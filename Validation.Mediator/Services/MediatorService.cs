using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQSender.Interfaces;

namespace Validation.Mediator.Services
{
    public class MediatorService : Mediator.MediatorBase
    {
        private readonly ILogger<MediatorService> _logger;
        private IRPCMQSender<NSPValidationRequest, NSPValidationReply> mqNsp;
        private IRPCMQSender<AddressValidationRequests, AddressValidationReplies> mqAddress;
        private IRPCMQSender<EmailValidationRequests, EmailValidationReplies> mqEmail;
        private IRPCMQSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies> mqPhone;
        private IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply> mqBirthDay;

        public MediatorService(ILogger<MediatorService> logger, IServiceProvider provider)
        {
            _logger = logger;
            mqNsp = provider
                .GetService<IRPCMQSender<NSPValidationRequest, NSPValidationReply>>();
            
            mqAddress = provider
                .GetService<IRPCMQSender<AddressValidationRequests, AddressValidationReplies>>();
            
            mqEmail = provider
                .GetService<IRPCMQSender<EmailValidationRequests, EmailValidationReplies>>();
            
            mqPhone = provider
                .GetService<IRPCMQSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>>();
            
            mqBirthDay = provider
                .GetService<IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply>>();
        }

        public override Task<RecordsValidationResult> Validate(RecordsValidationRequest request, ServerCallContext context)
        {
            var repliesList = request
                .Records
                .Select(recordValidationRequest => Validate(recordValidationRequest, context.CancellationToken).Result)
                .ToList();

            var result = new RecordsValidationResult
            {
                Records =
                {
                    repliesList
                }
            };
            
            return Task.FromResult(result);
        }

        private async Task<RecordValidationResult> Validate(RecordValidationRequest request, CancellationToken token)
        {
            var tasksList = new List<Task>();
            var nspValidationRequest = new NSPValidationRequest
            {
                Nsp = request.Nsp
            };
            
            var nspTask = mqNsp.CallAsync(nspValidationRequest, token);
            tasksList.Add(nspTask);

            var BDValidationRequest = new BirthDayValidationRequest()
            {
                BirthDay = request.Birthdate
            };
            
            var bdTask = mqBirthDay.CallAsync(BDValidationRequest, token);
            tasksList.Add(bdTask);

            var emailTask = EmailTask(request, token);
            tasksList.Add(emailTask);
            
            var phoneTask = PhonesTask(request, token);
            tasksList.Add(phoneTask);
            
            var addressTask = AddressTask(request, token);
            tasksList.Add(addressTask);
            
            Task.WaitAll(tasksList.ToArray());


            var emails = emailTask
                .Result.Emails
                .Select(email => email.Email)
                .ToList();
            
            var phoneNumbers = phoneTask
                .Result.PhoneNumbers
                .Select(p => p.PhoneNumber)
                .ToList();
            var addresses = addressTask
                .Result.Addresses.
                Select(address => address.Address)
                .ToList();

            return Build(nspTask.Result.Nsp, bdTask.Result.BirthDay, addresses, emails, phoneNumbers);
        }

        private RecordValidationResult Build(NSPValidationResult NSP, 
                                            TimestampValidationResult BirthDay,
                                            params List<StringValidationResult>[] Datas)
        {
            //TODO Exception when Datas.Length != 3
            var result = new RecordValidationResult();
            result.Nsp = NSP;
            result.Address.Add(Datas[0]);
            result.Emails.Add(Datas[1]);
            result.PhoneNumber.Add(Datas[2]);
            result.Birthdate = BirthDay;

            return result;
        }
        
        private Task<EmailValidationReplies> EmailTask(RecordValidationRequest request, CancellationToken token)
        {
            var emailValidationRequest = new EmailValidationRequests();
            foreach (var email in request.Emails)
                emailValidationRequest.Emails.Add(
                    new EmailValidationRequest
                    {
                        Email = email
                    }
                );

            return mqEmail.CallAsync(emailValidationRequest, token);
        }
        
        private Task<PhoneNumberValidationReplies> PhonesTask(RecordValidationRequest request, CancellationToken token)
        {
            var phoneNumberValidationRequests = new PhoneNumberValidationRequests();
            foreach (var phone in request.PhoneNumber)
                phoneNumberValidationRequests.PhoneNumbers.Add(
                    new PhoneNumberValidationRequest()
                    {
                        PhoneNumber = phone
                    }
                );

            return mqPhone.CallAsync(phoneNumberValidationRequests, token);
        }
        
        private Task<AddressValidationReplies> AddressTask(RecordValidationRequest request, CancellationToken token)
        {
            var addressValidationRequests = new AddressValidationRequests();
            foreach (var address in request.Address)
                addressValidationRequests.Addresses.Add(
                    new AddressValidationRequest
                    {
                        Address = address
                    }
                );

            return mqAddress.CallAsync(addressValidationRequests, token);;
        }
    }
}