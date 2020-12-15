using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using NLog;
using QS.Banks.Domain;
using Vodovoz.Domain.Contacts;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal.EntitySelector;
using QSOrmProject;
using QSProjectsLib;
using QS.Validation;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Repositories;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.ViewModel;
using Gtk;
using Vodovoz.Domain.Goods;
using QS.Project.Services;
using QS.Tdi;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.TempAdapters;
using Vodovoz.Models;
using Vodovoz.Domain;
using Vodovoz.Domain.EntityFactories;
using QS.DomainModel.Entity;

namespace Vodovoz
{
	public partial class CounterpartyDlg : QS.Dialog.Gtk.EntityDialogBase<Counterparty>, ICounterpartyInfoProvider, ITDICloseControlTab
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		private readonly IEmployeeService employeeService = VodovozGtkServicesConfig.EmployeeService;
		private readonly IUserRepository userRepository = UserSingletonRepository.GetInstance();

		private  INomenclatureRepository nomenclatureRepository;
		public virtual INomenclatureRepository NomenclatureRepository {
			get {
				if(nomenclatureRepository == null) {
					nomenclatureRepository = new EntityRepositories.Goods.NomenclatureRepository(new NomenclatureParametersProvider());
				};
				return nomenclatureRepository;
			}
		}
		
