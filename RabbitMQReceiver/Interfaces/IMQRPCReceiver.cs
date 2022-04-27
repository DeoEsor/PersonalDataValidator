// ReSharper disable InconsistentNaming

namespace RabbitMQReceiver.Interfaces;

public interface IMQRpcReceiver<TGet, TSend> : IMQReceiver, IDisposable
{ 
    Func<TGet, TSend>? RPC { get; set; }
}