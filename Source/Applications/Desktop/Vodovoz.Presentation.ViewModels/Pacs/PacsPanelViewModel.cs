using Microsoft.Extensions.Logging;
using Pacs.Operator.Client;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;
using Vodovoz.Settings.Pacs;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : WidgetViewModelBase
	{
		private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(10);

		private readonly ILogger<PacsPanelViewModel> _logger;
		private readonly IEmployeeService _employeeService;
		private readonly IOperatorClient _operatorClient;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly INavigationManager _navigationManager;
		private readonly IPacsRepository _pacsRepository;
		private readonly Employee _employee;

		private bool _canChange;
		private bool _pacsEnabled;
		private OperatorState _operatorState;
		private MangoState _mangoState;
		private bool _breakInProgress;

		public DelegateCommand BreakCommand { get; }
		public DelegateCommand RefreshCommand { get; }
		public DelegateCommand OpenPacsDialogCommand { get; }
		public DelegateCommand OpenMangoDialogCommand { get; }

		public PacsPanelViewModel(
			ILogger<PacsPanelViewModel> logger,
			IEmployeeService employeeService, 
			IOperatorClientFactory operatorClientFactory,
			IPacsSettings pacsSettings,
			IGuiDispatcher guiDispatcher, 
			INavigationManager navigationManager,
			IPacsRepository pacsRepository)
		{
			if(operatorClientFactory is null)
			{
				throw new ArgumentNullException(nameof(operatorClientFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
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
			}
			else
			{
				PacsEnabled = false;
				//Отдельная инициализация Манго по номеру сотрудника
			}

			BreakCommand = new DelegateCommand(() => Break(), () => CanBreak);
			BreakCommand.CanExecuteChangedWith(this, x => x.CanBreak);

			RefreshCommand = new DelegateCommand(Refresh);
			RefreshCommand.CanExecuteChangedWith(this, x => x.CanRefresh);

			OpenPacsDialogCommand = new DelegateCommand(OpenPacsDialog);
			OpenPacsDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenPacsDialog);

			OpenMangoDialogCommand = new DelegateCommand(OpenMangoDialog);
			OpenMangoDialogCommand.CanExecuteChangedWith(this, x => x.CanOpenMangoDialog);
		}

		public virtual bool CanChange
		{
			get => _canChange;
			set => SetField(ref _canChange, value);
		}

		#region Pacs

		[PropertyChangedAlso(nameof(BreakState))]
		[PropertyChangedAlso(nameof(PacsState))]
		public virtual OperatorState OperatorState
		{
			get => _operatorState;
			set => SetField(ref _operatorState, value);
		}

		public virtual bool PacsEnabled
		{
			get => _pacsEnabled;
			set => SetField(ref _pacsEnabled, value);
		}

		private void OperatorStateChanged(object sender, OperatorState state)
		{
			_guiDispatcher.RunInGuiTread(() => {
				OperatorState = state;
			});
		}

		[PropertyChangedAlso(nameof(CanOpenPacsDialog))]
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

		[PropertyChangedAlso(nameof(CanBreak))]
		public virtual BreakState BreakState
		{
			get {
				if(OperatorState == null)
				{
					return BreakState.BreakDenied;
				}

				switch(OperatorState.State)
				{
					case OperatorStateType.WaitingForCall:
						return BreakState.CanStartBreak;
					case OperatorStateType.Break:
						return BreakState.CanEndBreak;
					case OperatorStateType.New:
					case OperatorStateType.Connected:
					case OperatorStateType.Disconnected:
					case OperatorStateType.Talk:
					default:
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

				return BreakState == BreakState.CanStartBreak || BreakState == BreakState.CanEndBreak;
			}
		}


		private async Task Break()
		{
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

		[PropertyChangedAlso(nameof(CanOpenMangoDialog))]
		public virtual MangoState MangoState
		{
			get => _mangoState;
			set => SetField(ref _mangoState, value);
		}

		public bool CanOpenMangoDialog { get; set; }

		private void OpenMangoDialog()
		{
			//Встроить MangoManager
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
