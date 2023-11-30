using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Operator.Client
{
	public class OperatorStateConsumer : IConsumer<OperatorState>, IObservable<OperatorState>, IDisposable
	{
		private List<IObserver<OperatorState>> _observers;

		public OperatorStateConsumer()
		{
			_observers = new List<IObserver<OperatorState>>();
		}

		public async Task Consume(ConsumeContext<OperatorState> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<OperatorState> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<OperatorState>> _observers;
			private readonly IObserver<OperatorState> _observer;

			public Unsubscriber(List<IObserver<OperatorState>> observers, IObserver<OperatorState> observer)
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

				rmq.Bind<OperatorState>(c =>
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
