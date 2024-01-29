using Core.Infrastructure;
using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operators.Client;
using Pacs.Server;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.Services;
using Timer = System.Timers.Timer;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : WidgetViewModelBase, IObserver<GlobalBreakAvailability>, IObserver<SettingsEvent>, IDisposable
	{
		private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(10);

		private readonly ILogger<PacsPanelViewModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IMangoManager _mangoManager;
		private readonly IInteractiveService _interactiveService;
		private readonly IOperatorClient _operatorClient;
		private readonly OperatorKeepAliveController _keepAliveController;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly INavigationManager _navigationManager;
		private readonly IPacsRepository _pacsRepository;
		private readonly Employee _employee;
		private readonly IDisposable _breakAvailabilitySubscription;
		private readonly IDisposable _settingsSubscription;

		private Timer _pacsInfoUpdateTimer;
		private IPacsDomainSettings _settings;
		private bool _canChange;
		private bool _pacsEnabled;
		private string _pacsInfo = "";
		private MangoState _mangoState;
		private string _mangoPhone = "";
		private bool _breakInProgress;

		private IOperatorStateAgent _operatorStateAgent;
		private OperatorBreakAvailability _breakAvailability = new OperatorBreakAvailability();
		private GlobalBreakAvailability _globalBreakAvailability = new GlobalBreakAvailability();

		public DelegateCommand LongBreakCommand { get; }
		public DelegateCommand ShortBreakCommand { get; }
		public DelegateCommand OpenPacsDialogCommand { get; }
		public DelegateCommand OpenMangoDialogCommand { get; }

		public PacsPanelViewModel(
			ILogger<PacsPanelViewModel> logger,
			IEmployeeService employeeService,
			IOperatorClientFactory operatorClientFactory,
			IMangoManager mangoManager,
			IInteractiveService interactiveService,
			IOperatorStateAgent operatorStateAgent,
			IGuiDispatcher guiDispatcher,
			INavigationManager navigationManager,
			IObservable<GlobalBreakAvailability> globalBreakAvailabilityPublisher,
			IObservable<SettingsEvent> settingsPublisher,
			IPacsRepository pacsRepository)
		{
			if(operatorClientFactory is null)
			{
				throw new ArgumentNullException(nameof(operatorClientFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_mangoManager = mangoManager ?? throw new ArgumentNullException(nameof(mangoManager));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_operatorStateAgent = operatorStateAgent ?? throw new ArgumentNullException(nameof(operatorStateAgent));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));

			_employee = _employeeService.GetEmployeeForCurrentUser();
			if(_employee == null)
			{
				CanChange = false;
				PacsEnabled = false;
				return;
			}
			_pacsInfoUpdateTimer = new Timer(1000);
			_pacsInfoUpdateTimer.Elapsed += OnPacsInfoUpdated;
			_settings = _pacsRepository.GetPacsDomainSettings();

			if(_pacsRepository.PacsEnabledFor(_employee.Subdivision.Id))
			{
				PacsEnabled = true;
				_operatorClient = operatorClientFactory.CreateOperatorClient(_employee.Id);
				_keepAliveController = operatorClientFactory.CreateOperatorKeepAliveController(_employee.Id);
				_operatorClient.StateChanged += OperatorStateChanged;
				GlobalBreakAvailability = _operatorClient.GetGlobalBreakAvailability().Result;
				Connect();
			}
			else
			{
				PacsEnabled = false;
				if(_employee.InnerPhone.HasValue)
				{
					_mangoManager.Connect(_employee.InnerPhone.Value);
				}
			}

			LongBreakCommand = new DelegateCommand(() => StartLongBreak(), () => CanLongBreak);
			LongBreakCommand.CanExecuteChangedWith(this, x => x.CanLongBreak);

			ShortBreakCommand = new DelegateCommand(() => StartShortBreak(), () => CanShortBreak);
			ShortBreakCommand.CanExecuteChangedWith(this, x => x.CanShortBreak);

			OpenPacsDialogCommand = new DelegateCommand(OpenPacsDialog);
			OpenPacsDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenPacsDialog);

			OpenMangoDialogCommand = new DelegateCommand(OpenMangoDialog);
			OpenMangoDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenMangoDialog);

			_mangoManager.PropertyChanged += MangoManagerPropertyChanged;
			_breakAvailabilitySubscription = globalBreakAvailabilityPublisher.Subscribe(this);
			_settingsSubscription = settingsPublisher.Subscribe(this);
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

		public virtual string MangoPhone
		{
			get => _mangoPhone;
			private set => SetField(ref _mangoPhone, value);
		}

		private void UpdateMango()
		{
			if(!PacsEnabled)
			{
				if(_mangoManager.IsActive)
				{
					_mangoManager.Disconnect();
					MangoPhone = "";
					CanOpenMangoDialog = false;
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
					CanOpenMangoDialog = true;
				}
			}
			else
			{
				_mangoManager.Disconnect();
				MangoPhone = "";
				CanOpenMangoDialog = false;
			}
		}

		private void UpdateMangoState()
		{
			switch(_mangoManager.ConnectionState)
			{
				case ConnectionState.Connected:
					MangoState = MangoState.Connected;
					break;
				case ConnectionState.Disable:
					MangoState = MangoState.Disable;
					break;
				case ConnectionState.Disconnected:
					MangoState = MangoState.Disconnected;
					break;
				case ConnectionState.Ring:
					MangoState = MangoState.Ring;
					break;
				case ConnectionState.Talk:
					MangoState = MangoState.Talk;
					break;
				default:
					break;
			}
		}

		private void Connect()
		{
			Task.Run(() =>
			{
				try
				{
					var state = _operatorClient.Connect().Result;
					_keepAliveController.Start();
					UpdateState(state);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка подключения оператора к серверу СКУД");
				}
			});
		}

		public virtual bool CanChange
		{
			get => _canChange;
			set => SetField(ref _canChange, value);
		}

		#region Pacs

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
					OnPropertyChanged(nameof(CanOpenPacsDialog));
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
					OnPropertyChanged(nameof(CanOpenMangoDialog));
					UpdateMango();
					UpdatePacsInfo();
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

		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			private set => SetField(ref _pacsEnabled, value);
		}

		private void OperatorStateChanged(object sender, OperatorStateEvent state)
		{
			UpdateState(state);
		}

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

		public virtual string PacsInfo
		{
			get => _pacsInfo;
			private set => SetField(ref _pacsInfo, value);
		}

		private void UpdatePacsInfo()
		{
			BreakTimeGone = false;
			_pacsInfoUpdateTimer.Stop();
			switch(OperatorState.State)
			{
				case OperatorStateType.Connected:
					PacsInfo = "Подключен";
					break;
				case OperatorStateType.WaitingForCall:
					PacsInfo = "Ожидание";
					break;
				case OperatorStateType.Talk:
					_pacsInfoUpdateTimer.Start();
					PacsInfo = GetTalkDurationTime();
					break;
				case OperatorStateType.Break:
					_pacsInfoUpdateTimer.Start();
					PacsInfo = GetBreakTimeRemains();
					break;
				case OperatorStateType.New:
				case OperatorStateType.Disconnected:
				default:
					PacsInfo = "Отключен";
					break;
			}
		}
		private bool _breakTimeGone;
		public virtual bool BreakTimeGone
		{
			get => _breakTimeGone;
			private set => SetField(ref _breakTimeGone, value);
		}

		private void OnPacsInfoUpdated(object sender, ElapsedEventArgs e)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				switch(OperatorState.State)
				{
					case OperatorStateType.Talk:
						PacsInfo = GetTalkDurationTime();
						break;
					case OperatorStateType.Break:
						PacsInfo = GetBreakTimeRemains();
						break;
					default:
						PacsInfo = "";
						break;
				}
			});
		}

		private string GetBreakTimeRemains()
		{
			if(OperatorState.State != OperatorStateType.Break)
			{
				return "";
			}

			TimeSpan remains;
			if(OperatorState.BreakType == OperatorBreakType.Long)
			{
				remains = OperatorState.Started + _settings.LongBreakDuration - DateTime.Now;
			}
			else
			{
				remains = OperatorState.Started + _settings.ShortBreakDuration - DateTime.Now;
			}

			BreakTimeGone = remains < TimeSpan.Zero;
			var format = (_breakTimeGone ? "\\-" : "") + "m\\м\\.\\ ss\\с\\.";
			return remains.ToString(format);
		}

		private string GetTalkDurationTime()
		{
			if(OperatorState.State != OperatorStateType.Talk)
			{
				return "";
			}
			return (DateTime.Now - OperatorState.Started).ToString("m\\м\\.\\ ss\\с\\.");
		}

		public bool CanOpenPacsDialog { get; set; }

		private void OpenPacsDialog()
		{
			_navigationManager.OpenViewModel<PacsViewModel>(null);
		}

		private void UpdateState(OperatorStateEvent operatorState)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				OperatorState = operatorState.State;
				BreakAvailability = operatorState.BreakAvailability;
			});
		}

		#endregion Pacs

		#region Break

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


		private async Task StartLongBreak()
		{
			string question;
			switch(LongBreakState)
			{
				case BreakState.CanStartBreak:
					question = "Хотите взять большой перерыв?";
					break;
				case BreakState.CanEndBreak:
					question = "Закончить перерыв?";
					break;
				case BreakState.BreakDenied:
				default:
					return;
			}

			if(!_interactiveService.Question(question, "Перерыв"))
			{
				return;
			}

			_guiDispatcher.RunInGuiTread(() =>
			{
				_breakInProgress = true;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			});

			try
			{
				OperatorStateEvent state;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(LongBreakState == BreakState.CanStartBreak)
				{
					state = await _operatorClient.StartBreak(OperatorBreakType.Long, cts.Token);
				}
				else
				{
					state = await _operatorClient.EndBreak(cts.Token);
				}

				UpdateState(state);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время выполнения команды начала/завершения перерыва оператора");
				throw;
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_breakInProgress = false;
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
				});
			}
		}

		private async Task StartShortBreak()
		{
			string question;
			switch(ShortBreakState)
			{
				case BreakState.CanStartBreak:
					question = "Хотите взять малый перерыв?";
					break;
				case BreakState.CanEndBreak:
					question = "Закончить перерыв?";
					break;
				case BreakState.BreakDenied:
				default:
					return;
			}

			if(!_interactiveService.Question(question, "Перерыв"))
			{
				return;
			}

			_guiDispatcher.RunInGuiTread(() =>
			{
				_breakInProgress = true;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			});

			try
			{
				OperatorStateEvent state;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(ShortBreakState == BreakState.CanStartBreak)
				{
					state = await _operatorClient.StartBreak(OperatorBreakType.Short, cts.Token);
				}
				else
				{
					state = await _operatorClient.EndBreak(cts.Token);
				}

				UpdateState(state);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка во время выполнения команды начала/завершения перерыва оператора");
				throw;
			}
			finally
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					_breakInProgress = false;
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
				});
			}
		}

		#endregion Break

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

		#region Refresh

		public bool CanRefresh { get; }

		private void Refresh()
		{
		}

		#endregion Refresh

		#region Mango

		private string _mangoInfo;
		public virtual string MangoInfo
		{
			get => _mangoInfo;
			set => SetField(ref _mangoInfo, value);
		}

		[PropertyChangedAlso(nameof(CanOpenMangoDialog))]
		public virtual MangoState MangoState
		{
			get => _mangoState;
			private set => SetField(ref _mangoState, value);
		}

		public bool CanOpenMangoDialog { get; set; }

		private void OpenMangoDialog()
		{
			_mangoManager.OpenMangoDialog();
		}

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
			_guiDispatcher.RunInGuiTread(() =>
			{
				GlobalBreakAvailability = value;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			});
		}

		public void Dispose()
		{
			_pacsInfoUpdateTimer.Dispose();
			_breakAvailabilitySubscription.Dispose();
			_settingsSubscription.Dispose();
		}

		#endregion Mango

		#region IObserver<SettingsEvent>

		void IObserver<SettingsEvent>.OnCompleted()
		{
			_settingsSubscription.Dispose();
		}

		void IObserver<SettingsEvent>.OnError(Exception error)
		{
			_logger.LogError(error, "");
		}

		void IObserver<SettingsEvent>.OnNext(SettingsEvent value)
		{
			_settings = value.Settings;
		}

		#endregion IObserver<SettingsEvent>
	}

	public enum BreakState
	{
		BreakDenied,
		CanStartBreak,
		CanEndBreak
	}

	public enum PacsState
	{
		Disconnected,
		Connected,
		WorkShift,
		Break,
		Talk
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
