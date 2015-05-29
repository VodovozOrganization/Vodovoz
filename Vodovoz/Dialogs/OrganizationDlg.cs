using System;
using QSTDI;
using QSOrmProject;
using System.Data.Bindings;
using NLog;
using System.Collections.Generic;
using QSContacts;
using QSValidation;
using Vodovoz.Domain;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class OrganizationDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		IUnitOfWorkGeneric<Organization> uow;
		private Adaptor adaptorOrg = new Adaptor ();

		public ITdiTabParent TabParent { set; get; }

		public event EventHandler<TdiTabNameChangedEventArgs> TabNameChanged;
		public event EventHandler<TdiTabCloseEventArgs> CloseTab;

		public bool HasChanges { 
			get { return uow.HasChanges; }
		}

		private string _tabName = "Новая организация";

		public string TabName {
			get { return _tabName; }
			set {
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged (this, new TdiTabNameChangedEventArgs (value));
			}

		}

		public IUnitOfWork UoW {
			get
			{
				return uow;
			}
		}

		public object Subject {
			get { return uow.Root; }
		}

		public OrganizationDlg ()
		{
			this.Build ();
			uow = UnitOfWorkFactory.CreateWithNewRoot<Organization>();
			ConfigureDlg ();
		}

		public OrganizationDlg (int id)
		{
			this.Build ();
			uow = UnitOfWorkFactory.CreateForRoot<Organization>(id);
			TabName = uow.Root.Name;
			ConfigureDlg ();
		}

		public OrganizationDlg (Organization sub) : this(sub.Id)
		{
			
		} 

		private void ConfigureDlg ()
		{
			adaptorOrg.Target = uow.Root;
			datatableMain.DataSource = adaptorOrg;
			dataentryEmail.ValidationMode = QSWidgetLib.ValidationType.email;
			dataentryINN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			dataentryOGRN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			notebookMain.Page = 0;
			notebookMain.ShowTabs = false;
			accountsview1.ParentReference = new OrmParentReference (Session, Subject, "Accounts");
			referenceBuhgalter.SubjectType = typeof(Employee);
			referenceLeader.SubjectType = typeof(Employee);
			phonesview1.Session = Session;
			if (uow.Root.Phones == null)
				uow.Root.Phones = new List<Phone> ();
			phonesview1.Phones = uow.Root.Phones;
		}

		public bool Save ()
		{
			var valid = new QSValidator<Organization> (uow.Root);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;
			logger.Info ("Сохраняем организацию...");
			try {
				phonesview1.SaveChanges ();
				uow.Save();
				return true;
			} catch (Exception ex) {
				string text = "Организация не сохранилась...";
				logger.ErrorException (text, ex);
				QSProjectsLib.QSMain.ErrorMessage ((Gtk.Window)this.Toplevel, ex, text);
				return false;
			}
		}

		public override void Destroy ()
		{
			uow.Dispose();
			adaptorOrg.Disconnect ();
			base.Destroy ();
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save ())
				OnCloseTab (false);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab (false);
		}

		protected void OnCloseTab (bool askSave)
		{
			if (CloseTab != null)
				CloseTab (this, new TdiTabCloseEventArgs (askSave));
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

