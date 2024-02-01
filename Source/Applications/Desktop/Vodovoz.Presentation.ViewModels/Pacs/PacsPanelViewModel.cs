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
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Application.Mango;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Vodovoz.Domain.Employees;
using Vodovoz.Presentation.ViewModels.Employees;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.Services;
using Timer = System.Timers.Timer;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : ReactiveWidgetViewModel, IObserver<SettingsEvent>, IDisposable
	{
		private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(10);

		private readonly ILogger<PacsPanelViewModel> _logger;
		private readonly OperatorService _operatorService;

		private readonly IInteractiveService _interactiveService;
		private readonly IGuiDispatcher _guiDispatcher;
		private readonly INavigationManager _navigationManager;
		private readonly IDisposable _settingsSubscription;

		private PacsState _pacsState;
		private BreakState _longBreakState;
		private BreakState _shortBreakState;
		private string _mangoPhone;
		private MangoState _mangoState;

		private Timer _pacsInfoUpdateTimer;
		private IPacsDomainSettings _settings;
		private bool _canChange;
		private bool _pacsEnabled;
		private string _pacsInfo = "";
		private bool _breakInProgress;

		private OperatorBreakAvailability _breakAvailability = new OperatorBreakAvailability();
		private GlobalBreakAvailability _globalBreakAvailability = new GlobalBreakAvailability();

		public DelegateCommand LongBreakCommand { get; }
		public DelegateCommand ShortBreakCommand { get; }
		public DelegateCommand OpenPacsDialogCommand { get; }
		public DelegateCommand OpenMangoDialogCommand { get; }

		public PacsPanelViewModel(
			ILogger<PacsPanelViewModel> logger,
			OperatorService operatorService,
			IInteractiveService interactiveService,
			IGuiDispatcher guiDispatcher,
			INavigationManager navigationManager,
			IObservable<SettingsEvent> settingsPublisher,
			IPacsRepository pacsRepository)
		{
			if(pacsRepository is null)
			{
				throw new ArgumentNullException(nameof(pacsRepository));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_operatorService = operatorService ?? throw new ArgumentNullException(nameof(operatorService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_guiDispatcher = guiDispatcher ?? throw new ArgumentNullException(nameof(guiDispatcher));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));

			//_employee = _employeeService.GetEmployeeForCurrentUser();
			//if(_employee == null)
			//{
			//	CanChange = false;
			//	PacsEnabled = false;
			//	return;
			//}

			_pacsInfoUpdateTimer = new Timer(1000);
			_pacsInfoUpdateTimer.Elapsed += OnPacsInfoUpdated;
			_settings = pacsRepository.GetPacsDomainSettings();

			PacsEnabled = _operatorService.IsInitialized;

			//if(_pacsRepository.PacsEnabledFor(_employee.Subdivision.Id))
			//{
			//	PacsEnabled = true;
			//	_operatorClient = operatorClientFactory.CreateOperatorClient(_employee.Id);
			//	_keepAliveController = operatorClientFactory.CreateOperatorKeepAliveController(_employee.Id);
			//	_operatorClient.StateChanged += OperatorStateChanged;
			//	GlobalBreakAvailability = _operatorClient.GetGlobalBreakAvailability().Result;
			//	Connect();
			//}
			//else
			//{
			//	PacsEnabled = false;
			//	if(_employee.InnerPhone.HasValue)
			//	{
			//		_mangoManager.Connect(_employee.InnerPhone.Value);
			//	}
			//}

			LongBreakCommand = new DelegateCommand(() => StartLongBreak().Wait(), () => _operatorService.CanLongBreak);
			LongBreakCommand.CanExecuteChangedWith(_operatorService, x => x.CanLongBreak);

			ShortBreakCommand = new DelegateCommand(() => StartShortBreak().Wait(), () => _operatorService.CanShortBreak);
			ShortBreakCommand.CanExecuteChangedWith(_operatorService, x => x.CanShortBreak);

			OpenPacsDialogCommand = new DelegateCommand(OpenPacsDialog);

			OpenMangoDialogCommand = new DelegateCommand(() => _operatorService.OpenMango());
			OpenMangoDialogCommand.CanExecuteChangedWith(_operatorService, x => x.CanOpenMango);

			_settingsSubscription = settingsPublisher.Subscribe(this);

			//_operatorService.WhenAnyValue(x => x.OperatorState)
			//	.Subscribe(x => _guiDispatcher.RunInGuiTread(() => UpdatePacsInfo()))
			//	.DisposeWith(Subscriptions);

			//_operatorService.WhenAnyValue(x => x.PacsState)
			//	.Subscribe(x => _guiDispatcher.RunInGuiTread(() => PacsState = x))
			//	.DisposeWith(Subscriptions);

			//_operatorService.WhenAnyValue(x => x.LongBreakState)
			//	.Subscribe(x => _guiDispatcher.RunInGuiTread(() => LongBreakState = x))
			//	.DisposeWith(Subscriptions);

			//_operatorService.WhenAnyValue(x => x.ShortBreakState)
			//	.Subscribe(x => _guiDispatcher.RunInGuiTread(() => ShortBreakState = x))
			//	.DisposeWith(Subscriptions);

			//_operatorService.WhenAnyValue(x => x.MangoPhone)
			//	.Subscribe(x => _guiDispatcher.RunInGuiTread(() => MangoPhone = x))
			//	.DisposeWith(Subscriptions);

			//_operatorService.WhenAnyValue(x => x.MangoState)
			//	.Subscribe(x => _guiDispatcher.RunInGuiTread(() => MangoState = x))
			//	.DisposeWith(Subscriptions);

			_operatorService.PropertyChanged += OperatorServicePropertyChanged;
		}

		private void OperatorServicePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case nameof(OperatorService.OperatorState):
					_guiDispatcher.RunInGuiTread(() => UpdatePacsInfo());
					break;
				case nameof(OperatorService.PacsState):
					_guiDispatcher.RunInGuiTread(() => PacsState = _operatorService.PacsState);
					break;
				case nameof(OperatorService.LongBreakState):
					_guiDispatcher.RunInGuiTread(() => LongBreakState = _operatorService.LongBreakState);
					break;
				case nameof(OperatorService.ShortBreakState):
					_guiDispatcher.RunInGuiTread(() => ShortBreakState = _operatorService.ShortBreakState);
					break;
				case nameof(OperatorService.MangoPhone):
					_guiDispatcher.RunInGuiTread(() => MangoPhone = _operatorService.MangoPhone);
					break;
				case nameof(OperatorService.MangoState):
					_guiDispatcher.RunInGuiTread(() => MangoState = _operatorService.MangoState);
					break;
				default:
					break;
			}
		}

		#region Pacs

		public virtual bool PacsEnabled { get; }

		public virtual PacsState PacsState
		{
			get => _pacsState;
			private set => SetField(ref _pacsState, value);
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
			switch(_operatorService.OperatorState.State)
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
				switch(_operatorService.OperatorState.State)
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
			if(_operatorService.OperatorState.State != OperatorStateType.Break)
			{
				return "";
			}

			TimeSpan remains;
			if(_operatorService.OperatorState.BreakType == OperatorBreakType.Long)
			{
				remains = _operatorService.OperatorState.Started + _settings.LongBreakDuration - DateTime.Now;
			}
			else
			{
				remains = _operatorService.OperatorState.Started + _settings.ShortBreakDuration - DateTime.Now;
			}

			BreakTimeGone = remains < TimeSpan.Zero;
			var format = (_breakTimeGone ? "\\-" : "") + "m\\м\\.\\ ss\\с\\.";
			return remains.ToString(format);
		}

		private string GetTalkDurationTime()
		{
			if(_operatorService.OperatorState.State != OperatorStateType.Talk)
			{
				return "";
			}
			return (DateTime.Now - _operatorService.OperatorState.Started).ToString("m\\м\\.\\ ss\\с\\.");
		}

		private void OpenPacsDialog()
		{
			_navigationManager.OpenViewModel<PacsViewModel>(null);
		}

		#endregion Pacs

		#region Break

		public virtual BreakState LongBreakState
		{
			get => _longBreakState;
			private set => SetField(ref _longBreakState, value);
		}

		public virtual BreakState ShortBreakState
		{
			get => _shortBreakState;
			set => SetField(ref _shortBreakState, value);
		}

		private async Task StartLongBreak()
		{
			string question;
			switch(_operatorService.LongBreakState)
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

			await _operatorService.StartLongBreak();
		}

		private async Task StartShortBreak()
		{
			string question;
			switch(_operatorService.ShortBreakState)
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

			await _operatorService.StartShortBreak();
		}

		#endregion Break

		#region Mango


		public virtual string MangoPhone
		{
			get => _mangoPhone;
			set => SetField(ref _mangoPhone, value);
		}

		public virtual MangoState MangoState
		{
			get => _mangoState;
			set => SetField(ref _mangoState, value);
		}

		public override void Dispose()
		{
			_pacsInfoUpdateTimer.Dispose();
			_settingsSubscription.Dispose();
			base.Dispose();
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
}
