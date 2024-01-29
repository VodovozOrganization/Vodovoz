using MoreLinq;
using MoreLinq.Extensions;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using PacsCallState = Vodovoz.Core.Domain.Pacs.CallState;
using CallEvent = Vodovoz.Core.Domain.Pacs.CallEvent;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class CallModel : PropertyChangedBase
	{
		private readonly IEnumerable<OperatorModel> _operators;
		private bool _connected;

		public CallModel(IEnumerable<OperatorModel> operators)
		{
			CallEvents = new GenericObservableList<CallEvent>();
			_operators = operators ?? throw new ArgumentNullException(nameof(operators));
		}

		public OperatorModel Operator { get; set; }
		public DateTime Started => CallEvents.Last().EventTime;
		public DateTime Ended => CurrentState.EventTime;
		public TimeSpan Duration => Ended - Started;
		public CallEvent CurrentState => CallEvents.First();
		public GenericObservableList<CallEvent> CallEvents { get; }

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
					CheckConnected(callEvent);
					CheckMissed();
					return;
				}
			}
		}

		private void CheckConnected(CallEvent callEvent)
		{
			if(CurrentState.CallState == PacsCallState.Connected)
			{
				_connected = true;
				Operator = _operators.FirstOrDefault(x => x.CurrentState.PhoneNumber == callEvent.ToExtension);
				if(Operator != null)
				{
					Operator.ConnectedCall = this;
				}
			}

			if(CurrentState.CallState != PacsCallState.Connected && Operator != null)
			{
				Operator.ConnectedCall = null;
			}
		}

		private void CheckMissed()
		{
			if(CurrentState.CallState != PacsCallState.Disconnected)
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
