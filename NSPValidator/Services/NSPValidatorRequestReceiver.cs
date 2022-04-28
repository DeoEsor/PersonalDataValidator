﻿using Grpc.Net.Client;
using RabbitMQReceiver.Interfaces;
using Validation.Mediator;

// ReSharper disable PrivateFieldCanBeConvertedToLocalVariable

namespace NSPValidator.Services;

//TODO : Refactor RequestReceivers to Generic
public class NSPValidatorRequestReceiver
{
    private readonly ILogger<NSPValidatorRequestReceiver> _logger;

    private readonly Validation.NSPValidator.NSPValidatorClient _client;

    private readonly IMQRpcReceiver<NSPValidationRequest, NSPValidationReply> _mqRpcReceiver;

    public NSPValidatorRequestReceiver(ILogger<NSPValidatorRequestReceiver> logger, 
                                            IMQRpcReceiver<NSPValidationRequest, NSPValidationReply> mqRpcReceiver)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqRpcReceiver = mqRpcReceiver ?? throw new ArgumentNullException(nameof(mqRpcReceiver));

        var httpHandler = new HttpClientHandler();

        httpHandler.ServerCertificateCustomValidationCallback =
            
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel.ForAddress("https://localhost:5001",
            new GrpcChannelOptions
            {
                HttpHandler = httpHandler
            });

        _client = new Validation.NSPValidator.NSPValidatorClient(channel);
        _mqRpcReceiver.RPC = ValidateAddressAsync;
    }
    
    private async Task<NSPValidationReply> ValidateAddressAsync(NSPValidationRequest request, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation($"Received to validate from Rabbit - {request}");
            return await _client.ValidateAsync(request, new Grpc.Core.CallOptions(cancellationToken: token));
        }
        catch (Exception ex)
        {
            _logger.LogCritical($" Exception : {ex.Message}");
        }

        return null!;
    }
}