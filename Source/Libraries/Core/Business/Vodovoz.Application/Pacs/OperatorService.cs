using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client;
using Pacs.Operators.Client.Consumers;
using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Application.Mango;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;
using Timer = System.Timers.Timer;

namespace Vodovoz.Application.Pacs
{
	public class OperatorService : PropertyChangedBase, 
		IObserver<GlobalBreakAvailabilityEvent>,
		IObserver<SettingsEvent>,
		IObserver<OperatorsOnBreakEvent>,
		IDisposable
	{
		private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(10);

		private readonly ILogger<OperatorService> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly Employee _employee;
		private readonly IOperatorClient _client;
		private readonly IMangoManager _mangoManager;
		private readonly OperatorKeepAliveController _operatorKeepAliveController;
		private readonly IOperatorStateMachine _operatorStateAgent;
		private readonly IPacsRepository _pacsRepository;
		private readonly IObservable<GlobalBreakAvailabilityEvent> _globalBreakPublisher;
		private readonly OperatorSettingsConsumer _operatorSettingsConsumer;
		private readonly IObservable<OperatorsOnBreakEvent> _operatorsOnBreakPublisher;
		private readonly IPacsEmployeeProvider _pacsEmployeeProvider;
		private Timer _connectingTimer;
		private Timer _delayedBreakUpdateTimer;
		private bool _isConnected;
		private bool _isConnecting;
		private bool _breakInProgress;
		private OperatorBreakAvailability _breakAvailability;
		private GlobalBreakAvailabilityEvent _globalBreakAvailability;
		private IPacsDomainSettings _settings;
		private IEnumerable<OperatorState> _operatorsonBreak = Enumerable.Empty<OperatorState>();
		private PacsState _pacsState;
		private bool _canStartLongBreak;
		private BreakState _longBreakState;
		private bool _canStartShortBreak;
		private BreakState _shortBreakState;
		private bool _canEndBreak;
		private IEnumerable<string> _availablePhones;

		private IDisposable _breakAvailabilitySubscription;
		private IDisposable _settingsSubscription;
		private IDisposable _operatorsOnBreakSubscription;

		public OperatorService(
			ILogger<OperatorService> logger,
			IEmployeeService employeeService,
			IOperatorClient operatorClient,
			IMangoManager mangoManager,
			IOperatorStateMachine operatorStateAgent,
			IPacsRepository pacsRepository,
			IObservable<GlobalBreakAvailabilityEvent> globalBreakPublisher,
			OperatorSettingsConsumer operatorSettingsConsumer,
			IObservable<OperatorsOnBreakEvent> operatorsOnBreakPublisher,
			IPacsEmployeeProvider pacsEmployeeProvider,
			OperatorKeepAliveController operatorKeepAliveController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_operatorStateAgent = operatorStateAgent ?? throw new ArgumentNullException(nameof(operatorStateAgent));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
			_globalBreakPublisher = globalBreakPublisher ?? throw new ArgumentNullException(nameof(globalBreakPublisher));
			_operatorSettingsConsumer = operatorSettingsConsumer ?? throw new ArgumentNullException(nameof(operatorSettingsConsumer));
			_operatorsOnBreakPublisher = operatorsOnBreakPublisher ?? throw new ArgumentNullException(nameof(operatorsOnBreakPublisher));
			_pacsEmployeeProvider = pacsEmployeeProvider ?? throw new ArgumentNullException(nameof(pacsEmployeeProvider));
			_client = operatorClient ?? throw new ArgumentNullException(nameof(operatorClient));
			_operatorKeepAliveController = operatorKeepAliveController ?? throw new ArgumentNullException(nameof(operatorKeepAliveController));

			_breakAvailability = new OperatorBreakAvailability();
			_globalBreakAvailability = new GlobalBreakAvailabilityEvent { EventId = Guid.NewGuid() };
			AvailablePhones = new List<string>();
			_delayedBreakUpdateTimer = new Timer();
			_delayedBreakUpdateTimer.Elapsed += async (s, e) => await OnBreakAvailabilityTimerElapsedAsync(s, e);

			_employee = _employeeService.GetEmployeeForCurrentUser();

			IsAdministrator = _pacsEmployeeProvider.IsAdministrator;
			IsOperator = _pacsEmployeeProvider.IsOperator;
			IsInitialized = _client.OperatorId.HasValue || IsAdministrator;

			_mangoManager.PropertyChanged += MangoManagerPropertyChanged;

			if(IsOperator)
			{
				StartConnecting();
			}
			else
			{
				UpdateMango();
			}
		}

		public bool IsInitialized { get; }
		public bool IsAdministrator { get; }
		public bool IsOperator { get; }

		public virtual bool IsConnected
		{
			get => _isConnected;
			private set => SetField(ref _isConnected, value);
		}

		private async Task OnBreakAvailabilityTimerElapsedAsync(object sender,  ElapsedEventArgs e)
		{
			await RefreshBreakAvailability();
			UpdateBreakInfo();
		}

		private void StartConnecting()
		{
			if(!IsInitialized || !IsOperator)
			{
				_logger.LogWarning("Подключение невозможно, так как не инициализирован сервис оператора");
				return;
			}

			if(_connectingTimer != null)
			{
				return;
			}

			_connectingTimer = new Timer(5000);
			_connectingTimer.Elapsed += OnReconnectTimerElapsed;
			_connectingTimer.Start();
		}

		private void OnReconnectTimerElapsed(object sender, ElapsedEventArgs e)
		{
			if(OperatorState == null || OperatorState.State == OperatorStateType.Disconnected)
			{
				try
				{
					Connect().Wait();
					_operatorKeepAliveController.Start();
					AvailablePhones = _pacsRepository.GetAvailablePhones();
					OperatorsOnBreak = _client.GetOperatorsOnBreak().Result.OnBreak;
					_settings = _pacsRepository.GetPacsDomainSettings();
					UpdateMango();
					SubscribeEvents();
					_connectingTimer.Elapsed -= OnReconnectTimerElapsed;
				}
				catch(AggregateException ex)
				{
					int counter = 1;

					foreach(var exception in ex.InnerExceptions)
					{
						_logger.LogError(
							ex,
							"Произошла ошибка при подключении к службам СКУД ({CurrentExceptionNumber}/{ExceptionsCount}): {ExceptionMessage}",
							exception.Message,
							counter,
							ex.InnerExceptions.Count);

						counter++;
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Произошла ошибка при подключении к службам СКУД: {ExceptionMessage}", ex.Message);
				}
			}
		}

		private async Task Connect()
		{
			try
			{
				if(_isConnecting)
				{
					return;
				}
				_isConnecting = true;
				var stateEvent = await _client.Connect();
				SetState(stateEvent);
				_client.StateChanged += StateChanged;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время подключения оператора");
				_isConnecting = false;
				return;
			}
			_isConnecting = false;
			IsConnected = true;
		}

		private async Task Disconnect()
		{
			_connectingTimer.Stop();
			_client.StateChanged -= StateChanged;
			var stateEvent = await _client.Disconnect();
			SetState(stateEvent);
			UnsubscribeEvents();
		}

		private void StateChanged(object sender, OperatorStateEvent stateEvent)
		{
			SetState(stateEvent);
		}

		private void SetState(OperatorStateEvent stateEvent)
		{
			_logger.LogInformation(
				"Изменение состояния оператора с {PreviousOperatorState} на {NewOperatorState}",
				OperatorState?.State,
				stateEvent?.State?.State);

			var previousShortBreakAvailable = BreakAvailability?.ShortBreakAvailable;
			var previousLongBreakAvailable = BreakAvailability?.LongBreakAvailable;

			if(OperatorState == null || OperatorState.Id != stateEvent.State.Id)
			{
				OperatorState = stateEvent.State;
			}

			_logger.LogInformation(
				"Изменение доступности малого перерыва оператора с {PreviousShortBreakAvailability} на {NewShortBreakAvailability}," +
				" большого перерыва оператора с {PreviousLongBreakAvailability} на {NewLongBreakAvailability}",
				previousShortBreakAvailable,
				stateEvent?.BreakAvailability?.ShortBreakAvailable,
				previousLongBreakAvailable,
				stateEvent?.BreakAvailability?.LongBreakAvailable);

			if(BreakAvailability == null || !BreakAvailability.Equals(stateEvent.BreakAvailability))
			{
				BreakAvailability = stateEvent.BreakAvailability;
			}
		}

		public async Task RefreshBreakAvailability()
		{
			if(OperatorState is null)
			{
				return;
			}

			var globalBreakAvailability = await _client.GetOperatorBreakAvailability(OperatorState.OperatorId);

			BreakAvailability = globalBreakAvailability;
		}

		private void SubscribeEvents()
		{
			_breakAvailabilitySubscription = _globalBreakPublisher.Subscribe(this);
			_settingsSubscription = _operatorSettingsConsumer.Subscribe(this);
			_operatorsOnBreakSubscription = _operatorsOnBreakPublisher.Subscribe(this);
		}

		private void UnsubscribeEvents()
		{
			_breakAvailabilitySubscription?.Dispose();
			_settingsSubscription?.Dispose();
			_operatorsOnBreakSubscription?.Dispose();
		}

		#region Pacs

		public virtual PacsState PacsState
		{
			get => _pacsState;
			private set => SetField(ref _pacsState, value);
		}

		public virtual OperatorState OperatorState
		{
			get => _operatorStateAgent.OperatorState;
			private set
			{
				if(_operatorStateAgent.OperatorState != value)
				{
					_operatorStateAgent.OperatorState = value;
					OnPropertyChanged(nameof(OperatorState));
					OnPropertyChanged(nameof(CanChangePhone));
					OnPropertyChanged(nameof(CanStartWorkShift));
					OnPropertyChanged(nameof(CanEndWorkShift));

					UpdateBreakInfo();
					UpdateLongBreak();
					UpdateShortBreak();
					UpdateEndBreak();
					UpdatePacsState();
					UpdateMango();
				}
			}
		}

		private void UpdatePacsState()
		{
			if(OperatorState == null)
			{
				PacsState = PacsState.Disconnected;
				return;
			}

			switch(OperatorState.State)
			{
				case OperatorStateType.Connected:
					PacsState = PacsState.Connected;
					break;
				case OperatorStateType.WaitingForCall:
					PacsState = PacsState.WorkShift;
					break;
				case OperatorStateType.Talk:
					PacsState = PacsState.Talk;
					break;
				case OperatorStateType.Break:
					PacsState = PacsState.Break;
					break;
				case OperatorStateType.Disconnected:
				default:
					PacsState = PacsState.Disconnected;
					break;
			}
		}

		#region Breaks

		public OperatorBreakAvailability BreakAvailability
		{
			get => _breakAvailability;
			private set
			{
				_breakAvailability = value;
				OnPropertyChanged(nameof(BreakAvailability));
				UpdateBreakInfo();
				UpdateLongBreak();
				UpdateShortBreak();
			}
		}

		public GlobalBreakAvailabilityEvent GlobalBreakAvailability
		{
			get => _globalBreakAvailability;
			private set
			{
				_globalBreakAvailability = value;
				OnPropertyChanged(nameof(GlobalBreakAvailability));
				UpdateBreakInfo();
				UpdateLongBreak();
				UpdateShortBreak();
			}
		}

		private string _breakInfo;
		public virtual string BreakInfo
		{
			get => _breakInfo;
			private set => SetField(ref _breakInfo, value);
		}

		public void UpdateBreakInfo()
		{
			BreakInfo = GetBreakInfo();
			StartDelayedBreakUpdate();
		}

		private void StartDelayedBreakUpdate()
		{
			_delayedBreakUpdateTimer.Stop();

			if(BreakAvailability.ShortBreakSupposedlyAvailableAfter == null)
			{
				return;
			}

			var interval = BreakAvailability.ShortBreakSupposedlyAvailableAfter.Value - DateTime.Now;

			if(interval < TimeSpan.Zero)
			{
				return;
			}

			interval.Add(TimeSpan.FromSeconds(2));

			_delayedBreakUpdateTimer.Interval = interval.TotalMilliseconds;
			_delayedBreakUpdateTimer.AutoReset = false;
			_delayedBreakUpdateTimer.Start();
		}

		public string GetBreakInfo()
		{
			var stringBuilder = new StringBuilder();

			if(GlobalBreakAvailability == null || BreakAvailability == null)
			{
				return stringBuilder.ToString();
			}

			if(!GlobalBreakAvailability.LongBreakAvailable && !string.IsNullOrWhiteSpace(GlobalBreakAvailability.LongBreakDescription))
			{
				stringBuilder.Append(GlobalBreakAvailability.LongBreakDescription);
			}

			if(!BreakAvailability.LongBreakAvailable && !string.IsNullOrWhiteSpace(BreakAvailability.LongBreakDescription))
			{
				stringBuilder.AppendLine(BreakAvailability.LongBreakDescription);
			}

			if(!GlobalBreakAvailability.ShortBreakAvailable && !string.IsNullOrWhiteSpace(GlobalBreakAvailability.ShortBreakDescription))
			{
				stringBuilder.AppendLine(GlobalBreakAvailability.ShortBreakDescription);
			}

			if(!BreakAvailability.ShortBreakAvailable && !string.IsNullOrWhiteSpace(BreakAvailability.ShortBreakDescription))
			{
				stringBuilder.AppendLine(BreakAvailability.ShortBreakDescription);

				if(BreakAvailability.ShortBreakSupposedlyAvailableAfter.HasValue)
				{
					stringBuilder.AppendLine($"Малый перерыв будет доступен после: {BreakAvailability.ShortBreakSupposedlyAvailableAfter.Value:dd.MM HH:mm}");
				}
			}

			return stringBuilder.ToString();
		}

		#region Long break

		public virtual bool CanStartLongBreak
		{
			get => _canStartLongBreak;
			private set => SetField(ref _canStartLongBreak, value);
		}

		public virtual BreakState LongBreakState
		{
			get => _longBreakState;
			private set => SetField(ref _longBreakState, value);
		}

		private void UpdateLongBreak()
		{
			var breakUnavailable = !BreakAvailability.LongBreakAvailable
			|| !GlobalBreakAvailability.LongBreakAvailable;

			CanStartLongBreak = _operatorStateAgent.CanStartBreak && !breakUnavailable;

			if(_operatorStateAgent.CanStartBreak && CanStartLongBreak)
			{
				LongBreakState = BreakState.CanStartBreak;
			}
			else if(_operatorStateAgent.CanEndBreak)
			{
				LongBreakState = BreakState.CanEndBreak;
			}
			else
			{
				LongBreakState = BreakState.BreakDenied;
			}
		}

		public async Task StartLongBreak()
		{
			if(BreakInProgress)
			{
				return;
			}

			if(!CanStartLongBreak)
			{
				return;
			}

			BreakInProgress = true;

			try
			{
				var cts = new CancellationTokenSource(_commandTimeout);
				OperatorStateEvent operatorState = await _client.StartBreak(OperatorBreakType.Long, cts.Token);
				_breakInProgress = false;
				SetState(operatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время начала большого перерыва оператора");
				throw;
			}
			finally
			{
				BreakInProgress = false;
			}
		}

		#endregion Long break

		#region Short break

		public virtual BreakState ShortBreakState
		{
			get => _shortBreakState;
			private set => SetField(ref _shortBreakState, value);
		}

		public virtual bool CanStartShortBreak
		{
			get => _canStartShortBreak;
			private set => SetField(ref _canStartShortBreak, value);
		}

		private void UpdateShortBreak()
		{
			var breakAvailable = BreakAvailability.ShortBreakAvailable
				&& GlobalBreakAvailability.ShortBreakAvailable;

			CanStartShortBreak = _operatorStateAgent.CanStartBreak && breakAvailable;

			if(_operatorStateAgent.CanStartBreak && CanStartShortBreak)
			{
				ShortBreakState = BreakState.CanStartBreak;
			}
			else if(_operatorStateAgent.CanEndBreak)
			{
				ShortBreakState = BreakState.CanEndBreak;
			}
			else
			{
				ShortBreakState = BreakState.BreakDenied;
			}
		}

		public async Task StartShortBreak()
		{
			if(BreakInProgress)
			{
				return;
			}

			if(!CanStartShortBreak)
			{
				return;
			}

			BreakInProgress = true;

			try
			{
				var cts = new CancellationTokenSource(_commandTimeout);
				OperatorStateEvent operatorState = await _client.StartBreak(OperatorBreakType.Short, cts.Token);
				_breakInProgress = false;
				SetState(operatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время начала малого перерыва оператора");
				throw;
			}
			finally
			{
				BreakInProgress = false;
			}
		}

		#endregion Short break

		public virtual bool CanEndBreak
		{
			get => _canEndBreak;
			private set => SetField(ref _canEndBreak, value);
		}

		private void UpdateEndBreak()
		{
			CanEndBreak = _operatorStateAgent.CanEndBreak;
		}

		public async Task EndBreak()
		{
			if(BreakInProgress)
			{
				return;
			}

			if(!CanEndBreak)
			{
				return;
			}

			BreakInProgress = true;

			try
			{
				var cts = new CancellationTokenSource(_commandTimeout);
				var operatorState = await _client.EndBreak(cts.Token);
				_breakInProgress = false;
				SetState(operatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время завершения перерыва оператора");
				throw;
			}
			finally
			{
				BreakInProgress = false;
			}
		}

		public virtual IEnumerable<OperatorState> OperatorsOnBreak
		{
			get => _operatorsonBreak;
			private set => SetField(ref _operatorsonBreak, value);
		}

		public virtual bool BreakInProgress
		{
			get => _breakInProgress;
			private set
			{
				if(SetField(ref _breakInProgress, value))
				{
					OnPropertyChanged(nameof(CanStartLongBreak));
					OnPropertyChanged(nameof(CanStartShortBreak));
					OnPropertyChanged(nameof(CanEndBreak));
				}
			}
		}

		#endregion Breaks

		public virtual IPacsDomainSettings Settings
		{
			get => _settings;
			private set => SetField(ref _settings, value);
		}

		public virtual IEnumerable<string> AvailablePhones
		{
			get => _availablePhones;
			private set => SetField(ref _availablePhones, value);
		}

		public bool CanChangePhone => _operatorStateAgent.CanChangePhone;

		public async Task ChangePhone(string phone)
		{
			try
			{
				var operatorState = await _client.ChangeNumber(phone);
				SetState(operatorState);
				UpdateMango();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время смены телефона оператора");
				throw;
			}
		}

		public bool CanStartWorkShift => _operatorStateAgent.CanStartWorkShift;

		public async Task StartWorkShift(string phone)
		{
			try
			{
				var operatorState = await _client.StartWorkShift(phone);
				SetState(operatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время начала смены оператора");
				throw;
			}
		}

		public bool CanEndWorkShift => _operatorStateAgent.CanEndWorkShift;

		public async Task EndWorkShift(string reason)
		{
			try
			{
				var operatorState = await _client.EndWorkShift(reason);
				SetState(operatorState);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время завершения смены оператора");
				throw;
			}
		}

		#endregion Pacs

		#region Mango

		private bool _canOpenMango;
		public virtual bool CanOpenMango
		{
			get => _canOpenMango;
			private set => SetField(ref _canOpenMango, value);
		}

		private MangoState _mangoState;
		public virtual MangoState MangoState
		{
			get => _mangoState;
			private set => SetField(ref _mangoState, value);
		}

		private string _mangoPhone = "";
		public virtual string MangoPhone
		{
			get => _mangoPhone;
			private set => SetField(ref _mangoPhone, value);
		}

		private void MangoManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(IMangoManager.ConnectionState):
					UpdateMangoState();
					break;
				default:
					break;
			}
		}

		private void UpdateMangoState()
		{
			switch(_mangoManager.ConnectionState)
			{
				case ConnectionState.Disable:
					MangoState = MangoState.Disable;
					break;
				case ConnectionState.Connected:
					MangoState = MangoState.Connected;
					break;
				case ConnectionState.Ring:
					MangoState = MangoState.Ring;
					break;
				case ConnectionState.Talk:
					MangoState = MangoState.Talk;
					break;
				case ConnectionState.Disconnected:
					MangoState = MangoState.Disconnected;
					break;
				default:
					break;
			}
		}

		private void UpdateMango()
		{
			if(IsOperator)
			{
				if(!IsInitialized)
				{
					if(_mangoManager.IsActive)
					{
						_mangoManager.Disconnect();
						MangoPhone = "";
						CanOpenMango = false;
					}

					return;
				}

				//инициализация со скуд
				var hasPhone = uint.TryParse(OperatorState?.PhoneNumber, out var phone);
				if(!hasPhone)
				{
					_logger.LogWarning("Внутренний телефон оператора имеет не корректный формат и не может использоваться в Манго. Тел: {Phone}", OperatorState?.PhoneNumber);
				}

				if(_operatorStateAgent.OnWorkshift)
				{
					MangoPhone = OperatorState.PhoneNumber;
					if(_mangoManager.CanConnect && hasPhone)
					{
						_mangoManager.Connect(phone);
						CanOpenMango = true;
					}
				}
				else
				{
					_mangoManager.Disconnect();
					MangoPhone = "";
					CanOpenMango = false;
				}
			}
			else
			{
				//инициализация без скуд
				if(_employee.InnerPhone == null)
				{
					_logger.LogWarning("Не указан внутренний телефон сотрудника Id {EmployeeId} используемый для Манго.", _employee.Id);
				}

				if(_mangoManager.CanConnect && _employee.InnerPhone.HasValue)
				{
					_mangoManager.Connect(_employee.InnerPhone.Value);
					MangoPhone = _employee.InnerPhone.Value.ToString();
					CanOpenMango = true;
				}
				else
				{
					_mangoManager.Disconnect();
					MangoPhone = "";
					CanOpenMango = false;
				}
			}
		}

		public void OpenMango()
		{
			_mangoManager.OpenMangoDialog();
		}


		#endregion Mango

		#region IObserver<GlobalBreakAvailability>

		void IObserver<GlobalBreakAvailabilityEvent>.OnCompleted()
		{
			_breakAvailabilitySubscription.Dispose();
		}

		void IObserver<GlobalBreakAvailabilityEvent>.OnError(Exception error)
		{
			_logger.LogError(error, "");
		}

		void IObserver<GlobalBreakAvailabilityEvent>.OnNext(GlobalBreakAvailabilityEvent value)
		{
			GlobalBreakAvailability = value;
		}

		#endregion IObserver<GlobalBreakAvailability>

		#region IObserver<SettingsEvent>

		void IObserver<SettingsEvent>.OnCompleted()
		{
			_settingsSubscription.Dispose();
		}

		void IObserver<SettingsEvent>.OnError(Exception error)
		{
		}

		void IObserver<SettingsEvent>.OnNext(SettingsEvent value)
		{
			_settings = value.Settings;
		}

		#endregion IObserver<SettingsEvent>

		#region IObserver<OperatorsOnBreakEvent>

		void IObserver<OperatorsOnBreakEvent>.OnCompleted()
		{
			_operatorsOnBreakSubscription.Dispose();
		}

		void IObserver<OperatorsOnBreakEvent>.OnError(Exception error)
		{
		}

		void IObserver<OperatorsOnBreakEvent>.OnNext(OperatorsOnBreakEvent value)
		{
			OperatorsOnBreak = value.OnBreak;
		}

		#endregion IObserver<OperatorsOnBreakEvent>

		public bool IsOperatorShiftActive()
		{
			if(OperatorState == null)
			{
				return false;
			}

			return OperatorState.State.IsNotIn(
				OperatorStateType.New,
				OperatorStateType.Connected,
				OperatorStateType.Disconnected);
		}

		public void Dispose()
		{
			if(_connectingTimer != null)
			{
				_connectingTimer.Stop();
				_connectingTimer.Elapsed -= OnReconnectTimerElapsed;
			}
			_connectingTimer?.Dispose();
			_delayedBreakUpdateTimer?.Dispose();
			UnsubscribeEvents();
		}
	}
}
