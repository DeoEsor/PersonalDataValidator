// ReSharper disable InconsistentNaming

using System.Collections.Concurrent;

namespace RabbitMQSender.Interfaces;

public interface IRPCMQSender<TSend, TGet> : IMQSender
    where TGet : class
    where TSend : class
{
    public ConcurrentDictionary<string, TaskCompletionSource<TGet>> CallbackMapper { get; init; }

    public event Action<TGet>? Recived;
    
    public Task<TGet> CallAsync(TSend message, CancellationToken cancellationToken = default);
}