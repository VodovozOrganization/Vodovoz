using MoreLinq;
using MoreLinq.Extensions;
using QS.DomainModel.Entity;
using System;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class CallModel : PropertyChangedBase
	{
		private bool _connected;

		public OperatorModel Operator { get; set; }
		public DateTime Started => CallEvents.Last().EventTime;
		public DateTime Ended => CurrentState.EventTime;
		public CallEvent CurrentState => CallEvents.First();
		public GenericObservableList<CallEvent> CallEvents { get; set; }

		public event EventHandler CallMissed;

		public void AddEvent(CallEvent callEvent)
		{
			if(CallEvents.Count == 0)
			{
				CallEvents.Add(callEvent);
				CheckMissed();
				return;
			}

			for(int i = 0; i < CallEvents.Count; i++)
			{
				if(callEvent.CallSequence > CallEvents[i].CallSequence)
				{
					CallEvents.Insert(i, callEvent);
					OnPropertyChanged(nameof(CurrentState));
					CheckConnected();
					CheckMissed();
					return;
				}
			}
		}

		private void CheckConnected()
		{
			if(CurrentState.CallState == CallState.Connected)
			{
				_connected = true;
			}
		}

		private void CheckMissed()
		{
			if(CurrentState.CallState != CallState.Disconnected)
			{
				return;
			}

			if(!_connected)
			{
				CallMissed?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
