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
using QS.ViewModels.Control.EEVM;
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
		private NotificationMessage LastMessage;
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
			QS.DomainModel.NotifyChange.NotifyConfiguration.Instance.BatchSubscribe(OnUserChanged).IfEntity<Employee>()
				.AndWhere(x => x.User.Id == userId);
		}

		public ConnectionState ConnectionState {
			#region Current State
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

		public DateTime? StageBegin => LastMessage?.Timestamp.ToDateTime();
		public TimeSpan? StageDuration => DateTime.Now - StageBegin;

		public string CallerName => LastMessage.CallFrom.Names != null ? String.Join("\n", LastMessage.CallFrom.Names.Select(x => x.Name)) : null;
		public string CallerNumber => LastMessage?.CallFrom.Number;
		public Phone Phone => new Phone(CallerNumber); 
		public bool IsOutgoing => LastMessage?.Direction == CallDirection.Outgoing || IncomingCalls.Any(x => x.IsOutgoing);
		public bool IsTransfer => LastMessage?.IsTransfer ?? false;
		public Caller PrimaryCaller => LastMessage?.PrimaryCaller;

		public List<IncomingCall> IncomingCalls { get; set; } = new List<IncomingCall>();
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
					LastMessage = null;
				}

				extension = employee.InnerPhone.Value;
				ConnectionState = ConnectionState.Disconnected;
				notificationCancellation = new CancellationTokenSource();
				notificationClient = new MangoNotificationClient(extension, notificationCancellation.Token);
				notificationClient.ChanalStateChanged+= NotificationClient_ChanalStateChanged;
				ConnectionState = notificationClient.IsNotificationActive ? ConnectionState.Connected : ConnectionState.Disconnected;
				notificationClient.IncomeCall += NotificationClientOnIncomeCall;
			}
		}

		#endregion
		#region Обработка событий
		private void OnUserChanged(EntityChangeEvent[] changeevents)
		{
			logger.Info("Текущий сотрудник именён, мог поменятся номер привязки, переподключаемся...");
			notificationCancellation?.Cancel();
			notificationClient?.Dispose();
			Connect();
		}

		private void NotificationClientOnIncomeCall(object sender, IncomeCallEventArgs e)
		{
			LastMessage = e.Message;
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
				//if(LastMessage != null)
				//	HandleMessage(LastMessage);
				//else {
				CurrentPage = navigation.OpenViewModel<SubscriberSelectionViewModel, MangoManager, SubscriberSelectionViewModel.DialogType>(null, this, SubscriberSelectionViewModel.DialogType.Telephone);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
				//}
			} 
		}

		void CurrentPage_PageClosed(object sender, PageClosedEventArgs e)
		{
			CurrentPage = null;
			ConnectionState = ConnectionState.Connected;
		}

		bool HandleTimeoutHandler()
		{
			if(LastMessage != null)
				OnPropertyChanged(nameof(StageDuration));
			if(IncomingCalls.Any())
				OnPropertyChanged("IncomingCalls.Time");


			IncomingCalls.RemoveAll(x => x.StageDuration.Value.TotalSeconds > 120d);
			return true;
		}
		#endregion

		private IEnumerable<int> callerClients => LastMessage.CallFrom.Names.Where(n => n.CounterpartyId > 0).Select(n => Convert.ToInt32(n.CounterpartyId)).Distinct();
		public List<int> Clients { get; private set; } = new List<int>();
		public int Employee => LastMessage.CallFrom.Names.Where(n => n.EmployeeId > 0).Select(n => Convert.ToInt32(n.EmployeeId)).FirstOrDefault();
		#region Работа с сообщениями

		private void FoundByPhoneItemsConfigure()
		{
			if (Clients != null)
			{
				Clients = new List<int>();
				if(callerClients.Count() > 0)
					Clients.AddRange(callerClients);
			}
		}

		private void HandleMessage(NotificationMessage message)
		{
			if(message.State == CallState.Appeared) {
				ConnectionState = ConnectionState.Ring;
				AddNewIncome(message);
				if(CurrentPage == null) {
					CurrentPage = navigation.OpenViewModel<IncomingCallViewModel, MangoManager>(null, this , OpenPageOptions.IgnoreHash);
					CurrentPage.PageClosed += CurrentPage_PageClosed;
				}
				return;
			}

			if(message.State == CallState.Disconnected) {
				if(TryRemoveIncome(message)) { //HACK сожалению другие способы уменьшения окна с телефонами не сработали. Поэтому просто преотрываем окно.
					if(CurrentPage != null)
						navigation.ForceClosePage(CurrentPage);
					if(IncomingCalls.Any()) {
						CurrentPage = navigation.OpenViewModel<IncomingCallViewModel, MangoManager>(null, this,OpenPageOptions.IgnoreHash);
						CurrentPage.PageClosed += CurrentPage_PageClosed;
					}
					return;
				}
				LastMessage = null;
			}

			if(CurrentPage != null) {
				navigation.ForceClosePage(CurrentPage);
			}

			if(message.State == CallState.Connected)
			{
				CleanIncomingCalls();
				ConnectionState = ConnectionState.Talk;
				FoundByPhoneItemsConfigure();
				if(message.CallFrom.Type == CallerType.Internal) {
					CurrentPage = navigation.OpenViewModel<InternalTalkViewModel, MangoManager>(null,this);
					CurrentPage.PageClosed += CurrentPage_PageClosed;
				}
				else
				{
					if(Clients != null && Clients.Count() > 0) {
						CurrentPage = navigation.OpenViewModel<CounterpartyTalkViewModel, MangoManager, IEnumerable<int>>(null, this, Clients);
						CurrentPage.PageClosed += CurrentPage_PageClosed;
					} else {
						CurrentPage = navigation.OpenViewModel<UnknowTalkViewModel, MangoManager>(null, this);
						CurrentPage.PageClosed += CurrentPage_PageClosed;
					}
				}
			}
		}

		private void AddNewIncome(NotificationMessage message)
		{
			if(IncomingCalls.Any(x => x.Message == message)) {
				return;
			}
			IncomingCalls.Add(new IncomingCall(message));
			OnPropertyChanged(nameof(IncomingCalls));
		}

		private void CleanIncomingCalls()
		{
			if(!IncomingCalls.Any()) {
				return;
			}
			IncomingCalls.Clear();
			OnPropertyChanged(nameof(IncomingCalls));
		}

		private bool TryRemoveIncome(NotificationMessage message)
		{
			if(IncomingCalls.RemoveAll(x => x.CallId == message.CallId) > 0) {
				OnPropertyChanged(nameof(IncomingCalls));
				return true;
			}
			return false;
		}

		public void AddedCounterpartyToCall(Counterparty client , bool changeCallState)
		{
			if (Clients == null)
				Clients = new List<int>();
			Clients.Add(client.Id);
			if(changeCallState) {
				CurrentPage = navigation.OpenViewModel<CounterpartyTalkViewModel, MangoManager, IEnumerable<int>>(null, this, Clients);
				CurrentPage.PageClosed += CurrentPage_PageClosed;
				//if(LastMessage != null && CurrentPage == null)
					//HandleMessage(LastMessage);
			}
		}

		#endregion
		#region MangoController_Methods

		public void HangUp()
		{
			mangoController.HangUp(LastMessage.CallId);
			LastMessage = null;
			CleanIncomingCalls();
			if(CurrentPage != null)
				navigation.ForceClosePage(CurrentPage);
		}

		public IEnumerable<MangoService.DTO.Group.Group> GetAllVPBXGroups()
		{
			return mangoController.GetAllVpbxGroups();
		}

		public IEnumerable<MangoService.DTO.Users.User> GetAllVPBXEmploies()
		{
			return mangoController.GetAllVPBXEmploies();
		}

		public void MakeCall(string to_extension)
		{
			mangoController.MakeCall(Convert.ToString(this.extension),to_extension);
		}

		public void ForwardCall(string to_extension,ForwardingMethod method)
		{
			mangoController.ForwardCall(LastMessage.CallId,Convert.ToString(this.extension),to_extension, method);
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
