using MassTransit;
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
