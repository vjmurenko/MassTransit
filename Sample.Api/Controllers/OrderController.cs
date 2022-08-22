using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sample.Contracts;

namespace Sample.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class OrderController : ControllerBase
	{
		private readonly ILogger<OrderController> _logger;
		private readonly IRequestClient<SubmitOrder> _requestClientSubmitOrder;
		private readonly ISendEndpointProvider _sendEndpointProvider;
		private readonly IRequestClient<CheckOrder> _checkOrderClient;

		public OrderController(ILogger<OrderController> logger,
			IRequestClient<SubmitOrder> requestClientSubmitOrder,
			ISendEndpointProvider sendEndpointProvider,
			IRequestClient<CheckOrder> checkOrderClient)
		{
			_logger = logger;
			_requestClientSubmitOrder = requestClientSubmitOrder;
			_sendEndpointProvider = sendEndpointProvider;
			_checkOrderClient = checkOrderClient;
		}


		[HttpGet]
		public async Task<IActionResult> Get(Guid id)
		{
			var (status, notfound) = await _checkOrderClient.GetResponse<OrderStatus, OrderNotFound>(new {OrderId = id});
			if (status.IsCompletedSuccessfully)
			{
				var response = await status;
				return Ok(response.Message);
			}
			else
			{
				var response = await notfound;
				return NotFound(response.Message);
			}
		}

		[HttpPost]
		public async Task<IActionResult> Post(Guid id, string CustomerNumber)
		{
			var (accept, reject) = await _requestClientSubmitOrder.GetResponse<OrderSubmissionAccepted, OrderSubmissionRejected>(new
				{
					OrderId = id,
					Timestamp = InVar.Timestamp,
					CustomerNumber = CustomerNumber
				}
			);

			if (accept.IsCompleted)
			{
				var response = await accept;
				return Accepted(response.Message);
			}
			else
			{
				var response = await reject;
				return BadRequest(response.Message);
			}
		}

		[HttpPut]
		public async Task<IActionResult> Put(Guid id, string CustomerNumber)
		{
			var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));
			await endpoint.Send<SubmitOrder>(new
			{
				OrderId = id,
				Timestamp = InVar.Timestamp,
				CustomerNumber = CustomerNumber
			});

			return Accepted();
		}
	}
}