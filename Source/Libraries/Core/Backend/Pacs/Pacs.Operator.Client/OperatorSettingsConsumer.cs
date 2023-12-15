using Core.Infrastructure;
using MassTransit;
using MassTransit.Transports;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pacs.Admin.Client
{
	public class OperatorSettingsConsumer : IConsumer<SettingsEvent>, IObservable<SettingsEvent>, IDisposable
	{
		private List<IObserver<SettingsEvent>> _observers;

		public OperatorSettingsConsumer()
		{
			_observers = new List<IObserver<SettingsEvent>>();
		}

		public async Task Consume(ConsumeContext<SettingsEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<SettingsEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<SettingsEvent>> _observers;
			private readonly IObserver<SettingsEvent> _observer;

			public Unsubscriber(List<IObserver<SettingsEvent>> observers, IObserver<SettingsEvent> observer)
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
			foreach(var observer in _observers.ToList())
			{
				observer.OnCompleted();
			}
		}
	}

	public class OperatorSettingsConsumerDefinition : ConsumerDefinition<OperatorSettingsConsumer>
	{
		private readonly int _operatorId;

		public OperatorSettingsConsumerDefinition(IPacsOperatorProvider operatorProvider)
		{
			if(operatorProvider.OperatorId == null)
			{
				throw new PacsInitException("Невозможно подключить получение настроек СКУД, " +
					"так как текущий пользователь не является оператором СКУД.");
			}

			_operatorId = operatorProvider.OperatorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.settings.consumer-operator-{_operatorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorSettingsConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<SettingsEvent>();
			}
		}
	}
}
