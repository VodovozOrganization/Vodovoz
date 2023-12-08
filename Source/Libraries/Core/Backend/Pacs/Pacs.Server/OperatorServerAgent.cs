using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Server;
using QS.DomainModel.UoW;
using Stateless;
using System;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Settings.Pacs;
using StateMachine = Stateless.StateMachine<
	Vodovoz.Core.Domain.Pacs.OperatorStateType,
	Pacs.Core.OperatorStateTrigger>;
using Timer = System.Timers.Timer;

namespace Pacs.Server
{
	public class OperatorServerAgent : IDisposable
	{
		private readonly ILogger<OperatorServerAgent> _logger;
		private readonly IPacsSettings _pacsSettings;
		private readonly IOperatorRepository _operatorRepository;
		private readonly IOperatorNotifier _operatorNotifier;
		private readonly IPhoneController _phoneController;
		private readonly OperatorBreakController _operatorBreakController;
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

		public OperatorBreakAvailability BreakAvailability { get; private set; }

		public Operator Operator { get; private set; }
		public OperatorState OperatorState { get; private set; }
		public OperatorSession Session { get; private set; }
		public int OperatorId => Operator.Id;

		public OperatorServerAgent(
			int operatorId,
			ILogger<OperatorServerAgent> logger,
			IPacsSettings pacsSettings,
			IOperatorRepository operatorRepository,
			IOperatorNotifier operatorNotifier,
			IPhoneController phoneController,
			OperatorBreakController operatorBreakController,
			IUnitOfWorkFactory uowFactory
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_operatorRepository = operatorRepository ?? throw new ArgumentNullException(nameof(operatorRepository));
			_operatorNotifier = operatorNotifier ?? throw new ArgumentNullException(nameof(operatorNotifier));
			_phoneController = phoneController ?? throw new ArgumentNullException(nameof(phoneController));
			_operatorBreakController = operatorBreakController ?? throw new ArgumentNullException(nameof(operatorBreakController));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			_timer = new Timer();
			_timer.Interval = _pacsSettings.OperatorKeepAliveInterval.TotalMilliseconds;
			_timer.Elapsed += InactivityTimerElapsed;
			_disconnectedTime = DateTime.Now;

			LoadOperatorState(operatorId);

			BreakAvailability = _operatorBreakController.GetBreakAvailability();
			_operatorBreakController.BreakAvailabilityChanged += BreakAvailabilityChanged;

			ConfigureStateMachine();
		}

