using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PacsCallEvent = Pacs.Core.Messages.Events.PacsCallEvent;

namespace Pacs.Calls.Consumers
{
	public class PacsCallEventConsumer : IConsumer<PacsCallEvent>, IObservable<PacsCallEvent>
	{
		private List<IObserver<PacsCallEvent>> _observers;

		public PacsCallEventConsumer()
		{
			
			_observers = new List<IObserver<PacsCallEvent>>();
		}

		public async Task Consume(ConsumeContext<PacsCallEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<PacsCallEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<PacsCallEvent>> _observers;
			private readonly IObserver<PacsCallEvent> _observer;

			public Unsubscriber(List<IObserver<PacsCallEvent>> observers, IObserver<PacsCallEvent> observer)
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
}
