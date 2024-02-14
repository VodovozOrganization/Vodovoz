using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Admin.Client.Consumers
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
			foreach(var observer in _observers.ToList())
			{
				observer.OnCompleted();
			}
		}
	}
}