		private void BreakAvailabilityChanged(object sender, OperatorBreakAvailability newBreakAvailability)
		{
			BreakAvailability = newBreakAvailability;
			_operatorNotifier.OperatorChanged(OperatorState, BreakAvailability);
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

			OperatorStateAgent.ConfigureBaseStates(_machine);

			_takeCallTrigger = _machine.SetTriggerParameters<string>(OperatorStateTrigger.TakeCall);
			_changePhoneTrigger = _machine.SetTriggerParameters<string>(OperatorStateTrigger.ChangePhone);
			_startWorkShiftTrigger = _machine.SetTriggerParameters<string>(OperatorStateTrigger.StartWorkShift);
			_disconnectTrigger = _machine.SetTriggerParameters<DisconnectionType>(OperatorStateTrigger.Disconnect);

			_machine.Configure(OperatorStateType.Connected)
				.OnEntryFrom(OperatorStateTrigger.Connect, OnConnect)
				.OnEntryFrom(OperatorStateTrigger.EndWorkShift, OnEndWorkShift)
				.OnActivate(OnKeepAlive)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.WaitingForCall)
				.OnEntryFrom(OperatorStateTrigger.StartWorkShift, OnStartWorkShift)
				.OnActivate(OnKeepAlive)
				.InternalTransitionAsync(OperatorStateTrigger.ChangePhone, OnChangePhone)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.Talk)
				.OnEntryFrom(_takeCallTrigger, SetCall)
				.OnExit(ClearCall)
				.OnActivate(OnKeepAlive)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.Break)
				.OnEntry(OnBreakStarted)
				.OnExit(OnBreakEnded)
				.OnActivate(OnKeepAlive)
				.InternalTransitionAsync(OperatorStateTrigger.ChangePhone, OnChangePhone)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.Disconnected)
				.OnEntry(OnDisconnected);

			_machine.OnTransitionCompletedAsync(OnTransitionComplete);
		}

		#endregion Initialization

		private void ChangeState(OperatorStateType newState)
		{
			var timestamp = DateTime.Now;

			_previuosState = OperatorState;
			switch(_previuosState.State)
			{
				case OperatorStateType.Connected:
				case OperatorStateType.WaitingForCall:
				case OperatorStateType.Talk:
				case OperatorStateType.Break:
					_previuosState.Ended = timestamp.AddMilliseconds(-1);
					break;
				case OperatorStateType.New:
				case OperatorStateType.Disconnected:
				default:
					break;
			}

			OperatorState = OperatorState.Copy(_previuosState);
			OperatorState.State = newState;
			OperatorState.Started = timestamp;
			OperatorState.Ended = null;

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

			if(OperatorStateType.Break.IsIn(transition.Source, transition.Destination))
			{
				BreakAvailability = _operatorBreakController.GetBreakAvailability();
			}

			await _operatorNotifier.OperatorChanged(OperatorState, BreakAvailability);
		}

		private async Task SaveState()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				await uow.SaveAsync(Operator);
				await uow.SaveAsync(Session);
				await uow.SaveAsync(OperatorState);
				if(_previuosState.State != OperatorStateType.New)
				{
					await uow.SaveAsync(_previuosState);
				}

				await uow.CommitAsync();
			}
		}

		#endregion Save

		#region Connect

		public async Task Connect()
		{
			await _machine.FireAsync(OperatorStateTrigger.Connect);
		}

		private void OnConnect()
		{
			ClearPhoneNumber();
			OperatorState.Started = DateTime.Now;
			OpenSession();
			_logger.LogInformation("Оператор {OperatorId} подключен.", OperatorId);
		}

		private void OpenSession()
		{
			if(Session != null)
			{
				CloseSession();
			}

			Session = new OperatorSession
			{
				Id = Guid.NewGuid(),
				OperatorId = OperatorId,
				Started = OperatorState.Started
			};

			OperatorState.Session = Session;

			StartCheckInactivity();
			_logger.LogInformation("Открыта сессия {SessionId} для оператора {OperatorId}.", Session.Id, OperatorId);
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
			if(reason == DisconnectionType.InactivityTimeout)
			{
				OperatorState.Started = _disconnectedTime + _pacsSettings.OperatorKeepAliveInterval;
			}
			else
			{
				OperatorState.Started = _disconnectedTime;
			}
			OperatorState.Ended = OperatorState.Started;

			CloseSession();
			ClearPhoneNumber();

			var reasonExplain = reason == DisconnectionType.InactivityTimeout ? "автоматически по таймауту" : "сам";
			_logger.LogInformation($"Оператор {{OperatorId}} отключился {reasonExplain}", OperatorId);
		}

		private void CloseSession()
		{
			if(Session == null)
			{
				return;
			}

			Session.Ended = OperatorState.Ended;
			StopCheckInactivity();

			var endTimeExplain = Session.Ended.HasValue ? " Завершена в {SesstionEndTime}." : "";
			_logger.LogInformation("Закрыта сессия {SessionId} для оператора {OperatorId}." + endTimeExplain, 
				Session.Id, OperatorId, Session.Ended?.ToString("dd.MM.yyyy HH:mm:ss"));
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
			await _machine.FireAsync(OperatorStateTrigger.KeepAlive);
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
			_phoneController.AssignPhone(phoneNumber, OperatorId);
			OperatorState.PhoneNumber = phoneNumber;
			_logger.LogInformation("Оператор {OperatorId} начал рабочую смену", OperatorId);
		}

		public async Task EndWorkShift()
		{
			await _machine.FireAsync(OperatorStateTrigger.EndWorkShift);
		}

		private void OnEndWorkShift(StateMachine.Transition transition)
		{
			ClearPhoneNumber();
			_logger.LogInformation("Оператор {OperatorId} завершил рабочую смену", OperatorId);
		}

		#endregion Work shift

		#region Break

		public async Task StartBreak()
		{
			await _machine.FireAsync(OperatorStateTrigger.StartBreak);
		}

		private void OnBreakStarted()
		{
			//_operatorBreakController.StartBreak(OperatorId);
			_logger.LogInformation("Оператор {OperatorId} начал перерыв", OperatorId);
		}

		public async Task EndBreak()
		{
			await _machine.FireAsync(OperatorStateTrigger.EndBreak);
		}

		private void OnBreakEnded()
		{
			//_operatorBreakController.EndBreak(OperatorId);
			_logger.LogInformation("Оператор {OperatorId} завершил перерыв", OperatorId);
		}

		#endregion Break

		#region Phone

		public async Task ChangePhone(string phoneNumber)
		{
			await _machine.FireAsync(_changePhoneTrigger, phoneNumber);
		}

		private async Task OnChangePhone(StateMachine.Transition transition)
		{
			var oldPhone = OperatorState.PhoneNumber;
			var newPhone = (string)transition.Parameters[0];

			_phoneController.ReleasePhone(oldPhone);
			_phoneController.AssignPhone(newPhone, OperatorId);
			ChangeState(OperatorState.State);
			OperatorState.PhoneNumber = newPhone;

			await OnTransitionComplete(transition);
			_logger.LogInformation("Оператор {OperatorId} сменил телефон с {OldPhone} на {NewPhone}", OperatorId, oldPhone, newPhone);
		}

		private void ClearPhoneNumber()
		{
			_phoneController.ReleasePhone(OperatorState.PhoneNumber);
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
			_logger.LogInformation("Оператор {OperatorId} принял звонок {CallId}", OperatorId, callId);
		}

		public async Task EndCallEvent()
		{
			await _machine.FireAsync(OperatorStateTrigger.EndCall);
		}

		private void ClearCall()
		{
			var callId = OperatorState.CallId;
			OperatorState.CallId = null;
			_logger.LogInformation("Оператор {OperatorId} завершил звонок {CallId}", OperatorId, callId);
		}

		#endregion

		#region Private Enum

		private OperatorTrigger ConvertTrigger(OperatorStateTrigger trigger)
		{
			switch(trigger)
			{
				case OperatorStateTrigger.Connect:
					return OperatorTrigger.Connect;
				case OperatorStateTrigger.StartWorkShift:
					return OperatorTrigger.StartWorkShift;
				case OperatorStateTrigger.TakeCall:
					return OperatorTrigger.TakeCall;
				case OperatorStateTrigger.EndCall:
					return OperatorTrigger.EndCall;
				case OperatorStateTrigger.StartBreak:
					return OperatorTrigger.StartBreak;
				case OperatorStateTrigger.EndBreak:
					return OperatorTrigger.EndBreak;
				case OperatorStateTrigger.ChangePhone:
					return OperatorTrigger.ChangePhone;
				case OperatorStateTrigger.EndWorkShift:
					return OperatorTrigger.EndWorkShift;
				case OperatorStateTrigger.Disconnect:
					return OperatorTrigger.Disconnect;
				case OperatorStateTrigger.KeepAlive:
				case OperatorStateTrigger.CheckInactivity:
				default:
					throw new InvalidOperationException(
						$"Триггер {trigger} не конвертируется в тип {nameof(OperatorTrigger)}, " +
						$"так как не предусмотрено его сохранение. " +
						$"Необходимо проверить настройки состояний.");
			}
		}

		private OperatorStateTrigger ConvertTrigger(OperatorTrigger trigger)
		{
			switch(trigger)
			{
				case OperatorTrigger.Connect:
					return OperatorStateTrigger.Connect;
				case OperatorTrigger.StartWorkShift:
					return OperatorStateTrigger.StartWorkShift;
				case OperatorTrigger.TakeCall:
					return OperatorStateTrigger.TakeCall;
				case OperatorTrigger.EndCall:
					return OperatorStateTrigger.EndCall;
				case OperatorTrigger.StartBreak:
					return OperatorStateTrigger.StartBreak;
				case OperatorTrigger.EndBreak:
					return OperatorStateTrigger.EndBreak;
				case OperatorTrigger.ChangePhone:
					return OperatorStateTrigger.ChangePhone;
				case OperatorTrigger.EndWorkShift:
					return OperatorStateTrigger.EndWorkShift;
				case OperatorTrigger.Disconnect:
					return OperatorStateTrigger.Disconnect;
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
