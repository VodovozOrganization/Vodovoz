using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ClientMangoService;
using Gtk;
using MangoService;
using NHibernate.Util;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Utilities;
using QS.Utilities.Debug;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories;
using Vodovoz.Infrastructure.Services;
using Vodovoz.ViewModels.Mango;
using Vodovoz.ViewModels.Mango.Talks;

namespace Vodovoz.Infrastructure.Mango
{
	public class MangoManager : PropertyChangedBase, IDisposable
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private readonly Gtk.Action toolbarIcon;
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		private readonly IEmployeeService employeeService;
		private readonly IUserService userService;
		private readonly INavigationManager navigation;
		private readonly PhoneRepository phoneRepository;
		private ConnectionState connectionState;
		private uint extension;
		private MangoNotificationClient notificationClient;
		private CancellationTokenSource notificationCancellation;
		private IPage CurrentPage;
		private uint timer;
		private MangoService.MangoController mangoController;

		public MangoManager(Gtk.Action toolbarIcon,
			IUnitOfWorkFactory unitOfWorkFactory,
			IEmployeeService employeeService,
			IUserService userService,
			INavigationManager navigation,
			BaseParametersProvider parametrs,
			PhoneRepository phoneRepository)
		{
			this.toolbarIcon = toolbarIcon;
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
			this.navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			this.phoneRepository = phoneRepository ?? throw new ArgumentNullException(nameof(phoneRepository));
 			this.mangoController = new MangoService.MangoController(parametrs.VpbxApiKey, parametrs.VpbxApiSalt);

			timer = GLib.Timeout.Add (1000, new GLib.TimeoutHandler(HandleTimeoutHandler));
			toolbarIcon.Activated += ToolbarIcon_Activated;
			var userId = this.userService.CurrentUserId;
			NotifyConfiguration.Instance.BatchSubscribe(OnUserChanged).IfEntity<Employee>()
				.AndWhere(x => x.User.Id == userId);
		}

		#region Current State
		public ConnectionState ConnectionState {
			get => connectionState; private set {
				connectionState = value;
				var iconName = $"phone-{value.ToString().ToLower()}";
				toolbarIcon.StockId = iconName;
				if(ConnectionState == ConnectionState.Connected || ConnectionState == ConnectionState.Disconnected)
					toolbarIcon.ShortLabel = extension.ToString();
				else
					toolbarIcon.ShortLabel = "Mango";
				GtkHelper.WaitRedraw();
			}
		}
		
		public TimeSpan? StageDuration => CurrentTalk?.StageDuration;

		public string CallerName => CurrentTalk?.CallerName;
		public Phone Phone => CurrentTalk != null ? new Phone(CurrentTalk.CallerNumber) : null; 
		public bool IsOutgoing => CurrentTalk?.Message.Direction == CallDirection.Outgoing || RingingCalls.Any(x => x.IsOutgoing);
		public List<ActiveCall> ActiveCalls { get; set; } = new List<ActiveCall>();

