using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQSender.Interfaces;
using RabbitMQSender.Sender;

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
                .GetService<IRPCMQSender<NSPValidationRequest, NSPValidationReply>>()
                ?? new MqRpcSender<NSPValidationRequest, NSPValidationReply>(null!, logger);
            
            mqAddress = provider
                .GetService<IRPCMQSender<AddressValidationRequests, AddressValidationReplies>>()
                        ?? new MqRpcSender<AddressValidationRequests, AddressValidationReplies>(null!, logger);
            
            mqEmail = provider
                .GetService<IRPCMQSender<EmailValidationRequests, EmailValidationReplies>>()
                      ?? new MqRpcSender<EmailValidationRequests, EmailValidationReplies>(null!, logger);
            
            mqPhone = provider
                .GetService<IRPCMQSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>>()
                      ?? new MqRpcSender<PhoneNumberValidationRequests, PhoneNumberValidationReplies>(null!, logger);
            
            mqBirthDay = provider
                .GetService<IRPCMQSender<BirthDayValidationRequest, BirthDayValidationReply>>()
                         ?? new MqRpcSender<BirthDayValidationRequest, BirthDayValidationReply>(null!, logger);
        }

        public override async Task<RecordsValidationResult> Validate(RecordsValidationRequest request, ServerCallContext context)
        {
            var repliesList = new List<Task<RecordValidationResult>>();
            foreach (var requestRecord in request.Records)
                repliesList.Add(Validate(requestRecord, context.CancellationToken));

            Task.WaitAll(repliesList.ToArray());

            var result = new RecordsValidationResult();

            foreach (var reply in repliesList)
                result.Records.Add(reply.Result);
            
            return result;
        }

        private async Task<RecordValidationResult> Validate(RecordValidationRequest request, CancellationToken token)
        {
            var nspValidationRequest = new NSPValidationRequest
            {
                Nsp = request.Nsp
            };

            var BDValidationRequest = new BirthDayValidationRequest()
            {
                BirthDay = request.Birthdate
            };

            try
            {
                var nspTask = await mqNsp.CallAsync(nspValidationRequest, token);

                var bdTask = await mqBirthDay.CallAsync(BDValidationRequest, token);
                

                var emailTask =
                    request.Emails.Count == 0 ? new EmailValidationReplies() : await EmailTask(request, token);

                var phoneTask =
                    request.PhoneNumber.Count == 0
                        ? new PhoneNumberValidationReplies()
                        : await PhonesTask(request, token);

                var addressTask =
                    request.Address.Count == 0 ? new AddressValidationReplies{} : await AddressTask(request, token);


                var emails = emailTask
                    .Emails
                    .Select(email => email.Email)
                    .ToList();

                var phoneNumbers = phoneTask
                    .PhoneNumbers
                    .Select(p => p.PhoneNumber)
                    .ToList();
                var addresses = addressTask
                    .Addresses
                    .Select(address => address.Address)
                    .ToList();

                return Build(nspTask.Nsp, bdTask.BirthDay, addresses, emails, phoneNumbers);
            }
            catch
            {
                return new RecordValidationResult();
            }
            finally
            {
                
            }
            
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