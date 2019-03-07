using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using NLog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QSBanks;
using QSContacts;
using QSOrmProject;
using QSProjectsLib;
using QS.Project.Repositories;
using QSValidation;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Repository;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModel;

namespace Vodovoz
{
	public partial class CounterpartyDlg : QS.Dialog.Gtk.EntityDialogBase<Counterparty>, ICounterpartyInfoProvider
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.CounterpartyView };

		public Counterparty Counterparty => UoWGeneric.Root;
		public override bool HasChanges {
			get {
				phonesView.RemoveEmpty();
				emailsView.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		public CounterpartyDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();
			ConfigureDlg();
		}

		public CounterpartyDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Counterparty>(id);
			ConfigureDlg();
		}

		public CounterpartyDlg(Counterparty sub) : this(sub.Id) { }

		public CounterpartyDlg(IUnitOfWork baseUoW, int id)
		{
			this.Build();
			var rootObject = baseUoW.GetById<Counterparty>(id);
			UoWGeneric = UnitOfWorkFactory.CreateForChildRoot<Counterparty>(rootObject, baseUoW);
			ConfigureDlg();
		}

		public CounterpartyDlg(IUnitOfWork baseUoW, Counterparty sub) : this(baseUoW, sub.Id) { }

		private void ConfigureDlg()
		{
			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;
			//Initializing null fields
			emailsView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Emails == null)
				UoWGeneric.Root.Emails = new List<Email>();
			emailsView.Emails = UoWGeneric.Root.Emails;
			phonesView.UoW = UoWGeneric;
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
			txtRingUpPhones.Binding.AddBinding(Entity, e => e.RingUpPhone, w => w.Buffer.Text).InitializeFromSource();

			enumPayment.ItemsEnum = typeof(PaymentType);
			enumPayment.Binding.AddBinding(Entity, s => s.PaymentMethod, w => w.SelectedItemOrNull).InitializeFromSource();

			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();

			enumDefaultDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDefaultDocumentType.Binding.AddBinding(Entity, s => s.DefaultDocumentType, w => w.SelectedItemOrNull).InitializeFromSource();

			chkNeedNewBottles.Binding.AddBinding(Entity, e => e.NewBottlesNeeded, w => w.Active).InitializeFromSource();

			ycheckIsArchived.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			ycheckIsArchived.Sensitive = UserPermissionRepository.CurrentUserPresetPermissions["can_arc_counterparty_and_deliverypoint"];

			entryJurAddress.Binding.AddBinding(Entity, e => e.RawJurAddress, w => w.Text).InitializeFromSource();

			lblVodovozNumber.LabelProp = Entity.VodovozInternalId.ToString();
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
			ycheckSpecialDocuments.Binding.AddBinding(Entity, e => e.UseSpecialDocFields, w => w.Active).InitializeFromSource();
			radioSpecialDocFields.Visible = Entity.UseSpecialDocFields;
			yentryCargoReceiver.Binding.AddBinding(Entity, e => e.CargoReceiver, w => w.Text).InitializeFromSource();
			yentryCustomer.Binding.AddBinding(Entity, e => e.SpecialCustomer, w => w.Text).InitializeFromSource();
			yentrySpecialContract.Binding.AddBinding(Entity, e => e.SpecialContractNumber, w => w.Text).InitializeFromSource();
			yentrySpecialKPP.Binding.AddBinding(Entity, e => e.SpecialKPP, w => w.Text).InitializeFromSource();
			yentryGovContract.Binding.AddBinding(Entity, e => e.GovContract, w => w.Text).InitializeFromSource();
			yentrySpecialDeliveryAddress.Binding.AddBinding(Entity, e => e.SpecialDeliveryAddress, w => w.Text).InitializeFromSource();
			buttonLoadFromDP.Clicked += ButtonLoadFromDP_Clicked;

			//Setting Contacts
			contactsview1.CounterpartyUoW = UoWGeneric;
			//Setting permissions
			spinMaxCredit.Sensitive = UserPermissionRepository.CurrentUserPresetPermissions["max_loan_amount"];
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

			enumNeedOfCheque.ItemsEnum = typeof(ChequeResponse);
			enumNeedOfCheque.Binding.AddBinding(Entity, c => c.NeedCheque, w => w.SelectedItemOrNull).InitializeFromSource();

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
			referenceCameFrom.Sensitive = Entity.Id == 0;
			enumNeedOfCheque.Visible = lblNeedCheque.Visible = CounterpartyRepository.IsCashPayment(Entity.PaymentMethod);
		}

		void ButtonLoadFromDP_Clicked(object sender, EventArgs e)
		{
			var deliveryPointSelectDlg = new ReferenceRepresentation(new ClientDeliveryPointsVM(UoW, Entity)) {
				Mode = OrmReferenceMode.Select
			};
			deliveryPointSelectDlg.ObjectSelected += DeliveryPointRep_ObjectSelected;
			TabParent.AddSlaveTab(this, deliveryPointSelectDlg);
		}

		void DeliveryPointRep_ObjectSelected(object sender, ReferenceRepresentationSelectedEventArgs e)
		{
			if(e.VMNode is DeliveryPointVMNode node)
				yentrySpecialDeliveryAddress.Text = node.CompiledAddress;
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

			ReferenceRepresentation OrdersDialog = new ReferenceRepresentation(new OrdersVM(filter)) {
				Mode = OrmReferenceMode.Normal
			};
			OrdersDialog.Buttons(UserPermissionRepository.CurrentUserPresetPermissions["can_delete"] ? ReferenceButtonMode.CanAll : (ReferenceButtonMode.CanAdd | ReferenceButtonMode.CanEdit));

			TabParent.AddTab(OrdersDialog, this, false);
		}

		public override bool Save()
		{
			Entity.UoW = UoW;
			var valid = new QSValidator<Counterparty>(UoWGeneric.Root);
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;
			logger.Info("Сохраняем контрагента...");
			phonesView.RemoveEmpty();
			emailsView.RemoveEmpty();
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
			return counterarties != null && counterarties.Any(x => x.Id != UoWGeneric.Root.Id);
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
			enumNeedOfCheque.Visible = lblNeedCheque.Visible = CounterpartyRepository.IsCashPayment(Entity.PaymentMethod);
		}

		protected void OnEnumPaymentChangedByUser(object sender, EventArgs e)
		{
			if(Entity.PaymentMethod == PaymentType.cashless)
				Entity.DefaultDocumentType = DefaultDocumentType.upd;
			else
				Entity.DefaultDocumentType = null;
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
			if(e.Subject is Tag tag)
				Entity.ObservableTags.Add(tag);
		}

		protected void OnButtonAddTagClicked(object sender, EventArgs e)
		{
			var refWin = new OrmReference(typeof(Tag)) {
				Mode = OrmReferenceMode.Select
			};
			refWin.ObjectSelected += RefWin_ObjectSelected;
			TabParent.AddSlaveTab(this, refWin);
		}

		protected void OnButtonDeleteTagClicked(object sender, EventArgs e)
		{
			if(ytreeviewTags.GetSelectedObject() is Tag tag)
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

		protected void OnRadioSpecialDocFieldsToggled(object sender, EventArgs e)
		{
			if(radioSpecialDocFields.Active)
				notebook1.CurrentPage = 9;
		}

		protected void OnYcheckSpecialDocumentsToggled(object sender, EventArgs e)
		{
			radioSpecialDocFields.Visible = ycheckSpecialDocuments.Active;
		}
	}
}