// ReSharper disable InconsistentNaming

namespace RabbitMQReceiver.Interfaces;

public interface IMQRpcReceiver<TGet, TSend> : IMQReceiver, IDisposable
{ 
    Func<TGet,CancellationToken, Task<TSend>>? RPC { get; set; }
}