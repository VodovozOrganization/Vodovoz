using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Server.Breaks;
using Pacs.Server.Phones;
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

namespace Pacs.Server.Operators
{
	public partial class OperatorServerStateMachine : IDisposable
	{
		private readonly ILogger<OperatorServerStateMachine> _logger;
		private readonly IPacsSettings _pacsSettings;
		private readonly IOperatorRepository _operatorRepository;
		private readonly IOperatorNotifier _operatorNotifier;
		private readonly IOperatorPhoneService _phoneController;
		private readonly IGlobalBreakController _globalBreakController;
		private readonly IOperatorBreakAvailabilityService _operatorBreakController;
		private readonly IUnitOfWorkFactory _uowFactory;

		private Timer _timer;
		private StateMachine _machine;
		private DateTime _disconnectedTime;
		private OperatorState _previuosState;

		private StateMachine.TriggerWithParameters<int> _connectTrigger;
		private StateMachine.TriggerWithParameters<string> _takeCallTrigger;
		private StateMachine.TriggerWithParameters<string> _changePhoneTrigger;
		private StateMachine.TriggerWithParameters<string> _startWorkShiftTrigger;
		private StateMachine.TriggerWithParameters<WorkShiftEndArgs> _endWorkShiftTrigger;
		private StateMachine.TriggerWithParameters<DisconnectionType> _disconnectTrigger;

		private StateMachine.TriggerWithParameters<BreakStartArgs> _startBreakTrigger;
		private StateMachine.TriggerWithParameters<BreakEndArgs> _endBreakTrigger;

		public event EventHandler<int> OnDisconnect;

		public int OperatorId { get; }
		public OperatorSession Session { get; private set; }
		public OperatorState OperatorState { get; private set; }

		public OperatorServerStateMachine(
			int operatorId,
			ILogger<OperatorServerStateMachine> logger,
			IPacsSettings pacsSettings,
			IOperatorRepository operatorRepository,
			IOperatorNotifier operatorNotifier,
			IOperatorPhoneService phoneController,
			IGlobalBreakController globalBreakController,
			IUnitOfWorkFactory uowFactory)
		{
			OperatorId = operatorId;
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_pacsSettings = pacsSettings ?? throw new ArgumentNullException(nameof(pacsSettings));
			_operatorRepository = operatorRepository ?? throw new ArgumentNullException(nameof(operatorRepository));
			_operatorNotifier = operatorNotifier ?? throw new ArgumentNullException(nameof(operatorNotifier));
			_phoneController = phoneController ?? throw new ArgumentNullException(nameof(phoneController));
			_globalBreakController = globalBreakController ?? throw new ArgumentNullException(nameof(globalBreakController));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));

			_timer = new Timer();
			_timer.Interval = _pacsSettings.OperatorKeepAliveInterval.TotalMilliseconds;
			_timer.Elapsed += InactivityTimerElapsed;
			_disconnectedTime = DateTime.Now;

			LoadOperatorState(OperatorId);

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
			_machine = new StateMachine(
				() => OperatorState.State,
				ChangeState,
				FiringMode.Queued);

			OperatorStateMachine.ConfigureBaseStates(_machine);

			_takeCallTrigger = _machine.SetTriggerParameters<string>(OperatorStateTrigger.TakeCall);
			_changePhoneTrigger = _machine.SetTriggerParameters<string>(OperatorStateTrigger.ChangePhone);
			_startWorkShiftTrigger = _machine.SetTriggerParameters<string>(OperatorStateTrigger.StartWorkShift);
			_endWorkShiftTrigger = _machine.SetTriggerParameters<WorkShiftEndArgs>(OperatorStateTrigger.EndWorkShift);
			_disconnectTrigger = _machine.SetTriggerParameters<DisconnectionType>(OperatorStateTrigger.Disconnect);
			_startBreakTrigger = _machine.SetTriggerParameters<BreakStartArgs>(OperatorStateTrigger.StartBreak);
			_endBreakTrigger = _machine.SetTriggerParameters<BreakEndArgs>(OperatorStateTrigger.EndBreak);

