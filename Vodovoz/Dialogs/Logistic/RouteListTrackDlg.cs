using System;
using QSOrmProject;
using QSTDI;
using Chat;
using System.ServiceModel;
using System.Threading;
using Vodovoz.Repository.Chat;
using ChatClass = Vodovoz.Domain.Chat.Chat;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Chat;
using Vodovoz.Repository;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListTrackDlg : TdiTabBase, IChatCallbackObserver
	{
		private IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		public RouteListTrackDlg()
		{
			this.Build();
			this.TabName = "Мониторинг";
			yTreeViewDrivers.RepresentationModel = new ViewModel.WorkingDriversVM(uow);
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;
			buttonChat.Sensitive = false;
			if (!ChatCallbackObservable.IsInitiated)
				ChatCallbackObservable.CreateInstance(EmployeeRepository.GetEmployeeForCurrentUser(uow).Id);
			ChatCallbackObservable.GetInstance().AddObserver(this);
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
			if (chat == null)
			{
				var chatUoW = UnitOfWorkFactory.CreateWithNewRoot<ChatClass>();
				chatUoW.Root.ChatType = ChatType.DriverAndLogists;
				chatUoW.Root.Driver = driver;
				chatUoW.Save();
				chat = chatUoW.Root;

			}
			TabParent.OpenTab(ChatWidget.GenerateHashName(chat.Id),
				() => new ChatWidget(chat.Id)
			);
		}

		public override void Destroy()
		{
			ChatCallbackObservable.GetInstance().RemoveObserver(this);
			base.Destroy();
		}

		#region IChatCallbackObserver implementation

		public void HandleChatUpdate()
		{
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
		}

		public int? ChatId { get { return null; } }

		public int? RequestedRefreshInterval { get { return null; } }
		#endregion
	}
}

