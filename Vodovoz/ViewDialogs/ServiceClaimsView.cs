using System;
using QSTDI;
using NLog;
using Vodovoz.ViewModel;
using QSOrmProject.UpdateNotification;
using Vodovoz.Domain.Service;
using QSOrmProject;
using Gtk;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ServiceClaimsView : TdiTabBase
	{
		static Logger logger = LogManager.GetCurrentClassLogger ();

		public ServiceClaimsView ()
		{
			this.Build ();
			this.TabName = "Журнал заявок сервиса";
			tableServiceClaims.RepresentationModel = new ServiceClaimVM ();
			hboxFilter.Add (tableServiceClaims.RepresentationModel.RepresentationFilter as Widget);
			(tableServiceClaims.RepresentationModel.RepresentationFilter as Widget).Show ();
			tableServiceClaims.RepresentationModel.UpdateNodes ();
			tableServiceClaims.Selection.Changed += OnSelectionChanged;
			buttonEdit.Sensitive = buttonDelete.Sensitive = false;
		}

		void OnRefObjectUpdated (object sender, OrmObjectUpdatedEventArgs e)
		{
			tableServiceClaims.RepresentationModel.UpdateNodes ();

		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = tableServiceClaims.Selection.CountSelectedRows () > 0;
		}

		#region Buttons

		protected void OnButtonAddEnumItemClicked (object sender, EnumItemClickedEventArgs e)
		{
			ServiceClaimDlg dlg = new ServiceClaimDlg (ServiceClaimType.JustService);
			var type = (ServiceClaimTypesForAdding)e.ItemEnum;

			switch (type) {
			case ServiceClaimTypesForAdding.JustService:
				dlg = new ServiceClaimDlg (ServiceClaimType.JustService);
				break;
			case ServiceClaimTypesForAdding.RepairmanCall:
				dlg = new ServiceClaimDlg (ServiceClaimType.RepairmanCall);
				break;
			}
			TabParent.AddTab (dlg, this);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			var dlg = new ServiceClaimDlg ((tableServiceClaims.GetSelectedObjects () [0] as ServiceClaimVMNode).Id);
			TabParent.AddTab (dlg, this);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			OrmMain.DeleteObject (tableServiceClaims.GetSelectedObjects () [0]);
		}

		protected void OnButtonFilterToggled (object sender, EventArgs e)
		{
			hboxFilter.Visible = buttonFilter.Active;
		}

		#endregion

		protected void OnTableServiceClaimsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}
	}
}

