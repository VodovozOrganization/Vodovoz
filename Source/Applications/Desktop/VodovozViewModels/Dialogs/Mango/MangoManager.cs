using Mango.Client;
using Mango.Grpc.Client;
using MangoService;
using Microsoft.Extensions.Logging;
using Pacs.Operators.Client;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.Navigation;
using QS.Services;
using QS.Utilities.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Application.Mango;
using Vodovoz.Presentation.ViewModels.Mango;
using Vodovoz.Settings.Mango;
using Vodovoz.ViewModels.Dialogs.Mango.Talks;
using xNetStandard;
using Timer = System.Timers.Timer;

namespace Vodovoz.ViewModels.Dialogs.Mango
{
	public class MangoManager : PropertyChangedBase, IDisposable, IMangoManager
	{
		private readonly ILogger<MangoManager> _logger;
		//private readonly Gtk.Action _toolbarIcon;
		private readonly ILoggerFactory _loggerFactory;
		private readonly IGuiDispatcher _dispatcher;

		//private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		//private readonly IEmployeeService _employeeService;
		private readonly IUserService _userService;
		private readonly INavigationManager _navigation;
		private readonly IMangoSettings _mangoSettings;
		private readonly IMangoViewModelNavigator _mangoViewModelNavigator;
		private readonly IInteractiveService _interactiveService;
		private ConnectionState _connectionState;
		//private uint _extension;
		private MangoServiceClient _mangoServiceClient;
		private CancellationTokenSource _notificationCancellation;
		private IPage _currentPage;
		private Timer _timer;
		private MangoController _mangoController;

		private uint _extensionNumber;

		public MangoManager(
			//Gtk.Action toolbarIcon,
			ILoggerFactory loggerFactory,
			//IUnitOfWorkFactory unitOfWorkFactory,
			//IEmployeeService employeeService,
			IGuiDispatcher dispatcher,
			IUserService userService,
			INavigationManager navigation,
			IMangoSettings mangoSettings,
            IMangoViewModelNavigator mangoViewModelNavigator,
            IInteractiveService interactiveService)
		{
			//this._toolbarIcon = toolbarIcon;
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_dispatcher = dispatcher;
			_logger = _loggerFactory.CreateLogger<MangoManager>();
			//this._unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			//this._employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this._userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this._navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_mangoSettings = mangoSettings ?? throw new ArgumentNullException(nameof(mangoSettings));
			_mangoViewModelNavigator = mangoViewModelNavigator ?? throw new ArgumentNullException(nameof(mangoViewModelNavigator));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			var mangoControllerlogger = _loggerFactory.CreateLogger<MangoController>();
			_mangoController = new MangoController(mangoControllerlogger, mangoSettings.VpbxApiKey, mangoSettings.VpbxApiSalt);

			_timer = new Timer(1000);
			_timer.Elapsed += (s, e) => HandleTimeoutHandler();
			_timer.Start();
			//toolbarIcon.Activated += ToolbarIcon_Activated;
			var userId = this._userService.CurrentUserId;
			/*NotifyConfiguration.Instance.BatchSubscribe(OnUserChanged).IfEntity<Employee>()
				.AndWhere(x => x.User != null && x.User.Id == userId);*/
		}

		#region Current State

		public virtual uint ExtensionNumber
		{
			get => _extensionNumber;
			private set => SetField(ref _extensionNumber, value);
		}


		public ConnectionState ConnectionState
		{
			get => _connectionState;
			private set => SetField(ref _connectionState, value);
		}

        public bool IsActive => ConnectionState != ConnectionState.Disable
            && ConnectionState != ConnectionState.Disconnected;

        public bool CanConnect => ConnectionState == ConnectionState.Disable;

        public TimeSpan? StageDuration => CurrentCall?.StageDuration;

