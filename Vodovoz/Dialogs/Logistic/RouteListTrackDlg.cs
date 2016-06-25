using System;
using QSOrmProject;
using QSTDI;
using Chat;
using System.ServiceModel;
using System.Threading;
using Vodovoz.Repository.Chat;
using Vodovoz.Domain.Employees;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListTrackDlg : TdiTabBase, IChatCallback
	{
		private TimerCallback callback;
		private Timer timer;
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();
		private IChatCallbackService proxy;
		private const int REFRESH_PERIOD = 180000;

		public RouteListTrackDlg()
		{
			this.Build();
			this.TabName = "Мониторинг";
			yTreeViewDrivers.RepresentationModel = new ViewModel.WorkingDriversVM(uow);
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;
			buttonChat.Sensitive = false;
			proxy = new DuplexChannelFactory<IChatCallbackService>(
				new InstanceContext(this), 
				new NetTcpBinding(), 
				"net.Tcp://vod-srv.qsolution.ru:9001/ChatCallbackService").CreateChannel();
			proxy.SubscribeForUpdates(1);

			//Initiates connection keep alive query every 5 minutes.
			callback = new TimerCallback(Refresh);
			timer = new Timer(callback, null, 0, REFRESH_PERIOD);
		}

		void OnSelectionChanged(object sender, EventArgs e)
		{
			bool selected = yTreeViewDrivers.Selection.CountSelectedRows() > 0;
			buttonChat.Sensitive = selected;
		}

		protected void OnToggleButtonHideAddressesToggled(object sender, EventArgs e)
		{
			if (toggleButtonHideAddresses.Active)
			{
				//TODO Hiding and showing logic
			}
			else
			{
				//TODO
			}
		}

		protected void OnYTreeViewDriversRowActivated(object o, Gtk.RowActivatedArgs args)
		{
			yTreeAddresses.RepresentationModel = new ViewModel.DriverRouteListAddressesVM(uow, yTreeViewDrivers.GetSelectedId());
			yTreeAddresses.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonChatClicked(object sender, EventArgs e)
		{
			var driver = uow.GetById<Employee>(yTreeViewDrivers.GetSelectedId());
			var chat = ChatRepository.GetChatForDriver(uow, driver);
			if (chat != null)
			{
				TabParent.OpenTab(ChatWidget.GenerateHashName(chat.Id),
					() => new ChatWidget(chat.Id)
				);
			}
		}

		private void Refresh(Object StateInfo)
		{
			proxy.KeepAlive();
		}

		#region IChatCallback implementation

		public void NotifyNewMessage(int chatId)
		{
			Console.WriteLine("Chat updated: {0}", chatId);
		}
			
		#endregion
	}
}

