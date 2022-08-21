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

        public OrderController(ILogger<OrderController> logger, IRequestClient<SubmitOrder> requestClientSubmitOrder, ISendEndpointProvider sendEndpointProvider)
        {
            _logger = logger;
            _requestClientSubmitOrder = requestClientSubmitOrder;
            _sendEndpointProvider = sendEndpointProvider;
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
                Timestamp =  InVar.Timestamp,
                CustomerNumber = CustomerNumber
            });

            return Accepted();
        }
    }
}