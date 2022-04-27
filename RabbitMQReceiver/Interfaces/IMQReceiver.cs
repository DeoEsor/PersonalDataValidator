using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// ReSharper disable InconsistentNaming

namespace RabbitMQReceiver.Interfaces;

public interface IMQReceiver
{
    IModel Channel { get; init; }
    IConnection Connection { get; init; }
    
    ILogger Logger { get; set; }
    AsyncEventingBasicConsumer Consumer { get; init; }
    
    void Close()
    {
        Channel.Close();
        Connection.Close();
    }
}