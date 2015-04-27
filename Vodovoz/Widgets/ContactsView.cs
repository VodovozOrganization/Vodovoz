using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using NHibernate;
using QSOrmProject;
using QSTDI;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ContactsView : Gtk.Bin
	{
		private IContactOwner contactOwner;
		private GenericObservableList<Contact> contactsList;
		private ISession session;

		public ISession Session {
			get { return session; }
			set { session = value; }
		}

		public IContactOwner ContactOwner {
			get { return contactOwner; }
			set {
				contactOwner = value;
				if (ContactOwner.Contacts == null)
					ContactOwner.Contacts = new List<Contact> ();
				contactsList = new GenericObservableList<Contact> (ContactOwner.Contacts);
				datatreeviewContacts.ItemsDataSource = contactsList;
			}
		}

		OrmParentReference parentReference;

		public OrmParentReference ParentReference {
			set {
				parentReference = value;
				if (parentReference != null) {
					Session = parentReference.Session;
					if (!(parentReference.ParentObject is IContactOwner)) {
						throw new ArgumentException (String.Format ("Родительский объект в parentReference должен реализовывать интерфейс {0}", typeof(IContactOwner)));
					}
					ContactOwner = (IContactOwner)parentReference.ParentObject;
				}
			}
			get { return parentReference; }
		}

		public string ColumnMappings {
			get { return datatreeviewContacts.ColumnMappings; }
			set { datatreeviewContacts.ColumnMappings = value; }
		}

		public ContactsView ()
		{
			this.Build ();
			datatreeviewContacts.Selection.Changed += OnSelectionChanged;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			bool selected = datatreeviewContacts.Selection.CountSelectedRows () > 0;
			buttonEdit.Sensitive = buttonDelete.Sensitive = selected;
		}

		protected void OnButtonAddClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ContactDlg dlg = new ContactDlg (ParentReference);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			ContactDlg dlg = new ContactDlg (ParentReference, datatreeviewContacts.GetSelectedObjects () [0] as Contact);
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnDatatreeviewAccountsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

			contactsList.Remove (datatreeviewContacts.GetSelectedObjects () [0] as Contact);
		}
	}
}

