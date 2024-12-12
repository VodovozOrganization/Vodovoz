using Microsoft.Extensions.Logging;
using Pacs.Core;
using Pacs.Core.Messages.Events;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using Vodovoz.Application.Pacs;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Pacs;
using Timer = System.Timers.Timer;

namespace Vodovoz.Presentation.ViewModels.Pacs
{
	public class PacsPanelViewModel : WidgetViewModelBase, IObserver<SettingsEvent>, IDisposable
	{
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
		private bool _breakTimeGone;

		private Timer _pacsInfoUpdateTimer;
		private IPacsDomainSettings _settings;
		private string _pacsInfo = "";

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

			_pacsInfoUpdateTimer = new Timer(1000);
			_pacsInfoUpdateTimer.Elapsed += OnInfoTick;
			_settings = pacsRepository.GetPacsDomainSettings();

			PacsEnabled = _operatorService.IsInitialized;
			IsOperator = PacsEnabled && _operatorService.IsOperator;

			LongBreakCommand = new DelegateCommand(() => StartLongBreak().Wait(), () => CanLongBreakOrEnd);
			LongBreakCommand.CanExecuteChangedWith(this, x => x.CanLongBreakOrEnd);

			ShortBreakCommand = new DelegateCommand(() => StartShortBreak().Wait(), () => CanShortBreakOrEnd);
			ShortBreakCommand.CanExecuteChangedWith(this, x => x.CanShortBreakOrEnd);

			OpenPacsDialogCommand = new DelegateCommand(OpenPacsDialog);

			OpenMangoDialogCommand = new DelegateCommand(() => _operatorService.OpenMango());
			OpenMangoDialogCommand.CanExecuteChangedWith(_operatorService, x => x.CanOpenMango);

			_settingsSubscription = settingsPublisher.Subscribe(this);

			_operatorService.PropertyChanged += OperatorServicePropertyChanged;

			PacsState = _operatorService.PacsState;
			LongBreakState = _operatorService.LongBreakState;
			ShortBreakState = _operatorService.ShortBreakState;
			MangoPhone = _operatorService.MangoPhone;
			MangoState = _operatorService.MangoState;
		}

		private void OperatorServicePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			_guiDispatcher.RunInGuiTread(() => {
				switch(e.PropertyName)
				{
					case nameof(OperatorService.OperatorState):
						UpdatePacsInfo();
						break;
					case nameof(OperatorService.PacsState):
						PacsState = _operatorService.PacsState;
						break;
					case nameof(OperatorService.LongBreakState):
						LongBreakState = _operatorService.LongBreakState;
						break;
					case nameof(OperatorService.CanStartLongBreak):
						OnPropertyChanged(nameof(CanLongBreakOrEnd));
						break;
					case nameof(OperatorService.ShortBreakState):
						ShortBreakState = _operatorService.ShortBreakState;
						break;
					case nameof(OperatorService.CanStartShortBreak):
						OnPropertyChanged(nameof(CanShortBreakOrEnd));
						break;
					case nameof(OperatorService.CanEndBreak):
						OnPropertyChanged(nameof(CanLongBreakOrEnd));
						OnPropertyChanged(nameof(CanShortBreakOrEnd));
						break;
					case nameof(OperatorService.MangoPhone):
						MangoPhone = _operatorService.MangoPhone;
						break;
					case nameof(OperatorService.MangoState):
						MangoState = _operatorService.MangoState;
						break;
					default:
						break;
				}
			});
		}

		#region Pacs

		public bool PacsEnabled { get; }
		public bool IsOperator { get; }

		public PacsState PacsState
		{
			get => _pacsState;
			private set => SetField(ref _pacsState, value);
		}

		public string PacsInfo
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

		public bool BreakTimeGone
		{
			get => _breakTimeGone;
			private set => SetField(ref _breakTimeGone, value);
		}

		private void OnInfoTick(object sender, ElapsedEventArgs e)
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

		[PropertyChangedAlso(nameof(CanLongBreakOrEnd))]
		public BreakState LongBreakState
		{
			get => _longBreakState;
			private set => SetField(ref _longBreakState, value);
		}

		public bool CanLongBreakOrEnd => (_operatorService.CanStartLongBreak && LongBreakState == BreakState.CanStartBreak) 
			|| (_operatorService.CanEndBreak && LongBreakState == BreakState.CanEndBreak);

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

			try
			{
				switch(_operatorService.ShortBreakState)
				{
					case BreakState.CanStartBreak:
						await _operatorService.StartLongBreak();
						break;
					case BreakState.CanEndBreak:
						await _operatorService.EndBreak();
						break;
					case BreakState.BreakDenied:
					default:
						return;
				}
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Error, ex.Message));
			}
		}

		[PropertyChangedAlso(nameof(CanShortBreakOrEnd))]
		public BreakState ShortBreakState
		{
			get => _shortBreakState;
			private set => SetField(ref _shortBreakState, value);
		}

		public bool CanShortBreakOrEnd => (_operatorService.CanStartShortBreak && ShortBreakState == BreakState.CanStartBreak)
			|| (_operatorService.CanEndBreak && ShortBreakState == BreakState.CanEndBreak);

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

			try
			{
				switch(_operatorService.ShortBreakState)
				{
					case BreakState.CanStartBreak:
						await _operatorService.StartShortBreak();
						break;
					case BreakState.CanEndBreak:
						await _operatorService.EndBreak();
						break;
					case BreakState.BreakDenied:
					default:
						return;
				}
			}
			catch(PacsException ex)
			{
				_guiDispatcher.RunInGuiTread(() => _interactiveService.ShowMessage(ImportanceLevel.Error, ex.Message));
			}
		}

		#endregion Break

		#region Mango

		public string MangoPhone
		{
			get => _mangoPhone;
			private set => SetField(ref _mangoPhone, value);
		}

		public MangoState MangoState
		{
			get => _mangoState;
			private set => SetField(ref _mangoState, value);
		}

		public void Dispose()
		{
			_pacsInfoUpdateTimer.Dispose();
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
}
