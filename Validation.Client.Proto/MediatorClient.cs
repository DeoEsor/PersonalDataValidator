using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        public async Task ValidateCardsAsync(IEnumerable<Card> cardsToValidate, CancellationToken token = default)
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
                        Birthdate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(card.BirthDay.ToUniversalTime())
                    };

                    cardRequest.Address.AddRange(card.Address);
                    cardRequest.Emails.AddRange(card.Emails);
                    cardRequest.PhoneNumber.AddRange(card.PhoneNumber);

                    request.Records.Add(cardRequest);
                }

                var response = await _client.ValidateAsync(request, new Grpc.Core.CallOptions(cancellationToken: token));
                if (response == null)
                    _logger?.LogInformation($"Response in null");
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        #endregion

    }

}
