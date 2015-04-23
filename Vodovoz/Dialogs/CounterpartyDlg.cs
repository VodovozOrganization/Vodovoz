using System;
using QSOrmProject;
using QSTDI;
using NLog;
using NHibernate;
using System.Data.Bindings;
using System.Collections.Generic;
using QSContacts;
using NHibernate.Criterion;
using QSBanks;
using QSValidation;
using QSProjectsLib;

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger ();
		private ISession session;
		private Adaptor adaptor = new Adaptor ();
		private Counterparty subject;

		public CounterpartyDlg ()
		{
			this.Build ();
			subject = new Counterparty ();
			Session.Persist (subject);
			ConfigureDlg ();
		}

		public CounterpartyDlg (int id)
		{
			this.Build ();
			subject = Session.Load<Counterparty> (id);
			TabName = subject.Name;
			ConfigureDlg ();
		}

		public CounterpartyDlg (Counterparty sub)
		{
			this.Build ();
			subject = Session.Load<Counterparty> (sub.Id);
			TabName = subject.Name;
			ConfigureDlg ();
		}

		private void ConfigureDlg ()
		{
			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;
			//Initializing null fields
			emailsView.Session = phonesView.Session = Session;
			if (subject.Emails == null)
				subject.Emails = new List<Email> ();
			emailsView.Emails = subject.Emails;
			if (subject.Phones == null)
				subject.Phones = new List<Phone> ();
			phonesView.Phones = subject.Phones;
			if (subject.CounterpartyContracts == null) {
				subject.CounterpartyContracts = new List<CounterpartyContract> ();
			}
			//Setting up editable property
			entryName.IsEditable = entryJurAddress.IsEditable = entryFullName.IsEditable = true;
			dataComment.Editable = dataWaybillComment.Editable = true;
			//Other fields properties
			validatedINN.ValidationMode = validatedKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			validatedINN.MaxLength = validatedKPP.MaxLength = 12;
			//Setting up adaptors
			adaptor.Target = subject;
			//Setting up fields sources
			datatable1.DataSource = datatable2.DataSource = datatable3.DataSource = datatable4.DataSource = adaptor;
			enumPayment.DataSource = enumPersonType.DataSource = enumCounterpartyType.DataSource = adaptor;
			validatedINN.DataSource = validatedKPP.DataSource = adaptor;
			//Setting subjects
			accountsView.ParentReference = new OrmParentReference (Session, Subject, "Accounts");
			deliveryPointView.ParentReference = new OrmParentReference (Session, Subject, "DeliveryPoints");
			counterpartyContractsView.ParentReference = new OrmParentReference (Session, Subject, "CounterpartyContracts");
			referenceSignificance.SubjectType = typeof(Significance);
			referenceStatus.SubjectType = typeof(CounterpartyStatus);
			referenceAccountant.SubjectType = referenceBottleManager.SubjectType = referenceSalesManager.SubjectType = typeof(Employee);
			referenceMainCounterparty.ItemsCriteria = Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Not (Restrictions.Eq ("id", subject.Id)));
			referenceMainCounterparty.SubjectType = typeof(Counterparty);
			proxiesview1.ParentReference = new OrmParentReference (Session, Subject, "Proxies");
			dataentryMainContact.ParentReference = new OrmParentReference (Session, Subject, "Contacts");
			dataentryFinancialContact.ParentReference = new OrmParentReference (Session, Subject, "Contacts");
			//Setting Contacts
			contactsview1.ParentReference = new OrmParentReference (Session, Subject, "Contacts");
			contactsview1.ColumnMappings = "{Vodovoz.Contact} FullName[Имя]; PostName[Должность]; MainPhoneString[Телефон]; Comment[Комментарий];";
			//Setting permissions
			spinMaxCredit.Sensitive = QSMain.User.Permissions ["max_loan_amount"];
		}

		#region ITdiTab implementation

		public event EventHandler<QSTDI.TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<QSTDI.TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Новый контрагент";

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

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			var valid = new QSValidator<Counterparty> (subject);
			if (valid.RunDlgIfNotValid ((Gtk.Window)this.Toplevel))
				return false;

			logger.Info ("Сохраняем контрагента...");
			phonesView.SaveChanges ();
			emailsView.SaveChanges ();
			Session.Flush ();
			logger.Info ("Ok.");
			OrmMain.NotifyObjectUpdated (subject);

			return true;
		}

		public bool HasChanges {
			get { return Session.IsDirty (); }
		}

		#endregion

		#region IOrmDialog implementation

		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set { session = value; }
		}

		public object Subject {
			get { return subject; }
			set {
				if (value is Counterparty)
					subject = value as Counterparty;
			}
		}

		#endregion

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

		protected void OnRadioInfoToggled (object sender, EventArgs e)
		{
			if (radioInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadioContactsToggled (object sender, EventArgs e)
		{
			if (radioContacts.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadioDetailsToggled (object sender, EventArgs e)
		{
			if (radioDetails.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnRadioCuratorsToggled (object sender, EventArgs e)
		{
			if (radioCurators.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnRadioContactPersonsToggled (object sender, EventArgs e)
		{
			if (radioContactPersons.Active)
				notebook1.CurrentPage = 4;
		}

		protected void OnRadiobuttonProxiesToggled (object sender, EventArgs e)
		{
			if (radiobuttonProxies.Active)
				notebook1.CurrentPage = 5;
		}

		protected void OnRadioContractsToggled (object sender, EventArgs e)
		{
			if (radioContracts.Active)
				notebook1.CurrentPage = 6;
		}

		protected void OnRadioDeliveryPointToggled (object sender, EventArgs e)
		{
			if (radioDeliveryPoint.Active)
				notebook1.CurrentPage = 7;
		}

		public override void Destroy ()
		{
			Session.Close ();
			contactsview1.Destroy ();
			adaptor.Disconnect ();
			base.Destroy ();
		}
	}
}

