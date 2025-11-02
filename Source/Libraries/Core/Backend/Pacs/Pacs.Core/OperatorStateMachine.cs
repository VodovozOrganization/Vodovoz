using Stateless;
using System.Linq;
using Vodovoz.Core.Domain.Pacs;
using StateMachine = Stateless.StateMachine<
	Vodovoz.Core.Domain.Pacs.OperatorStateType,
	Pacs.Core.OperatorStateTrigger>;

namespace Pacs.Core
{
	public class OperatorStateMachine : IOperatorStateMachine
	{
		private StateMachine _machine;
		public OperatorState OperatorState { get; set; }

		public OperatorStateMachine()
		{
			ConfigureStateMachine();
		}

		public bool CanStartWorkShift => _machine.CanFire(OperatorStateTrigger.StartWorkShift);
		public bool CanEndWorkShift => _machine.CanFire(OperatorStateTrigger.EndWorkShift);
		public bool CanChangePhone => _machine.CanFire(OperatorStateTrigger.ChangePhone);
		public bool CanStartBreak => _machine.CanFire(OperatorStateTrigger.StartBreak);
		public bool CanEndBreak => _machine.CanFire(OperatorStateTrigger.EndBreak);

		public bool OnWorkshift
		{
			get
			{
				var onWorkshiftStates = new[]
				{
					OperatorStateType.WaitingForCall,
					OperatorStateType.Talk,
					OperatorStateType.Break
				};
				return onWorkshiftStates.Contains(OperatorState.State);
			}
		}

		private void ConfigureStateMachine()
		{
			_machine = new StateMachine<OperatorStateType, OperatorStateTrigger>(
				StateAccessor,
				StateMutator,
				FiringMode.Queued);

			ConfigureBaseStates(_machine);

			_machine.Configure(OperatorStateType.WaitingForCall)
				.PermitReentry(OperatorStateTrigger.ChangePhone);

			_machine.Configure(OperatorStateType.Break)
				.PermitReentry(OperatorStateTrigger.ChangePhone);
		}

		private OperatorStateType StateAccessor()
		{
			if(OperatorState == null)
			{
				return OperatorStateType.Disconnected;
			}

			return OperatorState.State;
		}

		private void StateMutator(OperatorStateType newState)
		{
			if(OperatorState == null)
			{
				return;
			}

			OperatorState.State = newState;
		}

		public static void ConfigureBaseStates(StateMachine machine)
		{
			machine.Configure(OperatorStateType.New)
				.Permit(OperatorStateTrigger.Connect, OperatorStateType.Connected);

			machine.Configure(OperatorStateType.Connected)
				.Permit(OperatorStateTrigger.StartWorkShift, OperatorStateType.WaitingForCall)
				.Permit(OperatorStateTrigger.Disconnect, OperatorStateType.Disconnected);

			machine.Configure(OperatorStateType.WaitingForCall)
				.Permit(OperatorStateTrigger.TakeCall, OperatorStateType.Talk)
				.Permit(OperatorStateTrigger.StartBreak, OperatorStateType.Break)
				.Permit(OperatorStateTrigger.EndWorkShift, OperatorStateType.Connected)
				.Permit(OperatorStateTrigger.Disconnect, OperatorStateType.Disconnected);

			machine.Configure(OperatorStateType.Talk)
				.Permit(OperatorStateTrigger.EndCall, OperatorStateType.WaitingForCall)
				.Permit(OperatorStateTrigger.Disconnect, OperatorStateType.Disconnected);

			machine.Configure(OperatorStateType.Break)
				.Permit(OperatorStateTrigger.EndBreak, OperatorStateType.WaitingForCall)
				.Permit(OperatorStateTrigger.EndWorkShift, OperatorStateType.Connected)
				.Permit(OperatorStateTrigger.Disconnect, OperatorStateType.Disconnected);

			machine.Configure(OperatorStateType.Disconnected)
				.Permit(OperatorStateTrigger.Connect, OperatorStateType.Connected);
		}
	}
}
