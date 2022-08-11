using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Transform;
using NLog;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Project.Services.FileDialog;
using QS.Services;
using QS.Tdi;
using QS.Utilities;
using QS.ViewModels.Extension;
using QSOrmProject;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewers;
using Vodovoz.JournalViewModels;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Dialogs.Counterparty;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewWidgets;

namespace Vodovoz
{
	public partial class CounterpartyDlg : QS.Dialog.Gtk.EntityDialogBase<Counterparty>, ICounterpartyInfoProvider, ITDICloseControlTab,
		IAskSaveOnCloseViewModel
	{
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly bool _canSetWorksThroughOrganization =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_organization_from_order_and_counterparty");
		private readonly int _currentUserId = ServicesConfig.UserService.CurrentUserId;
		private readonly IEmployeeService _employeeService = VodovozGtkServicesConfig.EmployeeService;
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly IBottlesRepository _bottlesRepository = new BottlesRepository();
		private readonly IDepositRepository _depositRepository = new DepositRepository();
		private readonly IMoneyRepository _moneyRepository = new MoneyRepository();
		private readonly ICounterpartyRepository _counterpartyRepository = new CounterpartyRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IPhoneRepository _phoneRepository = new PhoneRepository();
		private readonly IEmailRepository _emailRepository = new EmailRepository();
		private readonly IContactsParameters _contactsParameters = new ContactParametersProvider(new ParametersProvider());
		private readonly ISubdivisionParametersProvider _subdivisionParametersProvider =
			new SubdivisionParametersProvider(new ParametersProvider());
		private RoboatsJournalsFactory _roboatsJournalsFactory;
		private readonly IEmailParametersProvider _emailParametersProvider = new EmailParametersProvider(new ParametersProvider());
		private readonly ICommonServices _commonServices = ServicesConfig.CommonServices;
		private IUndeliveredOrdersJournalOpener _undeliveredOrdersJournalOpener;
		private ISubdivisionRepository _subdivisionRepository;
		private IRouteListItemRepository _routeListItemRepository;
		private IFileDialogService _fileDialogService;
		private ICounterpartyJournalFactory _counterpartySelectorFactory;
		private IEntityAutocompleteSelectorFactory _nomenclatureSelectorFactory;
		private INomenclatureRepository _nomenclatureRepository;
		private ValidationContext _validationContext;
		private Employee _currentEmployee;
		private PhonesViewModel _phonesViewModel;
		private double _emailLastScrollPosition;

		private bool _currentUserCanEditCounterpartyDetails = false;
		private bool _deliveryPointsConfigured = false;
		private bool _documentsConfigured = false;

		public ThreadDataLoader<EmailRow> EmailDataLoader { get; private set; }

		public virtual IUndeliveredOrdersJournalOpener UndeliveredOrdersJournalOpener =>
			_undeliveredOrdersJournalOpener ?? (_undeliveredOrdersJournalOpener = new UndeliveredOrdersJournalOpener());

		public virtual ISubdivisionRepository SubdivisionRepository =>
			_subdivisionRepository ?? (_subdivisionRepository = new SubdivisionRepository(new ParametersProvider()));

		public virtual IRouteListItemRepository RouteListItemRepository =>
			_routeListItemRepository ?? (_routeListItemRepository = new RouteListItemRepository());

		public virtual IFileDialogService FilePickerService =>
			_fileDialogService ?? (_fileDialogService = new FileDialogService());

		public virtual INomenclatureRepository NomenclatureRepository =>
			_nomenclatureRepository ?? (_nomenclatureRepository =
				new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider())));

		public virtual ICounterpartyJournalFactory CounterpartySelectorFactory =>
			_counterpartySelectorFactory ?? (_counterpartySelectorFactory = new CounterpartyJournalFactory());

		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory =>
			_nomenclatureSelectorFactory ?? (_nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, new NomenclatureFilterViewModel(),
					CounterpartySelectorFactory,
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

		public bool HasOgrn => Counterparty.CounterpartyType == CounterpartyType.Dealer;

		private bool CanEdit => permissionResult.CanUpdate || permissionResult.CanCreate && Entity.Id == 0;

		public override bool HasChanges
		{
			get
			{
				_phonesViewModel.RemoveEmpty();
				emailsView.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => CanEdit;

		#endregion

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

		public CounterpartyDlg(NewCounterpartyParameters parameters)
		{
			Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();

			Entity.Name = parameters.Name;
			Entity.FullName = parameters.FullName;
			Entity.INN = parameters.INN;
			Entity.KPP = parameters.KPP;
			Entity.PaymentMethod = parameters.PaymentMethod;
			Entity.TypeOfOwnership = parameters.TypeOfOwnership;
			Entity.PersonType = parameters.PersonType;
			Entity.AddAccount(parameters.Account);

			ConfigureDlg();
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
			phone.Counterparty = Entity;
			Entity.Phones.Add(phone);
			ConfigureDlg();
		}

		private Employee CurrentEmployee =>
			_currentEmployee ?? (_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _currentUserId));

		private void ConfigureDlg()
		{
			var roboatsSettings = new RoboatsSettings(new ParametersProvider());
			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			var fileDialogService = new FileDialogService();
			var roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			var nomenclatureSelectorFactory = new NomenclatureJournalFactory();
			_roboatsJournalsFactory = new RoboatsJournalsFactory(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, roboatsViewModelFactory, nomenclatureSelectorFactory);

			buttonSave.Sensitive = CanEdit;
			btnCancel.Clicked += (sender, args) => OnCloseTab(false, CloseSource.Cancel);

			notebook1.CurrentPage = 0;
			notebook1.ShowTabs = false;
			radioSpecialDocFields.Visible = Entity.UseSpecialDocFields;
			rbnPrices.Toggled += OnRbnPricesToggled;

			_currentUserCanEditCounterpartyDetails =
				UoW.IsNew || ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission(
					"can_edit_counterparty_details", _currentUserId);

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

			datatable4.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;

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
			enumPersonType.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();

			yEnumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yEnumCounterpartyType.Binding
				.AddBinding(Entity, c => c.CounterpartyType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			yEnumCounterpartyType.Sensitive = CanEdit;
			yEnumCounterpartyType.Changed += OnEnumCounterpartyTypeChanged;
			yEnumCounterpartyType.ChangedByUser += OnEnumCounterpartyTypeChangedByUser;
			OnEnumCounterpartyTypeChanged(this, EventArgs.Empty);

			if((Entity.Id == 0 && permissionResult.CanCreate)
				|| (Entity.Id > 0
					&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
						"can_change_delay_days_for_buyers_and_chain_store")
					&& permissionResult.CanUpdate))
			{
				checkIsChainStore.Sensitive = true;
				DelayDaysForBuyerValue.Sensitive = true;
			}
			else
			{
				checkIsChainStore.Sensitive = false;
				DelayDaysForBuyerValue.Sensitive = false;
			}

			checkIsChainStore.Toggled += CheckIsChainStoreOnToggled;
			checkIsChainStore.Binding
				.AddBinding(Entity, e => e.IsChainStore, w => w.Active)
				.InitializeFromSource();

			ycheckIsArchived.Binding
				.AddBinding(Entity, e => e.IsArchive, w => w.Active)
				.InitializeFromSource();
			SetSensitivityByPermission("can_arc_counterparty_and_deliverypoint", ycheckIsArchived);

			lblVodovozNumber.LabelProp = Entity.VodovozInternalId.ToString();

			hboxCameFrom.Visible = (Entity.Id != 0 && Entity.CameFrom != null) || Entity.Id == 0;

			ySpecCmbCameFrom.SetRenderTextFunc<ClientCameFrom>(f => f.Name);

			ySpecCmbCameFrom.Sensitive = Entity.Id == 0 && CanEdit;
			ySpecCmbCameFrom.ItemsList = _counterpartyRepository.GetPlacesClientCameFrom(
				UoW,
				Entity.CameFrom == null || !Entity.CameFrom.IsArchive
			);

			ySpecCmbCameFrom.Binding
				.AddBinding(Entity, f => f.CameFrom, w => w.SelectedItem)
				.InitializeFromSource();

			ycheckIsForRetail.Binding
				.AddBinding(Entity, e => e.IsForRetail, w => w.Active)
				.InitializeFromSource();
			ycheckIsForRetail.Sensitive = CanEdit;

			ycheckNoPhoneCall.Binding
				.AddBinding(Entity, e => e.NoPhoneCall, w => w.Active)
				.InitializeFromSource();
			SetSensitivityByPermission("user_can_activate_no_phone_call_in_counterparty", ycheckNoPhoneCall);
			ycheckNoPhoneCall.Visible = Entity.IsForRetail;

			DelayDaysForBuyerValue.Binding
				.AddBinding(Entity, e => e.DelayDaysForBuyers, w => w.ValueAsInt)
				.InitializeFromSource();
			lblDelayDaysForBuyer.Visible = DelayDaysForBuyerValue.Visible = Entity?.IsChainStore ?? false;

			yspinDelayDaysForTechProcessing.Binding
				.AddBinding(Entity, e => e.TechnicalProcessingDelay, w => w.ValueAsInt)
				.InitializeFromSource();
			yspinDelayDaysForTechProcessing.Sensitive = CanEdit;

			entryFIO.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			entryFIO.Sensitive = CanEdit;

			datalegalname1.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			datalegalname1.Binding.AddSource(Entity)
				.AddBinding(s => s.Name, t => t.OwnName)
				.AddBinding(s => s.TypeOfOwnership, t => t.Ownership)
				.InitializeFromSource();

			entryFullName.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			entryFullName.Binding
				.AddBinding(Entity, e => e.FullName, w => w.Text)
				.InitializeFromSource();

			entryMainCounterparty
				.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryMainCounterparty.Binding
				.AddBinding(Entity, e => e.MainCounterparty, w => w.Subject)
				.InitializeFromSource();
			entryMainCounterparty.Sensitive = CanEdit;

			entryPreviousCounterparty
				.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory());
			entryPreviousCounterparty.Binding
				.AddBinding(Entity, e => e.PreviousCounterparty, w => w.Subject)
				.InitializeFromSource();
			entryPreviousCounterparty.Sensitive = CanEdit;

			enumPayment.ItemsEnum = typeof(PaymentType);
			enumPayment.Binding
				.AddBinding(Entity, s => s.PaymentMethod, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			enumPayment.Sensitive = CanEdit;

			enumDefaultDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDefaultDocumentType.Binding
				.AddBinding(Entity, s => s.DefaultDocumentType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			enumDefaultDocumentType.Sensitive = CanEdit;

			lblTax.Binding
				.AddFuncBinding(Entity, e => e.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			specialListCmbWorksThroughOrganization.ItemsList = UoW.GetAll<Organization>();
			specialListCmbWorksThroughOrganization.Binding
				.AddBinding(Entity, e => e.WorksThroughOrganization, w => w.SelectedItem)
				.InitializeFromSource();
			specialListCmbWorksThroughOrganization.Sensitive = _canSetWorksThroughOrganization && CanEdit;

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
			enumTax.Sensitive = CanEdit;

			spinMaxCredit.Binding
				.AddBinding(Entity, e => e.MaxCredit, w => w.ValueAsDecimal)
				.InitializeFromSource();
			SetSensitivityByPermission("max_loan_amount", spinMaxCredit);

			dataComment.Binding
				.AddBinding(Entity, e => e.Comment, w => w.Buffer.Text)
				.InitializeFromSource();
			dataComment.Editable = CanEdit;

			// Прикрепляемые документы

			var filesViewModel =
				new CounterpartyFilesViewModel(Entity, UoW, new FileDialogService(), ServicesConfig.CommonServices, _userRepository)
				{
					ReadOnly = !CanEdit
				};
			counterpartyfilesview1.ViewModel = filesViewModel;

			chkNeedNewBottles.Binding
				.AddBinding(Entity, e => e.NewBottlesNeeded, w => w.Active)
				.InitializeFromSource();
			chkNeedNewBottles.Sensitive = CanEdit;

			ycheckSpecialDocuments.Binding
				.AddBinding(Entity, e => e.UseSpecialDocFields, w => w.Active)
				.InitializeFromSource();
			ycheckSpecialDocuments.Sensitive = CanEdit;

			ycheckAlwaysSendReceitps.Binding
				.AddBinding(Entity, e => e.AlwaysSendReceipts, w => w.Active)
				.InitializeFromSource();
			ycheckAlwaysSendReceitps.Visible =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_manage_cash_receipts");
			ycheckAlwaysSendReceitps.Sensitive = CanEdit;

			ycheckExpirationDateControl.Binding
				.AddBinding(Entity, e => e.SpecialExpireDatePercentCheck, w => w.Active)
				.InitializeFromSource();
			ycheckExpirationDateControl.Sensitive = CanEdit;
			yspinExpirationDatePercent.Binding.AddSource(Entity)
				.AddBinding(e => e.SpecialExpireDatePercentCheck, w => w.Visible)
				.AddBinding(e => e.SpecialExpireDatePercent, w => w.ValueAsDecimal)
				.InitializeFromSource();
			yspinExpirationDatePercent.Sensitive = CanEdit;

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
				ytreeviewSalesChannels.Sensitive = CanEdit;
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

			buttonCloseDelivery.Sensitive = CanEdit;
			SetVisibilityForCloseDeliveryComments();
		}

		private void ConfigureTabContacts()
		{
			_phonesViewModel =
				new PhonesViewModel(_phoneRepository, UoW, _contactsParameters, _roboatsJournalsFactory, _commonServices)
			{
				PhonesList = Entity.ObservablePhones,
				Counterparty = Entity,
				ReadOnly = !CanEdit
			};
			phonesView.ViewModel = _phonesViewModel;

			emailsView.UoW = UoWGeneric;
			if(UoWGeneric.Root.Emails == null)
			{
				UoWGeneric.Root.Emails = new List<Email>();
			}

			emailsView.Emails = UoWGeneric.Root.Emails;
			emailsView.Sensitive = CanEdit;

			var employeeJournalFactory = new EmployeeJournalFactory();
			if(SetSensitivityByPermission("can_set_personal_sales_manager", entrySalesManager))
			{
				entrySalesManager.SetEntityAutocompleteSelectorFactory(GetEmployeeFactoryWithResetFilter(employeeJournalFactory));
			}

			entrySalesManager.Binding
				.AddBinding(Entity, e => e.SalesManager, w => w.Subject)
				.InitializeFromSource();

			if(SetSensitivityByPermission("can_set_personal_accountant", entryAccountant))
			{
				entryAccountant.SetEntityAutocompleteSelectorFactory(GetEmployeeFactoryWithResetFilter(employeeJournalFactory));
			}

			entryAccountant.Binding
				.AddBinding(Entity, e => e.Accountant, w => w.Subject)
				.InitializeFromSource();

			if(SetSensitivityByPermission("can_set_personal_bottles_manager", entryBottlesManager))
			{
				entryBottlesManager.SetEntityAutocompleteSelectorFactory(GetEmployeeFactoryWithResetFilter(employeeJournalFactory));
			}

			entryBottlesManager.CanEditReference = true;
			entryBottlesManager.Binding
				.AddBinding(Entity, e => e.BottlesManager, w => w.Subject)
				.InitializeFromSource();

			//FIXME данный виджет создан с Visible = false и нигде не меняется
			dataentryMainContact.RepresentationModel = new ContactsVM(UoW, Entity);
			dataentryMainContact.Binding
				.AddBinding(Entity, e => e.MainContact, w => w.Subject)
				.InitializeFromSource();

			//FIXME данный виджет создан с Visible = false и нигде не меняется
			dataentryFinancialContact.RepresentationModel = new ContactsVM(UoW, Entity);
			dataentryFinancialContact.Binding
				.AddBinding(Entity, e => e.FinancialContact, w => w.Subject)
				.InitializeFromSource();

			txtRingUpPhones.Binding
				.AddBinding(Entity, e => e.RingUpPhone, w => w.Buffer.Text)
				.InitializeFromSource();
			txtRingUpPhones.Editable = CanEdit;

			contactsview1.CounterpartyUoW = UoWGeneric;
			contactsview1.Visible = true;
			contactsview1.Sensitive = CanEdit;
		}

		private bool SetSensitivityByPermission(string permission, Widget widget)
		{
			return widget.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(permission) && CanEdit;
		}

		private IEntityAutocompleteSelectorFactory GetEmployeeFactoryWithResetFilter(IEmployeeJournalFactory employeeJournalFactory)
		{
			var filter = new EmployeeFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = EmployeeCategory.office,
				x => x.Status = EmployeeStatus.IsWorking);
			employeeJournalFactory.SetEmployeeFilterViewModel(filter);
			return employeeJournalFactory.CreateEmployeeAutocompleteSelectorFactory();
		}

		private void ConfigureTabRequisites()
		{
			validatedOGRN.ValidationMode = validatedINN.ValidationMode = validatedKPP.ValidationMode = QSWidgetLib.ValidationType.numeric;
			
			validatedOGRN.Binding
				.AddBinding(Entity, e => e.OGRN, w => w.Text)
				.InitializeFromSource();
			validatedOGRN.MaxLength = 13;
			validatedOGRN.IsEditable = CanEdit;

			validatedINN.Binding
				.AddBinding(Entity, e => e.INN, w => w.Text)
				.InitializeFromSource();
			validatedINN.IsEditable = CanEdit;

			yentrySignFIO.Binding
				.AddBinding(Entity, e => e.SignatoryFIO, w => w.Text)
				.InitializeFromSource();
			yentrySignFIO.IsEditable = CanEdit;
			validatedKPP.Binding
				.AddBinding(Entity, e => e.KPP, w => w.Text)
				.InitializeFromSource();
			validatedKPP.IsEditable = CanEdit;
			yentrySignPost.Binding
				.AddBinding(Entity, e => e.SignatoryPost, w => w.Text)
				.InitializeFromSource();
			yentrySignPost.IsEditable = CanEdit;
			entryJurAddress.Binding
				.AddBinding(Entity, e => e.RawJurAddress, w => w.Text)
				.InitializeFromSource();
			entryJurAddress.IsEditable = CanEdit;
			yentrySignBaseOf.Binding
				.AddBinding(Entity, e => e.SignatoryBaseOf, w => w.Text)
				.InitializeFromSource();
			yentrySignBaseOf.IsEditable = CanEdit;

			accountsView.CanEdit = _currentUserCanEditCounterpartyDetails && CanEdit;
			accountsView.SetAccountOwner(UoW, Entity);
		}

		private void ConfigureTabProxies()
		{
			proxiesview1.CounterpartyUoW = UoWGeneric;
			proxiesview1.Sensitive = CanEdit;
		}

		private void ConfigureTabContracts()
		{
			counterpartyContractsView.CounterpartyUoW = UoWGeneric;
			counterpartyContractsView.Sensitive = CanEdit;
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
			buttonAddTag.Sensitive = CanEdit;
			buttonDeleteTag.Sensitive = CanEdit;
		}

		private void ConfigureTabSpecialFields()
		{
			enumcomboCargoReceiverSource.ItemsEnum = typeof(CargoReceiverSource);
			enumcomboCargoReceiverSource.Binding
				.AddBinding(Entity, e => e.CargoReceiverSource, w => w.SelectedItem)
				.InitializeFromSource();
			enumcomboCargoReceiverSource.Sensitive = CanEdit;

			yentryCargoReceiver.Binding
				.AddBinding(Entity, e => e.CargoReceiver, w => w.Text)
				.InitializeFromSource();
			yentryCargoReceiver.IsEditable = CanEdit;
			yentryCustomer.Binding
				.AddBinding(Entity, e => e.SpecialCustomer, w => w.Text)
				.InitializeFromSource();
			yentryCustomer.IsEditable = CanEdit;
			yentrySpecialContract.Binding
				.AddBinding(Entity, e => e.SpecialContractNumber, w => w.Text)
				.InitializeFromSource();
			yentrySpecialContract.IsEditable = CanEdit;
			yentrySpecialKPP.Binding
				.AddBinding(Entity, e => e.PayerSpecialKPP, w => w.Text)
				.InitializeFromSource();
			yentrySpecialKPP.IsEditable = CanEdit;
			yentryGovContract.Binding
				.AddBinding(Entity, e => e.GovContract, w => w.Text)
				.InitializeFromSource();
			yentryGovContract.IsEditable = CanEdit;
			yentrySpecialDeliveryAddress.Binding
				.AddBinding(Entity, e => e.SpecialDeliveryAddress, w => w.Text)
				.InitializeFromSource();
			yentrySpecialDeliveryAddress.IsEditable = CanEdit;

			buttonLoadFromDP.Clicked += OnButtonLoadFromDeliveryPointClicked;
			buttonLoadFromDP.Sensitive = CanEdit;

			yentryOKPO.Binding
				.AddBinding(Entity, e => e.OKPO, w => w.Text)
				.InitializeFromSource();
			yentryOKPO.IsEditable = CanEdit;
			yentryOKDP.Binding
				.AddBinding(Entity, e => e.OKDP, w => w.Text)
				.InitializeFromSource();
			yentryOKDP.IsEditable = CanEdit;

			int?[] docCount = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

			yspeccomboboxTTNCount.ItemsList = docCount;
			yspeccomboboxTorg2Count.ItemsList = docCount;
			yspeccomboboxUPDForNonCashlessCount.ItemsList = docCount;

			yspeccomboboxTorg2Count.Binding
				.AddBinding(Entity, e => e.Torg2Count, w => w.SelectedItem)
				.InitializeFromSource();
			yspeccomboboxTorg2Count.Sensitive = CanEdit;
			yspeccomboboxTTNCount.Binding
				.AddBinding(Entity, e => e.TTNCount, w => w.SelectedItem)
				.InitializeFromSource();
			yspeccomboboxTTNCount.Sensitive = CanEdit;
			yspeccomboboxUPDForNonCashlessCount.Binding
				.AddBinding(Entity, e => e.UPDCount, w => w.SelectedItem)
				.InitializeFromSource();
			yspeccomboboxUPDForNonCashlessCount.Sensitive = CanEdit;

			if(Entity.IsForRetail)
			{
				yspeccomboboxUPDCount.ItemsList = docCount;
				yspeccomboboxTorg12Count.ItemsList = docCount;
				yspeccomboboxShetFacturaCount.ItemsList = docCount;
				yspeccomboboxCarProxyCount.ItemsList = docCount;

				yspeccomboboxUPDCount.Binding
					.AddBinding(Entity, e => e.AllUPDCount, w => w.SelectedItem)
					.InitializeFromSource();
				yspeccomboboxUPDCount.Sensitive = CanEdit;
				yspeccomboboxTorg12Count.Binding
					.AddBinding(Entity, e => e.Torg12Count, w => w.SelectedItem)
					.InitializeFromSource();
				yspeccomboboxTorg12Count.Sensitive = CanEdit;
				yspeccomboboxShetFacturaCount.Binding
					.AddBinding(Entity, e => e.ShetFacturaCount, w => w.SelectedItem)
					.InitializeFromSource();
				yspeccomboboxShetFacturaCount.Sensitive = CanEdit;
				yspeccomboboxCarProxyCount.Binding
					.AddBinding(Entity, e => e.CarProxyCount, w => w.SelectedItem)
					.InitializeFromSource();
				yspeccomboboxCarProxyCount.Sensitive = CanEdit;
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
			ytreeviewSpecialNomenclature.Sensitive = CanEdit;

			ybuttonAddNom.Sensitive = CanEdit;
			ybuttonRemoveNom.Sensitive = CanEdit;
		}

		private void ConfigureTabDeliveryPoints()
		{
			deliveryPointsManagementView.Counterparty = Entity;
		}

		private void ConfigureTabDocuments()
		{
			counterpartydocumentsview.Config(UoWGeneric, Entity);
			counterpartydocumentsview.Sensitive = CanEdit;
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
					CounterpartySelectorFactory,
					new NomenclatureJournalFactory(),
					NomenclatureRepository,
					_userRepository);
			supplierPricesWidget.Sensitive = CanEdit;
		}

		private void ConfigureTabFixedPrices()
		{
			var waterFixedPricesGenerator = new WaterFixedPricesGenerator(NomenclatureRepository);
			var nomenclatureFixedPriceFactory = new NomenclatureFixedPriceFactory();
			var fixedPriceController = new NomenclatureFixedPriceController(nomenclatureFixedPriceFactory, waterFixedPricesGenerator);
			var fixedPricesModel = new CounterpartyFixedPricesModel(UoW, Entity, fixedPriceController);
			var nomSelectorFactory = new NomenclatureJournalFactory();
			FixedPricesViewModel fixedPricesViewModel = new FixedPricesViewModel(UoW, fixedPricesModel, nomSelectorFactory, this);
			fixedpricesview.ViewModel = fixedPricesViewModel;
			SetSensitivityByPermission("can_edit_counterparty_fixed_prices", fixedpricesview);
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

		private void ConfigureTabEmails()
		{
			if(EmailDataLoader != null)
			{
				return;
			}

			_emailLastScrollPosition = 0;
			EmailDataLoader = new ThreadDataLoader<EmailRow>(UnitOfWorkFactory.GetDefaultFactory) { PageSize = 50 };
			EmailDataLoader.AddQuery(EmailItemsSourceQueryFunction);

			ytreeviewEmails.ColumnsConfig = FluentColumnsConfig<EmailRow>.Create()
				.AddColumn("Дата отправки").AddTextRenderer(x => x.Date.ToString())
				.AddColumn("Тип письма").AddTextRenderer(x => x.Type.GetEnumTitle())
				.AddColumn("Статус").AddTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Тема письма").AddTextRenderer(x => x.Subject)
				.Finish();

			EmailDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Application.Invoke((s, arg) =>
				{
					ytreeviewEmails.ItemsDataSource = EmailDataLoader.Items;
					GtkHelper.WaitRedraw();
					ytreeviewEmails.Vadjustment.Value = _emailLastScrollPosition;
				});
			};

			ytreeviewEmails.Vadjustment.ValueChanged += (sender, args) =>
			{
				if(ytreeviewEmails.Vadjustment.Value + ytreeviewEmails.Vadjustment.PageSize < ytreeviewEmails.Vadjustment.Upper)
				{
					return;
				}

				if(EmailDataLoader.HasUnloadedItems)
				{
					_emailLastScrollPosition = ytreeviewEmails.Vadjustment.Value;
					EmailDataLoader.LoadData(true);
				}
			};

			ytreeviewEmails.ItemsDataSource = EmailDataLoader.Items;

			EmailDataLoader.LoadData(false);

			RefreshBulkEmailEventStatus();
		}

		private void RefreshBulkEmailEventStatus()
		{
			var lastBulkEmailEvent = _emailRepository.GetLastBulkEmailEvent(UoW, Entity.Id);

			if(lastBulkEmailEvent == null || lastBulkEmailEvent is SubscribingBulkEmailEvent)
			{
				ylabelBulkEmailEventDate.LabelProp = "Контрагент подписан на массовую рассылку";
				ybuttonSubscribe.Visible = false;
				ybuttonUnsubscribe.Visible = true;
				return;
			}

			ylabelBulkEmailEventDate.LabelProp = lastBulkEmailEvent.ActionTime.ToString();
			ybuttonSubscribe.Visible = true;
			ybuttonUnsubscribe.Visible = false;
		}

		private Func<IUnitOfWork, IQueryOver<CounterpartyEmail>> EmailItemsSourceQueryFunction => (uow) =>
		{
			CounterpartyEmail counterpartyEmailAlias = null;
			StoredEmail storedEmailAlias = null;
			EmailRow resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => counterpartyEmailAlias)
				.JoinAlias(() => counterpartyEmailAlias.StoredEmail, () => storedEmailAlias)
				.Where(() => counterpartyEmailAlias.Counterparty.Id == Entity.Id);

			itemsQuery
				.SelectList(list => list
					.Select(()=> storedEmailAlias.SendDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyEmailAlias.Type).WithAlias(() => resultAlias.Type)
					.Select(() => storedEmailAlias.Subject).WithAlias(() => resultAlias.Subject)
					.Select(() => storedEmailAlias.State).WithAlias(() => resultAlias.State)
				)
				.OrderBy(() => storedEmailAlias.SendDate).Desc
				.TransformUsing(Transformers.AliasToBean<EmailRow>());

			return itemsQuery;
		};

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

		private void OnButtonLoadFromDeliveryPointClicked(object sender, EventArgs e)
		{
			var filter = new DeliveryPointJournalFilterViewModel
			{
				Counterparty = Entity
			};
			var dpFactory = new DeliveryPointJournalFactory(filter);
			var dpJournal = dpFactory.CreateDeliveryPointByClientJournal();
			dpJournal.SelectionMode = JournalSelectionMode.Single;
			dpJournal.OnEntitySelectedResult += OnDeliveryPointJournalEntitySelected;
			TabParent.AddSlaveTab(this, dpJournal);
		}

		private void OnDeliveryPointJournalEntitySelected(object sender, JournalSelectedNodesEventArgs e)
		{
			if(e.SelectedNodes.FirstOrDefault() is DeliveryPointByClientJournalNode node)
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
				new NomenclatureJournalFactory(),
				new UndeliveredOrdersRepository(),
				new SubdivisionRepository(new ParametersProvider()),
				new FileDialogService()
			);

			TabParent.AddTab(orderJournalViewModel, this, false);
		}

		private void ComplaintViewOnActivated(object sender, EventArgs e)
		{
			ISubdivisionJournalFactory subdivisionJournalFactory = new SubdivisionJournalFactory();

			var filter = new ComplaintFilterViewModel(
				ServicesConfig.CommonServices, SubdivisionRepository, new EmployeeJournalFactory(), CounterpartySelectorFactory);
			filter.SetAndRefilterAtOnce(x => x.Counterparty = Entity);

			var complaintsJournalViewModel = new ComplaintsJournalViewModel(
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				UndeliveredOrdersJournalOpener,
				_employeeService,
				CounterpartySelectorFactory,
				RouteListItemRepository,
				_subdivisionParametersProvider,
				filter,
				FilePickerService,
				SubdivisionRepository,
				new GtkTabsOpener(),
				NomenclatureRepository,
				_userRepository,
				new OrderSelectorFactory(),
				new EmployeeJournalFactory(),
				new CounterpartyJournalFactory(),
				new DeliveryPointJournalFactory(),
				subdivisionJournalFactory,
				new SalesPlanJournalFactory(),
				new NomenclatureJournalFactory(),
				new EmployeeSettings(new ParametersProvider()),
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
			btnCancel.Sensitive = isSensetive;
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
				_phonesViewModel.RemoveEmpty();
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

		protected void OnRadioEmailsToggled(object sender, EventArgs e)
		{
			if(rbnEmails.Active)
			{
				notebook1.CurrentPage = 11;
				ConfigureTabEmails();
			}
		}

		private void OnEnumCounterpartyTypeChanged(object sender, EventArgs e)
		{
			rbnPrices.Visible = Entity.CounterpartyType == CounterpartyType.Supplier;
			validatedOGRN.Visible = labelOGRN.Visible = HasOgrn;
			if (Entity.CounterpartyType == CounterpartyType.Dealer)
			{
				Entity.PersonType = PersonType.legal;
			}
		}

		private void OnEnumCounterpartyTypeChangedByUser(object sender, EventArgs e)
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
			if(!CanEdit)
			{
				return;
			}
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
			if(!CanEdit)
			{
				return;
			}
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

			if(permissionResult.CanUpdate)
			{
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
			else
			{
				buttonSaveCloseComment.Sensitive = false;
				buttonEditCloseDeliveryComment.Sensitive = false;
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

		protected void OnButtonUnsubscribeClicked(object sender, EventArgs e)
		{
			var unsubscribingReason = _emailRepository.GetBulkEmailEventOperatorReason(UoW, _emailParametersProvider);

			var unsubscribingEvent = new UnsubscribingBulkEmailEvent
			{
				Reason = unsubscribingReason,
				ReasonDetail = CurrentEmployee.GetPersonNameWithInitials(),
				Counterparty = Entity
			};

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Сохранение отписки от массовой рассылки"))
			{
				unitOfWork.Save(unsubscribingEvent);
				unitOfWork.Commit();
			}

			RefreshBulkEmailEventStatus();
		}

		protected void OnButtonSubscribeClicked(object sender, EventArgs e)
		{
			var subscribingReason = _emailRepository.GetBulkEmailEventOperatorReason(UoW, _emailParametersProvider);

			var subscribingEvent = new SubscribingBulkEmailEvent
			{
				Reason = subscribingReason,
				ReasonDetail = CurrentEmployee.GetPersonNameWithInitials(),
				Counterparty = Entity
			};

			using(var unitOfWork = UnitOfWorkFactory.CreateWithoutRoot("Сохранение подписки на массовую рассылку"))
			{
				unitOfWork.Save(subscribingEvent);
				unitOfWork.Commit();
			}

			RefreshBulkEmailEventStatus();
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

	public class EmailRow
	{
		public DateTime Date { get; set; }
		public CounterpartyEmailType Type { get; set; }
		public string Subject { get; set; }
		public StoredEmailStates State { get; set; }
	}
}
