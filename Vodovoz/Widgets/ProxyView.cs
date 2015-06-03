using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ProxyView : Gtk.Bin
	{
		private IProxyOwner proxyOwner;
		private GenericObservableList<Proxy> proxiesList;

		public IProxyOwner ProxyOwner {
			get { return proxyOwner; }
			set {
				proxyOwner = value;
				if (ProxyOwner.Proxies == null)
					ProxyOwner.Proxies = new List<Proxy> ();
				proxiesList = new GenericObservableList<Proxy> (ProxyOwner.Proxies);
				datatreeviewProxies.ItemsDataSource = proxiesList;
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

			if(parentDlg.UoW.IsNew)
			{
				if (CommonDialogs.SaveBeforeCreateSlaveEntity (parentDlg.Subject.GetType (), typeof(Proxy))) {
					parentDlg.UoW.Save ();
				} else
					return;
			}

			ITdiDialog dlg = new ProxyDlg (ProxyOwner as Counterparty);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = new ProxyDlg (datatreeviewProxies.GetSelectedObjects () [0] as Proxy);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnDatatreeviewProxiesRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			var proxy = datatreeviewProxies.GetSelectedObjects () [0] as Proxy;

			if (OrmMain.DeleteObject (proxy)) {
				proxiesList.Remove (proxy);
			}
		}
	}
}

