using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pacs.Operator.Client
{
	public class BreakAvailabilityConsumer : IConsumer<BreakAvailabilityEvent>, IObservable<BreakAvailabilityEvent>, IDisposable
	{
		private List<IObserver<BreakAvailabilityEvent>> _observers;

		public Guid Id { get; set; } = Guid.NewGuid();

		public BreakAvailabilityConsumer()
		{
			_observers = new List<IObserver<BreakAvailabilityEvent>>();
		}

		public async Task Consume(ConsumeContext<BreakAvailabilityEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}
			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<BreakAvailabilityEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<BreakAvailabilityEvent>> _observers;
			private readonly IObserver<BreakAvailabilityEvent> _observer;

			public Unsubscriber(List<IObserver<BreakAvailabilityEvent>> observers, IObserver<BreakAvailabilityEvent> observer)
			{
				_observers = observers;
				_observer = observer;
				_observers.Add(observer);
			}

			public void Dispose()
			{
				if(_observer == null)
				{
					return;
				}

				if(!_observers.Contains(_observer))
				{
					return;
				}

				_observers.Remove(_observer);
			}
		}

		public void Dispose()
		{
			foreach(var observer in _observers)
			{
				observer.OnCompleted();
			}
		}
	}

	public class BreakAvailabilityConsumerDefinition : ConsumerDefinition<BreakAvailabilityConsumer>
	{
		private readonly int _operatorId;

		public BreakAvailabilityConsumerDefinition(IPacsOperatorProvider operatorProvider)
		{
			if(operatorProvider.OperatorId == null)
			{
				throw new PacsException("Невозможно получение события возможности перерыва. Так как в системе не определен оператор.");
			}
			_operatorId = operatorProvider.OperatorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.break_availability.consumer-operator-{_operatorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<BreakAvailabilityConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<BreakAvailabilityEvent>();
			}
		}
	}
}
