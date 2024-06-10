using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pacs.Operators.Client.Consumers
{
	public class GlobalBreakAvailabilityConsumer : IConsumer<GlobalBreakAvailabilityEvent>, IObservable<GlobalBreakAvailabilityEvent>, IDisposable
	{
		private List<IObserver<GlobalBreakAvailabilityEvent>> _observers;

		public GlobalBreakAvailabilityConsumer()
		{
			_observers = new List<IObserver<GlobalBreakAvailabilityEvent>>();
		}

		public async Task Consume(ConsumeContext<GlobalBreakAvailabilityEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<GlobalBreakAvailabilityEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<GlobalBreakAvailabilityEvent>> _observers;
			private readonly IObserver<GlobalBreakAvailabilityEvent> _observer;

			public Unsubscriber(List<IObserver<GlobalBreakAvailabilityEvent>> observers, IObserver<GlobalBreakAvailabilityEvent> observer)
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
