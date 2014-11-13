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

namespace Vodovoz
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CounterpartyDlg : Gtk.Bin, QSTDI.ITdiDialog, IOrmDialog
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		private ISession session;
		private Adaptor adaptor = new Adaptor();
		private Counterparty subject;

		public CounterpartyDlg()
		{
			this.Build();
			subject = new Counterparty();
			Session.Persist (subject);
			ConfigureDlg();
		}

		public CounterpartyDlg(int id)
		{
			this.Build();
			subject = Session.Load<Counterparty>(id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		public CounterpartyDlg(Counterparty sub)
		{
			this.Build();
			subject = Session.Load<Counterparty>(sub.Id);
			TabName = subject.Name;
			ConfigureDlg();
		}

		private void ConfigureDlg()
		{
			entryName.IsEditable = entryJurAddress.IsEditable = entryFullName.IsEditable = true;
			dataComment.Editable = dataWaybillComment.Editable = true;
			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;
			adaptor.Target = subject;
			accountsView.AccountOwner = (IAccountOwner)Subject;
			validatedINN.ValidationMode = validatedKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			validatedINN.MaxLength = validatedKPP.MaxLength = 12;
			validatedINN.DataSource = validatedKPP.DataSource = adaptor;
			datatable1.DataSource = datatable2.DataSource = datatable3.DataSource = datatable4.DataSource = datatable5.DataSource = adaptor;
			enumPayment.DataSource = enumPersonType.DataSource = enumCounterpartyType.DataSource = adaptor;
			referenceSignificance.SubjectType = typeof(Significance);
			referenceStatus.SubjectType = typeof(CounterpartyStatus);
			referenceAccountant.SubjectType = referenceBottleManager.SubjectType = 
				referenceSalesManager.SubjectType = typeof(Employee);
			referenceMainCounterparty.ItemsCriteria = Session.CreateCriteria<Counterparty> ()
				.Add (Restrictions.Not(Restrictions.Eq("id", subject.Id)));
			referenceMainCounterparty.SubjectType = typeof(Counterparty);
			contactsview1.ParentReference = new OrmParentReference (Session, Subject, "Contacts");
			proxiesview1.ParentReference = new OrmParentReference (Session, Subject, "Proxies");
			additionalagreementsview1.ParentReference = new OrmParentReference (Session, Subject, "AdditionalAgreements");
			dataentryMainContact.ParentReference = new OrmParentReference (Session, Subject, "Contacts");
			dataentryFinancialContact.ParentReference = new OrmParentReference (Session, Subject, "Contacts");
			emailsView.Session = phonesView.Session = Session;
			if (subject.Emails == null)
				subject.Emails = new List<Email>();
			emailsView.Emails = subject.Emails;
			if (subject.Phones == null)
				subject.Phones = new List<Phone>();
			phonesView.Phones = subject.Phones;
		}

		#region ITdiTab implementation

		public event EventHandler<QSTDI.TdiTabNameChangedEventArgs> TabNameChanged;

		public event EventHandler<QSTDI.TdiTabCloseEventArgs> CloseTab;

		private string _tabName = "Новый контрагент";
		public string TabName
		{
			get{return _tabName;}
			set{
				if (_tabName == value)
					return;
				_tabName = value;
				if (TabNameChanged != null)
					TabNameChanged(this, new TdiTabNameChangedEventArgs(value));
			}

		}

		public QSTDI.ITdiTabParent TabParent { get ; set ; }

		#endregion

		#region ITdiDialog implementation

		public bool Save ()
		{
			logger.Info("Сохраняем контрагента...");
			if (entryName.Text == String.Empty) {
				logger.Error ("Не введено имя контрагента.");
				return false;
			}
			phonesView.SaveChanges();
			emailsView.SaveChanges ();
			Session.Flush();
			OrmMain.NotifyObjectUpdated(subject);

			return true;
		}

		public bool HasChanges {
			get {return Session.IsDirty();}
		}

		#endregion

		#region IOrmDialog implementation

		public NHibernate.ISession Session {
			get {
				if (session == null)
					Session = OrmMain.Sessions.OpenSession ();
				return session;
			}
			set {
				session = value;
			}
		}

		public object Subject {
			get {return subject;}
			set {
				if (value is Counterparty)
					subject = value as Counterparty;
			}
		}
		#endregion

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			if (!this.HasChanges || Save())
				OnCloseTab(false);
		}

		protected void OnButtonCancelClicked (object sender, EventArgs e)
		{
			OnCloseTab(false);
		}

		protected void OnCloseTab(bool askSave)
		{
			if (CloseTab != null)
				CloseTab(this, new TdiTabCloseEventArgs(askSave));
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

		public override void Destroy()
		{
			Session.Close();
			contactsview1.Destroy ();
			adaptor.Disconnect();
			base.Destroy();
		}
	}
}

