using QS.DomainModel.UoW;
using Stateless;
using System;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Core.Domain.Pacs;
using StateMachine = Stateless.StateMachine<
	Vodovoz.Core.Domain.Pacs.OperatorStateType,
	Pacs.Server.OperatorAgent.Trigger>;
using Timer = System.Timers.Timer;

namespace Pacs.Server
{
	public class OperatorAgent : IDisposable
	{
		private readonly IPacsSettings _pacsSettings;
		private readonly IOperatorRepository _operatorRepository;
		private readonly IOperatorNotifier _operatorNotifier;
		private readonly IPhoneController _phoneController;
		private readonly IUnitOfWorkFactory _uowFactory;

		private Timer _timer;
		private StateMachine _machine;
		private DateTime _disconnectedTime;
		private OperatorState _previuosState;

		private StateMachine.TriggerWithParameters<int> _connectTrigger;
		private StateMachine.TriggerWithParameters<string> _takeCallTrigger;
		private StateMachine.TriggerWithParameters<string> _changePhoneTrigger;
		private StateMachine.TriggerWithParameters<string> _startWorkShiftTrigger;
		private StateMachine.TriggerWithParameters<DisconnectionType> _disconnectTrigger;

		public event EventHandler<int> OnDisconnect;

		public Operator Operator { get; private set; }
		public OperatorState OperatorState { get; private set; }
		public OperatorSession Session { get; private set; }
		public int OperatorId => Operator.Id;

		public OperatorAgent(
			int operatorId,
			IPacsSettings pacsSettings,
			IOperatorRepository operatorRepository,
			IOperatorNotifier operatorNotifier,
			IPhoneController phoneController,
			IUnitOfWorkFactory uowFactory
		)
		{
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_operatorRepository = operatorRepository ?? throw new ArgumentNullException(nameof(operatorRepository));
			_operatorNotifier = operatorNotifier ?? throw new ArgumentNullException(nameof(operatorNotifier));
			_phoneController = phoneController ?? throw new ArgumentNullException(nameof(phoneController));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			_timer = new Timer();
			_timer.Interval = _pacsSettings.OperatorKeepAliveInterval.TotalMilliseconds;
			_timer.Elapsed += InactivityTimerElapsed;
			_disconnectedTime = DateTime.Now;

			LoadOperatorState(operatorId);

			ConfigureStateMachine();
		}

		#region Initialization

		private void LoadOperatorState(int operatorId)
		{
			OperatorState = _operatorRepository.GetOperatorState(operatorId);

			if(OperatorState == null || OperatorState.State == OperatorStateType.Disconnected)
			{
				CreateNew(operatorId);
			}
			else
			{
				Session = OperatorState.Session;
				StartCheckInactivity();
			}

			Operator = new Operator
			{
				Id = operatorId,
				State = OperatorState
			};
		}

		private void CreateNew(int operatorId)
		{
			OperatorState = new OperatorState
			{
				Started = DateTime.Now,
				OperatorId = operatorId,
				State = OperatorStateType.New,
			};
		}

		private void ConfigureStateMachine()
		{
			_machine = new StateMachine(() => OperatorState.State, ChangeState, FiringMode.Queued);

			//_connectTrigger = _machine.SetTriggerParameters<int>(Trigger.Connect);
			_takeCallTrigger = _machine.SetTriggerParameters<string>(Trigger.TakeCall);
			_changePhoneTrigger = _machine.SetTriggerParameters<string>(Trigger.ChangePhone);
			_startWorkShiftTrigger = _machine.SetTriggerParameters<string>(Trigger.StartWorkShift);
			_disconnectTrigger = _machine.SetTriggerParameters<DisconnectionType>(Trigger.Disconnect);

			_machine.Configure(OperatorStateType.New)
				.Permit(Trigger.Connect, OperatorStateType.Connected);

			_machine.Configure(OperatorStateType.Connected)
				.OnEntryFrom(Trigger.Connect, OnConnect)
				.OnEntryFrom(Trigger.EndWorkShift, ClearPhoneNumber)
				.InternalTransition(Trigger.KeepAlive, OnKeepAlive)
				.Permit(Trigger.StartWorkShift, OperatorStateType.WaitingForCall)
				.Permit(Trigger.Disconnect, OperatorStateType.Disconnected);

			_machine.Configure(OperatorStateType.WaitingForCall)
				.OnEntryFrom(Trigger.StartWorkShift, OnStartWorkShift)
				.InternalTransitionAsync(Trigger.ChangePhone, OnChangePhone)
				.InternalTransition(Trigger.KeepAlive, OnKeepAlive)
				.Permit(Trigger.TakeCall, OperatorStateType.Talk)
				.Permit(Trigger.StartBreak, OperatorStateType.Break)
				.Permit(Trigger.EndWorkShift, OperatorStateType.Connected)
				.Permit(Trigger.Disconnect, OperatorStateType.Disconnected);

			_machine.Configure(OperatorStateType.Talk)
				.OnEntryFrom(_takeCallTrigger, SetCall)
				.OnExit(ClearCall)
				.InternalTransition(Trigger.KeepAlive, OnKeepAlive)
				.Permit(Trigger.EndCall, OperatorStateType.WaitingForCall)
				.Permit(Trigger.Disconnect, OperatorStateType.Disconnected);

			_machine.Configure(OperatorStateType.Break)
				.InternalTransitionAsync(Trigger.ChangePhone, OnChangePhone)
				.InternalTransition(Trigger.KeepAlive, OnKeepAlive)
				.Permit(Trigger.EndBreak, OperatorStateType.WaitingForCall)
				.Permit(Trigger.EndWorkShift, OperatorStateType.Connected)
				.Permit(Trigger.Disconnect, OperatorStateType.Disconnected);

			_machine.Configure(OperatorStateType.Disconnected)
				.OnEntry(OnDisconnected);

			_machine.OnTransitionCompletedAsync(OnTransitionComplete);
		}

