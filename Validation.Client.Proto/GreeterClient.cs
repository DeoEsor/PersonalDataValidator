using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Validation.Mediator;

namespace Validation.Client.Proto
{

    public sealed class GreeterClient
    {

        #region Fields

        // TODO: Get GreeterClientSettings from DI/IoC impl
        private readonly ILogger<GreeterClient> _logger;
        private readonly Greeter.GreeterClient _client;

        #endregion

        #region Constructors

        public GreeterClient(ILogger<GreeterClient> logger = null)
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

            _client = new Greeter.GreeterClient(channel);
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
                        Birthdat = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(card.BirthDay.ToUniversalTime())
                    };

                    cardRequest.Address.AddRange(card.Address);
                    cardRequest.Emails.AddRange(card.Emails);
                    cardRequest.PhoneNumber.AddRange(card.PhoneNumber);

                    request.Records.Add(cardRequest);
                }

                var response = await _client.ValidateAsync(request, new Grpc.Core.CallOptions(cancellationToken: token));
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

    }

}
