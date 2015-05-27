using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
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
		private ISession session;

		public ISession Session {
			get { return session; }
			set { session = value; }
		}

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

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IProxyOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IProxyOwner)));
					}
					ProxyOwner = (IProxyOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
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

			ProxyDlg dlg = new ProxyDlg (ParentReference);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ProxyDlg dlg = new ProxyDlg (ParentReference, datatreeviewProxies.GetSelectedObjects () [0] as Proxy);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnDatatreeviewProxiesRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			proxiesList.Remove (datatreeviewProxies.GetSelectedObjects () [0] as Proxy);
		}
	}
}