			_machine.Configure(OperatorStateType.Connected)
				.OnEntryFrom(OperatorStateTrigger.Connect, OnConnect)
				.OnEntryFrom(OperatorStateTrigger.EndWorkShift, OnEndWorkShift)
				.OnEntry(OnKeepAlive)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.WaitingForCall)
				.OnEntryFrom(OperatorStateTrigger.StartWorkShift, OnStartWorkShift)
				.OnEntry(OnKeepAlive)
				.InternalTransitionAsync(OperatorStateTrigger.ChangePhone, OnChangePhone)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.Talk)
				.OnEntryFrom(_takeCallTrigger, SetCall)
				.OnExit(ClearCall)
				.OnEntry(OnKeepAlive)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.Break)
				.OnEntryFrom(_startBreakTrigger, OnBreakStarted)
				.OnExit(OnBreakEnded)
				.OnEntry(OnKeepAlive)
				.InternalTransitionAsync(OperatorStateTrigger.ChangePhone, OnChangePhone)
				.InternalTransition(OperatorStateTrigger.KeepAlive, OnKeepAlive);

			_machine.Configure(OperatorStateType.Disconnected)
				.OnEntry(OnDisconnected);

			_machine.OnTransitionCompletedAsync(OnTransitionComplete);
		}

		#endregion Initialization

		public bool OperatorEnabled()
		{
			var @operator =  _operatorRepository.GetOperator(OperatorId);

			var operatorDisabled = !@operator.PacsEnabled && OperatorState.State.IsIn(
				OperatorStateType.Disconnected,
				OperatorStateType.New);

			return !operatorDisabled;
		}

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

			if(newState == OperatorStateType.WaitingForCall)
			{
				OperatorState.CallId = _previuosState.CallId;
			}

			if(OperatorState.State == OperatorStateType.WaitingForCall
				&& newState == OperatorStateType.Connected
				&& OperatorState.WorkShift?.Ended != null)
			{
				OperatorState.WorkShift = null;
			}

			OperatorState.State = newState;
			OperatorState.Started = timestamp;
			OperatorState.Ended = null;
		}

		public bool CanChangedBy(OperatorTrigger trigger)
		{
			return _machine.CanFire(trigger.ToOperatorStateTrigger());
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

			OperatorState.Trigger = transition.Trigger.ToOperatorTrigger();

			if(OperatorState.WorkShift != null
				&& OperatorState.WorkShift.Ended.HasValue
				&& transition.Trigger != OperatorStateTrigger.EndWorkShift)
			{
				OperatorState.WorkShift = null;
			}

			await SaveState();

			if(OperatorStateType.Break.IsIn(transition.Source, transition.Destination))
			{
				_globalBreakController.UpdateBreakAvailability();
			}
		}

		private async Task SaveState()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				_logger.LogInformation("Сохраняются состояния: {OperatorStateId}, {PreviousOperatorStateId}", OperatorState.Id, _previuosState.Id);
				await uow.SaveAsync(OperatorState);
				_logger.LogInformation("Сохранилось состояние: {OperatorStateId}", OperatorState.Id);
				await uow.SaveAsync(Session);
				if(_previuosState.State != OperatorStateType.New)
				{
					await uow.SaveAsync(_previuosState);
				}
				_logger.LogInformation("Сохранилось прошлое состояние: {PreviousOperatorStateId}", _previuosState.Id);
				await uow.CommitAsync();
			}
		}

		#endregion Save

		#region Connect

		public async Task Connect()
		{
			LoadOperatorState(OperatorId);

			if(_machine.CanFire(OperatorStateTrigger.Connect))
			{
				await _machine.FireAsync(OperatorStateTrigger.Connect);
			}
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

			_previuosState.Ended = OperatorState.Started;
			//У состояния Disconnected нет времени окончания, так как это конечное состояние
			OperatorState.Ended = OperatorState.Started;

			CloseSession();
			ClearPhoneNumber();

			OnDisconnect?.Invoke(this, OperatorId);

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
			try
			{
				var phoneNumber = (string)transition.Parameters[0];
				_phoneController.AssignPhone(phoneNumber, OperatorId);
				var @operator = _operatorRepository.GetOperator(OperatorId);
				OperatorState.PhoneNumber = phoneNumber;
				OperatorState.WorkShift = OperatorWorkshift.Create(OperatorId, DateTime.Now, @operator.WorkShift);
				_logger.LogInformation("Оператор {OperatorId} начал рабочую смену", OperatorId);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Не удалось начать смену: {ExceptionMessage}, возврат в состояние: {State}", ex.Message, _previuosState.State);
				if(transition.Destination != _previuosState.State)
				{
					ChangeState(_previuosState.State);
				}
			}
		}

		public async Task EndWorkShift(string reason)
		{
			var args = new WorkShiftEndArgs
			{
				Reason = reason,
				WorkShiftChangedBy = WorkShiftChangedBy.Operator
			};

			await _machine.FireAsync(_endWorkShiftTrigger, args);
		}

		private void OnEndWorkShift(StateMachine.Transition transition)
		{
			var args = (WorkShiftEndArgs)transition.Parameters[0];
			ClearPhoneNumber();

			if(OperatorState.WorkShift == null && args.AdminId != null)
			{
				_logger.LogWarning("Администратор {AdminId} завершил не существующую смену оператора {OperatorId} завершил рабочую смену по причине: {Reason}", args.AdminId, OperatorId, args.Reason ?? "Не указано");

				return;
			}

			if(!CanEndWorkshift(args.Reason))
			{
				throw new PacsException("Необходимо указать причину закрытия смены, если завершается раньше планируемого");
			}

			OperatorState.WorkShift.Ended = DateTime.Now;

			if(args.AdminId != null)
			{
				OperatorState.WorkShift.Reason = args.Reason;
				_logger.LogInformation("Администратор {AdminId} завершил смену оператора {OperatorId} завершил рабочую смену по причине: {Reason}", args.AdminId, OperatorId, args.Reason ?? "Не указано");
			}
			else
			{
				OperatorState.WorkShift.Reason = args.Reason;
				_logger.LogInformation("Оператор {OperatorId} завершил рабочую смену по причине: {Reason}", OperatorId, args.Reason ?? "Не указано");
			}
		}

		public bool CanEndWorkshift(string reason)
		{
			var currentWorkshift = OperatorState.WorkShift;
			if(currentWorkshift == null)
			{
				_logger.LogInformation("Попытка закрытия рабочей смены оператора {OperatorId}, которая не была открыта", OperatorId);
				return true;
			}

			var currentWorkshiftDuration = DateTime.Now - currentWorkshift.Started;
			var workshiftNotFinished = currentWorkshiftDuration < currentWorkshift.PlannedWorkShift.Duration;
			if(workshiftNotFinished)
			{
				if(reason.IsNullOrWhiteSpace())
				{
					_logger.LogInformation("Попытка закрытия не завершенной рабочей смены оператора {OperatorId}, без указания основания", OperatorId);
					return false;
				}
				else
				{
					_logger.LogInformation("Внеплановое завершение рабочей смены оператора {OperatorId}", OperatorId);
					return true;
				}
			}
			else
			{
				_logger.LogInformation("Плановое завершение рабочей смены оператора {OperatorId}", OperatorId);
				return true;
			}
		}

		#endregion Work shift

		#region Break

		public async Task StartBreak(OperatorBreakType breakType)
		{
			var args = new BreakStartArgs
			{
				BreakChangedBy = BreakChangedBy.Operator,
				BreakType = breakType,
			};
			await _machine.FireAsync(_startBreakTrigger, args);
		}

		public async Task AdminStartBreak(OperatorBreakType breakType, int adminId, string reason)
		{
			var args = new BreakStartArgs
			{
				BreakChangedBy = BreakChangedBy.Admin,
				BreakType = breakType,
				AdminId = adminId,
				Reason = reason
			};
			await _machine.FireAsync(_startBreakTrigger, args);
		}

		private void OnBreakStarted(StateMachine.Transition transition)
		{
			var args = (BreakStartArgs)transition.Parameters[0];
			if(args.BreakChangedBy == BreakChangedBy.Admin)
			{
				OperatorState.BreakChangedBy = BreakChangedBy.Admin;
				OperatorState.BreakChangedByAdminId = args.AdminId;
				OperatorState.BreakAdminReason = args.Reason;
				OperatorState.BreakType = args.BreakType;
				_logger.LogInformation("Оператор {OperatorId} начал перерыв, вызванный коммандой администратора {AdminId}", OperatorId, args.AdminId);
			}
			else
			{
				OperatorState.BreakType = args.BreakType;
				_logger.LogInformation("Оператор {OperatorId} начал перерыв", OperatorId);
			}
		}

		public async Task EndBreak()
		{
			var args = new BreakEndArgs
			{
				BreakChangedBy = BreakChangedBy.Operator,
			};

			await _machine.FireAsync(_endBreakTrigger, args);
		}

		public async Task AdminEndBreak(int adminId, string reason)
		{
			var args = new BreakEndArgs
			{
				BreakChangedBy = BreakChangedBy.Admin,
				AdminId = adminId,
				Reason = reason
			};

			await _machine.FireAsync(_endBreakTrigger, args);
		}

		public async Task AdminEndWorkShift(int adminId, string reason)
		{
			var args = new WorkShiftEndArgs
			{
				WorkShiftChangedBy = WorkShiftChangedBy.Admin,
				AdminId = adminId,
				Reason = reason
			};

			await _machine.FireAsync(_endWorkShiftTrigger, args);
		}

		private void OnBreakEnded(StateMachine.Transition transition)
		{
			if(transition.Parameters[0] is BreakEndArgs args)
			{
				if(args.BreakChangedBy == BreakChangedBy.Admin)
				{
					OperatorState.BreakChangedBy = BreakChangedBy.Admin;
					OperatorState.BreakChangedByAdminId = args.AdminId;
					OperatorState.BreakAdminReason = args.Reason;
					_logger.LogInformation("Оператор {OperatorId} завершил перерыв, вызванный коммандой администратора {AdminId}",
						OperatorId, args.AdminId);
				}
				else
				{
					_logger.LogInformation("Оператор {OperatorId} завершил перерыв", OperatorId);
				}
			}
			else
			{
				OperatorState.BreakChangedBy = BreakChangedBy.Operator;
				OperatorState.BreakAdminReason = "Завершен автоматически при закрытии смены";
			}
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
			_logger.LogInformation("Оператор {OperatorId} завершил звонок {CallId}", OperatorId, OperatorState?.CallId);
		}

		#endregion

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