		public string CallerName => CurrentTalk?.CallerName;
		public bool IsOutgoing => CurrentTalk?.Message.Direction == CallDirection.Outgoing || RingingCalls.Any(x => x.IsOutgoing);
		public List<ActiveCall> ActiveCalls { get; set; } = new List<ActiveCall>();

		public IEnumerable<ActiveCall> RingingCalls => ActiveCalls.Where(x => x.CallState == CallState.Appeared);
		public ActiveCall CurrentTalk => ActiveCalls.LastOrDefault(x => x.CallState == CallState.Connected);
		public ActiveCall CurrentHold => ActiveCalls.LastOrDefault(x => x.CallState == CallState.OnHold);
		public ActiveCall CurrentOutgoingRing => RingingCalls.LastOrDefault(x => x.IsOutgoing);
		/// <summary>
		/// Текущий звонок, либо актинвый разговор либо звонок на удержании.
		/// </summary>
		public ActiveCall CurrentCall => CurrentTalk ?? CurrentHold;
		#endregion

		#region Методы

		public void Connect(uint extensionNumber)
		{
			if(!_mangoSettings.MangoEnabled)
			{
				ConnectionState = ConnectionState.Disable;
				return;
			}

			//using(var uow = _unitOfWorkFactory.CreateWithoutRoot("MangoManager Connect")) {
			/*var employee = _employeeService.GetEmployeeForUser(uow, _userService.CurrentUserId);
			if(employee?.InnerPhone == null) {
				ConnectionState = ConnectionState.Disable;
				return;
			}*/

			//На случай переподключения закрываем текущий диалог.
			if(_currentPage != null)
			{
				_navigation.ForceClosePage(_currentPage);
				ActiveCalls.Clear();
			}

			//_extension = employee.InnerPhone.Value;
			ConnectionState = ConnectionState.Disconnected;
			_notificationCancellation = new CancellationTokenSource();
			var mangoServiceClientlogger = _loggerFactory.CreateLogger<MangoServiceClient>();
			ExtensionNumber = extensionNumber;
			_mangoServiceClient = new MangoServiceClient(mangoServiceClientlogger, _mangoSettings, extensionNumber, _notificationCancellation.Token);
			_mangoServiceClient.ChannelStateChanged += MangoServiceClientChannelStateChanged;
			ConnectionState = _mangoServiceClient.IsNotificationActive ? ConnectionState.Connected : ConnectionState.Disconnected;
			_mangoServiceClient.AppearedMessage += MangoServiceClientOnAppearedMessage;
			//}
		}

		public void Disconnect()
		{
			_notificationCancellation?.Cancel();
			_mangoServiceClient?.Dispose();
			ConnectionState = ConnectionState.Disable;
		}

		#endregion

		#region Обработка событий
		private void OnUserChanged(EntityChangeEvent[] changeevents)
		{
			/*_logger.LogInformation("Текущий сотрудник изменён, мог поменятся номер привязки, переподключаемся...");
			_notificationCancellation?.Cancel();
			_mangoServiceClient?.Dispose();
			Connect();*/
		}

		private void MangoServiceClientOnAppearedMessage(object sender, AppearedMessageEventArgs e)
		{
			_dispatcher.RunInGuiTread(() =>
			{
				HandleMessage(e.Message);
			});
		}

		void MangoServiceClientChannelStateChanged(object sender, ConnectionStateEventArgs e)
		{
			_dispatcher.RunInGuiTread(() =>
			{
				if(_mangoServiceClient.IsNotificationActive)
				{
					ConnectionState = ConnectionState.Connected;
				}
				else if(ConnectionState != ConnectionState.Disable)
				{
					ConnectionState = ConnectionState.Disconnected;
				}
			});
		}

		public void OpenMangoDialog()
		{
			if(_connectionState == ConnectionState.Disable || _connectionState == ConnectionState.Disconnected)
			{
				return;
			}

			if(_currentPage == null)
			{
				if(CurrentTalk != null)
				{
					OpenTalkDlg();
				}
				else if(RingingCalls.Any())
				{
					OpenRingDlg();
				}
				else
				{
					_navigation.OpenViewModel<SubscriberSelectionViewModel, MangoManager, DialogType>(null, this, DialogType.Telephone);
				}
			}
			else
			{
				_navigation.SwitchOn(_currentPage);
			}
		}

