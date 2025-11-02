using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pacs.Operators.Client.Consumers
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
			foreach(var observer in _observers.ToList())
			{
				observer.OnCompleted();
			}
		}
	}
}
