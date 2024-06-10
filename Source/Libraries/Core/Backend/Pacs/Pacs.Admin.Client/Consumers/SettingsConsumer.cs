using MassTransit;
using Pacs.Core.Messages.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pacs.Admin.Client.Consumers
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
			foreach(var observer in _observers.ToList())
			{
				observer.OnCompleted();
			}
		}
	}
}
