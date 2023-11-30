using Core.Infrastructure;
using MassTransit;
using MassTransit.Transports;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pacs.Admin.Client
{
	public class SettingsConsumer : IConsumer<SettingsEvent>, IObservable<SettingsEvent>, IDisposable
	{
		private List<IObserver<SettingsEvent>> _observers;

		public SettingsConsumer()
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
			foreach(var observer in _observers)
			{
				observer.OnCompleted();
			}
		}
	}

	public class SettingsConsumerDefinition : ConsumerDefinition<SettingsConsumer>
	{
		private readonly int _administratorId;

		public SettingsConsumerDefinition(IPacsAdministratorProvider adminProvider)
		{
			if(adminProvider.AdministratorId == null)
			{
				throw new PacsInitException("Невозможно подключить получение сообщений администратора, " +
					"так как текущий пользователь не является администратором СКУД.");
			}

			_administratorId = adminProvider.AdministratorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.settings.consumer-admin-{_administratorId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<SettingsConsumer> consumerConfigurator)
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
