using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
// ReSharper disable InconsistentNaming

namespace RabbitMQSender.Interfaces;

public interface IMQSender
{
    public ILogger Logger { get; set; }
    public IConnection Connection { get; init; }
    public IModel Channel { get; init; }
    public string ReplyQueueName { get; init; }
    public EventingBasicConsumer Consumer { get; init; }
    public IBasicProperties Properties { get; set; }
}