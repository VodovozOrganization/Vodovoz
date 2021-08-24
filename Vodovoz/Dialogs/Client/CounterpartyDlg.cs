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
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Filters.ViewModels;
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
using Vodovoz.Domain.Retail;
using System.Data.Bindings.Collections.Generic;
using NHibernate.Transform;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain.Service.BaseParametersServices;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Factories;
using Vodovoz.FilterViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalViewers;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.ViewWidgets;

namespace Vodovoz
{
	public partial class CounterpartyDlg : QS.Dialog.Gtk.EntityDialogBase<Counterparty>, ICounterpartyInfoProvider, ITDICloseControlTab
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly IEmployeeService _employeeService = VodovozGtkServicesConfig.EmployeeService;
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly IBottlesRepository _bottlesRepository = new BottlesRepository();
		private readonly IDepositRepository _depositRepository = new DepositRepository();
		private readonly IMoneyRepository _moneyRepository = new MoneyRepository();
		private readonly ICounterpartyRepository _counterpartyRepository = new CounterpartyRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
		private IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		private ISubdivisionRepository _subdivisionRepository;
		private IRouteListItemRepository _routeListItemRepository;
		private IFilePickerService _filePickerService;
		private ICounterpartyJournalFactory _counterpartySelectorFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureSelectorFactory;
		private INomenclatureRepository _nomenclatureRepository;
		private ValidationContext _validationContext;
		private Employee _currentEmployee;

		private bool _currentUserCanEditCounterpartyDetails = false;
		private bool _deliveryPointsConfigured = false;
		private bool _documentsConfigured = false;

		public virtual IUndeliveredOrdersJournalOpener UndeliveredOrdersJournalOpener =>
			_undeliveredOrdersJournalOpener ?? (_undeliveredOrdersJournalOpener = new UndeliveredOrdersJournalOpener());

		public virtual IEntityAutocompleteSelectorFactory EmployeeSelectorFactory =>
			_employeeSelectorFactory ??
			(_employeeSelectorFactory = new EmployeeJournalFactory().CreateEmployeeAutocompleteSelectorFactory());

		public virtual ISubdivisionRepository SubdivisionRepository =>
			_subdivisionRepository ?? (_subdivisionRepository = new SubdivisionRepository(new ParametersProvider()));

		public virtual IRouteListItemRepository RouteListItemRepository =>
			_routeListItemRepository ?? (_routeListItemRepository = new RouteListItemRepository());

		public virtual IFilePickerService FilePickerService =>
			_filePickerService ?? (_filePickerService = new GtkFilePicker());

