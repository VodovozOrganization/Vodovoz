using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client;
using Pacs.Server;
using QS.DomainModel.Entity;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Application.Mango;
using Vodovoz.Core.Domain.Pacs;
using Timer = System.Timers.Timer;

namespace Vodovoz.Application.Pacs
{
	public class OperatorService : PropertyChangedBase, IObserver<GlobalBreakAvailability>, IDisposable
	{
		private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(10);

		private readonly ILogger<OperatorService> _logger;
		private readonly IOperatorClient _client;
		private readonly IMangoManager _mangoManager;
		private readonly OperatorKeepAliveController _operatorKeepAliveController;
		private readonly IOperatorStateAgent _operatorStateAgent;
		private readonly IDisposable _breakAvailabilitySubscription;

		private Timer _connectingTimer;
		private bool _isConnected;
		private bool _isConnecting;
		private OperatorBreakAvailability _breakAvailability = new OperatorBreakAvailability();
		private GlobalBreakAvailability _globalBreakAvailability = new GlobalBreakAvailability();


		public OperatorService(
			ILogger<OperatorService> logger,
			IOperatorClient operatorClient,
			IMangoManager mangoManager,
			IOperatorStateAgent operatorStateAgent,
			IObservable<GlobalBreakAvailability> globalBreakPublisher,
			OperatorKeepAliveController operatorKeepAliveController
		)
		{
			if(globalBreakPublisher is null)
			{
				throw new ArgumentNullException(nameof(globalBreakPublisher));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_operatorStateAgent = operatorStateAgent ?? throw new ArgumentNullException(nameof(operatorStateAgent));
			_client = operatorClient ?? throw new ArgumentNullException(nameof(operatorClient));
			_operatorKeepAliveController = operatorKeepAliveController ?? throw new ArgumentNullException(nameof(operatorKeepAliveController));

			IsInitialized = _client.OperatorId.HasValue;

			_breakAvailabilitySubscription = globalBreakPublisher.Subscribe(this);
			_mangoManager.PropertyChanged += MangoManagerPropertyChanged;

			StartConnecting();
		}

		public bool IsInitialized { get; }

		public virtual bool IsConnected
		{
			get => _isConnected;
			private set => SetField(ref _isConnected, value);
		}

		private void StartConnecting()
		{
			if(!IsInitialized)
			{
				_logger.LogWarning("Подключение невозможно, так как не инициализирован сервис оператора");
				return;
			}

			if(_connectingTimer != null)
			{
				return;
			}

			_connectingTimer = new Timer(5000);
			_connectingTimer.Elapsed += (s, e) =>
			{
				if(OperatorState == null || OperatorState.State == OperatorStateType.Disconnected)
				{
					Connect().Wait();
					_operatorKeepAliveController.Start();
				}
			};
			_connectingTimer.Start();
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
			_connectingTimer?.Dispose();
			_connectingTimer = null;
			_client.StateChanged -= StateChanged;
			var stateEvent = await _client.Disconnect();
			SetState(stateEvent);
		}

		private void StateChanged(object sender, OperatorStateEvent stateEvent)
		{
			SetState(stateEvent);
		}

		private void SetState(OperatorStateEvent stateEvent)
		{
			OperatorState = stateEvent.State;
			BreakAvailability = stateEvent.BreakAvailability;
		}

		#region Pacs

		public virtual PacsState PacsState
		{
			get
			{
				if(OperatorState == null)
				{
					return PacsState.Disconnected;
				}

				switch(OperatorState.State)
				{
					case OperatorStateType.Connected:
						return PacsState.Connected;
					case OperatorStateType.WaitingForCall:
						return PacsState.WorkShift;
					case OperatorStateType.Talk:
						return PacsState.Talk;
					case OperatorStateType.Break:
						return PacsState.Break;
					case OperatorStateType.Disconnected:
					default:
						return PacsState.Disconnected;
				}
			}
		}

		public virtual OperatorState OperatorState
		{
			get => _operatorStateAgent.OperatorState;
			set
			{
				if(_operatorStateAgent.OperatorState != value)
				{
					_operatorStateAgent.OperatorState = value;
					OnPropertyChanged(nameof(OperatorState));
					OnPropertyChanged(nameof(LongBreakState));
					OnPropertyChanged(nameof(ShortBreakState));
					OnPropertyChanged(nameof(PacsState));
					OnPropertyChanged(nameof(CanOpenPacs));
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
					OnPropertyChanged(nameof(CanOpenMango));
					UpdateMango();
				}
			}
		}

		public OperatorBreakAvailability BreakAvailability
		{
			get => _breakAvailability;
			private set
			{
				_breakAvailability = value;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
				OnPropertyChanged(nameof(LongBreakState));
				OnPropertyChanged(nameof(ShortBreakState));
			}
		}
		public GlobalBreakAvailability GlobalBreakAvailability
		{
			get => _globalBreakAvailability;
			private set
			{
				_globalBreakAvailability = value;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
				OnPropertyChanged(nameof(LongBreakState));
				OnPropertyChanged(nameof(ShortBreakState));
			}
		}

		private bool _canOpenPacs;
		public virtual bool CanOpenPacs
		{
			get => _canOpenPacs;
			set => SetField(ref _canOpenPacs, value);
		}


		public virtual BreakState LongBreakState
		{
			get
			{
				if(_operatorStateAgent.CanStartBreak && CanLongBreak)
				{
					return BreakState.CanStartBreak;
				}
				else if(_operatorStateAgent.CanEndBreak)
				{
					return BreakState.CanEndBreak;
				}
				else
				{
					return BreakState.BreakDenied;
				}
			}
		}

		public virtual BreakState ShortBreakState
		{
			get
			{
				if(_operatorStateAgent.CanStartBreak && CanShortBreak)
				{
					return BreakState.CanStartBreak;
				}
				else if(_operatorStateAgent.CanEndBreak)
				{
					return BreakState.CanEndBreak;
				}
				else
				{
					return BreakState.BreakDenied;
				}
			}
		}

		private bool _breakInProgress;
		public virtual bool BreakInProgress
		{
			get => _breakInProgress;
			set => SetField(ref _breakInProgress, value);
		}

		public bool CanLongBreak
		{
			get
			{
				if(_breakInProgress)
				{
					return false;
				}

				if(_operatorStateAgent.CanEndBreak)
				{
					return true;
				}

				var breakUnavailable = !BreakAvailability.LongBreakAvailable
					|| !GlobalBreakAvailability.LongBreakAvailable;

				return _operatorStateAgent.CanStartBreak && !breakUnavailable;
			}
		}

		public bool CanShortBreak
		{
			get
			{
				if(_breakInProgress)
				{
					return false;
				}

				if(_operatorStateAgent.CanEndBreak)
				{
					return true;
				}
				var breakUnavailable = !BreakAvailability.ShortBreakAvailable
					|| !GlobalBreakAvailability.ShortBreakAvailable;

				return _operatorStateAgent.CanStartBreak && !breakUnavailable;
			}
		}

		public async Task StartLongBreak()
		{
			if(BreakInProgress)
			{
				return;
			}

			BreakInProgress = true;
			OnPropertyChanged(nameof(CanLongBreak));
			OnPropertyChanged(nameof(CanShortBreak));

			try
			{
				OperatorStateEvent operatorState;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(LongBreakState == BreakState.CanStartBreak)
				{
					operatorState = await _client.StartBreak(OperatorBreakType.Long, cts.Token);
				}
				else
				{
					operatorState = await _client.EndBreak(cts.Token);
				}

				OperatorState = operatorState.State;
				BreakAvailability = operatorState.BreakAvailability;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время выполнения команды начала/завершения перерыва оператора");
				throw;
			}
			finally
			{
				BreakInProgress = false;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			}
		}

		public async Task StartShortBreak()
		{
			if(BreakInProgress)
			{
				return;
			}

			BreakInProgress = true;
			OnPropertyChanged(nameof(CanLongBreak));
			OnPropertyChanged(nameof(CanShortBreak));

			try
			{
				OperatorStateEvent operatorState;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(ShortBreakState == BreakState.CanStartBreak)
				{
					operatorState = await _client.StartBreak(OperatorBreakType.Short, cts.Token);
				}
				else
				{
					operatorState = await _client.EndBreak(cts.Token);
				}

				OperatorState = operatorState.State;
				BreakAvailability = operatorState.BreakAvailability;
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время выполнения команды начала/завершения перерыва оператора");
				throw;
			}
			finally
			{
				BreakInProgress = false;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			}
		}

		#endregion Pacs

		#region Mango

		private bool _canOpenMango;
		public virtual bool CanOpenMango
		{
			get => _canOpenMango;
			set => SetField(ref _canOpenMango, value);
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
					UpdateMango();
					break;
				default:
					break;
			}
		}

