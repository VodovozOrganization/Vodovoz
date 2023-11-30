using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using Pacs.Operator.Client;
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
	public class PacsPanelViewModel : WidgetViewModelBase, IObserver<BreakAvailabilityEvent>, IDisposable
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
		private bool _breakAvailable;

		private IOperatorStateAgent _operatorStateAgent;

		public DelegateCommand BreakCommand { get; }
		public DelegateCommand RefreshCommand { get; }
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
			IObservable<BreakAvailabilityEvent> breakAvailabilityPublisher,
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

			BreakCommand = new DelegateCommand(() => Break(), () => CanBreak);
			BreakCommand.CanExecuteChangedWith(this, x => x.CanBreak);

			RefreshCommand = new DelegateCommand(Refresh);
			RefreshCommand.CanExecuteChangedWith(this, x => x.CanRefresh);

			OpenPacsDialogCommand = new DelegateCommand(OpenPacsDialog);
			OpenPacsDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenPacsDialog);

			OpenMangoDialogCommand = new DelegateCommand(OpenMangoDialog);
			OpenMangoDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenMangoDialog);

			_mangoManager.PropertyChanged += MangoManagerPropertyChanged;
			_breakAvailabilitySubscription = breakAvailabilityPublisher.Subscribe(this);

			_breakAvailable = _operatorClient.GetBreakAvailability().Result;
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
					_guiDispatcher.RunInGuiTread(() => {
						OperatorState = state;
					});
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
					OnPropertyChanged(nameof(CanBreak));
					OnPropertyChanged(nameof(CanOpenMangoDialog));
					UpdateMango();
				}
			}
		}

		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			private set => SetField(ref _pacsEnabled, value);
		}

		private void OperatorStateChanged(object sender, OperatorState state)
		{
			_guiDispatcher.RunInGuiTread(() => {
				OperatorState = state;
			});
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

		public bool CanBreak
		{
			get
			{
				if(_breakInProgress)
				{
					return false;
				}

				if(!_breakAvailable && BreakState == BreakState.CanStartBreak)
				{
					return false;
				}

				return _operatorStateAgent.CanStartBreak || _operatorStateAgent.CanEndBreak;
			}
		}


		private async Task Break()
		{
			string question;
			switch(BreakState)
			{
				case BreakState.CanStartBreak:
					question = "Хотите взять перерыв?";
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
				OnPropertyChanged(nameof(CanBreak));
			});

			try
			{
				OperatorState state;
				var cts = new CancellationTokenSource(_commandTimeout);
				if(BreakState == BreakState.CanStartBreak)
				{
					state = await _operatorClient.StartBreak(cts.Token);
				}
				else
				{
					state = await _operatorClient.EndBreak(cts.Token);
				}

				_guiDispatcher.RunInGuiTread(() => {
					OperatorState = state;
				});
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
					OnPropertyChanged(nameof(CanBreak));
				});
			}
		}

		#endregion Break

		#region Refresh

		public bool CanRefresh { get; }

		private void Refresh()
		{
			OperatorState = _operatorClient.GetState().Result;
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

		void IObserver<BreakAvailabilityEvent>.OnCompleted()
		{
			_breakAvailabilitySubscription.Dispose();
		}

		void IObserver<BreakAvailabilityEvent>.OnError(Exception error)
		{
		}

		void IObserver<BreakAvailabilityEvent>.OnNext(BreakAvailabilityEvent value)
		{
			_guiDispatcher.RunInGuiTread(() =>
			{
				_breakAvailable = value.BreakAvailable;
				OnPropertyChanged(nameof(CanBreak));
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
