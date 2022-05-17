using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Text;
using Google.Protobuf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQSender.Interfaces;

namespace RabbitMQSender.Sender
{
    public class MqRpcSender<TSend, TGet> : IRPCMQSender<TSend, TGet>
        where TGet : IMessage<TGet>, new()
        where TSend : IMessage<TSend>, new()
        
    {
        public ILogger Logger { get; set; }
        public IConnection Connection { get; init; }
        public IModel Channel { get; init; }
        public IBasicProperties Properties { get; set; }
        public EventingBasicConsumer Consumer { get; init; }
        
        public string ReplyQueueName { get; init; }
        
        public string QueueName { get; init; }

        public ConcurrentDictionary<string, TaskCompletionSource<TGet>> CallbackMapper { get; init; }
            = new ConcurrentDictionary<string, TaskCompletionSource<TGet>>();

        public event Action<TGet>? Recived;

        public MqRpcSender(IConfigurationSection configuration, ILogger logger)
        {
            Logger = logger;
            var factory = new ConnectionFactory {HostName = configuration.GetSection("HostName").Value};

            QueueName = configuration.GetSection("QueueName").Value;
            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();
            ReplyQueueName = Channel.
                QueueDeclare().QueueName;

            Consumer = new EventingBasicConsumer(Channel);
            Consumer.Received += OnReceived;
        }

        public Task<TGet> CallAsync(TSend message, CancellationToken cancellationToken = default)
        {
            Logger?.LogInformation($" [x] Requesting {Channel.DefaultConsumer.Model} : ({message})");
            
            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<TGet>();
            CallbackMapper.TryAdd(correlationId, tcs);
            
            var props = Channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = ReplyQueueName;

            var messageBytes = message.ToByteArray();
            Channel.BasicPublish("", ReplyQueueName, props, messageBytes);
            Channel.BasicConsume(consumer: Consumer, ReplyQueueName, autoAck: true);

            cancellationToken.Register(() => CallbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }

        public void Close() => Connection.Close();

        private void OnReceived(object model, BasicDeliverEventArgs ea)
        {
            var suchTaskExists = CallbackMapper
                .TryRemove(ea.BasicProperties.CorrelationId, out var tcs);
            
            if (!suchTaskExists) return;
            
            tcs?.TrySetResult(OnConfirmedReceived(model, ea));
        }   
        
        private TGet OnConfirmedReceived(object model, BasicDeliverEventArgs ea)
        {
            TGet response = default!;
            var body = ea.Body;
            var parser = new MessageParser<TGet>(() => new TGet());
            try
            {
                using (var ms = new MemoryStream())
                {
                    
                    ms.Write(body.ToArray(), 0, body.Length);
                    ms.Seek(0, SeekOrigin.Begin);
                    response = parser.ParseFrom(body.ToArray());
                }

                if (Logger == null)
                    Console.WriteLine(" [.] Got '{0}'", response);
                else
                    Logger?.LogInformation(" [.] Got '{0}'", response);

                Recived?.Invoke(response);
            }
            catch (Exception e)
            {
                if (Logger == null)
                    Console.WriteLine("Parse Failed");
                else
                    Logger?.LogError("Parse Failed");
                throw new SerializationException($"Cant parse {typeof(TGet)} from data {e.Data}");
            }
            finally
            {
                
            }
            
            return response;
        }   
    }    
}

