﻿using MassTransit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CallEvent = Pacs.Core.Messages.Events.CallEvent;
using DomainCallEvent = Vodovoz.Core.Domain.Pacs.CallEvent;

namespace Pacs.Calls.Consumers
{
	public class PacsCallEventConsumer : IConsumer<CallEvent>, IObservable<DomainCallEvent>
	{
		private List<IObserver<DomainCallEvent>> _observers;

		public PacsCallEventConsumer()
		{
			
			_observers = new List<IObserver<DomainCallEvent>>();
		}

		public async Task Consume(ConsumeContext<CallEvent> context)
		{
			foreach(var observer in _observers)
			{
				observer.OnNext(context.Message);
			}

			await Task.CompletedTask;
		}

		public IDisposable Subscribe(IObserver<DomainCallEvent> observer)
		{
			return new Unsubscriber(_observers, observer);
		}

		private class Unsubscriber : IDisposable
		{
			private readonly List<IObserver<DomainCallEvent>> _observers;
			private readonly IObserver<DomainCallEvent> _observer;

			public Unsubscriber(List<IObserver<DomainCallEvent>> observers, IObserver<DomainCallEvent> observer)
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