		void CurrentPage_PageClosed(object sender, PageClosedEventArgs e)
		{
			_currentPage = null;
			ConnectionState = ConnectionState.Connected;
		}

		bool HandleTimeoutHandler()
		{
			_dispatcher.RunInGuiTread(() =>
			{
				if(CurrentCall != null)
				{
					OnPropertyChanged(nameof(StageDuration));
				}

				if(RingingCalls.Any())
				{
					OnPropertyChanged("IncomingCalls.Time");
				}

				ActiveCalls.RemoveAll(x => (x.CallState == CallState.Appeared && x.StageDuration?.TotalSeconds > 120d) ||
										   (x.CallState != CallState.Appeared && x.StageDuration?.TotalMinutes > 60));
			});
			return true;
		}
		#endregion

		#region Обработка сообщения
		private void HandleMessage(NotificationMessage message)
		{
			switch(message.State)
			{
				case CallState.Appeared:
					HandleAppeared(message);
					break;
				case CallState.Connected:
					HandleConnected(message);
					break;
				case CallState.Disconnected:
					HandleDisconnected(message);
					break;
				case CallState.OnHold:
					HandleOnHold(message);
					break;
			}
			_logger.LogTrace("ActiveCalls=\n{ActiveCalls}", DebugPrint.Values(ActiveCalls));
		}

		private void HandleAppeared(NotificationMessage message)
		{
			AddNewMessage(message);
			//Не показывам информацию о новом входящем звонке в момент разговора.
			if(CurrentTalk != null)
			{
				_logger.LogInformation("Звонок не показываем. Идет разговор.");
				return;
			}
			if(_currentPage?.ViewModel is IncomingCallViewModel)
			{
				return;
			}

			if(_currentPage != null)
			{
				_navigation.ForceClosePage(_currentPage);
			}

			if(_currentPage == null)
			{
				OpenRingDlg();
			}
		}

		private void OpenRingDlg()
		{
			_currentPage = _navigation.OpenViewModel<IncomingCallViewModel, MangoManager>(null, this);
			_currentPage.PageClosed += CurrentPage_PageClosed;
			ConnectionState = ConnectionState.Ring;
		}

		private void HandleConnected(NotificationMessage message)
		{
			CleanCalls(CallState.Appeared);
			AddNewMessage(message);
			if(_currentPage != null)
			{
				if(_currentPage.ViewModel is TalkViewModelBase talkViewModel && talkViewModel.ActiveCall.CallId == message.CallId)
				{
					return;
				}
				else
				{
					_navigation.ForceClosePage(_currentPage);
				}
			}
			OpenTalkDlg();
		}

		private void OpenTalkDlg()
		{
			var openCall = CurrentTalk ?? CurrentHold;

			if(openCall.Message.CallFrom.Type == CallerType.Internal)
			{
				_currentPage = _navigation.OpenViewModel<InternalTalkViewModel, MangoManager>(null, this);
				_currentPage.PageClosed += CurrentPage_PageClosed;
			}
			else if(openCall.CounterpartyIds != null && openCall.CounterpartyIds.Any())
			{
                _currentPage = _mangoViewModelNavigator.OpenCounterpartyTalkViewModel();
				_currentPage.PageClosed += CurrentPage_PageClosed;
			}
			else
			{
				_currentPage = _mangoViewModelNavigator.OpenUnknowTalkViewModel();
                _currentPage.PageClosed += CurrentPage_PageClosed;
			}
			ConnectionState = ConnectionState.Talk;
		}

