using Core.Infrastructure;
using MassTransit;
using Pacs.Core.Messages.Events;
using Pacs.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Pacs.Operators.Client
{
	public class OperatorsOnBreakConsumer : IConsumer<OperatorsOnBreakEvent>, IObservable<OperatorsOnBreakEvent>, IDisposable
	{
		private List<IObserver<OperatorsOnBreakEvent>> _observers;

		public OperatorsOnBreakConsumer()
		{
			_observers = new List<IObserver<OperatorsOnBreakEvent>>();
		}

		public async Task Consume(ConsumeContext<OperatorsOnBreakEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<OperatorsOnBreakEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<OperatorsOnBreakEvent>> _observers;
			private readonly IObserver<OperatorsOnBreakEvent> _observer;

			public Unsubscriber(List<IObserver<OperatorsOnBreakEvent>> observers, IObserver<OperatorsOnBreakEvent> observer)
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

	public class OperatorsOnBreakConsumerDefinition : ConsumerDefinition<OperatorsOnBreakConsumer>
	{
		private readonly int _operatorId;

		public OperatorsOnBreakConsumerDefinition(IPacsOperatorProvider operatorProvider)
		{
			if(operatorProvider.OperatorId == null)
			{
				throw new PacsException("Невозможно получение состояния оператора. Так как в системе не определен оператор.");
			}
			_operatorId = operatorProvider.OperatorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.operators_on_break.consumer-operator-{_operatorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorsOnBreakConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OperatorStateEvent>();
			}
		}
	}
}
