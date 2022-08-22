using System;
using Automatonymous;
using MassTransit;
using MassTransit.RedisIntegration;
using Sample.Contracts;

namespace Sample.Component.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmittedEvent, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderStatusRequested, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if (context.RequestId.HasValue)
                    {
                        await context.RespondAsync<OrderNotFound>(new
                        {
                            context.Message.OrderId
                        });
                    }
                }));
            });
            InstanceState(x => x.CurrentState);

            Initially(When(OrderSubmittedEvent)
                .Then(context =>
                {
                    context.Instance.CustomerNumber = context.Data.CustomerNumber;
                    context.Instance.SubmitedDate = context.Data.Timestamp;
                    context.Instance.Updated = DateTime.UtcNow;
                })
                .TransitionTo(Submitted));
            
            During(Submitted, Ignore(OrderSubmittedEvent));
            
            DuringAny(
                When(OrderStatusRequested)
                    .RespondAsync(x => x.Init<OrderStatus>(new
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState
                    }))
            );
            
            DuringAny(When(OrderSubmittedEvent)
                .Then(context =>
                {
                    context.Instance.CustomerNumber ??= context.Data.CustomerNumber;
                    context.Instance.SubmitedDate ??= context.Data.Timestamp;     
                }));
        }
            
        public State Submitted { get; private set; }
        public Event<OrderSubmited> OrderSubmittedEvent { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
    }

    public class OrderState : SagaStateMachineInstance, IVersionedSaga
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public int Version { get; set; }
        
        public string CustomerNumber { get; set; }
        public DateTime Updated { get; set; }
        public DateTime? SubmitedDate { get; set; }

    }
}