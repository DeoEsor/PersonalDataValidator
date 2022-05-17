using System.Text;
using System.Text.Unicode;
using Google.Protobuf;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQReceiver.Interfaces;

namespace RabbitMQReceiver.RPCReceivers;

public class MqRpcReceiver<TGet, TSend> : IMQRpcReceiver<TGet, TSend> 
    where TGet : IMessage<TGet>, new()
    where TSend : IMessage<TSend>
{
    public IModel Channel { get; init; }
    public IConnection Connection { get; init; }
    public AsyncEventingBasicConsumer Consumer { get; init; }
    public  Func<TGet,CancellationToken, Task<TSend>>? RPC { get; set; }
    public ILogger Logger { get; set; }
    
    ~MqRpcReceiver()
    {
        
        Close();
    }

    public MqRpcReceiver(IConfigurationSection configuration, ILogger logger = null)
    {
        Logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = configuration.GetSection("HostName").Value ?? "localhost",
            DispatchConsumersAsync = true
        };

        Connection = factory.CreateConnection();
        Channel = Connection.CreateModel();

        Channel.QueueDeclare(configuration.GetSection("QueueName").Value, false, false, false, null);
        Channel.BasicQos(0, 1, false);

        Consumer = new AsyncEventingBasicConsumer(Channel);
        Consumer.Received += OnReceived;

        Channel.BasicConsume(configuration.GetSection("QueueName").Value ?? "rpc_queue", false, Consumer);
    }

    private async Task OnReceived(object model, BasicDeliverEventArgs ea)
    {
        TSend? response = default;

        var body = ea.Body;
        var props = ea.BasicProperties;
        var replyProps = Channel.CreateBasicProperties();
        replyProps.CorrelationId = props.CorrelationId;

        Console.WriteLine($"[] Recived {model}");
        try
        {
            var a = new MessageParser<TGet>(() => new TGet());
            var message = a.ParseFrom(body.ToArray());
            response = await RpcCalAsync(message!);
        }
        catch (Exception e)
        {
            Console.WriteLine(" [.] " + e.Message);
            response = default;
        }
        finally
        {
            var responseBytes = response.ToByteArray();
            
            Channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
            Channel.BasicAck(ea.DeliveryTag, false);
        }
    }

    private async Task<TSend> RpcCalAsync(TGet message, CancellationToken token = default)
    {
        if (RPC == null)
            throw new RuntimeBinderException("RPC callback not linked");
        return await RPC.Invoke(message, token);
    }

    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    public void Close()
    {
        Channel.Close();
        Connection.Close();
    }
}   