		private IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory {
			get {
				if(counterpartySelectorFactory == null) {
					counterpartySelectorFactory =
						new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
							CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);
				};
				return counterpartySelectorFactory;
			}
		}

		private IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory {
			get {
				if(nomenclatureSelectorFactory == null) {
					nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), CounterpartySelectorFactory,
							NomenclatureRepository, userRepository);
				}
				return nomenclatureSelectorFactory;
			}
		}

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

		public CounterpartyDlg(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory)
		{
			this.Build();
			UoWGeneric = uowBuilder.CreateUoW<Counterparty>(unitOfWorkFactory);
			ConfigureDlg();
		}

		public CounterpartyDlg(Phone phone)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();
			Entity.Phones.Add(phone);
			ConfigureDlg();
		}
		void ConfigureDlg()
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
			supplierPricesWidget.ViewModel =
				new ViewModels.Client.SupplierPricesWidgetViewModel(Entity, 
																	UoW, 
																	this, 
																	ServicesConfig.CommonServices,
																	employeeService,
																	CounterpartySelectorFactory,
																	NomenclatureSelectorFactory,
																	NomenclatureRepository,
																	userRepository);
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
			ycheckIsArchived.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_arc_counterparty_and_deliverypoint");

			entryJurAddress.Binding.AddBinding(Entity, e => e.RawJurAddress, w => w.Text).InitializeFromSource();

			ycheckExpirationDateControl.Binding.AddBinding(Entity, e => e.SpecialExpireDatePercentCheck, w => w.Active).InitializeFromSource();
			yspinExpirationDatePercent.Binding.AddBinding(Entity, e => e.SpecialExpireDatePercentCheck, w => w.Visible).InitializeFromSource();
			yspinExpirationDatePercent.Binding.AddBinding(Entity, e => e.SpecialExpireDatePercent, w => w.ValueAsDecimal).InitializeFromSource();

			lblVodovozNumber.LabelProp = Entity.VodovozInternalId.ToString();
			if (Entity.IsChainStore)
			{
				DelayDaysForProviderValue.Binding.AddBinding(Entity, e => e.DelayDaysForProviders, w => w.ValueAsInt).InitializeFromSource();
			}
			else
			{
				DelayDaysForProvider.Visible = false;
				DelayDaysForProviderValue.Visible = false;
			}
			entryMainCounterparty.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices));
			entryMainCounterparty.Binding.AddBinding(Entity, e => e.MainCounterparty, w => w.Subject).InitializeFromSource();
			entryPreviousCounterparty.SetEntityAutocompleteSelectorFactory(new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(QS.Project.Services.ServicesConfig.CommonServices));
			entryPreviousCounterparty.Binding.AddBinding(Entity, e => e.PreviousCounterparty, w => w.Subject).InitializeFromSource();

			//Setting subjects
			accountsView.ParentReference = new ParentReferenceGeneric<Counterparty, Account>(UoWGeneric, c => c.Accounts);
			deliveryPointView.DeliveryPointUoW = UoWGeneric;
			counterpartyContractsView.CounterpartyUoW = UoWGeneric;
			counterpartydocumentsview.Config(UoWGeneric, Entity);

			ySpecCmbCameFrom.SetRenderTextFunc<ClientCameFrom>(f => f.Name);

			ySpecCmbCameFrom.Sensitive = Entity.Id == 0;
			ySpecCmbCameFrom.ItemsList = CounterpartyRepository.GetPlacesClientCameFrom(
				UoW,
				Entity.CameFrom == null || !Entity.CameFrom.IsArchive
			);

			ySpecCmbCameFrom.Binding.AddBinding(Entity, f => f.CameFrom, w => w.SelectedItem).InitializeFromSource();

			var filterAccountant = new EmployeeFilterViewModel();
			filterAccountant.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceAccountant.RepresentationModel = new EmployeesVM(filterAccountant);
			referenceAccountant.Binding.AddBinding(Entity, e => e.Accountant, w => w.Subject).InitializeFromSource();
			var filterSalesManager = new EmployeeFilterViewModel();
			filterSalesManager.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceSalesManager.RepresentationModel = new EmployeesVM(filterSalesManager);
			referenceSalesManager.Binding.AddBinding(Entity, e => e.SalesManager, w => w.Subject).InitializeFromSource();
			var filterBottleManager = new EmployeeFilterViewModel();
			filterBottleManager.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking
			);
			referenceBottleManager.RepresentationModel = new EmployeesVM(filterBottleManager);
			referenceBottleManager.Binding.AddBinding(Entity, e => e.BottlesManager, w => w.Subject).InitializeFromSource();
			proxiesview1.CounterpartyUoW = UoWGeneric;
			dataentryMainContact.RepresentationModel = new ViewModel.ContactsVM(UoW, Entity);
			dataentryMainContact.Binding.AddBinding(Entity, e => e.MainContact, w => w.Subject).InitializeFromSource();
			dataentryFinancialContact.RepresentationModel = new ViewModel.ContactsVM(UoW, Entity);
			dataentryFinancialContact.Binding.AddBinding(Entity, e => e.FinancialContact, w => w.Subject).InitializeFromSource();
			buttonLoadFromDP.Clicked += ButtonLoadFromDP_Clicked;

			//Setting Contacts
			contactsview1.CounterpartyUoW = UoWGeneric;
			//Setting permissions
			spinMaxCredit.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("max_loan_amount");
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

			yEnumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yEnumCounterpartyType.Binding.AddBinding(Entity, c => c.CounterpartyType, w => w.SelectedItemOrNull).InitializeFromSource();
			yEnumCounterpartyType.Changed += YEnumCounterpartyType_Changed;
			yEnumCounterpartyType.ChangedByUser += YEnumCounterpartyType_ChangedByUser;
			YEnumCounterpartyType_Changed(this, new EventArgs());

			checkIsChainStore.Binding.AddBinding(Entity, e => e.IsChainStore, w => w.Active).InitializeFromSource();


			//make actions menu
			var menu = new Gtk.Menu();
			
			var menuItem = new Gtk.MenuItem("Все заказы контрагента");
			menuItem.Activated += AllOrders_Activated;
			menu.Add(menuItem);
			
			var menuItemFixedPrices = new Gtk.MenuItem("Фикс. цены для самовывоза");
			menuItemFixedPrices.Activated += (s, e) => OpenFixedPrices();
			menu.Add(menuItemFixedPrices);
			
			menuActions.Menu = menu;
			menu.ShowAll();

			menuActions.Sensitive = !UoWGeneric.IsNew;
			contactsview1.Visible = false;
			hboxCameFrom.Visible = (Entity.Id != 0 && Entity.CameFrom != null) || Entity.Id == 0;
			rbnPrices.Toggled += OnRbnPricesToggled;
			SetVisibilityForCloseDeliveryComments();

			int userId = ServicesConfig.CommonServices.UserService.CurrentUserId;
			bool canEditCounterpartyDetails = UoW.IsNew || ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_edit_counterparty_details", userId);
			accountsView.CanEdit = canEditCounterpartyDetails;
			datalegalname1.Sensitive = canEditCounterpartyDetails;
			enumPersonType.Sensitive = canEditCounterpartyDetails;
			datatable4.Sensitive = canEditCounterpartyDetails;
			entryFullName.Sensitive = canEditCounterpartyDetails;

			var waterFixedPricesGenerator = new WaterFixedPricesGenerator(NomenclatureRepository);
			var nomenclatureFixedPriceFactory = new NomenclatureFixedPriceFactory();
			var fixedPriceController = new NomenclatureFixedPriceController(nomenclatureFixedPriceFactory, waterFixedPricesGenerator);
			var fixedPricesModel = new CounterpartyFixedPricesModel(UoW, Entity, fixedPriceController);
			var nomSelectorFactory = new NomenclatureSelectorFactory();
			FixedPricesViewModel fixedPricesViewModel = new FixedPricesViewModel(UoW, fixedPricesModel, nomSelectorFactory, this);
			fixedpricesview.ViewModel = fixedPricesViewModel;

			//accountsView.

			ycheckAlwaysSendReceitps.Binding.AddBinding(Entity, e => e.AlwaysSendReceitps, w => w.Active).InitializeFromSource();
            ycheckAlwaysSendReceitps.Visible =
                ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_cash_receipts");
            
			#region Особая печать

			ytreeviewSpecialNomenclature.ColumnsConfig = ColumnsConfigFactory.Create<SpecialNomenclature>()
				.AddColumn("№").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Id.ToString() : "0")
				.AddColumn("ТМЦ").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : string.Empty)
				.AddColumn("Код").AddNumericRenderer(node => node.SpecialId).Adjustment(new Adjustment(0, 0, 100000, 1, 1, 1)).Editing()
				.Finish();
			ytreeviewSpecialNomenclature.ItemsDataSource = Entity.ObservableSpecialNomenclatures;

			ycheckSpecialDocuments.Binding.AddBinding(Entity, e => e.UseSpecialDocFields, w => w.Active).InitializeFromSource();
			radioSpecialDocFields.Visible = Entity.UseSpecialDocFields;
			yentryCargoReceiver.Binding.AddBinding(Entity, e => e.CargoReceiver, w => w.Text).InitializeFromSource();
			yentryCustomer.Binding.AddBinding(Entity, e => e.SpecialCustomer, w => w.Text).InitializeFromSource();
			yentrySpecialContract.Binding.AddBinding(Entity, e => e.SpecialContractNumber, w => w.Text).InitializeFromSource();
			yentrySpecialKPP.Binding.AddBinding(Entity, e => e.PayerSpecialKPP, w => w.Text).InitializeFromSource();
			yentryGovContract.Binding.AddBinding(Entity, e => e.GovContract, w => w.Text).InitializeFromSource();
			yentrySpecialDeliveryAddress.Binding.AddBinding(Entity, e => e.SpecialDeliveryAddress, w => w.Text).InitializeFromSource();

			yentryOKDP.Binding.AddBinding(Entity, e => e.OKDP, w => w.Text).InitializeFromSource();
			yentryOKPO.Binding.AddBinding(Entity, e => e.OKPO, w => w.Text).InitializeFromSource();

			int?[] docCount = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
			yspeccomboboxTTNCount.ItemsList = docCount;
			yspeccomboboxTorg2Count.ItemsList = docCount;
			yspeccomboboxUPDForNonCashlessCount.ItemsList = docCount;
			yspeccomboboxTorg2Count.Binding.AddBinding(Entity, e => e.Torg2Count, w => w.SelectedItem).InitializeFromSource();
			yspeccomboboxTTNCount.Binding.AddBinding(Entity, e => e.TTNCount, w => w.SelectedItem).InitializeFromSource();
			yspeccomboboxUPDForNonCashlessCount.Binding.AddBinding(Entity, e => e.UPDCount, w => w.SelectedItem).InitializeFromSource();

			enumcomboCargoReceiverSource.ItemsEnum = typeof(CargoReceiverSource);
			enumcomboCargoReceiverSource.Binding.AddBinding(Entity, e => e.CargoReceiverSource, w => w.SelectedItem).InitializeFromSource();

			lblTax.Binding.AddFuncBinding(Entity, e => e.PersonType == PersonType.legal, w => w.Visible).InitializeFromSource();
			enumTax.ItemsEnum = typeof(TaxType);
			
			if (Entity.CreateDate != null)
			{
				Enum[] hideEnums = { TaxType.None };
				enumTax.AddEnumToHideList(hideEnums);
			}
			
			enumTax.Binding.AddBinding(Entity, e => e.TaxType, w => w.SelectedItem).InitializeFromSource();
			enumTax.Binding.AddFuncBinding(Entity, e => e.PersonType == PersonType.legal, w => w.Visible).InitializeFromSource();
			
			UpdateCargoReceiver();

			#endregion Особая печать
		}

		void ButtonLoadFromDP_Clicked(object sender, EventArgs e)
		{
			var deliveryPointSelectDlg = new PermissionControlledRepresentationJournal(new ClientDeliveryPointsVM(UoW, Entity)) {
				Mode = JournalSelectMode.Single
			};
			deliveryPointSelectDlg.ObjectSelected += DeliveryPointRep_ObjectSelected;
			TabParent.AddSlaveTab(this, deliveryPointSelectDlg);
		}

		void DeliveryPointRep_ObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			if(e.GetNodes<ClientDeliveryPointVMNode>().FirstOrDefault() is ClientDeliveryPointVMNode node)
				yentrySpecialDeliveryAddress.Text = node.CompiledAddress;
		}


		public void ActivateContactsTab()
		{
			if(radioContacts.Sensitive)
				radioContacts.Active = true;
		}

		public void ActivateDetailsTab()
		{
			if(radioDetails.Sensitive)
				radioDetails.Active = true;
		}

		void AllOrders_Activated(object sender, EventArgs e)
		{
			var filter = new OrdersFilter(UoW);
			filter.SetAndRefilterAtOnce(x => x.RestrictCounterparty = Entity);
			Buttons buttons = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_delete") ? Buttons.All : (Buttons.Add | Buttons.Edit);
			PermissionControlledRepresentationJournal OrdersDialog = new PermissionControlledRepresentationJournal(new OrdersVM(filter), buttons) {
				Mode = JournalSelectMode.None
			};

			TabParent.AddTab(OrdersDialog, this, false);
		}

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения сохранения и повторите");
			return canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}

		public override bool Save()
		{
			try {
				SetSensetivity(false);
				if(Entity.PayerSpecialKPP == String.Empty)
					Entity.PayerSpecialKPP = null;
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
			} finally{
				SetSensetivity(true);
			}
		}

		/// <summary>
		/// Поиск контрагентов с таким же ИНН
		/// </summary>
		/// <returns><c>true</c>, if duplicate was checked, <c>false</c> otherwise.</returns>
		private bool CheckDuplicate()
		{
			string INN = UoWGeneric.Root.INN;
			IList<Counterparty> counterarties = Repositories.CounterpartyRepository.GetCounterpartiesByINN(UoW, INN);
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

		protected void OnRadioTagsToggled(object sender, EventArgs e)
		{
			if(radioTags.Active)
				notebook1.CurrentPage = 8;
		}

		protected void OnRadioSpecialDocFieldsToggled(object sender, EventArgs e)
		{
			if(radioSpecialDocFields.Active)
				notebook1.CurrentPage = 9;
		}

		protected void OnRbnPricesToggled(object sender, EventArgs e)
		{
			if(rbnPrices.Active)
				notebook1.CurrentPage = 10;
		}

		public void OpenFixedPrices()
		{
			notebook1.CurrentPage = 11;
		}

		void YEnumCounterpartyType_Changed(object sender, EventArgs e)
		{
			rbnPrices.Visible = Entity.CounterpartyType == CounterpartyType.Supplier;
		}

		void YEnumCounterpartyType_ChangedByUser(object sender, EventArgs e)
		{
			if(Entity.ObservableSuplierPriceItems.Any() && Entity.CounterpartyType == CounterpartyType.Buyer) {
				var response = MessageDialogHelper.RunWarningDialog(
					"Смена типа контрагента",
					"При смене контрагента с поставщика на покупателя произойдёт очистка списка цен на поставляемые им номенклатуры. Продолжить?",
					Gtk.ButtonsType.YesNo
				);
				if(response)
					Entity.ObservableSuplierPriceItems.Clear();
				else
					Entity.CounterpartyType = CounterpartyType.Supplier;
			}
		}

		protected void OnEnumPersonTypeChanged(object sender, EventArgs e)
		{
			labelFIO.Visible = entryFIO.Visible = Entity.PersonType == PersonType.natural;
			labelShort.Visible = datalegalname1.Visible =
					labelFullName.Visible = entryFullName.Visible =
					entryMainCounterparty.Visible = labelMainCounterparty.Visible =
						radioDetails.Visible = radiobuttonProxies.Visible = lblPaymentType.Visible =
							enumPayment.Visible = (Entity.PersonType == PersonType.legal);

			if (Entity.PersonType != PersonType.legal && Entity.TaxType != TaxType.None)
				Entity.TaxType = TaxType.None;
		}

		protected void OnEnumPaymentEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			enumDefaultDocumentType.Visible = labelDefaultDocumentType.Visible = (PaymentType)e.SelectedItem == PaymentType.cashless;
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
				var list = Repositories.CounterpartyRepository.GetUniqueSignatoryPosts(UoW);
				yentrySignPost.Completion.Model = ListStoreWorks.CreateFromEnumerable(list);
				yentrySignPost.Completion.TextColumn = 0;
				yentrySignPost.Completion.Complete();
			}
		}

		protected void OnYentrySignBaseOfFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			if(yentrySignBaseOf.Completion == null) {
				yentrySignBaseOf.Completion = new Gtk.EntryCompletion();
				var list = Repositories.CounterpartyRepository.GetUniqueSignatoryBaseOf(UoW);
				yentrySignBaseOf.Completion.Model = ListStoreWorks.CreateFromEnumerable(list);
				yentrySignBaseOf.Completion.TextColumn = 0;
				yentrySignBaseOf.Completion.Complete();
			}
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

		protected void OnYcheckSpecialDocumentsToggled(object sender, EventArgs e)
		{
			radioSpecialDocFields.Visible = ycheckSpecialDocuments.Active;
		}

		#region CloseDelivery //Переделать на PermissionCommentView

		private void SetVisibilityForCloseDeliveryComments()
		{

			labelCloseDelivery.Visible = Entity.IsDeliveriesClosed;
			GtkScrolledWindowCloseDelivery.Visible = Entity.IsDeliveriesClosed;
			buttonSaveCloseComment.Visible = Entity.IsDeliveriesClosed;
			buttonEditCloseDeliveryComment.Visible = Entity.IsDeliveriesClosed;
			buttonCloseDelivery.Label = Entity.IsDeliveriesClosed ? "Открыть поставки" : "Закрыть поставки";
			ytextviewCloseComment.Buffer.Text = Entity.IsDeliveriesClosed ? Entity.CloseDeliveryComment : String.Empty;

			if(!Entity.IsDeliveriesClosed)
				return;

			labelCloseDelivery.LabelProp = "Поставки закрыл : " + Entity.GetCloseDeliveryInfo() + Environment.NewLine + "<b>Комментарий по закрытию поставок:</b>";

			if(String.IsNullOrWhiteSpace(Entity.CloseDeliveryComment)) {
				buttonSaveCloseComment.Sensitive = true;
				buttonEditCloseDeliveryComment.Sensitive = false;
				ytextviewCloseComment.Sensitive = true;
			} else {
				buttonEditCloseDeliveryComment.Sensitive = true;
				buttonSaveCloseComment.Sensitive = false;
				ytextviewCloseComment.Sensitive = false;
			}
		}

		protected void OnButtonSaveCloseCommentClicked(object sender, EventArgs e)
		{
			if(String.IsNullOrWhiteSpace(ytextviewCloseComment.Buffer.Text))
				return;

			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty")) {
				MessageDialogHelper.RunWarningDialog("У вас нет прав для изменения комментария по закрытию поставок");
				return;
			}

			Entity.AddCloseDeliveryComment(ytextviewCloseComment.Buffer.Text, UoW);
			SetVisibilityForCloseDeliveryComments();
		}

		protected void OnButtonEditCloseDeliveryCommentClicked(object sender, EventArgs e)
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty")) {
				MessageDialogHelper.RunWarningDialog("У вас нет прав для изменения комментария по закрытию поставок");
				return;
			}

			if(MessageDialogHelper.RunQuestionDialog("Вы уверены что хотите изменить комментарий (преведущий комментарий будет удален)?")) {
				Entity.CloseDeliveryComment = ytextviewCloseComment.Buffer.Text = String.Empty;
				SetVisibilityForCloseDeliveryComments();
			}
		}

		protected void OnButtonCloseDeliveryClicked(object sender, EventArgs e)
		{
			if(!Entity.ToogleDeliveryOption(UoW)) {
				MessageDialogHelper.RunWarningDialog("У вас нет прав для закрытия/открытия поставок");
				return;
			}
			SetVisibilityForCloseDeliveryComments();
		}

		#endregion CloseDelivery

		protected void OnYbuttonAddNomClicked(object sender, EventArgs e)
		{
			var nomenclatureSelectDlg = new OrmReference(typeof(Nomenclature));
			nomenclatureSelectDlg.Mode = OrmReferenceMode.Select;
			nomenclatureSelectDlg.ObjectSelected += NomenclatureSelectDlg_ObjectSelected;
			this.TabParent.AddSlaveTab(this, nomenclatureSelectDlg);
		}

		private void NomenclatureSelectDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var specNom = new SpecialNomenclature();
			specNom.Nomenclature = e.Subject as Nomenclature;
			specNom.Counterparty = Entity;

			if(Entity.ObservableSpecialNomenclatures.Any(x => x.Nomenclature.Id == specNom.Nomenclature.Id))
				return;

			Entity.ObservableSpecialNomenclatures.Add(specNom);
		}

		protected void OnYbuttonRemoveNomClicked(object sender, EventArgs e)
		{
			Entity.ObservableSpecialNomenclatures.Remove(ytreeviewSpecialNomenclature.GetSelectedObject<SpecialNomenclature>());
		}

		protected void OnEnumcomboCargoReceiverSourceChangedByUser(object sender, EventArgs e)
		{
			UpdateCargoReceiver();
		}

		private string cargoReceiverBackupBuffer;

		private void UpdateCargoReceiver()
		{
			if(Entity.CargoReceiverSource != CargoReceiverSource.Special) {
				if(Entity.CargoReceiver != cargoReceiverBackupBuffer && !string.IsNullOrWhiteSpace(Entity.CargoReceiver)) {
					cargoReceiverBackupBuffer = Entity.CargoReceiver;
				}
				Entity.CargoReceiver = null;
			} else {
				Entity.CargoReceiver = cargoReceiverBackupBuffer;
			}
			yentryCargoReceiver.Visible = Entity.CargoReceiverSource == CargoReceiverSource.Special;
		}
	}
}