		public virtual INomenclatureRepository NomenclatureRepository =>
			_nomenclatureRepository ?? (_nomenclatureRepository =
				new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())));

		public virtual ICounterpartyJournalFactory CounterpartySelectorFactory =>
			_counterpartySelectorFactory ?? (_counterpartySelectorFactory = new CounterpartyJournalFactory());

		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory =>
			_nomenclatureSelectorFactory ?? (_nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, new NomenclatureFilterViewModel(),
					CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
					NomenclatureRepository, _userRepository));

		#region Список каналов сбыта

		private GenericObservableList<SalesChannelSelectableNode> _salesChannels = new GenericObservableList<SalesChannelSelectableNode>();

		private GenericObservableList<SalesChannelSelectableNode> SalesChannels
		{
			get => _salesChannels;
			set
			{
				UnsubscribeOnCheckChanged();
				_salesChannels = value;
				SubscribeOnCheckChanged();
			}
		}

		private void UnsubscribeOnCheckChanged()
		{
			foreach(SalesChannelSelectableNode selectableSalesChannel in SalesChannels)
			{
				selectableSalesChannel.PropertyChanged -= OnStatusCheckChanged;
			}
		}

		private void SubscribeOnCheckChanged()
		{
			foreach(SalesChannelSelectableNode selectableSalesChannel in SalesChannels)
			{
				selectableSalesChannel.PropertyChanged += OnStatusCheckChanged;
			}
		}

		private void OnStatusCheckChanged(object sender, PropertyChangedEventArgs e)
		{
			var salesChannelSelectableNode = sender as SalesChannelSelectableNode;

			if(salesChannelSelectableNode.Selected)
			{
				if(Entity.SalesChannels.All(x => x.Id != salesChannelSelectableNode.Id))
				{
					Entity.SalesChannels.Add(UoW.Session.Get<SalesChannel>(salesChannelSelectableNode.Id));
				}
			}
			else
			{
				var salesChannelToRemove = Entity.SalesChannels.FirstOrDefault(x => x.Id == salesChannelSelectableNode.Id);
				if(salesChannelToRemove != null)
				{
					Entity.SalesChannels.Remove(salesChannelToRemove);
				}
			}
		}

		#endregion

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.CounterpartyView };

		public Counterparty Counterparty => UoWGeneric.Root;

		public override bool HasChanges
		{
			get
			{
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

		public CounterpartyDlg(Counterparty sub) : this(sub.Id)
		{
		}

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

		private Employee CurrentEmployee => _currentEmployee ??
		                                    (_currentEmployee =
			                                    _employeeService.GetEmployeeForUser(UoW, ServicesConfig.UserService.CurrentUserId));

		private void ConfigureDlg()
		{
			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;
			radioSpecialDocFields.Visible = Entity.UseSpecialDocFields;
			rbnPrices.Toggled += OnRbnPricesToggled;

			_currentUserCanEditCounterpartyDetails =
				UoW.IsNew
				|| ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
					"can_edit_counterparty_details",
					ServicesConfig.CommonServices.UserService.CurrentUserId);


			if(UoWGeneric.Root.CounterpartyContracts == null)
			{
				UoWGeneric.Root.CounterpartyContracts = new List<CounterpartyContract>();
			}

			ConfigureTabInfo();
			ConfigureTabContacts();
			ConfigureTabProxies();
			ConfigureTabContracts();
			ConfigureTabRequisites();
			ConfigureTabTags();
			ConfigureTabSpecialFields();
			ConfigureTabPrices();
			ConfigureTabFixedPrices();
			ConfigureValidationContext();

			//make actions menu
			var menu = new Gtk.Menu();

			var menuItem = new Gtk.MenuItem("Все заказы контрагента");
			menuItem.Activated += AllOrders_Activated;
			menu.Add(menuItem);

			var menuItemFixedPrices = new Gtk.MenuItem("Фикс. цены для самовывоза");
			menuItemFixedPrices.Activated += (s, e) => OpenFixedPrices();
			menu.Add(menuItemFixedPrices);

			var menuComplaint = new Gtk.MenuItem("Рекламации контрагента");
			menuComplaint.Activated += ComplaintViewOnActivated;
			menu.Add(menuComplaint);

			menuActions.Menu = menu;
			menu.ShowAll();

			menuActions.Sensitive = !UoWGeneric.IsNew;

			datatable4.Sensitive = _currentUserCanEditCounterpartyDetails;

			UpdateCargoReceiver();
			Entity.PropertyChanged += (sender, args) =>
			{
				if(args.PropertyName == nameof(Entity.SalesManager)
				|| args.PropertyName == nameof(Entity.Accountant)
				|| args.PropertyName == nameof(Entity.BottlesManager))
				{
					CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
				}
			};
		}

		private void ConfigureTabInfo()
		{
			enumPersonType.Sensitive = _currentUserCanEditCounterpartyDetails;
			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();

			yEnumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yEnumCounterpartyType.Binding.AddBinding(Entity, c => c.CounterpartyType, w => w.SelectedItemOrNull).InitializeFromSource();
			yEnumCounterpartyType.Changed += YEnumCounterpartyType_Changed;
			yEnumCounterpartyType.ChangedByUser += YEnumCounterpartyType_ChangedByUser;
			YEnumCounterpartyType_Changed(this, EventArgs.Empty);

			if(Entity.Id != 0 && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
				"can_change_delay_days_for_buyers_and_chain_store"))
			{
				checkIsChainStore.Sensitive = false;
				DelayDaysForBuyerValue.Sensitive = false;
			}

			checkIsChainStore.Toggled += CheckIsChainStoreOnToggled;
			checkIsChainStore.Binding.AddBinding(Entity, e => e.IsChainStore, w => w.Active).InitializeFromSource();

			ycheckIsArchived.Binding.AddBinding(Entity, e => e.IsArchive, w => w.Active).InitializeFromSource();
			SetSensitivityByPermission("can_arc_counterparty_and_deliverypoint", ycheckIsArchived);

			lblVodovozNumber.LabelProp = Entity.VodovozInternalId.ToString();

			hboxCameFrom.Visible = (Entity.Id != 0 && Entity.CameFrom != null) || Entity.Id == 0;

			ySpecCmbCameFrom.SetRenderTextFunc<ClientCameFrom>(f => f.Name);

			ySpecCmbCameFrom.Sensitive = Entity.Id == 0;
			ySpecCmbCameFrom.ItemsList = _counterpartyRepository.GetPlacesClientCameFrom(
				UoW,
				Entity.CameFrom == null || !Entity.CameFrom.IsArchive
			);

			ySpecCmbCameFrom.Binding.AddBinding(Entity, f => f.CameFrom, w => w.SelectedItem).InitializeFromSource();

			ycheckIsForRetail.Binding.AddBinding(Entity, e => e.IsForRetail, w => w.Active).InitializeFromSource();

			ycheckNoPhoneCall.Binding.AddBinding(Entity, e => e.NoPhoneCall, w => w.Active).InitializeFromSource();
			SetSensitivityByPermission("user_can_activate_no_phone_call_in_counterparty", ycheckNoPhoneCall);
			ycheckNoPhoneCall.Visible = Entity.IsForRetail;

			DelayDaysForBuyerValue.Binding.AddBinding(Entity, e => e.DelayDaysForBuyers, w => w.ValueAsInt).InitializeFromSource();
			lblDelayDaysForBuyer.Visible = DelayDaysForBuyerValue.Visible = Entity?.IsChainStore ?? false;

			yspinDelayDaysForTechProcessing.Binding.AddBinding(Entity, e => e.TechnicalProcessingDelay, w => w.ValueAsInt)
				.InitializeFromSource();

			entryFIO.Binding.AddBinding(Entity, e => e.Name, w => w.Text).InitializeFromSource();

			datalegalname1.Sensitive = _currentUserCanEditCounterpartyDetails;

			datalegalname1.Binding.AddSource(Entity)
				.AddBinding(s => s.Name, t => t.OwnName)
				.AddBinding(s => s.TypeOfOwnership, t => t.Ownership)
				.InitializeFromSource();

			entryFullName.Sensitive = _currentUserCanEditCounterpartyDetails;
			entryFullName.Binding.AddBinding(Entity, e => e.FullName, w => w.Text).InitializeFromSource();

			entryMainCounterparty
				.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryMainCounterparty.Binding.AddBinding(Entity, e => e.MainCounterparty, w => w.Subject).InitializeFromSource();

			entryPreviousCounterparty
				.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryPreviousCounterparty.Binding.AddBinding(Entity, e => e.PreviousCounterparty, w => w.Subject).InitializeFromSource();

			enumPayment.ItemsEnum = typeof(PaymentType);
			enumPayment.Binding.AddBinding(Entity, s => s.PaymentMethod, w => w.SelectedItemOrNull).InitializeFromSource();

			enumDefaultDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDefaultDocumentType.Binding.AddBinding(Entity, s => s.DefaultDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			lblTax.Binding.AddFuncBinding(Entity, e => e.PersonType == PersonType.legal, w => w.Visible).InitializeFromSource();

			enumTax.ItemsEnum = typeof(TaxType);

			if(Entity.CreateDate != null)
			{
				Enum[] hideEnums = { TaxType.None };
				enumTax.AddEnumToHideList(hideEnums);
			}

			enumTax.Binding.AddSource(Entity)
				.AddBinding(e => e.TaxType, w => w.SelectedItem)
				.AddFuncBinding(e => e.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			spinMaxCredit.Binding.AddBinding(Entity, e => e.MaxCredit, w => w.ValueAsDecimal).InitializeFromSource();
			SetSensitivityByPermission("max_loan_amount", spinMaxCredit);

			dataComment.Binding.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text).InitializeFromSource();

			// Прикрепляемые документы

			var filesViewModel = new CounterpartyFilesViewModel(
				Entity, UoW, new GtkFilePicker(), ServicesConfig.CommonServices, _userRepository);
			counterpartyfilesview1.ViewModel = filesViewModel;

			chkNeedNewBottles.Binding.AddBinding(Entity, e => e.NewBottlesNeeded, w => w.Active).InitializeFromSource();

			ycheckSpecialDocuments.Binding.AddBinding(Entity, e => e.UseSpecialDocFields, w => w.Active).InitializeFromSource();

			ycheckAlwaysSendReceitps.Binding.AddBinding(Entity, e => e.AlwaysSendReceitps, w => w.Active).InitializeFromSource();
			ycheckAlwaysSendReceitps.Visible =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_cash_receipts");

			ycheckExpirationDateControl.Binding.AddBinding(Entity, e => e.SpecialExpireDatePercentCheck, w => w.Active)
				.InitializeFromSource();
			yspinExpirationDatePercent.Binding.AddSource(Entity)
				.AddBinding(e => e.SpecialExpireDatePercentCheck, w => w.Visible)
				.AddBinding(e => e.SpecialExpireDatePercent, w => w.ValueAsDecimal)
				.InitializeFromSource();

			// Настройка каналов сбыта

			if(Entity.IsForRetail)
			{
				ytreeviewSalesChannels.ColumnsConfig = ColumnsConfigFactory.Create<SalesChannelSelectableNode>()
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.AddColumn("").AddToggleRenderer(x => x.Selected)
					.Finish();

				SalesChannel salesChannelAlias = null;
				SalesChannelSelectableNode salesChannelSelectableNodeAlias = null;

				var list = UoW.Session.QueryOver(() => salesChannelAlias)
					.SelectList(scList => scList
						.SelectGroup(() => salesChannelAlias.Id).WithAlias(() => salesChannelSelectableNodeAlias.Id)
						.Select(() => salesChannelAlias.Name).WithAlias(() => salesChannelSelectableNodeAlias.Name)
					).TransformUsing(Transformers.AliasToBean<SalesChannelSelectableNode>()).List<SalesChannelSelectableNode>();

				SalesChannels = new GenericObservableList<SalesChannelSelectableNode>(list);

				foreach(var selectableChannel in SalesChannels.Where(x => Entity.SalesChannels.Any(sc => sc.Id == x.Id)))
				{
					selectableChannel.Selected = true;
				}

				ytreeviewSalesChannels.ItemsDataSource = SalesChannels;
			}
			else
			{
				yspinDelayDaysForTechProcessing.Visible = false;
				lblDelayDaysForTechProcessing.Visible = false;
				frame2.Visible = false;
				frame3.Visible = false;
				label46.Visible = false;
				label47.Visible = false;
				label48.Visible = false;
				label49.Visible = false;
			}

			SetVisibilityForCloseDeliveryComments();
		}

		private void ConfigureTabContacts()
		{
			phonesView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Phones == null)
			{
				UoWGeneric.Root.Phones = new List<Phone>();
			}

			phonesView.Phones = UoWGeneric.Root.Phones;

			emailsView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Emails == null)
			{
				UoWGeneric.Root.Emails = new List<Email>();
			}

			emailsView.Emails = UoWGeneric.Root.Emails;

			var employeeJournalFactory = new EmployeeJournalFactory();
			if(SetSensitivityByPermission("can_set_personal_sales_manager", entrySalesManager))
			{
				entrySalesManager.SetEntityAutocompleteSelectorFactory(GetEmployeeFactoryWithResetFilter(employeeJournalFactory));
			}

			entrySalesManager.Binding.AddBinding(Entity, e => e.SalesManager, w => w.Subject).InitializeFromSource();

			if(SetSensitivityByPermission("can_set_personal_accountant", entryAccountant))
			{
				entryAccountant.SetEntityAutocompleteSelectorFactory(GetEmployeeFactoryWithResetFilter(employeeJournalFactory));
			}

			entryAccountant.Binding.AddBinding(Entity, e => e.Accountant, w => w.Subject).InitializeFromSource();

			if(SetSensitivityByPermission("can_set_personal_bottles_manager", entryBottlesManager))
			{
				entryBottlesManager.SetEntityAutocompleteSelectorFactory(GetEmployeeFactoryWithResetFilter(employeeJournalFactory));
			}

			entryBottlesManager.CanEditReference = true;
			entryBottlesManager.Binding.AddBinding(Entity, e => e.BottlesManager, w => w.Subject).InitializeFromSource();

			//FIXME данный виджет создан с Visible = false и нигде не меняется
			dataentryMainContact.RepresentationModel = new ContactsVM(UoW, Entity);
			dataentryMainContact.Binding.AddBinding(Entity, e => e.MainContact, w => w.Subject).InitializeFromSource();

			//FIXME данный виджет создан с Visible = false и нигде не меняется
			dataentryFinancialContact.RepresentationModel = new ContactsVM(UoW, Entity);
			dataentryFinancialContact.Binding.AddBinding(Entity, e => e.FinancialContact, w => w.Subject).InitializeFromSource();

			txtRingUpPhones.Binding.AddBinding(Entity, e => e.RingUpPhone, w => w.Buffer.Text).InitializeFromSource();

			contactsview1.CounterpartyUoW = UoWGeneric;
			contactsview1.Visible = true;
		}

		private bool SetSensitivityByPermission(string permission, Widget widget)
		{
			return widget.Sensitive =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(permission);
		}

		private IEntityAutocompleteSelectorFactory GetEmployeeFactoryWithResetFilter(IEmployeeJournalFactory employeeJournalFactory)
		{
			var filter = new EmployeeFilterViewModel
			{
				RestrictCategory = EmployeeCategory.office,
				Status = EmployeeStatus.IsWorking
			};
			employeeJournalFactory.SetEmployeeFilterViewModel(filter);
			return employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
		}

		private void ConfigureTabRequisites()
		{
			validatedINN.ValidationMode = validatedKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			validatedINN.Binding.AddBinding(Entity, e => e.INN, w => w.Text).InitializeFromSource();

			yentrySignFIO.Binding.AddBinding(Entity, e => e.SignatoryFIO, w => w.Text).InitializeFromSource();
			validatedKPP.Binding.AddBinding(Entity, e => e.KPP, w => w.Text).InitializeFromSource();
			yentrySignPost.Binding.AddBinding(Entity, e => e.SignatoryPost, w => w.Text).InitializeFromSource();
			entryJurAddress.Binding.AddBinding(Entity, e => e.RawJurAddress, w => w.Text).InitializeFromSource();
			yentrySignBaseOf.Binding.AddBinding(Entity, e => e.SignatoryBaseOf, w => w.Text).InitializeFromSource();

			accountsView.CanEdit = _currentUserCanEditCounterpartyDetails;

			accountsView.ParentReference = new ParentReferenceGeneric<Counterparty, Account>(UoWGeneric, c => c.Accounts);
		}

		private void ConfigureTabProxies()
		{
			proxiesview1.CounterpartyUoW = UoWGeneric;
		}

		private void ConfigureTabContracts()
		{
			counterpartyContractsView.CounterpartyUoW = UoWGeneric;
		}

		private void ConfigureTabTags()
		{
			ytreeviewTags.ColumnsConfig = ColumnsConfigFactory.Create<Tag>()
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Цвет").AddTextRenderer()
				.AddSetter((cell, node) => { cell.Markup = $"<span foreground=\" {node.ColorText}\">♥</span>"; })
				.AddColumn("")
				.Finish();

			ytreeviewTags.ItemsDataSource = Entity.ObservableTags;
		}

		private void ConfigureTabSpecialFields()
		{
			enumcomboCargoReceiverSource.ItemsEnum = typeof(CargoReceiverSource);
			enumcomboCargoReceiverSource.Binding.AddBinding(Entity, e => e.CargoReceiverSource, w => w.SelectedItem).InitializeFromSource();

			yentryCargoReceiver.Binding.AddBinding(Entity, e => e.CargoReceiver, w => w.Text).InitializeFromSource();
			yentryCustomer.Binding.AddBinding(Entity, e => e.SpecialCustomer, w => w.Text).InitializeFromSource();
			yentrySpecialContract.Binding.AddBinding(Entity, e => e.SpecialContractNumber, w => w.Text).InitializeFromSource();
			yentrySpecialKPP.Binding.AddBinding(Entity, e => e.PayerSpecialKPP, w => w.Text).InitializeFromSource();
			yentryGovContract.Binding.AddBinding(Entity, e => e.GovContract, w => w.Text).InitializeFromSource();
			yentrySpecialDeliveryAddress.Binding.AddBinding(Entity, e => e.SpecialDeliveryAddress, w => w.Text).InitializeFromSource();

			buttonLoadFromDP.Clicked += ButtonLoadFromDP_Clicked;

			yentryOKPO.Binding.AddBinding(Entity, e => e.OKPO, w => w.Text).InitializeFromSource();
			yentryOKDP.Binding.AddBinding(Entity, e => e.OKDP, w => w.Text).InitializeFromSource();

			int?[] docCount = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			yspeccomboboxTTNCount.ItemsList = docCount;
			yspeccomboboxTorg2Count.ItemsList = docCount;
			yspeccomboboxUPDForNonCashlessCount.ItemsList = docCount;

			yspeccomboboxTorg2Count.Binding.AddBinding(Entity, e => e.Torg2Count, w => w.SelectedItem).InitializeFromSource();
			yspeccomboboxTTNCount.Binding.AddBinding(Entity, e => e.TTNCount, w => w.SelectedItem).InitializeFromSource();
			yspeccomboboxUPDForNonCashlessCount.Binding.AddBinding(Entity, e => e.UPDCount, w => w.SelectedItem).InitializeFromSource();

			if(Entity.IsForRetail)
			{
				yspeccomboboxUPDCount.ItemsList = docCount;
				yspeccomboboxTorg12Count.ItemsList = docCount;
				yspeccomboboxShetFacturaCount.ItemsList = docCount;
				yspeccomboboxCarProxyCount.ItemsList = docCount;

				yspeccomboboxUPDCount.Binding.AddBinding(Entity, e => e.AllUPDCount, w => w.SelectedItem).InitializeFromSource();
				yspeccomboboxTorg12Count.Binding.AddBinding(Entity, e => e.Torg12Count, w => w.SelectedItem).InitializeFromSource();
				yspeccomboboxShetFacturaCount.Binding.AddBinding(Entity, e => e.ShetFacturaCount, w => w.SelectedItem)
					.InitializeFromSource();
				yspeccomboboxCarProxyCount.Binding.AddBinding(Entity, e => e.CarProxyCount, w => w.SelectedItem).InitializeFromSource();
			}
			else
			{
				yspeccomboboxUPDCount.Visible = false;
				yspeccomboboxTorg12Count.Visible = false;
				yspeccomboboxShetFacturaCount.Visible = false;
				yspeccomboboxCarProxyCount.Visible = false;
			}

			ytreeviewSpecialNomenclature.ColumnsConfig = ColumnsConfigFactory.Create<SpecialNomenclature>()
				.AddColumn("№").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Id.ToString() : "0")
				.AddColumn("ТМЦ").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : string.Empty)
				.AddColumn("Код").AddNumericRenderer(node => node.SpecialId).Adjustment(new Adjustment(0, 0, 100000, 1, 1, 1)).Editing()
				.Finish();
			ytreeviewSpecialNomenclature.ItemsDataSource = Entity.ObservableSpecialNomenclatures;
		}

		private void ConfigureTabDeliveryPoints()
		{
			deliveryPointsManagementView.DeliveryPointUoW = UoWGeneric;
		}

		private void ConfigureTabDocuments()
		{
			counterpartydocumentsview.Config(UoWGeneric, Entity);
		}

		private void ConfigureTabPrices()
		{
			supplierPricesWidget.ViewModel =
				new ViewModels.Client.SupplierPricesWidgetViewModel(
					Entity,
					UoW,
					this,
					ServicesConfig.CommonServices,
					_employeeService,
					CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
					NomenclatureSelectorFactory,
					NomenclatureRepository,
					_userRepository);
		}

		private void ConfigureTabFixedPrices()
		{
			var waterFixedPricesGenerator = new WaterFixedPricesGenerator(NomenclatureRepository);
			var nomenclatureFixedPriceFactory = new NomenclatureFixedPriceFactory();
			var fixedPriceController = new NomenclatureFixedPriceController(nomenclatureFixedPriceFactory, waterFixedPricesGenerator);
			var fixedPricesModel = new CounterpartyFixedPricesModel(UoW, Entity, fixedPriceController);
			var nomSelectorFactory = new NomenclatureSelectorFactory();
			FixedPricesViewModel fixedPricesViewModel = new FixedPricesViewModel(UoW, fixedPricesModel, nomSelectorFactory, this);
			fixedpricesview.ViewModel = fixedPricesViewModel;
		}

		private void ConfigureValidationContext()
		{
			_validationContext = _validationContextFactory.CreateNewValidationContext(Entity);

			_validationContext.ServiceContainer.AddService(typeof(IBottlesRepository), _bottlesRepository);
			_validationContext.ServiceContainer.AddService(typeof(IDepositRepository), _depositRepository);
			_validationContext.ServiceContainer.AddService(typeof(IMoneyRepository), _moneyRepository);
			_validationContext.ServiceContainer.AddService(typeof(ICounterpartyRepository), _counterpartyRepository);
			_validationContext.ServiceContainer.AddService(typeof(IOrderRepository), _orderRepository);
		}

		private void CheckIsChainStoreOnToggled(object sender, EventArgs e)
		{
			if(Entity.IsChainStore)
			{
				lblDelayDaysForBuyer.Visible = DelayDaysForBuyerValue.Visible = true;
			}
			else
			{
				lblDelayDaysForBuyer.Visible = DelayDaysForBuyerValue.Visible = false;
				Entity.DelayDaysForBuyers = 0;
			}
		}

		private void ButtonLoadFromDP_Clicked(object sender, EventArgs e)
		{
			var deliveryPointSelectDlg = new PermissionControlledRepresentationJournal(new ClientDeliveryPointsVM(UoW, Entity))
			{
				Mode = JournalSelectMode.Single
			};
			deliveryPointSelectDlg.ObjectSelected += DeliveryPointRep_ObjectSelected;
			TabParent.AddSlaveTab(this, deliveryPointSelectDlg);
		}

		private void DeliveryPointRep_ObjectSelected(object sender, JournalObjectSelectedEventArgs e)
		{
			if(e.GetNodes<ClientDeliveryPointVMNode>().FirstOrDefault() is ClientDeliveryPointVMNode node)
			{
				yentrySpecialDeliveryAddress.Text = node.CompiledAddress;
			}
		}


		public void ActivateContactsTab()
		{
			if(radioContacts.Sensitive)
			{
				radioContacts.Active = true;
			}
		}

		public void ActivateDetailsTab()
		{
			if(radioDetails.Sensitive)
			{
				radioDetails.Active = true;
			}
		}

		private void AllOrders_Activated(object sender, EventArgs e)
		{
			ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

			var orderJournalFilter = new OrderJournalFilterViewModel(
				new CounterpartyJournalFactory(),
				new DeliveryPointJournalFactory()) { RestrictCounterparty = Entity };
			var orderJournalViewModel = new OrderJournalViewModel(
				orderJournalFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				NomenclatureRepository,
				_userRepository,
				new OrderSelectorFactory(),
				new EmployeeJournalFactory(),
				new CounterpartyJournalFactory(),
				new DeliveryPointJournalFactory(),
				subdivisionJournalFactory,
				new GtkTabsOpener(),
				new UndeliveredOrdersJournalOpener(),
				new NomenclatureSelectorFactory(),
				new UndeliveredOrdersRepository()
			);

			TabParent.AddTab(orderJournalViewModel, this, false);
		}

		private void ComplaintViewOnActivated(object sender, EventArgs e)
		{
			ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

			var filter = new ComplaintFilterViewModel(
				ServicesConfig.CommonServices, SubdivisionRepository, EmployeeSelectorFactory,
				CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			filter.SetAndRefilterAtOnce(x => x.Counterparty = Entity);

			var complaintsJournalViewModel = new ComplaintsJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				UndeliveredOrdersJournalOpener,
				_employeeService,
				CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
				RouteListItemRepository,
				SubdivisionParametersProvider.Instance,
				filter,
				FilePickerService,
				SubdivisionRepository,
				new GtkReportViewOpener(),
				new GtkTabsOpener(),
				NomenclatureRepository,
				_userRepository,
				new OrderSelectorFactory(),
				new EmployeeJournalFactory(),
				new CounterpartyJournalFactory(),
				new DeliveryPointJournalFactory(),
				subdivisionJournalFactory,
				new SalesPlanJournalFactory(),
				new NomenclatureSelectorFactory(),
				new UndeliveredOrdersRepository()
			);

			TabParent.AddTab(complaintsJournalViewModel, this, false);
		}

		private bool _canClose = true;

		public bool CanClose()
		{
			if(!_canClose)
			{
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения сохранения и повторите");
			}

			return _canClose;
		}

		private void SetSensetivity(bool isSensetive)
		{
			_canClose = isSensetive;
			buttonSave.Sensitive = isSensetive;
			buttonCancel.Sensitive = isSensetive;
		}

		public override bool Save()
		{
			try
			{
				SetSensetivity(false);

				if(Entity.PayerSpecialKPP == string.Empty)
				{
					Entity.PayerSpecialKPP = null;
				}

				Entity.UoW = UoW;

				if(!ServicesConfig.ValidationService.Validate(Entity, _validationContext))
				{
					return false;
				}

				_logger.Info("Сохраняем контрагента...");
				phonesView.RemoveEmpty();
				emailsView.RemoveEmpty();
				UoWGeneric.Save();
				_logger.Info("Ok.");
				return true;
			}
			finally
			{
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
			IList<Counterparty> counterarties = _counterpartyRepository.GetCounterpartiesByINN(UoW, INN);
			return counterarties != null && counterarties.Any(x => x.Id != UoWGeneric.Root.Id);
		}

		protected void OnRadioInfoToggled(object sender, EventArgs e)
		{
			if(radioInfo.Active)
			{
				notebook1.CurrentPage = 0;
			}
		}

		protected void OnRadioContactsToggled(object sender, EventArgs e)
		{
			if(radioContacts.Active)
			{
				notebook1.CurrentPage = 1;
			}
		}

		protected void OnRadioDetailsToggled(object sender, EventArgs e)
		{
			if(radioDetails.Active)
			{
				notebook1.CurrentPage = 2;
			}
		}

		protected void OnRadiobuttonProxiesToggled(object sender, EventArgs e)
		{
			if(radiobuttonProxies.Active)
			{
				notebook1.CurrentPage = 3;
			}
		}

		protected void OnRadioContractsToggled(object sender, EventArgs e)
		{
			if(radioContracts.Active)
			{
				notebook1.CurrentPage = 4;
			}
		}

		protected void OnRadioDocumentsToggled(object sender, EventArgs e)
		{
			if(!_documentsConfigured)
			{
				ConfigureTabDocuments();
				_documentsConfigured = true;
			}

			if(radioDocuments.Active)
			{
				notebook1.CurrentPage = 5;
			}
		}

		protected void OnRadioDeliveryPointToggled(object sender, EventArgs e)
		{
			if(!_deliveryPointsConfigured)
			{
				ConfigureTabDeliveryPoints();
				_deliveryPointsConfigured = true;
			}

			if(radioDeliveryPoint.Active)
			{
				notebook1.CurrentPage = 6;
			}
		}

		protected void OnRadioTagsToggled(object sender, EventArgs e)
		{
			if(radioTags.Active)
			{
				notebook1.CurrentPage = 7;
			}
		}

		protected void OnRadioSpecialDocFieldsToggled(object sender, EventArgs e)
		{
			if(radioSpecialDocFields.Active)
			{
				notebook1.CurrentPage = 8;
			}
		}

		protected void OnRbnPricesToggled(object sender, EventArgs e)
		{
			if(rbnPrices.Active)
			{
				notebook1.CurrentPage = 9;
			}
		}

		public void OpenFixedPrices()
		{
			notebook1.CurrentPage = 10;
		}

		private void YEnumCounterpartyType_Changed(object sender, EventArgs e)
		{
			rbnPrices.Visible = Entity.CounterpartyType == CounterpartyType.Supplier;
		}

		private void YEnumCounterpartyType_ChangedByUser(object sender, EventArgs e)
		{
			if(Entity.ObservableSuplierPriceItems.Any() && Entity.CounterpartyType == CounterpartyType.Buyer)
			{
				var response = MessageDialogHelper.RunWarningDialog(
					"Смена типа контрагента",
					"При смене контрагента с поставщика на покупателя произойдёт очистка списка цен на поставляемые им номенклатуры. Продолжить?",
					Gtk.ButtonsType.YesNo
				);
				if(response)
				{
					Entity.ObservableSuplierPriceItems.Clear();
				}
				else
				{
					Entity.CounterpartyType = CounterpartyType.Supplier;
				}
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

			if(Entity.PersonType != PersonType.legal && Entity.TaxType != TaxType.None)
			{
				Entity.TaxType = TaxType.None;
			}
		}

		protected void OnEnumPaymentEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			enumDefaultDocumentType.Visible = labelDefaultDocumentType.Visible = (PaymentType)e.SelectedItem == PaymentType.cashless;
		}

		protected void OnEnumPaymentChangedByUser(object sender, EventArgs e)
		{
			if(Entity.PaymentMethod == PaymentType.cashless)
			{
				Entity.DefaultDocumentType = DefaultDocumentType.upd;
			}
			else
			{
				Entity.DefaultDocumentType = null;
			}
		}

		protected void OnYentrySignPostFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			if(yentrySignPost.Completion == null)
			{
				yentrySignPost.Completion = new EntryCompletion();
				var list = _counterpartyRepository.GetUniqueSignatoryPosts(UoW);
				yentrySignPost.Completion.Model = ListStoreWorks.CreateFromEnumerable(list);
				yentrySignPost.Completion.TextColumn = 0;
				yentrySignPost.Completion.Complete();
			}
		}

		protected void OnYentrySignBaseOfFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			if(yentrySignBaseOf.Completion == null)
			{
				yentrySignBaseOf.Completion = new EntryCompletion();
				var list = _counterpartyRepository.GetUniqueSignatoryBaseOf(UoW);
				yentrySignBaseOf.Completion.Model = ListStoreWorks.CreateFromEnumerable(list);
				yentrySignBaseOf.Completion.TextColumn = 0;
				yentrySignBaseOf.Completion.Complete();
			}
		}

		private void RefWin_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(e.Subject is Tag tag)
			{
				Entity.ObservableTags.Add(tag);
			}
		}

		protected void OnButtonAddTagClicked(object sender, EventArgs e)
		{
			var refWin = new OrmReference(typeof(Tag))
			{
				Mode = OrmReferenceMode.Select
			};
			refWin.ObjectSelected += RefWin_ObjectSelected;
			TabParent.AddSlaveTab(this, refWin);
		}

		protected void OnButtonDeleteTagClicked(object sender, EventArgs e)
		{
			if(ytreeviewTags.GetSelectedObject() is Tag tag)
			{
				Entity.ObservableTags.Remove(tag);
			}
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
			{
				return;
			}

			labelCloseDelivery.LabelProp = "Поставки закрыл : " + Entity.GetCloseDeliveryInfo() + Environment.NewLine +
			                               "<b>Комментарий по закрытию поставок:</b>";

			if(string.IsNullOrWhiteSpace(Entity.CloseDeliveryComment))
			{
				buttonSaveCloseComment.Sensitive = true;
				buttonEditCloseDeliveryComment.Sensitive = false;
				ytextviewCloseComment.Sensitive = true;
			}
			else
			{
				buttonEditCloseDeliveryComment.Sensitive = true;
				buttonSaveCloseComment.Sensitive = false;
				ytextviewCloseComment.Sensitive = false;
			}
		}

		protected void OnButtonSaveCloseCommentClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrWhiteSpace(ytextviewCloseComment.Buffer.Text))
			{
				return;
			}

			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty"))
			{
				MessageDialogHelper.RunWarningDialog("У вас нет прав для изменения комментария по закрытию поставок");
				return;
			}

			Entity.AddCloseDeliveryComment(ytextviewCloseComment.Buffer.Text, CurrentEmployee);
			SetVisibilityForCloseDeliveryComments();
		}

		protected void OnButtonEditCloseDeliveryCommentClicked(object sender, EventArgs e)
		{
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_deliveries_for_counterparty"))
			{
				MessageDialogHelper.RunWarningDialog("У вас нет прав для изменения комментария по закрытию поставок");
				return;
			}

			if(MessageDialogHelper.RunQuestionDialog("Вы уверены что хотите изменить комментарий (преведущий комментарий будет удален)?"))
			{
				Entity.CloseDeliveryComment = ytextviewCloseComment.Buffer.Text = String.Empty;
				SetVisibilityForCloseDeliveryComments();
			}
		}

		protected void OnButtonCloseDeliveryClicked(object sender, EventArgs e)
		{
			if(!Entity.ToggleDeliveryOption(CurrentEmployee))
			{
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
			TabParent.AddSlaveTab(this, nomenclatureSelectDlg);
		}

		private void NomenclatureSelectDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var specNom = new SpecialNomenclature
			{
				Nomenclature = e.Subject as Nomenclature,
				Counterparty = Entity
			};

			if(Entity.ObservableSpecialNomenclatures.Any(x => x.Nomenclature.Id == specNom.Nomenclature.Id))
			{
				return;
			}

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

		private string _cargoReceiverBackupBuffer;

		private void UpdateCargoReceiver()
		{
			if(Entity.CargoReceiverSource != CargoReceiverSource.Special)
			{
				if(Entity.CargoReceiver != _cargoReceiverBackupBuffer && !string.IsNullOrWhiteSpace(Entity.CargoReceiver))
				{
					_cargoReceiverBackupBuffer = Entity.CargoReceiver;
				}

				Entity.CargoReceiver = null;
			}
			else
			{
				Entity.CargoReceiver = _cargoReceiverBackupBuffer;
			}

			yentryCargoReceiver.Visible = Entity.CargoReceiverSource == CargoReceiverSource.Special;
		}
	}

	public class SalesChannelSelectableNode : PropertyChangedBase
	{
		private int _id;

		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		private bool _selected;

		public virtual bool Selected
		{
			get => _selected;
			set => SetField(ref _selected, value);
		}

		private string _name;

		public virtual string Name
		{
			get => _name;
			set => SetField(ref _name, value);
		}

		public string Title => Name;

		public SalesChannelSelectableNode()
		{
		}

		public SalesChannelSelectableNode(SalesChannel salesChannel)
		{
			Id = salesChannel.Id;
			Name = salesChannel.Name;
		}
	}
}
