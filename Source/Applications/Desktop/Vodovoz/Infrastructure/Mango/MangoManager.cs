using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClientMangoService;
using Gtk;
using MangoService;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities.Debug;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Employees;
using Vodovoz.Services;
using Vodovoz.ViewModels.Mango;
using Vodovoz.ViewModels.Mango.Talks;
using xNetStandard;

namespace Vodovoz.Infrastructure.Mango
{
	public class MangoManager : PropertyChangedBase, IDisposable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private readonly Gtk.Action toolbarIcon;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IEmployeeService employeeService;
		private readonly IUserService userService;
		private readonly INavigationManager navigation;
		private readonly IInteractiveService _interactiveService;
		private ConnectionState connectionState;
		private uint extension;
		private MangoServiceClient mangoServiceClient;
		private CancellationTokenSource notificationCancellation;
		private IPage CurrentPage;
		private uint timer;
		private MangoController _mangoController;

		public MangoManager(Gtk.Action toolbarIcon,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IUserService userService,
			INavigationManager navigation,
			BaseParametersProvider parameters,
			IInteractiveService interactiveService)
		{
			this.toolbarIcon = toolbarIcon;
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_mangoController = new MangoController(parameters.VpbxApiKey, parameters.VpbxApiSalt);

			timer = GLib.Timeout.Add(1000, new GLib.TimeoutHandler(HandleTimeoutHandler));
			toolbarIcon.Activated += ToolbarIcon_Activated;
			var userId = this.userService.CurrentUserId;
			NotifyConfiguration.Instance.BatchSubscribe(OnUserChanged).IfEntity<Employee>()
				.AndWhere(x => x.User != null && x.User.Id == userId);
		}

		#region Current State
		public ConnectionState ConnectionState {
			get => connectionState; private set {
				if(connectionState == value)
					return;
				connectionState = value;
				var iconName = $"phone-{value.ToString().ToLower()}";
				toolbarIcon.StockId = iconName;
				if(ConnectionState != ConnectionState.Disable)
					toolbarIcon.ShortLabel = extension.ToString();
				else
					toolbarIcon.ShortLabel = "Mango";
			}
		}

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

		public void Connect()
		{
			using(var uow = unitOfWorkFactory.CreateWithoutRoot("MangoManager Connect")) {
				var employee = employeeService.GetEmployeeForUser(uow, userService.CurrentUserId);
				if(employee?.InnerPhone == null) {
					ConnectionState = ConnectionState.Disable;
					return;
				}

				//На случай переподключения закрываем текущий диалог.
				if(CurrentPage != null) {
					navigation.ForceClosePage(CurrentPage);
					ActiveCalls.Clear();
				}

				extension = employee.InnerPhone.Value;
				ConnectionState = ConnectionState.Disconnected;
				notificationCancellation = new CancellationTokenSource();
				mangoServiceClient = new MangoServiceClient(extension, notificationCancellation.Token);
				mangoServiceClient.ChannelStateChanged += MangoServiceClientChannelStateChanged;
				ConnectionState = mangoServiceClient.IsNotificationActive ? ConnectionState.Connected : ConnectionState.Disconnected;
				mangoServiceClient.AppearedMessage += MangoServiceClientOnAppearedMessage;
			}
		}

		#endregion
		#region Обработка событий
		private void OnUserChanged(EntityChangeEvent[] changeevents)
		{
			logger.Info("Текущий сотрудник изменён, мог поменятся номер привязки, переподключаемся...");
			notificationCancellation?.Cancel();
			mangoServiceClient?.Dispose();
			Connect();
		}

		private void MangoServiceClientOnAppearedMessage(object sender, AppearedMessageEventArgs e)
		{
			Application.Invoke((s, arg) => HandleMessage(e.Message));
		}

		void MangoServiceClientChannelStateChanged(object sender, ConnectionStateEventArgs e)
		{
			Gtk.Application.Invoke(delegate {
				if(mangoServiceClient.IsNotificationActive)
					ConnectionState = ConnectionState.Connected;
				else if(ConnectionState != ConnectionState.Disable)
					ConnectionState = ConnectionState.Disconnected;
			});
		}

		void ToolbarIcon_Activated(object sender, EventArgs e)
		{
			if(connectionState == ConnectionState.Disable || connectionState == ConnectionState.Disconnected)
				return;
			if(CurrentPage == null) {
				if(CurrentTalk != null)
					OpenTalkDlg();
				else if(RingingCalls.Any())
					OpenRingDlg();
				else
					navigation.OpenViewModel<SubscriberSelectionViewModel, MangoManager, SubscriberSelectionViewModel.DialogType>(null, this, SubscriberSelectionViewModel.DialogType.Telephone);
			} else
				navigation.SwitchOn(CurrentPage);
		}

		void CurrentPage_PageClosed(object sender, PageClosedEventArgs e)
		{
			CurrentPage = null;
			ConnectionState = ConnectionState.Connected;
		}

		bool HandleTimeoutHandler()
		{
			if(CurrentCall != null)
				OnPropertyChanged(nameof(StageDuration));
			if(RingingCalls.Any())
				OnPropertyChanged("IncomingCalls.Time");

			ActiveCalls.RemoveAll(x => (x.CallState == CallState.Appeared && x.StageDuration?.TotalSeconds > 120d) ||
									   (x.CallState != CallState.Appeared && x.StageDuration?.TotalMinutes > 60));
			return true;
		}
		#endregion

