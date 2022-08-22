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
			if (context.Message.CustomerNumber.Contains("Hello"))
			{
				if (context.RequestId != null)
				{
					await context.RespondAsync<OrderSubmissionRejected>(new
					{
						context.Message.OrderId,
						InVar.Timestamp,
						context.Message.CustomerNumber,
						Reason = "Rejected"
					});
				}
				return;
			}

			await context.Publish<OrderSubmited>(new
            {

                context.Message.OrderId,
                context.Message.Timestamp,
                context.Message.CustomerNumber
            });

			if (context.RequestId != null)
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