using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using NLog;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModel;
using QSWidgetLib;
using Vodovoz.Repository;
using FluentNHibernate.Data;

namespace Vodovoz
{
	public partial class CounterpartyDlg : OrmGtkDialogBase<Counterparty>, ICounterpartyInfoProvider
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public PanelViewType[] InfoWidgets {
			get {
				return new[]
				{
					PanelViewType.CounterpartyView,
				};
			}
		}

		public Counterparty Counterparty {
			get {
				return UoWGeneric.Root;
			}
		}

		public CounterpartyDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();
			Entity.VodovozInternalId = CounterpartyRepository.GetMaximalInternalID(UoW) + 1;
			ConfigureDlg();
		}

		public CounterpartyDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Counterparty>(id);
			ConfigureDlg();
		}

		public CounterpartyDlg(Counterparty sub) : this(sub.Id)
		{}

		private void ConfigureDlg()
		{
			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;
			//Initializing null fields
			emailsView.UoW = UoWGeneric;
			phonesView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Emails == null)
				UoWGeneric.Root.Emails = new List<Email>();
			emailsView.Emails = UoWGeneric.Root.Emails;
			if(UoWGeneric.Root.Phones == null)
				UoWGeneric.Root.Phones = new List<Phone>();
			phonesView.Phones = UoWGeneric.Root.Phones;
			if(UoWGeneric.Root.CounterpartyContracts == null) {
				UoWGeneric.Root.CounterpartyContracts = new List<CounterpartyContract>();
			}

			commentsview4.UoW = UoW;
			//Other fields properties
			validatedINN.ValidationMode = validatedKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			validatedINN.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();
			validatedKPP.Binding.AddBinding(Entity, e => e.KPP, w => w.Text).InitializeFromSource();

			yentrySignFIO.Binding.AddBinding(Entity, e => e.SignatoryFIO, w => w.Text).InitializeFromSource();
			yentrySignPost.Binding.AddBinding(Entity, e => e.SignatoryPost, w => w.Text).InitializeFromSource();
			yentrySignBaseOf.Binding.AddBinding(Entity, e => e.SignatoryBaseOf, w => w.Text).InitializeFromSource();

			entryFIO.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();
			entryFullName.Binding.AddBinding(Entity, e => e.FullName, w => w.Text).InitializeFromSource();

			spinMaxCredit.Binding.AddBinding(Entity, e => e.MaxCredit, w => w.ValueAsDecimal).InitializeFromSource();

			dataComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			enumPayment.ItemsEnum = typeof(PaymentType);
			enumPayment.Binding.AddBinding(Entity, s => s.PaymentMethod, w => w.SelectedItemOrNull).InitializeFromSource();

			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();

			enumDefaultDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDefaultDocumentType.Binding.AddBinding(Entity, s => s.DefaultDocumentType, w => w.SelectedItemOrNull).InitializeFromSource();

			chkNeedNewBottles.Binding.AddBinding(Entity, e => e.NewBottlesNeeded, w => w.Active).InitializeFromSource();

			ycheckIsArchived.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			ycheckIsArchived.Sensitive = QSMain.User.Permissions["can_arc_counterparty_and_deliverypoint"];

			entryJurAddress.Binding.AddBinding(Entity, e => e.JurAddress, w => w.Text).InitializeFromSource();

			yEntryVodovozNumber.ValidationMode = QSWidgetLib.ValidationType.numeric;
			yEntryVodovozNumber.Binding.AddBinding(Entity, e => e.VodovozInternalId, w => w.Text, new IntToStringConverter()).InitializeFromSource();

			var counterpatiesView = new ViewModel.CounterpartyVM(UoW);
			referenceMainCounterparty.RepresentationModel = counterpatiesView;
			referenceMainCounterparty.Binding.AddBinding(Entity, e => e.MainCounterparty, w => w.Subject).InitializeFromSource();
			referencePreviousCounterparty.RepresentationModel = counterpatiesView;
			referencePreviousCounterparty.Binding.AddBinding(Entity, e => e.PreviousCounterparty, w => w.Subject).InitializeFromSource();

			//Setting subjects
			accountsView.ParentReference = new ParentReferenceGeneric<Counterparty, Account>(UoWGeneric, c => c.Accounts);
			deliveryPointView.DeliveryPointUoW = UoWGeneric;
			counterpartyContractsView.CounterpartyUoW = UoWGeneric;
			counterpartydocumentsview.Config(UoWGeneric, Entity);
			referenceCameFrom.SubjectType = typeof(ClientCameFrom);
			referenceCameFrom.Binding.AddBinding(Entity, e => e.CameFrom, w => w.Subject).InitializeFromSource();
			referenceDefaultExpense.SubjectType = typeof(ExpenseCategory);
			referenceDefaultExpense.Binding.AddBinding(Entity, e => e.DefaultExpenseCategory, w => w.Subject).InitializeFromSource();
			var filterAccountant = new EmployeeFilter(UoW);
			filterAccountant.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.office);
			referenceAccountant.RepresentationModel = new EmployeesVM(filterAccountant);
			var filterSalesManager = new EmployeeFilter(UoW);
			filterSalesManager.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.office);
			referenceSalesManager.RepresentationModel = new EmployeesVM(filterSalesManager);
			var filterBottleManager = new EmployeeFilter(UoW);
			filterBottleManager.SetAndRefilterAtOnce(x => x.RestrictCategory = EmployeeCategory.office);
			referenceBottleManager.RepresentationModel = new EmployeesVM(filterBottleManager);
			proxiesview1.CounterpartyUoW = UoWGeneric;
			dataentryMainContact.RepresentationModel = new ViewModel.ContactsVM(UoW, Entity);
			dataentryMainContact.Binding.AddBinding(Entity, e => e.MainContact, w => w.Subject).InitializeFromSource();
			dataentryFinancialContact.RepresentationModel = new ViewModel.ContactsVM(UoW, Entity);
			dataentryFinancialContact.Binding.AddBinding(Entity, e => e.FinancialContact, w => w.Subject).InitializeFromSource();
			//Setting Contacts
			contactsview1.CounterpartyUoW = UoWGeneric;
			//Setting permissions
			spinMaxCredit.Sensitive = QSMain.User.Permissions["max_loan_amount"];
			datalegalname1.Binding.AddSource(Entity)
				.AddBinding(s => s.Name, t => t.OwnName)
				.AddBinding(s => s.TypeOfOwnership, t => t.Ownership)
				//.AddBinding(s => s.FullName, t => t.FullName)
				.InitializeFromSource();

			ytreeviewTags.ColumnsConfig = ColumnsConfigFactory.Create<Tag>()
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Цвет").AddTextRenderer()
				.AddSetter((cell, node) => { cell.Markup = String.Format("<span foreground=\" {0}\">♥</span>", node.ColorText); })
				.AddColumn("")
				.Finish();

			ytreeviewTags.ItemsDataSource = Entity.ObservableTags;

			//make actions menu
			var menu = new Gtk.Menu();
			var menuItem = new Gtk.MenuItem("Все заказы контрагента");
			menuItem.Activated += AllOrders_Activated;
			menu.Add(menuItem);
			menuActions.Menu = menu;
			menu.ShowAll();
			menuActions.Sensitive = !UoWGeneric.IsNew;
			contactsview1.Visible = false;
			hboxCameFrom.Visible = (Entity.Id != 0 && Entity.CameFrom != null) || Entity.Id == 0;
			referenceCameFrom.Sensitive = Entity.Id == 0;
		}

		public void ActivateContactsTab()
		{
			if(radioContacts.Sensitive)
				radioContacts.Active = true;
		}

		void AllOrders_Activated(object sender, EventArgs e)
		{
			var filter = new OrdersFilter(UnitOfWorkFactory.CreateWithoutRoot());
			filter.SetAndRefilterAtOnce(x => x.RestrictCounterparty = Entity);

			ReferenceRepresentation OrdersDialog = new ReferenceRepresentation(new ViewModel.OrdersVM(filter));
			OrdersDialog.Mode = OrmReferenceMode.Normal;

			TabParent.AddTab(OrdersDialog, this, false);
		}

		public override bool Save()
		{
			Entity.UoW = UoW;
			var valid = new QSValidator<Counterparty>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем контрагента...");
			phonesView.SaveChanges();
			emailsView.SaveChanges();
			UoWGeneric.Save();
			logger.Info("Ok.");
			return true;
		}

		/// <summary>
		/// Поиск контрагентов с таким же ИНН
		/// </summary>
		/// <returns><c>true</c>, if duplicate was checked, <c>false</c> otherwise.</returns>
		private bool CheckDuplicate()
		{
			string INN = UoWGeneric.Root.INN;
			IList<Counterparty> counterarties = Repository.CounterpartyRepository.GetCounterpartiesByINN(UoW, INN);
			if(counterarties == null)
				return false;
			if(counterarties.Count(x => x.Id != UoWGeneric.Root.Id) > 0)
				return true;
			return false;
		}

		protected void OnRadioInfoToggled(object sender, EventArgs e)
		{
			if(radioInfo.Active)
				notebook1.CurrentPage = 0;
		}

		protected void OnRadioCommentsToggled(object sender, EventArgs e)
		{
			if(radioComments.Active)
				notebook1.CurrentPage = 1;
		}

		protected void OnRadioContactsToggled(object sender, EventArgs e)
		{
			if(radioContacts.Active)
				notebook1.CurrentPage = 2;
		}

		protected void OnRadioDetailsToggled(object sender, EventArgs e)
		{
			if(radioDetails.Active)
				notebook1.CurrentPage = 3;
		}

		protected void OnRadiobuttonProxiesToggled(object sender, EventArgs e)
		{
			if(radiobuttonProxies.Active)
				notebook1.CurrentPage = 4;
		}

		protected void OnRadioContractsToggled(object sender, EventArgs e)
		{
			if(radioContracts.Active)
				notebook1.CurrentPage = 5;
		}

		protected void OnRadioDocumentsToggled(object sender, EventArgs e)
		{
			if(radioDocuments.Active)
				notebook1.CurrentPage = 6;
		}

		protected void OnRadioDeliveryPointToggled(object sender, EventArgs e)
		{
			if(radioDeliveryPoint.Active)
				notebook1.CurrentPage = 7;
		}

		protected void OnEnumPersonTypeChanged(object sender, EventArgs e)
		{
			labelFIO.Visible = entryFIO.Visible = Entity.PersonType == PersonType.natural;
			labelShort.Visible = datalegalname1.Visible =
					labelFullName.Visible = entryFullName.Visible =
					referenceMainCounterparty.Visible = labelMainCounterparty.Visible =
						radioDetails.Visible = radiobuttonProxies.Visible = lblPaymentType.Visible = 
							enumPayment.Visible = (Entity.PersonType == PersonType.legal);
		}

		protected void OnEnumPaymentEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			enumDefaultDocumentType.Visible = labelDefaultDocumentType.Visible = (PaymentType)e.SelectedItem == PaymentType.cashless;
			if((PaymentType)e.SelectedItem == PaymentType.cashless) {
				Entity.DefaultDocumentType = DefaultDocumentType.upd;
			} else {
				Entity.DefaultDocumentType = null;
			}
		}

		protected void OnReferencePreviousCounterpartyChangedByUser(object sender, EventArgs e)
		{
			if(DomainHelper.EqualDomainObjects(Entity.PreviousCounterparty, Entity))
				Entity.PreviousCounterparty = null;
		}

		protected void OnReferenceMainCounterpartyChangedByUser(object sender, EventArgs e)
		{
			if(DomainHelper.EqualDomainObjects(Entity.MainCounterparty, Entity))
				Entity.MainCounterparty = null;
		}

		protected void OnYentrySignPostFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			if(yentrySignPost.Completion == null) {
				yentrySignPost.Completion = new Gtk.EntryCompletion();
				var list = Repository.CounterpartyRepository.GetUniqueSignatoryPosts(UoW);
				yentrySignPost.Completion.Model = ListStoreWorks.CreateFromEnumerable(list);
				yentrySignPost.Completion.TextColumn = 0;
				yentrySignPost.Completion.Complete();
			}
		}

		protected void OnYentrySignBaseOfFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			if(yentrySignBaseOf.Completion == null) {
				yentrySignBaseOf.Completion = new Gtk.EntryCompletion();
				var list = Repository.CounterpartyRepository.GetUniqueSignatoryBaseOf(UoW);
				yentrySignBaseOf.Completion.Model = ListStoreWorks.CreateFromEnumerable(list);
				yentrySignBaseOf.Completion.TextColumn = 0;
				yentrySignBaseOf.Completion.Complete();
			}
		}

		protected void OnRadioTagsToggled(object sender, EventArgs e)
		{
			if(radioTags.Active)
				notebook1.CurrentPage = 8;
		}

		void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var tag = e.Subject as Tag;
			if(tag == null) {
				return;
			}
			Entity.ObservableTags.Add(tag);
		}

		protected void OnButtonAddTagClicked(object sender, EventArgs e)
		{
			var refWin = new OrmReference(typeof(Tag));
			refWin.Mode = OrmReferenceMode.Select;
			refWin.ObjectSelected += RefWin_ObjectSelected;
			TabParent.AddSlaveTab(this, refWin);
		}

		protected void OnButtonDeleteTagClicked(object sender, EventArgs e)
		{
			var tag = ytreeviewTags.GetSelectedObject() as Tag;
			if(tag == null) {
				return;
			}
			Entity.ObservableTags.Remove(tag);
		}

		protected void OnDatalegalname1OwnershipChanged(object sender, EventArgs e)
		{
			validatedKPP.Sensitive = Entity.TypeOfOwnership != "ИП";
		}

		protected void OnChkNeedNewBottlesToggled(object sender, EventArgs e)
		{
			Entity.NewBottlesNeeded = chkNeedNewBottles.Active;
		}
	}
}