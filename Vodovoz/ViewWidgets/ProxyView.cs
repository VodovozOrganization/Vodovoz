using System;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProxyView : Gtk.Bin
	{
		private IUnitOfWorkGeneric<Counterparty> counterpartyUoW;

		public IUnitOfWorkGeneric<Counterparty> CounterpartyUoW {
			get {
				return counterpartyUoW;
			}
			set {
				if (counterpartyUoW == value)
					return;
				counterpartyUoW = value;
				datatreeviewProxies.RepresentationModel = new ViewModel.ProxiesVM (value);
				datatreeviewProxies.RepresentationModel.UpdateNodes ();
			}
		}


		public ProxyView ()
		{
			this.Build ();
			datatreeviewProxies.Selection.Changed += OnSelectionChanged;


		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewProxies.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			var parentDlg = OrmMain.FindMyDialog (this);
			if (parentDlg == null)
				return;

			if (parentDlg.UoW.IsNew) {
				if (CommonDialogs.SaveBeforeCreateSlaveEntity (parentDlg.EntityObject.GetType (), typeof(Proxy))) {
					parentDlg.UoW.Save ();
				} else
					return;
			}

			ITdiDialog dlg = new ProxyDlg (CounterpartyUoW.Root);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = new ProxyDlg (datatreeviewProxies.GetSelectedId ());
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnDatatreeviewProxiesRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (OrmMain.DeleteObject (typeof(Proxy),
				    datatreeviewProxies.GetSelectedId ())) {
				datatreeviewProxies.RepresentationModel.UpdateNodes ();
			}
		}
	}
}

