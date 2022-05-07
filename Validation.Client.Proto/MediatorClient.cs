using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Models;
using Validation.Mediator;

namespace Validation.Client.Proto
{

    public sealed class MediatorClient
    {

        #region Fields
        private readonly ILogger<MediatorClient> _logger;
        private readonly Validation.Mediator.Mediator.MediatorClient _client;

        #endregion

        #region Constructors

        public MediatorClient(ILogger<MediatorClient> logger = null)
        {
            _logger = logger;// ?? throw new ArgumentNullException(nameof(logger));

            var httpHandler = new HttpClientHandler();

            httpHandler.ServerCertificateCustomValidationCallback =
                // Return `true` to allow certificates that are untrusted/invalid
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            var channel = GrpcChannel.ForAddress("https://localhost:5001",
                 new GrpcChannelOptions
                 {
                     HttpHandler = httpHandler
                 });

            _client = new Validation.Mediator.Mediator.MediatorClient(channel);
        }

        #endregion

        #region Methods

        public async Task<IEnumerable<Card>> ValidateCardsAsync(IEnumerable<Card> cardsToValidate, CancellationToken token = default)
        {
            try
            {
                var request = new RecordsValidationRequest();


                foreach (var card in cardsToValidate)
                {
                    var cardRequest = new RecordValidationRequest
                    {
                        Nsp = new NSP()
                        {
                            Name = card.Name,
                            Surname = card.Surname,
                            Patronymic = card.Patronymic
                        },
                        Birthdate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(card.BirthDay.Value.ToUniversalTime())
                    };

                    cardRequest.Address.AddRange(card.Address.Select(s => s.Value));
                    cardRequest.Emails.AddRange(card.Emails.Select(s => s.Value));
                    cardRequest.PhoneNumber.AddRange(card.PhoneNumber.Select(s => s.Value));

                    request.Records.Add(cardRequest);
                }

                var response = await _client.ValidateAsync(request, new Grpc.Core.CallOptions(cancellationToken: token));
                if (response == null)
                {
                    _logger?.LogInformation($"Response in null");
                    throw new AuthenticationException($"Response in null");
                }

                var result = new List<Card>();
                foreach (var recordValidation in response.Records)
                    result.Add(
                        new Card
                        {
                            Name = recordValidation.Nsp.Name.Value,
                            NameValid = recordValidation.Nsp.Name.IsValid ? ValidState.True : ValidState.False,
                            Surname = recordValidation.Nsp.Surname.Value,
                            SurnameValid = recordValidation.Nsp.Surname.IsValid ? ValidState.True : ValidState.False,
                            Patronymic = recordValidation.Nsp.Patronymic.Value,
                            PatronymicValid = recordValidation.Nsp.Patronymic.IsValid ? ValidState.True : ValidState.False,
                            
                            Emails = new ObservableCollection<ValueIsValid<string>>(recordValidation.Emails
                                .Select(s => new ValueIsValid<string>(s.Value, s.IsValid ? ValidState.True: ValidState.False))),
                            PhoneNumber = new ObservableCollection<ValueIsValid<string>>(recordValidation.PhoneNumber
                                .Select(s => new ValueIsValid<string>(s.Value, s.IsValid ? ValidState.True: ValidState.False))),
                             Address= new ObservableCollection<ValueIsValid<string>>(recordValidation.Address
                                .Select(s => new ValueIsValid<string>(s.Value, s.IsValid ? ValidState.True: ValidState.False))),
                             
                             BirthDay = new ValueIsValid<DateTime>(recordValidation.Birthdate.Value.ToDateTime(), recordValidation.Birthdate.IsValid  ? ValidState.True: ValidState.False)
                        }
                        );

                return result;
            }
            catch (Exception ex)
            {
                return await Task.FromException<IEnumerable<Card>>(ex);
            }
        }

        #endregion

    }

}
