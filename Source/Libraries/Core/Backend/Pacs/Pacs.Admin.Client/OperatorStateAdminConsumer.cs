using Core.Infrastructure;
using MassTransit;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Client
{
	public class OperatorStateAdminConsumer : IConsumer<OperatorStateEvent>, IObservable<OperatorState>, IDisposable
	{
		private List<IObserver<OperatorState>> _observers;

		public OperatorStateAdminConsumer()
		{
			_observers = new List<IObserver<OperatorState>>();
		}

		public async Task Consume(ConsumeContext<OperatorStateEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message.State);
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

	public class OperatorStateAdminConsumerDefinition : ConsumerDefinition<OperatorStateAdminConsumer>
	{
		private readonly int _adminId;

		public OperatorStateAdminConsumerDefinition(IPacsAdministratorProvider adminProvider)
		{
			if(adminProvider.AdministratorId == null)
			{
				throw new PacsInitException("Невозможно подключить получение уведомлений от всех операторов, " +
					"так как текущий пользователь не является администратором СКУД.");
			}
			_adminId = adminProvider.AdministratorId.Value;

			Endpoint(x =>
			{
				var key = SimpleKeyGenerator.GenerateKey(16);
				x.Name = $"pacs.event.operator_state.consumer-admin-{_adminId}";
				x.InstanceId = $"-{key}";
			});
		}

		protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
			IConsumerConfigurator<OperatorStateAdminConsumer> consumerConfigurator)
		{
			endpointConfigurator.ConfigureConsumeTopology = false;

			if(endpointConfigurator is IRabbitMqReceiveEndpointConfigurator rmq)
			{
				rmq.AutoDelete = true;
				rmq.Durable = true;
				rmq.ExchangeType = ExchangeType.Fanout;

				rmq.Bind<OperatorStateEvent>(c =>
				{
					c.RoutingKey = $"pacs.operator.state.#";
				});
			}
		}
	}
}
