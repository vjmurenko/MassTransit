using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Component.Consumers
{
    public class SubmitOrderConsumer : IConsumer<SubmitOrder>
    {
        private readonly ILogger<SubmitOrderConsumer> _logger;

        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<SubmitOrder> context)
        {
            _logger.Log(LogLevel.Debug, $"{context.Message.CustomerNumber }");
            if(context.Message.CustomerNumber.Contains("Hello"))
            {
                await context.RespondAsync<OrderSubmissionRejected>(new
                {
                    context.Message.OrderId,
                    InVar.Timestamp,
                    context.Message.CustomerNumber,
                    Reason = "Rejected"
                });
            }
            else
            {
                await context.RespondAsync<OrderSubmissionAccepted>(new
                {
                    context.Message.OrderId,
                    InVar.Timestamp,
                    CustomerNumber = "Hello from consumer"
                });
            }
            
        }
    }
}