		#endregion Initialization

		private void ChangeState(OperatorStateType newState)
		{
			var timestamp = DateTime.Now;

			_previuosState = OperatorState;
			if(_previuosState.State != OperatorStateType.New)
			{
				_previuosState.Ended = timestamp.AddMilliseconds(-1);
			}

			OperatorState = OperatorState.Copy(_previuosState);
			OperatorState.State = newState;
			OperatorState.Started = timestamp;

			Operator.State = OperatorState;
		}

		public bool CanChangedBy(OperatorTrigger trigger)
		{
			return _machine.CanFire(ConvertTrigger(trigger));
		}


		#region Save

		private async Task OnTransitionComplete(StateMachine.Transition transition)
		{
			if(transition.Destination == OperatorStateType.New)
			{
				throw new InvalidOperationException(
					"Сохранение состояния в статусе New не поддерживается. " +
					"Необходимо проверить настройки состояний.");
			}

			OperatorState.Trigger = ConvertTrigger(transition.Trigger);

			await SaveState();
			await _operatorNotifier.OperatorChanged(OperatorState);

			if(transition.Destination == OperatorStateType.Disconnected)
			{

			}
		}

		private async Task SaveState()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				await uow.TrySaveAsync(Operator);
				await uow.TrySaveAsync(Session);
				await uow.TrySaveAsync(OperatorState);
				if(_previuosState.State != OperatorStateType.New)
				{
					await uow.TrySaveAsync(_previuosState);
				}

