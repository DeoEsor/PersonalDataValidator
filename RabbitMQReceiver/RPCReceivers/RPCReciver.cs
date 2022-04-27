using System.Text;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQReceiver.Interfaces;

namespace RabbitMQReceiver.RPCReceivers;

public class RpcReceiver<TGet, TSend> : IMQRpcReceiver<TGet, TSend> where TGet : class
{
    public IModel Channel { get; init; }
    public IConnection Connection { get; init; }
    public AsyncEventingBasicConsumer Consumer { get; init; }
    public Func<TGet, TSend>? RPC { get; set; }
    public ILogger Logger { get; set; }
    
    ~RpcReceiver()
    {
        Close();
    }

    public RpcReceiver(IConfiguration configuration, ILogger logger)
    {
        Logger = logger;
        var factory = new ConnectionFactory()
        {
            HostName = configuration.GetSection("HostName").Value,
            DispatchConsumersAsync = true
        };

        Connection = factory.CreateConnection();
        Channel = Connection.CreateModel();

        Channel.QueueDeclare("rpc_queue", false, false, false, null);
        Channel.BasicQos(0, 1, false);

        Consumer = new AsyncEventingBasicConsumer(Channel);
        Consumer.Received += OnReceived;

        Channel.BasicConsume(configuration.GetSection("QueueName").Value, false, Consumer);
    }

    private async Task OnReceived(object model, BasicDeliverEventArgs ea)
    {
        TSend? response = default;

        var body = ea.Body;
        var props = ea.BasicProperties;
        var replyProps = Channel.CreateBasicProperties();
        replyProps.CorrelationId = props.CorrelationId;

        try
        {
            var message = (object)Encoding.UTF8.GetString(body.ToArray()) as TGet;
            //Console.WriteLine(" [.] fib({0})", message);
            response = await RpcCall(message!);
        }
        catch (Exception e)
        {
            Console.WriteLine(" [.] " + e.Message);
            response = default;
        }
        finally
        {
            var responseBytes = Encoding.UTF8.GetBytes(response?.ToString() ?? string.Empty); //TODO: response to byte array
            
            Channel.BasicPublish("", props.ReplyTo, replyProps, responseBytes);
            Channel.BasicAck(ea.DeliveryTag, false);
        }
    }

    private Task<TSend> RpcCall(TGet message)
    {
        if (RPC == null)
            throw new RuntimeBinderException("RPC callback not linked");
        return Task.FromResult(RPC.Invoke(message));
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