		private void UpdateMango()
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

			var hasPhone = uint.TryParse(OperatorState.PhoneNumber, out var phone);
			if(!hasPhone)
			{
				_logger.LogWarning("Внутренний телефон оператора имеет не корректный формат и не может использоваться в Манго. Тел: {Phone}", OperatorState.PhoneNumber);
			}

			if(_operatorStateAgent.OnWorkshift)
			{
				if(_mangoManager.CanConnect && hasPhone)
				{
					_mangoManager.Connect(phone);
					MangoPhone = OperatorState.PhoneNumber;
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

		public void OpenMango()
		{
			_mangoManager.OpenMangoDialog();
		}


		#endregion Mango

		#region IObserver<GlobalBreakAvailability>

		void IObserver<GlobalBreakAvailability>.OnCompleted()
		{
			_breakAvailabilitySubscription.Dispose();
		}

		void IObserver<GlobalBreakAvailability>.OnError(Exception error)
		{
			_logger.LogError(error, "");
		}

		void IObserver<GlobalBreakAvailability>.OnNext(GlobalBreakAvailability value)
		{
			GlobalBreakAvailability = value;
		}

		#endregion IObserver<GlobalBreakAvailability>

		public bool CanStopApplication()
		{
			if(OperatorState == null)
			{
				return true;
			}
			var canStop = OperatorState.State.IsIn(
				OperatorStateType.New,
				OperatorStateType.Connected,
				OperatorStateType.Disconnected
			);
			return canStop;
		}

		public void Dispose()
		{
			_breakAvailabilitySubscription?.Dispose();
		}
	}

	public enum PacsState
	{
		Disconnected,
		Connected,
		WorkShift,
		Break,
		Talk
	}

	public enum BreakState
	{
		BreakDenied,
		CanStartBreak,
		CanEndBreak
	}

	public enum MangoState
	{
		Disable,
		Disconnected,
		Connected,
		Ring,
		Talk
	}
}
