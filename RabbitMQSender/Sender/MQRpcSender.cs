using System.Collections.Concurrent;
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
        where TSend : IMessage<TSend>
        
    {
        public ILogger Logger { get; set; }
        public IConnection Connection { get; init; }
        public IModel Channel { get; init; }
        public string ReplyQueueName { get; init; }
        public IBasicProperties Properties { get; set; }
        public EventingBasicConsumer Consumer { get; init; }
        private readonly string queueName = "rpc_queue";

        public ConcurrentDictionary<string, TaskCompletionSource<TGet>> CallbackMapper { get; init; }
            = new ConcurrentDictionary<string, TaskCompletionSource<TGet>>();

        public event Action<TGet>? Recived;

        public MqRpcSender(IConfiguration configuration, ILogger logger = null)
        {
            Logger = logger;
            var factory = new ConnectionFactory {HostName = "localhost", Port = 15672};

            Connection = factory.CreateConnection();
            Channel = Connection.CreateModel();
            ReplyQueueName = Channel.QueueDeclare().QueueName;

            Consumer = new EventingBasicConsumer(Channel);
            Consumer.Received += OnReceived;
        }

        public Task<TGet> CallAsync(TSend message, CancellationToken cancellationToken = default)
        {
            Logger?.LogInformation($" [x] Requesting {Channel.DefaultConsumer.Model} : ({0})", message);
            
            var correlationId = Guid.NewGuid().ToString();
            var tcs = new TaskCompletionSource<TGet>();
            CallbackMapper.TryAdd(correlationId, tcs);
            
            var props = Channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = ReplyQueueName;

            var messageBytes = message.ToByteArray();
            Channel.BasicPublish("", queueName, props, messageBytes);
            Channel.BasicConsume(consumer: Consumer, queue: ReplyQueueName, autoAck: true);

            cancellationToken.Register(() => CallbackMapper.TryRemove(correlationId, out _));
            return tcs.Task;
        }

        public void Close() => Connection.Close();

        private void OnReceived(object model, BasicDeliverEventArgs ea)
        {
            Console.WriteLine($"Getted {ea.Body}");
            var suchTaskExists = CallbackMapper
                .TryRemove(ea.BasicProperties.CorrelationId, out var tcs);
            
            if (!suchTaskExists) return;
            
            tcs.TrySetResult(OnConfirmedReceived(model, ea));
        }   
        
        private TGet OnConfirmedReceived(object model, BasicDeliverEventArgs ea)
        {
            var body = ea.Body;
            var a = new Google.Protobuf.MessageParser<TGet>(() => new TGet());
            var response = a.ParseFrom(ea.Body.ToArray());

            Console.WriteLine(" [.] Got '{0}'", response);
            Recived?.Invoke(response);
            
            return response;
        }   
    }    
}

