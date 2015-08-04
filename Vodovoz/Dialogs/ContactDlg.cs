using System.Collections.Generic;
using NLog;
using QSContacts;
using QSOrmProject;
using Vodovoz.Domain;
using QSValidation;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class ContactDlg : OrmGtkDialogBase<Contact>
	{
		protected static Logger logger = LogManager.GetCurrentClassLogger ();

		public ContactDlg (Counterparty counterparty)
		{
			this.Build ();
			UoWGeneric = Contact.Create (counterparty);
			ConfigureDlg ();
		}

		public ContactDlg (Contact sub) : this (sub.Id)
		{
		}

		public ContactDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Contact> (id);
			ConfigureDlg ();
		}


		void ConfigureDlg ()
		{
			datatable1.DataSource = subjectAdaptor;
			entrySurname.IsEditable = entryName.IsEditable = entryLastname.IsEditable = true;
			dataComment.Editable = true;
			referencePost.SubjectType = typeof(Post);
			emailsView.Session = UoWGeneric.Session;
			if (UoWGeneric.Root.Emails == null)
				UoWGeneric.Root.Emails = new List<Email> ();
			emailsView.Emails = UoWGeneric.Root.Emails;
			phonesView.UoW = UoWGeneric;
			if (UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone> ();
			phonesView.Phones = UoWGeneric.Root.Phones;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Contact> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем доверенность...");
			phonesView.SaveChanges ();
			emailsView.SaveChanges ();
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}
	}
}

