using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pacs.Operators.Client
{
	public class OperatorStateConsumer : IConsumer<OperatorStateEvent>, IObservable<OperatorStateEvent>, IDisposable
	{
		private List<IObserver<OperatorStateEvent>> _observers;

		public OperatorStateConsumer()
		{
			_observers = new List<IObserver<OperatorStateEvent>>();
		}

		public async Task Consume(ConsumeContext<OperatorStateEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<OperatorStateEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<OperatorStateEvent>> _observers;
			private readonly IObserver<OperatorStateEvent> _observer;

			public Unsubscriber(List<IObserver<OperatorStateEvent>> observers, IObserver<OperatorStateEvent> observer)
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

	public class OperatorStateConsumerDefinition : ConsumerDefinition<OperatorStateConsumer>
	{
		private readonly int _operatorId;

		public OperatorStateConsumerDefinition(IPacsOperatorProvider operatorProvider)
		{
			if(operatorProvider.OperatorId == null)
			{
				throw new PacsException("Невозможно получение состояния оператора. Так как в системе не определен оператор.");
			}
			_operatorId = operatorProvider.OperatorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.operator_state.consumer-operator-{_operatorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorStateConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OperatorStateEvent>(c =>
				{
					c.RoutingKey = $"pacs.operator.state.{_operatorId}.#";
				});
			}
		}
	}




	/*public class OperatorStateForAdminConsumerDefinition : ConsumerDefinition<OperatorStateConsumer>
	{
		private readonly int _administratorId;

		public OperatorStateForAdminConsumerDefinition()
		{
			EndpointName = $"pacs.operator_state.consumer-admin-{_administratorId}";
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorStateConsumer> consumerConfigurator)
		{
			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;

				rmq.Bind<OperatorState>(c =>
				{
					c.Durable = true;
					c.AutoDelete = true;
				});
			}
		}
	}*/
}
