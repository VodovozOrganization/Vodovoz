using System;
using NHibernate;
using QSOrmProject;
using System.Data.Bindings.Collections.Generic;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class IncomingInvoiceItemsView : Gtk.Bin
	{
		public IncomingInvoiceItemsView ()
		{
			this.Build ();
			treeItemsList.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			buttonEdit.Sensitive = buttonDelete.Sensitive = treeItemsList.Selection.CountSelectedRows () > 0;
		}

		ISession session;

		public ISession Session {
			get { return session; }
			set { session = value; }
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null)
					Session = parentReference.Session;
				if (!(ParentReference.ParentObject is IncomingInvoice))
					throw new ArgumentException (String.Format ("Родительский объект в parentReference должен являться классом {0}", typeof(IncomingInvoice)));
				items = new GenericObservableList<IncomingInvoiceItem> ((ParentReference.ParentObject as IncomingInvoice).Items);
				treeItemsList.ItemsDataSource = items;
			}
			get { return parentReference; }
		}

		GenericObservableList<IncomingInvoiceItem> items;

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = OrmMain.CreateObjectDialog (ParentReference, treeItemsList.GetSelectedObjects () [0]);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			items.Remove (treeItemsList.GetSelectedObjects () [0] as IncomingInvoiceItem);
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			IncomingInvoiceItem sub = new IncomingInvoiceItem ();
			items.Add (sub);

			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ITdiDialog dlg = OrmMain.CreateObjectDialog (ParentReference, sub);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonCreateClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected void OnTreeItemsListRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}
	}
}

