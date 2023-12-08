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
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.Services;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : WidgetViewModelBase, IObserver<GlobalBreakAvailability>, IDisposable
	{
		private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(10);

		private readonly ILogger<PacsPanelViewModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IMangoManager _mangoManager;
		private readonly IInteractiveService _interactiveService;
		private readonly IOperatorClient _operatorClient;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly INavigationManager _navigationManager;
		private readonly IPacsRepository _pacsRepository;
		private readonly Employee _employee;
		private readonly IDisposable _breakAvailabilitySubscription;

		private bool _canChange;
		private bool _pacsEnabled;
		private MangoState _mangoState;
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

			if(_pacsRepository.PacsEnabledFor(_employee.Subdivision.Id))
			{
				PacsEnabled = true;
				_operatorClient = operatorClientFactory.CreateOperatorClient(_employee.Id);
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

		private void UpdateMango()
		{
			if(!PacsEnabled)
			{
				if(_mangoManager.IsActive)
				{
					_mangoManager.Disconnect();
				}

				return;
			}

			var hasPhone = uint.TryParse(OperatorState.PhoneNumber, out var phone);
				
			if(_operatorStateAgent.OnWorkshift)
			{
				if(_mangoManager.CanConnect && hasPhone)
				{
					_mangoManager.Connect(phone);
				}
			}
			else
			{
				_mangoManager.Disconnect();
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
			Task.Run(() => {
				try
				{
					var state = _operatorClient.Connect().Result;
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
					OnPropertyChanged(nameof(BreakState));
					OnPropertyChanged(nameof(PacsState));
					OnPropertyChanged(nameof(CanOpenPacsDialog));
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
					OnPropertyChanged(nameof(CanOpenMangoDialog));
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

		public virtual BreakState BreakState
		{
			get {
				if(_operatorStateAgent.CanStartBreak)
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

				var breakUnavailable = !BreakAvailability.LongBreakAvailable
					|| !GlobalBreakAvailability.LongBreakAvailable;
				if(breakUnavailable && BreakState == BreakState.CanStartBreak)
				{
					return false;
				}

				return _operatorStateAgent.CanStartBreak || _operatorStateAgent.CanEndBreak;
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
				var breakUnavailable = !BreakAvailability.ShortBreakAvailable
					|| !GlobalBreakAvailability.ShortBreakAvailable;
				if(breakUnavailable && BreakState == BreakState.CanStartBreak)
				{
					return false;
				}

				return _operatorStateAgent.CanStartBreak || _operatorStateAgent.CanEndBreak;
			}
		}


		private async Task StartLongBreak()
		{
			string question;
			switch(BreakState)
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

			_guiDispatcher.RunInGuiTread(() => {
				_breakInProgress = true;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			});

			try
			{
				OperatorStateEvent state;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(BreakState == BreakState.CanStartBreak)
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
				_guiDispatcher.RunInGuiTread(() => {
					_breakInProgress = false;
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
				});
			}
		}

		private async Task StartShortBreak()
		{
			string question;
			switch(BreakState)
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

			_guiDispatcher.RunInGuiTread(() => {
				_breakInProgress = true;
				OnPropertyChanged(nameof(CanLongBreak));
				OnPropertyChanged(nameof(CanShortBreak));
			});

			try
			{
				OperatorStateEvent state;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(BreakState == BreakState.CanStartBreak)
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
				_guiDispatcher.RunInGuiTread(() => {
					_breakInProgress = false;
					OnPropertyChanged(nameof(CanLongBreak));
					OnPropertyChanged(nameof(CanShortBreak));
				});
			}
		}

		#endregion Break

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
			_breakAvailabilitySubscription.Dispose();
		}

		#endregion Mango
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
