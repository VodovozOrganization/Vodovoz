using System;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListTrackDlg : TdiTabBase
	{
		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		public RouteListTrackDlg()
		{
			this.Build();
			this.TabName = "Мониторинг";
			yTreeViewDrivers.RepresentationModel = new ViewModel.WorkingDriversVM();
			yTreeViewDrivers.RepresentationModel.UpdateNodes();
			yTreeViewDrivers.Selection.Changed += OnSelectionChanged;
			buttonChat.Sensitive = false;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = yTreeViewDrivers.Selection.CountSelectedRows () > 0;
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


		protected void OnYTreeViewDriversRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			yTreeAddresses.RepresentationModel = new ViewModel.DriverRouteListAddressesVM(yTreeViewDrivers.GetSelectedId());
			yTreeAddresses.RepresentationModel.UpdateNodes();
		}

		protected void OnButtonChatClicked (object sender, EventArgs e)
		{
			//TODO
		}
	}
}