		#region Обработка сообщения
		private void HandleMessage(NotificationMessage message)
		{
			switch(message.State) {
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
			logger.Trace("ActiveCalls=\n" + DebugPrint.Values(ActiveCalls));
		}

		private void HandleAppeared(NotificationMessage message)
		{
			AddNewMessage(message);
			//Не показывам информацию о новом входящем звонке в момент разговора.
			if(CurrentTalk != null) {
				logger.Info("Звонок не показываем. Идет разговор.");
				return;
			}
			if(CurrentPage?.ViewModel is IncomingCallViewModel)
				return;

			if(CurrentPage != null)
				navigation.ForceClosePage(CurrentPage);

			if(CurrentPage == null) {
				OpenRingDlg();
			}
		}

		private void OpenRingDlg()
		{
			CurrentPage = navigation.OpenViewModel<IncomingCallViewModel, MangoManager>(null, this);
			CurrentPage.PageClosed += CurrentPage_PageClosed;
			ConnectionState = ConnectionState.Ring;
		}

		private void HandleConnected(NotificationMessage message)
		{
			CleanCalls(CallState.Appeared);
			AddNewMessage(message);
			if(CurrentPage != null) {
				if(CurrentPage.ViewModel is TalkViewModelBase talkViewModel && talkViewModel.ActiveCall.CallId == message.CallId)
					return;
				else
					navigation.ForceClosePage(CurrentPage);
			}
			OpenTalkDlg();
		}

		private void OpenTalkDlg()
		{
			var openCall = CurrentTalk ?? CurrentHold;

			if(openCall.Message.CallFrom.Type == CallerType.Internal) {
				CurrentPage = navigation.OpenViewModel<InternalTalkViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			} else if(openCall.CounterpartyIds != null && openCall.CounterpartyIds.Any()) {
				CurrentPage = navigation.OpenViewModel<CounterpartyTalkViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			} else {
				CurrentPage = navigation.OpenViewModel<UnknowTalkViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			}
			ConnectionState = ConnectionState.Talk;
		}

		private void HandleDisconnected(NotificationMessage message)
		{
			var currentState = ConnectionState;

			if(CurrentPage != null && (CurrentCall?.CallId == message.CallId || ConnectionState == ConnectionState.Ring)){
				navigation.ForceClosePage(CurrentPage);
			}
			TryRemoveCall(message);

			//Если есть какие то другие текущие звонки, создаем соответсвующие диалоги в порядке приоритета.
			if(CurrentTalk != null)
				OpenTalkDlg();
			else if(CurrentOutgoingRing != null)
				OpenRingDlg();
			else if(CurrentHold != null)
				OpenTalkDlg();
			else if(RingingCalls.Any())
				OpenRingDlg();
		}

		private void HandleOnHold(NotificationMessage message)
		{
			AddNewMessage(message);
			if(CurrentPage != null) {
				if(CurrentPage.ViewModel is TalkViewModelBase talkViewModel && talkViewModel.ActiveCall.CallId == message.CallId)
					return;
				else
					navigation.ForceClosePage(CurrentPage);
			}
			OpenTalkDlg();
		}
		#endregion
		#region Обработка коллекции сообщений
		private void AddNewMessage(NotificationMessage message)
		{
			var exist = ActiveCalls.FirstOrDefault(x => x.CallId == message.CallId);
			if(exist != null)
				exist.NewMessage(message);
			else
				ActiveCalls.Add(new ActiveCall(message));
			OnPropertyChanged(nameof(RingingCalls));
		}

		private void CleanCalls(CallState forState)
		{
			ActiveCalls.RemoveAll(x => x.CallState == forState);
			if(forState == CallState.Appeared)
				OnPropertyChanged(nameof(RingingCalls));
		}

		private bool TryRemoveCall(NotificationMessage message)
		{
			if(ActiveCalls.RemoveAll(x => x.CallId == message.CallId) > 0) {
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
			
			if(CurrentPage == null) 
			{
				CurrentPage = navigation.OpenViewModel<CounterpartyTalkViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			}
		}

		#region MangoController_Methods

		public void HangUp()
		{
			var toHangUpCall = CurrentTalk ?? CurrentOutgoingRing ?? ActiveCalls.FirstOrDefault();
			if(toHangUpCall != null) {
				_mangoController.HangUp(toHangUpCall.CallId);
				if(CurrentPage != null)
					navigation.ForceClosePage(CurrentPage);
			}
		}

		public List<PhoneEntry> GetPhoneBook()
		{
			return mangoServiceClient.GetPhonebook();
		}

		public void MakeCall(string to_extension)
		{
			try
			{
				_mangoController.MakeCall(Convert.ToString(this.extension), to_extension);
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
			
			_mangoController.ForwardCall(CurrentTalk.CallId, Convert.ToString(this.extension), to_extension, method);
		}

		#endregion
		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			notificationCancellation?.Cancel();
			if(mangoServiceClient != null)
				mangoServiceClient.Dispose();
			GLib.Source.Remove(timer);
		}

	}

	public enum ConnectionState
	{
		Connected,
		Disable,
		Disconnected,
		Ring,
		Talk
	}
}
