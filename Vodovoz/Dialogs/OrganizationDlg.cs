using System;
using QSOrmProject;
using NLog;
using System.Collections.Generic;
using QSContacts;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrganizationDlg : OrmGtkDialogBase<Organization>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();

		public OrganizationDlg ()
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Organization>();
			ConfigureDlg ();
		}

		public OrganizationDlg (int id)
		{
			this.Build ();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Organization>(id);
			TabName = UoWGeneric.Root.Name;
			ConfigureDlg ();
		}

		public OrganizationDlg (Organization sub) : this(sub.Id)
		{
			
		}

		private void ConfigureDlg ()
		{
			subjectAdaptor.Target = UoWGeneric.Root;
			datatableMain.DataSource = subjectAdaptor;
			dataentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;
			dataentryINN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryOGRN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			accountsview1.ParentReference = new OrmParentReference (Session, EntityObject, "Accounts");
			referenceBuhgalter.SubjectType = typeof(Employee);
			referenceLeader.SubjectType = typeof(Employee);
			phonesview1.Session = Session;
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
				UoWGeneric.Save();
				return true;
			} catch (Exception ex) {
				string text = "Организация не сохранилась...";
				logger.ErrorException (text, ex);
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