		public IEnumerable<ActiveCall> RingingCalls => ActiveCalls.Where(x => x.CallState == CallState.Appeared);
		public ActiveCall CurrentTalk => ActiveCalls.LastOrDefault(x => x.CallState == CallState.Connected);
		public ActiveCall CurrentHold => ActiveCalls.LastOrDefault(x => x.CallState == CallState.OnHold);
		public ActiveCall CurrentOutgoingRing => RingingCalls.LastOrDefault(x => x.IsOutgoing);
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
				notificationClient = new MangoNotificationClient(extension, notificationCancellation.Token);
				notificationClient.ChanalStateChanged+= NotificationClient_ChanalStateChanged;
				ConnectionState = notificationClient.IsNotificationActive ? ConnectionState.Connected : ConnectionState.Disconnected;
				notificationClient.AppearedMessage += NotificationClientOnAppearedMessage;
			}
		}

		#endregion
		#region Обработка событий
		private void OnUserChanged(EntityChangeEvent[] changeevents)
		{
			logger.Info("Текущий сотрудник изменён, мог поменятся номер привязки, переподключаемся...");
			notificationCancellation?.Cancel();
			notificationClient?.Dispose();
			Connect();
		}

		private void NotificationClientOnAppearedMessage(object sender, AppearedMessageEventArgs e)
		{
			Application.Invoke((s, arg) => HandleMessage(e.Message));
		}

		void NotificationClient_ChanalStateChanged(object sender, ConnectionStateEventArgs e)
		{
			Gtk.Application.Invoke(delegate {
				ConnectionState = notificationClient.IsNotificationActive ? ConnectionState.Connected : ConnectionState.Disconnected;
			});
		}

		void ToolbarIcon_Activated(object sender, EventArgs e)
		{
			if(CurrentPage == null) {
				CurrentPage = navigation.OpenViewModel<SubscriberSelectionViewModel, MangoManager, SubscriberSelectionViewModel.DialogType>(null, this, SubscriberSelectionViewModel.DialogType.Telephone);
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
			if(CurrentTalk != null)
				OnPropertyChanged(nameof(StageDuration));
			if(RingingCalls.Any())
				OnPropertyChanged("IncomingCalls.Time");

			ActiveCalls.RemoveAll(x => (x.CallState == CallState.Appeared && x.StageDuration?.TotalSeconds > 120d) ||
			                           (x.CallState != CallState.Appeared && x.StageDuration?.TotalMinutes > 60));
			return true;
		}
		#endregion

		private List<int> AddedClients = new List<int>();
		public IEnumerable<int> Clients => CurrentTalk?.CounterpartyIds.Concat(AddedClients);

		#region Обработка сообщения
		private void HandleMessage(NotificationMessage message)
		{
			logger.Trace("ActiveCalls=\n" + DebugPrint.Values(ActiveCalls));
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
		}

		private void HandleAppeared(NotificationMessage message)
		{
			AddNewMessage(message);
			//Не показывам информацию новом входящем звонке в момент разговора.
			if(CurrentTalk != null) {
				logger.Info("Звонок не показываем. Идет разговор.");
				return;
			}
			if(CurrentPage?.ViewModel is IncomingCallViewModel)
				return;

			ConnectionState = ConnectionState.Ring;
			if(CurrentPage != null)
				navigation.ForceClosePage(CurrentPage);

			if(CurrentPage == null) {
				CurrentPage = navigation.OpenViewModel<IncomingCallViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			}
		}

		private void HandleConnected(NotificationMessage message)
		{
			CleanCalls(CallState.Appeared);
			AddNewMessage(message);
			if(CurrentPage != null)
				navigation.ForceClosePage(CurrentPage);
			ConnectionState = ConnectionState.Talk;
			AddedClients.Clear();
			if(message.CallFrom.Type == CallerType.Internal) {
				CurrentPage = navigation.OpenViewModel<InternalTalkViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			} else if(Clients != null && Clients.Count() > 0) {
				CurrentPage = navigation.OpenViewModel<CounterpartyTalkViewModel, MangoManager, IEnumerable<int>>(null, this, Clients);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			} else {
				CurrentPage = navigation.OpenViewModel<UnknowTalkViewModel, MangoManager>(null, this);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			}
		}

		private void HandleDisconnected(NotificationMessage message)
		{
			if (CurrentTalk?.CallId == message.CallId)
			{
				if(CurrentPage != null)
					navigation.ForceClosePage(CurrentPage);
				ConnectionState = ConnectionState.Connected;
			}
			TryRemoveCall(message); 

			if(ConnectionState == ConnectionState.Ring) {
				//HACK сожалению другие способы уменьшения окна с телефонами не сработали. Поэтому просто преотрываем окно.
				if(CurrentPage != null)
					navigation.ForceClosePage(CurrentPage);
				if(RingingCalls.Any()) {
					CurrentPage = navigation.OpenViewModel<IncomingCallViewModel, MangoManager>(null, this);
					CurrentPage.PageClosed += CurrentPage_PageClosed;
				} else
					ConnectionState = ConnectionState.Connected;
			}
		}

		private void HandleOnHold(NotificationMessage message)
		{
			AddNewMessage(message);
			if(CurrentPage != null)
				navigation.ForceClosePage(CurrentPage);
			ConnectionState = ConnectionState.Connected;
		}
		#endregion
		#region Обработка коллекции сообщений
		private void AddNewMessage(NotificationMessage message)
		{
			ActiveCalls.RemoveAll(x => x.CallId == message.CallId);
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

		public void AddCounterpartyToCall(Counterparty client , bool changeCallState)
		{
			AddedClients.Add(client.Id);
			if(changeCallState) {
				CurrentPage = navigation.OpenViewModel<CounterpartyTalkViewModel, MangoManager, IEnumerable<int>>(null, this, Clients);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
			}
		}

		#region MangoController_Methods

		public void HangUp()
		{
			var toHangUpCall = CurrentTalk ?? CurrentOutgoingRing ?? ActiveCalls.FirstOrDefault();
			if (toHangUpCall != null)
			{
				mangoController.HangUp(toHangUpCall.CallId);
				if(CurrentPage != null)
					navigation.ForceClosePage(CurrentPage);
			}
		}

		public IEnumerable<MangoService.DTO.Group.Group> GetAllVPBXGroups()
		{
			return mangoController.GetAllVpbxGroups();
		}

		public IEnumerable<MangoService.DTO.Users.User> GetAllVpbxUsers()
		{
			return mangoController.GetAllVPBXUsers();
		}

		public void MakeCall(string to_extension)
		{
			mangoController.MakeCall(Convert.ToString(this.extension),to_extension);
		}

		public void ForwardCall(string to_extension,ForwardingMethod method)
		{
			mangoController.ForwardCall(CurrentTalk.CallId,Convert.ToString(this.extension),to_extension, method);
		}

		#endregion
		public void Dispose()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			notificationCancellation?.Cancel();
			if(notificationClient != null)
				notificationClient.Dispose();
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
