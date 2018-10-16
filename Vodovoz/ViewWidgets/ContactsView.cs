using System;
using QS.DomainModel.UoW;
using QSOrmProject;
using QSTDI;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ContactsView : Gtk.Bin
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
				datatreeviewContacts.RepresentationModel = new ViewModel.ContactsVM (value);
				datatreeviewContacts.RepresentationModel.UpdateNodes ();
			}
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

			var parentDlg = OrmMain.FindMyDialog (this);
			if (parentDlg == null)
				return;

			if (parentDlg.UoW.IsNew) {
				if (CommonDialogs.SaveBeforeCreateSlaveEntity (parentDlg.EntityObject.GetType (), typeof(Contact))) {
					parentDlg.UoW.Save ();
				} else
					return;
			}

			ITdiDialog dlg = new ContactDlg (CounterpartyUoW.Root);
			mytab.TabParent.AddTab (dlg, mytab);
		}

		protected void OnButtonEditClicked (object sender, EventArgs e)
		{
			ITdiTab mytab = TdiHelper.FindMyTab (this);
			if (mytab == null)
				return;

		
			ContactDlg dlg = new ContactDlg (datatreeviewContacts.GetSelectedId ());
			mytab.TabParent.AddSlaveTab (mytab, dlg);
		}

		protected void OnDatatreeviewContactsRowActivated (object o, Gtk.RowActivatedArgs args)
		{
			buttonEdit.Click ();
		}

		protected void OnButtonDeleteClicked (object sender, EventArgs e)
		{
			if (OrmMain.DeleteObject (typeof(Contact),
				    datatreeviewContacts.GetSelectedId ())) {
				datatreeviewContacts.RepresentationModel.UpdateNodes ();
			}
		}
	}
}

