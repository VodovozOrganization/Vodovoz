﻿using System.Collections.Generic;
using NLog;
using Vodovoz.Domain.Contacts;
using QS.DomainModel.UoW;
using QS.Validation;
using Vodovoz.Domain.Client;

namespace Vodovoz
{
	public partial class ContactDlg : QS.Dialog.Gtk.EntityDialogBase<Contact>
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
			referencePost.SubjectType = typeof(Post);
			referencePost.Binding.AddBinding(Entity, e => e.Post, w => w.Subject).InitializeFromSource();

			entryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entrySurname.Binding.AddBinding(Entity, e => e.Surname, w => w.Text).InitializeFromSource();
			entryPatronymic.Binding.AddBinding(Entity, e => e.Patronymic, w => w.Text).InitializeFromSource();

			checkbuttonFired.Binding.AddBinding(Entity, e => e.IsFired, w => w.Active).InitializeFromSource();

			dataComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			emailsView.UoW = UoWGeneric;
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
			var validator = new ObjectValidator(new GtkValidationViewFactory());
			if(!validator.Validate(Entity))
			{
				return false;
			}

			logger.Info ("Сохраняем  контактное лицо...");
			phonesView.RemoveEmpty();
			emailsView.RemoveEmpty();
			UoWGeneric.Save ();
			logger.Info ("Ok");
			return true;
		}
	}
}