				await uow.CommitAsync();
			}
		}

		#endregion Save

		#region Connect

		public async Task Connect()
		{
			if(_machine.State == OperatorStateType.Connected)
			{
				return;
			}
			await _machine.FireAsync(Trigger.Connect);
		}

		private void OnConnect()
		{
			OperatorState.Started = DateTime.Now;
			OpenSession();
		}

		private void OpenSession()
		{
			if(Session != null)
			{
				return;
			}

			Session = new OperatorSession
			{
				Id = Guid.NewGuid(),
				OperatorId = OperatorId,
				Started = OperatorState.Started
			};

			OperatorState.Session = Session;

			StartCheckInactivity();
		}

		#endregion Connect

		#region Disconnect

		public async Task Disconnect()
		{
			await _machine.FireAsync(_disconnectTrigger, DisconnectionType.Manual);
		}
		private void OnDisconnected(StateMachine.Transition transition)
		{
			var reason = (DisconnectionType)transition.Parameters[0];
			OperatorState.DisconnectionType = reason;
			OperatorState.Started = _disconnectedTime + _pacsSettings.OperatorKeepAliveInterval;
			OperatorState.Ended = OperatorState.Started;

			CloseSession();
		}

		private void CloseSession()
		{
			if(Session == null)
			{
				return;
			}

			Session.Ended = OperatorState.Ended;
			StopCheckInactivity();
		}

		#endregion Disconnect

		#region Inactivity

		private void StartCheckInactivity()
		{
			_timer.Start();
		}

		private void StopCheckInactivity()
		{
			_timer.Stop();
		}

		private void InactivityTimerElapsed(object sender, ElapsedEventArgs e)
		{
			var inactivityPeriod = DateTime.Now - _disconnectedTime;
			if(inactivityPeriod > _pacsSettings.OperatorInactivityTimeout)
			{
				_machine.FireAsync(_disconnectTrigger, DisconnectionType.InactivityTimeout);
			}
		}

		public async Task KeepAlive()
		{
			await _machine.FireAsync(Trigger.KeepAlive);
		}

		private void OnKeepAlive()
		{
			_disconnectedTime = DateTime.Now;
		}

		#endregion Inactivity

		#region Work shift

		public async Task StartWorkShift(string phoneNumber)
		{
			await _machine.FireAsync(_startWorkShiftTrigger, phoneNumber);
		}

		private void OnStartWorkShift(StateMachine.Transition transition)
		{
			var phoneNumber = (string)transition.Parameters[0];
			OperatorState.PhoneNumber = phoneNumber;
		}

		public async Task EndWorkShift()
		{
			await _machine.FireAsync(Trigger.EndWorkShift);
		}

		#endregion Work shift

		#region Break

		public async Task StartBreak()
		{
			await _machine.FireAsync(Trigger.StartBreak);
		}

		public async Task EndBreak()
		{
			await _machine.FireAsync(Trigger.EndBreak);
		}

		#endregion Break

		#region Phone

		public async Task ChangePhone(string phoneNumber)
		{
			await _machine.FireAsync(_changePhoneTrigger, phoneNumber);
		}

		private async Task OnChangePhone(StateMachine.Transition transition)
		{
			var newPhone = (string)transition.Parameters[0];

			_phoneController.AssignPhone(newPhone, OperatorId);
			ChangeState(OperatorState.State);
			OperatorState.PhoneNumber = newPhone;

			await OnTransitionComplete(transition);
		}

		private void ClearPhoneNumber()
		{
			OperatorState.PhoneNumber = null;
		}

		#endregion Phone

		#region Call

		public async Task TakeCallEvent(string callId)
		{
			await _machine.FireAsync(_takeCallTrigger, callId);
		}

		private void SetCall(StateMachine.Transition transition)
		{
			var callId = (string)transition.Parameters[0];
			OperatorState.CallId = callId;
		}

		public async Task EndCallEvent()
		{
			await _machine.FireAsync(Trigger.EndCall);
		}

		private void ClearCall()
		{
			OperatorState.CallId = null;
		}

		#endregion

		#region Private Enum

		internal enum Trigger
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

		private OperatorTrigger ConvertTrigger(Trigger trigger)
		{
			switch(trigger)
			{
				case Trigger.Connect:
					return OperatorTrigger.Connect;
				case Trigger.StartWorkShift:
					return OperatorTrigger.StartWorkShift;
				case Trigger.TakeCall:
					return OperatorTrigger.TakeCall;
				case Trigger.EndCall:
					return OperatorTrigger.EndCall;
				case Trigger.StartBreak:
					return OperatorTrigger.StartBreak;
				case Trigger.EndBreak:
					return OperatorTrigger.EndBreak;
				case Trigger.ChangePhone:
					return OperatorTrigger.ChangePhone;
				case Trigger.EndWorkShift:
					return OperatorTrigger.EndWorkShift;
				case Trigger.Disconnect:
					return OperatorTrigger.Disconnect;
				case Trigger.KeepAlive:
				case Trigger.CheckInactivity:
				default:
					throw new InvalidOperationException(
						$"Триггер {trigger} не конвертируется в тип {nameof(OperatorTrigger)}, " +
						$"так как не предусмотрено его сохранение. " +
						$"Необходимо проверить настройки состояний.");
			}
		}

		private Trigger ConvertTrigger(OperatorTrigger trigger)
		{
			switch(trigger)
			{
				case OperatorTrigger.Connect:
					return Trigger.Connect;
				case OperatorTrigger.StartWorkShift:
					return Trigger.StartWorkShift;
				case OperatorTrigger.TakeCall:
					return Trigger.TakeCall;
				case OperatorTrigger.EndCall:
					return Trigger.EndCall;
				case OperatorTrigger.StartBreak:
					return Trigger.StartBreak;
				case OperatorTrigger.EndBreak:
					return Trigger.EndBreak;
				case OperatorTrigger.ChangePhone:
					return Trigger.ChangePhone;
				case OperatorTrigger.EndWorkShift:
					return Trigger.EndWorkShift;
				case OperatorTrigger.Disconnect:
					return Trigger.Disconnect;
				default:
					throw new InvalidOperationException(
						$"Неизвестный триггер {trigger}. " +
						$"Необходимо проверить настройки состояний.");
			}
		}

		#endregion Private Enum

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
