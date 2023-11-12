using Stateless;
using Vodovoz.Core.Domain.Pacs;
using StateMachine = Stateless.StateMachine<
	Vodovoz.Core.Domain.Pacs.OperatorStateType,
	Pacs.Core.OperatorStateTrigger>;

namespace Pacs.Core
{
	public interface IOperatorStateAgent
	{
		OperatorState OperatorState { get; set; }

		bool CanChangePhone { get; }
		bool CanEndBreak { get; }
		bool CanEndWorkShift { get; }
		bool CanStartBreak { get; }
		bool CanStartWorkShift { get; }
	}

	public class OperatorStateAgent : IOperatorStateAgent
	{
		private StateMachine _machine;
		public OperatorState OperatorState { get; set; }

		public OperatorStateAgent()
		{
			ConfigureStateMachine();
		}

		public bool CanStartWorkShift => _machine.CanFire(OperatorStateTrigger.StartWorkShift);
		public bool CanEndWorkShift => _machine.CanFire(OperatorStateTrigger.EndWorkShift);
		public bool CanChangePhone => _machine.CanFire(OperatorStateTrigger.ChangePhone);
		public bool CanStartBreak => _machine.CanFire(OperatorStateTrigger.StartBreak);
		public bool CanEndBreak => _machine.CanFire(OperatorStateTrigger.EndBreak);

		private void ConfigureStateMachine()
		{
			_machine = new StateMachine(
				() => OperatorState.State,
				(newState) => OperatorState.State = newState,
				FiringMode.Queued);

			ConfigureBaseStates(_machine);
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
		}
	}

	public enum OperatorStateTrigger
	{
		Connect,
		StartWorkShift,
		TakeCall,
		EndCall,
		StartBreak,
		EndBreak,
		ChangePhone,
		EndWorkShift,
		Disconnect,
		KeepAlive,
		CheckInactivity
	}
}