		private void HandleDisconnected(NotificationMessage message)
		{
			var currentState = ConnectionState;

			if(_currentPage != null && (CurrentCall?.CallId == message.CallId || ConnectionState == ConnectionState.Ring))
			{
				_navigation.ForceClosePage(_currentPage);
			}
			TryRemoveCall(message);

			//Если есть какие то другие текущие звонки, создаем соответсвующие диалоги в порядке приоритета.
			if(CurrentTalk != null)
			{
				OpenTalkDlg();
			}
			else if(CurrentOutgoingRing != null)
			{
				OpenRingDlg();
			}
			else if(CurrentHold != null)
			{
				OpenTalkDlg();
			}
			else if(RingingCalls.Any())
			{
				OpenRingDlg();
			}
		}

		private void HandleOnHold(NotificationMessage message)
		{
			AddNewMessage(message);
			if(_currentPage != null)
			{
				if(_currentPage.ViewModel is TalkViewModelBase talkViewModel && talkViewModel.ActiveCall.CallId == message.CallId)
				{
					return;
				}
				else
				{
					_navigation.ForceClosePage(_currentPage);
				}
			}
			OpenTalkDlg();
		}
		#endregion

		#region Обработка коллекции сообщений
		private void AddNewMessage(NotificationMessage message)
		{
			var exist = ActiveCalls.FirstOrDefault(x => x.CallId == message.CallId);
			if(exist != null)
			{
				exist.NewMessage(message);
			}
			else
			{
				ActiveCalls.Add(new ActiveCall(message));
			}

			OnPropertyChanged(nameof(RingingCalls));
		}

		private void CleanCalls(CallState forState)
		{
			ActiveCalls.RemoveAll(x => x.CallState == forState);
			if(forState == CallState.Appeared)
			{
				OnPropertyChanged(nameof(RingingCalls));
			}
		}

		private bool TryRemoveCall(NotificationMessage message)
		{
			if(ActiveCalls.RemoveAll(x => x.CallId == message.CallId) > 0)
			{
				OnPropertyChanged(nameof(RingingCalls));
				return true;
			}
			return false;
		}
		#endregion

		public void AddCounterpartyToCall(int clientId)
		{
			if(CurrentCall == null)
			{
				return;
			}

			CurrentCall.AddClientId(clientId);

			if(_currentPage == null)
			{
                _currentPage = _mangoViewModelNavigator.OpenCounterpartyTalkViewModel();
                _currentPage.PageClosed += CurrentPage_PageClosed;
			}
		}

		#region MangoController_Methods

		public void HangUp()
		{
			var toHangUpCall = CurrentTalk ?? CurrentOutgoingRing ?? ActiveCalls.FirstOrDefault();
			if(toHangUpCall != null)
			{
				_mangoController.HangUp(toHangUpCall.CallId);
				if(_currentPage != null)
				{
					_navigation.ForceClosePage(_currentPage);
				}
			}
		}

		public List<PhoneEntry> GetPhoneBook()
		{
			if(_mangoServiceClient == null)
			{
				return new List<PhoneEntry>();
			}
			return _mangoServiceClient.GetPhonebook();
		}

		public void MakeCall(string to_extension)
		{
			try
			{
				_mangoController.MakeCall(Convert.ToString(this._extensionNumber), to_extension);
			}
			catch(HttpException e)
			{
				if(e.HttpStatusCode == HttpStatusCode.TooManyRequests || e.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Все линии заняты.\nПопробуйте позвонить позже");
				}
				else
				{
					throw;
				}
			}
		}

		public void ForwardCall(string to_extension, ForwardingMethod method)
		{
			if(CurrentTalk == null)
			{
				return;
			}

			_mangoController.ForwardCall(CurrentTalk.CallId, Convert.ToString(this._extensionNumber), to_extension, method);
		}

		#endregion

		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			_notificationCancellation?.Cancel();
			if(_mangoServiceClient != null)
			{
				_mangoServiceClient.Dispose();
			}

			_timer.Dispose();
		}
	}

	
}
