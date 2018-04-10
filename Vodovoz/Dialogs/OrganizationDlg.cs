using System;
using System.Collections.Generic;
using NLog;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSValidation;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class OrganizationDlg : OrmGtkDialogBase<Organization>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public OrganizationDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Organization> ();
			ConfigureDlg ();
		}

		public OrganizationDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Organization> (id);
			ConfigureDlg ();
		}

		public OrganizationDlg (Organization sub) : this (sub.Id)
		{
			
		}

		private void ConfigureDlg ()
		{
			dataentryName.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			dataentryFullName.Binding.AddBinding(Entity, e => e.FullName, w => w.Text).InitializeFromSource();

			dataentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;
			dataentryEmail.Binding.AddBinding(Entity, e => e.Email, w => w.Text).InitializeFromSource();
			dataentryINN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryINN.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
			dataentryKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryKPP.Binding.AddBinding(Entity, e => e.KPP, w => w.Text).InitializeFromSource();
			dataentryOGRN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryOGRN.Binding.AddBinding(Entity, e => e.OGRN, w => w.Text).InitializeFromSource();

			datatextviewAddress.Binding.AddBinding(Entity, e => e.Address, w => w.Buffer.Text).InitializeFromSource();
			datatextviewJurAddress.Binding.AddBinding(Entity, e => e.JurAddress, w => w.Buffer.Text).InitializeFromSource();

			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			accountsview1.ParentReference = new ParentReferenceGeneric<Organization, Account> (UoWGeneric, o => o.Accounts);

			var filterBuhgalter = new EmployeeFilter(UoW);
			referenceBuhgalter.RepresentationModel = new EmployeesVM(filterBuhgalter);
			referenceBuhgalter.Binding.AddBinding(Entity, e => e.Buhgalter, w => w.Subject).InitializeFromSource();
			var filterLeader = new EmployeeFilter(UoW);
			referenceLeader.RepresentationModel = new EmployeesVM(filterLeader);
			referenceLeader.Binding.AddBinding(Entity, e => e.Leader, w => w.Subject).InitializeFromSource();

			phonesview1.UoW = UoWGeneric;
			if (UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone> ();
			phonesview1.Phones = UoWGeneric.Root.Phones;
		}

		public override bool Save ()
		{
			var valid = new QSValidator<Organization> (UoWGeneric.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем организацию...");
			try {
				phonesview1.SaveChanges ();
				UoWGeneric.Save ();
				return true;
			} catch (Exception ex) {
				string text = "Организация не сохранилась...";
				logger.Error (ex, text);
				QSProjectsLib.QSMain.ErrorMessage ((Gtk.Window)this.Toplevel, ex, text);
				return false;
			}
		}

		protected void OnRadioTabInfoToggled (object sender, EventArgs e)
		{
			if (radioTabInfo.Active)
				notebookMain.CurrentPage = 0;
		}

		protected void OnRadioTabAccountsToggled (object sender, EventArgs e)
		{
			if (radioTabAccounts.Active)
				notebookMain.CurrentPage = 1;
		}
	}
}

