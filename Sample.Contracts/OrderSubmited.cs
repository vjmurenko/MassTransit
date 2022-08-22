using System;

namespace Sample.Contracts
{
    public interface OrderSubmited
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }
    }
}