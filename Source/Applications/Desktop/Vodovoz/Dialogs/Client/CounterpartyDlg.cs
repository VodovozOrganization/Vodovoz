using Autofac;
using EdoService.Library;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using NHibernate.Transform;
using NLog;
using QS.Banks.Domain;
using QS.Banks.Repositories;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.Utilities;
using QS.Utilities.Text;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using QSOrmProject;
using QSProjectsLib;
using RevenueService.Client;
using RevenueService.Client.Dto;
using RevenueService.Client.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using QS.Commands;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Application.FileStorage;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.Nodes;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Client.ClientClassification;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Retail;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using Vodovoz.Models;
using Vodovoz.Models.TrueMark;
using Vodovoz.Nodes;
using Vodovoz.Presentation.ViewModels.AttachedFiles;
using Vodovoz.Services;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Contacts;
using Vodovoz.Settings.Counterparty;
using Vodovoz.Settings.Edo;
using Vodovoz.Settings.Organizations;
using Vodovoz.Settings.Roboats;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.ViewModel;
using Vodovoz.ViewModels.Counterparties;
using Vodovoz.ViewModels.Dialogs.Complaints;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Contacts;
using Vodovoz.ViewModels.ViewModels.Counterparty;
using Vodovoz.ViewModels.ViewModels.Goods;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.Views.Client;
using VodovozBusiness.Controllers;
using VodovozBusiness.EntityRepositories.Counterparties;
using VodovozBusiness.EntityRepositories.Edo;
using VodovozBusiness.Nodes;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;

namespace Vodovoz
{
	public partial class CounterpartyDlg : QS.Dialog.Gtk.EntityDialogBase<Counterparty>, ICounterpartyInfoProvider, ITDICloseControlTab,
		IAskSaveOnCloseViewModel, INotifyPropertyChanged
	{
		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

		private readonly bool _canSetWorksThroughOrganization =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_organization_from_order_and_counterparty");
		private readonly bool _canEditClientRefer =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.CounterpartyPermissions.CanEditClientRefer);
		private readonly int _currentUserId = ServicesConfig.UserService.CurrentUserId;
		private readonly IEmployeeService _employeeService = ScopeProvider.Scope.Resolve<IEmployeeService>();
		private readonly IValidationContextFactory _validationContextFactory = new ValidationContextFactory();
		private readonly ICounterpartyRepository _counterpartyRepository = ScopeProvider.Scope.Resolve<ICounterpartyRepository>();
		private readonly IPhoneRepository _phoneRepository = ScopeProvider.Scope.Resolve<IPhoneRepository>();
		private readonly IEmailRepository _emailRepository = ScopeProvider.Scope.Resolve<IEmailRepository>();
		private readonly IOrganizationRepository _organizationRepository = ScopeProvider.Scope.Resolve<IOrganizationRepository>();
		private readonly IExternalCounterpartyRepository _externalCounterpartyRepository = ScopeProvider.Scope.Resolve<IExternalCounterpartyRepository>();
		private readonly IEdoDocflowRepository _edoDocflowRepository = ScopeProvider.Scope.Resolve<IEdoDocflowRepository>();
		private readonly IContactSettings _contactsSettings = ScopeProvider.Scope.Resolve<IContactSettings>();
		private readonly ICommonServices _commonServices = ServicesConfig.CommonServices;
		private readonly IInteractiveService _interactiveService = ServicesConfig.InteractiveService;
		private IExternalCounterpartyController _externalCounterpartyController;
		private RoboatsJournalsFactory _roboatsJournalsFactory;
		private IEdoOperatorsJournalFactory _edoOperatorsJournalFactory;
		private IEmailSettings _emailSettings;
		private ICounterpartyJournalFactory _counterpartySelectorFactory;
		private ValidationContext _validationContext;
		private Employee _currentEmployee;
		private PhonesViewModel _phonesViewModel;
		private double _emailLastScrollPosition;
		private CounterpartyEdoAccountsViewModel _counterpartyEdoAccountsViewModel;
		private ICounterpartyEdoAccountController _counterpartyEdoAccountController;
		private ITrueMarkApiClient _trueMarkApiClient;
		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private CancellationTokenSource _cancellationTokenCheckLiquidationSource = new CancellationTokenSource();
		private IEdoSettings _edoSettings;
		private ICounterpartySettings _counterpartySettings;
		private IOrganizationSettings _organizationSettings = ScopeProvider.Scope.Resolve<IOrganizationSettings>();
		private IRevenueServiceClient _revenueServiceClient;
		private ICounterpartyService _counterpartyService;
		private IDeleteEntityService _deleteEntityService;
		private ICurrentPermissionService _currentPermissionService;
		private IEdoService _edoService;
		private IAttachedFileInformationsViewModelFactory _attachmentsViewModelFactory;
		private ICounterpartyFileStorageService _counterpartyFileStorageService;
		private IGeneralSettings _generalSettings;
		private IObservableList<EdoDockflowData> _edoEdoDocumentDataNodes = new ObservableList<EdoDockflowData>();
		private IObservableList<EdoContainer> _edoContainers = new ObservableList<EdoContainer>();
		private GenericObservableList<ExternalCounterpartyNode> _externalCounterparties;
		private IObservableList<ConnectedCustomerInfoNode> _connectedCustomers = new ObservableList<ConnectedCustomerInfoNode>();
		private IConnectedCustomerRepository _connectedCustomerRepository;
		private IPhoneTypeSettings _phoneTypeSettings;

		private bool _currentUserCanEditCounterpartyDetails = false;
		private bool _deliveryPointsConfigured = false;
		private bool _documentsConfigured = false;
		private Organization _vodovozOrganization;
		private ConnectedCustomerInfoNode _selectedConnectedCustomer;

		public ThreadDataLoader<EmailRow> EmailDataLoader { get; private set; }

		public virtual ICounterpartyJournalFactory CounterpartySelectorFactory =>
			_counterpartySelectorFactory ?? (_counterpartySelectorFactory = new CounterpartyJournalFactory());

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
		public event PropertyChangedEventHandler PropertyChanged;

		public PanelViewType[] InfoWidgets => new[] { PanelViewType.CounterpartyView };

		public Counterparty Counterparty => UoWGeneric.Root;

		public bool HasOgrn => Counterparty.CounterpartyType == CounterpartyType.Dealer;

		private bool CanEdit => permissionResult.CanUpdate || permissionResult.CanCreate && Entity.Id == 0;

		public override bool HasChanges
		{
			get
			{
				_phonesViewModel.RemoveEmpty();
				emailsView.ViewModel.RemoveEmpty();
				return base.HasChanges;
			}
			set => base.HasChanges = value;
		}

		#region IAskSaveOnCloseViewModel

		public bool AskSaveOnClose => CanEdit;

		#endregion

		public CounterpartyDlg()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();
			ConfigureDlg();
		}

		public CounterpartyDlg(int id)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Counterparty>(id);
			ConfigureDlg();
		}

		public CounterpartyDlg(Counterparty sub) : this(sub.Id)
		{
		}

		public CounterpartyDlg(NewCounterpartyParameters parameters)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();

			Entity.Name = parameters.Name;
			Entity.FullName = parameters.FullName;
			Entity.INN = parameters.INN;
			Entity.KPP = parameters.KPP;
			Entity.PaymentMethod = parameters.PaymentMethod;
			Entity.TypeOfOwnership = parameters.TypeOfOwnership;
			Entity.PersonType = parameters.PersonType;

			if(!(string.IsNullOrWhiteSpace(parameters.CounterpartyCorrespondentAcc)
				|| string.IsNullOrWhiteSpace(parameters.CounterpartyCurrentAcc)
				|| string.IsNullOrWhiteSpace(parameters.CounterpartyBank)
				|| string.IsNullOrWhiteSpace(parameters.CounterpartyBik)))
			{
				var bank = FillBank(parameters);
				var account = new Account { Number = parameters.CounterpartyCurrentAcc, InBank = bank };
				Entity.AddAccount(account);
			}

			ConfigureDlg();
		}

		public CounterpartyDlg(IEntityUoWBuilder uowBuilder, IUnitOfWorkFactory unitOfWorkFactory)
		{
			Build();
			UoWGeneric = uowBuilder.CreateUoW<Counterparty>(unitOfWorkFactory);
			ConfigureDlg();
		}

		public CounterpartyDlg(Phone phone)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Counterparty>();
			phone.Counterparty = Entity;
			Entity.Phones.Add(phone);
			ConfigureDlg();
		}
		
		private DelegateCommand ActivateConnectConnectedCustomerCommand { get; set; }
		private DelegateCommand BlockConnectConnectedCustomerCommand { get; set; }

		private ConnectedCustomerInfoNode SelectedConnectedCustomer
		{
			get => _selectedConnectedCustomer;
			set
			{
				_selectedConnectedCustomer = value;
				UpdateStateConnectButtons();
			}
		}

		private Employee CurrentEmployee =>
			_currentEmployee ?? (_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _currentUserId));

		public string IsLiquidatingLabelText => (Entity?.IsLiquidating ?? false) ? $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">Ликвидирован по данным ФНС</span>" : "Ликвидирован по данным ФНС";

		private void ConfigureDlg()
		{
			var roboatsSettings = _lifetimeScope.Resolve<IRoboatsSettings>();
			_edoSettings = _lifetimeScope.Resolve<IEdoSettings>();
			_counterpartySettings = _lifetimeScope.Resolve<ICounterpartySettings>();
			_counterpartyService = _lifetimeScope.Resolve<ICounterpartyService>();
			_deleteEntityService = _lifetimeScope.Resolve<IDeleteEntityService>();
			_currentPermissionService = _lifetimeScope.Resolve<ICurrentPermissionService>();
			_edoService = _lifetimeScope.Resolve<IEdoService>();
			_attachmentsViewModelFactory = _lifetimeScope.Resolve<IAttachedFileInformationsViewModelFactory>();
			_counterpartyFileStorageService = _lifetimeScope.Resolve<ICounterpartyFileStorageService>();
			_counterpartyEdoAccountController = _lifetimeScope.Resolve<ICounterpartyEdoAccountController>();
			_generalSettings = _lifetimeScope.Resolve<IGeneralSettings>();

			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			var fileDialogService = new FileDialogService();
			var roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			_roboatsJournalsFactory = new RoboatsJournalsFactory(ServicesConfig.UnitOfWorkFactory, ServicesConfig.CommonServices, roboatsViewModelFactory, NavigationManager, _deleteEntityService, _currentPermissionService);
			_edoOperatorsJournalFactory = new EdoOperatorsJournalFactory(ServicesConfig.UnitOfWorkFactory);
			_emailSettings = _lifetimeScope.Resolve<IEmailSettings>();
			_trueMarkApiClient = _lifetimeScope.Resolve<ITrueMarkApiClient>();

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

			_vodovozOrganization = UoW.GetById<Organization>(_organizationSettings.VodovozOrganizationId);

			ConfigureTabInfo();
			ConfigureTabContacts();
			ConfigureTabProxies();
			ConfigureTabContracts();
			ConfigureTabRequisites();
			ConfigureTabTags();
			ConfigureTabSpecialFields();
			ConfigureTabPrices();
			ConfigureTabFixedPrices();
			CongigureTabEdo();
			ConfigureValidationContext();
			ConfigureTabEdoContainers();

			//make actions menu
			var menu = new Gtk.Menu();

			var menuItem = new Gtk.MenuItem("Все заказы контрагента");
			menuItem.Activated += OnAllCounterpartyOrdersActivated;
			menu.Add(menuItem);

			var menuItemFixedPrices = new Gtk.MenuItem("Фикс. цены для самовывоза");
			menuItemFixedPrices.Activated += (s, e) => OpenFixedPrices();
			menu.Add(menuItemFixedPrices);

			var menuComplaint = new Gtk.MenuItem("Рекламации контрагента");
			menuComplaint.Activated += OnCounterpartyComplaintsActivated;
			menu.Add(menuComplaint);

			menuActions.Menu = menu;
			menu.ShowAll();

			menuActions.Sensitive = !UoWGeneric.IsNew;

			datatable4.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;

			Entity.PropertyChanged += OnEntityPropertyChanged;

			ConfigureClientReferEntityEntry();
			ConfigureDelayDaysFromGeneralSettings();
		}

		private void InitializeEdoAccountsWidget()
		{
			_counterpartyEdoAccountController.AddDefaultEdoAccountsToCounterparty(Entity);
			
			_counterpartyEdoAccountsViewModel = _lifetimeScope.Resolve<CounterpartyEdoAccountsViewModel>(
				new TypedParameter(typeof(IUnitOfWork), UoW),
				new TypedParameter(typeof(Counterparty), Entity),
				new TypedParameter(typeof(ITdiTab), this)
			);
			
			var accountsView = new CounterpartyEdoAccountsView(_counterpartyEdoAccountsViewModel);
			vboxEdoAccounts.Add(accountsView);
			accountsView.Show();
		}

		private void ConfigureClientReferEntityEntry()
		{
			var builder = new LegacyEEVMBuilderFactory<Counterparty>(
				this,
				Entity,
				UoW,
				Startup.MainWin.NavigationManager,
				_lifetimeScope);


			entityentryClientRefer.ViewModel = builder.ForProperty(x => x.Referrer)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
			entityentryClientRefer.ViewModel.DisposeViewModel = false;
		}

		private void ConfigureDelayDaysFromGeneralSettings()
		{
			if(Entity.Id != 0)
			{
				return;
			}

			Entity.DelayDaysForBuyers = _generalSettings.DefaultPaymentDeferment;
		}
		
		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Entity.SalesManager)
				|| e.PropertyName == nameof(Entity.Accountant)
				|| e.PropertyName == nameof(Entity.BottlesManager))
			{
				CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
				return;
			}

			if(e.PropertyName == nameof(Entity.CounterpartyType))
			{
				if(Entity.CounterpartyType != CounterpartyType.AdvertisingDepartmentClient)
				{
					Entity.CounterpartySubtype = null;

					return;
				}

				if(Entity.CounterpartySubtype is null)
				{
					var barterSubtype = UoW.GetById<CounterpartySubtype>(1);

					Entity.CounterpartySubtype = barterSubtype;
				}

				return;
			}

			if(e.PropertyName == nameof(Entity.IsLiquidating))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLiquidatingLabelText)));
			}

			if(e.PropertyName == nameof(Entity.CameFrom) && Entity.CameFrom?.Id != _counterpartySettings.ReferFriendPromotionCameFromId)
			{
				Entity.Referrer = null;
			}
			
			if(e.PropertyName == nameof(Entity.PersonType))
			{
				OnPersonTypeChanged();
			}
		}

		private void ConfigureTabInfo()
		{
			ycheckbuttonIsLiquidating.Binding
				.AddBinding(Entity, e => e.IsLiquidating, w => w.Active)
				.AddFuncBinding(c => c.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			labelIsLiquidating.UseMarkup = true;
			labelIsLiquidating.Binding
				.AddBinding(this, dlg => dlg.IsLiquidatingLabelText, w => w.LabelProp)
				.AddFuncBinding(dlg => dlg.Entity.PersonType == PersonType.legal, w => w.Visible)
				.InitializeFromSource();

			enumPersonType.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			enumPersonType.ItemsEnum = typeof(PersonType);
			enumPersonType.Binding.AddBinding(Entity, s => s.PersonType, w => w.SelectedItemOrNull).InitializeFromSource();
			enumPersonType.ChangedByUser += OnEnumPersonTypeChangedByUser;

			yEnumCounterpartyType.ItemsEnum = typeof(CounterpartyType);
			yEnumCounterpartyType.Binding
				.AddBinding(Entity, c => c.CounterpartyType, w => w.SelectedItemOrNull)
				.InitializeFromSource();
			yEnumCounterpartyType.Sensitive = CanEdit;
			yEnumCounterpartyType.Changed += OnEnumCounterpartyTypeChanged;
			yEnumCounterpartyType.ChangedByUser += OnEnumCounterpartyTypeChangedByUser;
			OnEnumCounterpartyTypeChanged(this, EventArgs.Empty);

			var vm = new LegacyEEVMBuilderFactory<Counterparty>(this, Entity, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.CounterpartySubtype)
				.UseViewModelJournalAndAutocompleter<SubtypesJournalViewModel>()
				.UseViewModelDialog<SubtypeViewModel>()
				.Finish();

			SubtypeEntryViewModel = vm;

			entryCounterpartySubtype.ViewModel = SubtypeEntryViewModel;

			yhboxCounterpartySubtype.Binding
				.AddFuncBinding<Counterparty>(
					Entity,
					e => e.CounterpartyType == CounterpartyType.AdvertisingDepartmentClient,
					w => w.Visible)
				.InitializeFromSource();

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

			lblVodovozNumber.Visible = false;

			hboxCameFrom.Visible = (Entity.Id != 0 && Entity.CameFrom != null) || Entity.Id == 0 || _canEditClientRefer;
			
			
			
			yhboxReferrer.Binding.AddSource(Entity)
				.AddFuncBinding(e => 
						(e.CameFrom != null && e.CameFrom.Id == _counterpartySettings.ReferFriendPromotionCameFromId),
					w => w.Visible)
				.AddFuncBinding(e =>
						(e.Id == 0 && CanEdit)
						|| _canEditClientRefer,
					w => w.Sensitive)
				.InitializeFromSource();

			ySpecCmbCameFrom.SetRenderTextFunc<ClientCameFrom>(f => f.Name);

			ySpecCmbCameFrom.Sensitive = (Entity.Id == 0 && CanEdit) || _canEditClientRefer;
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

			ycheckIsForSalesDepartment.Binding
				.AddBinding(Entity, e => e.IsForSalesDepartment, w => w.Active)
				.InitializeFromSource();
			ycheckIsForSalesDepartment.Sensitive = CanEdit;

			ycheckNoPhoneCall.Binding
				.AddBinding(Entity, e => e.NoPhoneCall, w => w.Active)
				.InitializeFromSource();
			SetSensitivityByPermission("user_can_activate_no_phone_call_in_counterparty", ycheckNoPhoneCall);
			ycheckNoPhoneCall.Visible = Entity.IsForRetail;

			DelayDaysForBuyerValue.Binding
				.AddBinding(Entity, e => e.DelayDaysForBuyers, w => w.ValueAsInt)
				.InitializeFromSource();
			
			yspinDelayDaysForTechProcessing.Binding
				.AddBinding(Entity, e => e.TechnicalProcessingDelay, w => w.ValueAsInt)
				.InitializeFromSource();
			yspinDelayDaysForTechProcessing.Sensitive = CanEdit;

			entryFIO.Binding
				.AddBinding(Entity, e => e.Name, w => w.Text)
				.InitializeFromSource();
			entryFIO.Sensitive = false;

			yhboxPersonFullName.Binding
				.AddFuncBinding(Entity, e => e.TypeOfOwnership == "ИП" || e.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();

			ylabelPersonFullName.Binding
				.AddFuncBinding(Entity, e => e.TypeOfOwnership == "ИП" || e.PersonType == PersonType.natural, w => w.Visible)
				.InitializeFromSource();

			yentrySurname.Binding
				.AddBinding(Entity, e => e.Surname, w => w.Text)
				.InitializeFromSource();

			yentryFirstName.Binding
				.AddBinding(Entity, e => e.FirstName, w => w.Text)
				.InitializeFromSource();

			yentryPatronymic.Binding
				.AddBinding(Entity, e => e.Patronymic, w => w.Text)
				.InitializeFromSource();

			yentrySurname.Changed += OnEntryPersonNamePartChanged;
			yentryFirstName.Changed += OnEntryPersonNamePartChanged;
			yentryPatronymic.Changed += OnEntryPersonNamePartChanged;

			comboboxOpf.Sensitive = (_currentUserCanEditCounterpartyDetails || Entity.TypeOfOwnership is null) && CanEdit;
			FillComboboxOpf();
			comboboxOpf.Changed += ComboboxOpfChanged;

			yentryOrganizationName.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			yentryOrganizationName.Binding.AddBinding(Entity, s => s.Name, t => t.Text).InitializeFromSource();
			yentryOrganizationName.Binding.AddFuncBinding(Entity, s => s.TypeOfOwnership != "ИП", t => t.Sensitive).InitializeFromSource();

			entryFullName.Sensitive = _currentUserCanEditCounterpartyDetails && CanEdit;
			entryFullName.Binding
				.AddBinding(Entity, e => e.FullName, w => w.Text)
				.AddFuncBinding(s => s.TypeOfOwnership != "ИП", w => w.Sensitive)
				.InitializeFromSource();

			entryMainCounterparty
				.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
			entryMainCounterparty.Binding
				.AddBinding(Entity, e => e.MainCounterparty, w => w.Subject)
				.InitializeFromSource();
			entryMainCounterparty.Sensitive = CanEdit;

			entryPreviousCounterparty
				.SetEntityAutocompleteSelectorFactory(CounterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(_lifetimeScope));
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

			specialListCmbWorksThroughOrganization.ItemSelected += (s, e) =>
			{
				Entity.OurOrganizationAccountForBills = null;
				UpdateOurOrganizationSpecialAccountItemList();
			};

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

			_attachedFileInformationsViewModel = _attachmentsViewModelFactory.CreateAndInitialize<Counterparty, CounterpartyFileInformation>(
				UoW,
				Entity,
				_counterpartyFileStorageService,
				_cancellationTokenSource.Token,
				Entity.AddFileInformation,
				Entity.RemoveFileInformation);

			_attachedFileInformationsViewModel.ReadOnly = !CanEdit;

			smallfileinformationsview.ViewModel = _attachedFileInformationsViewModel;

			chkNeedNewBottles.Binding
				.AddBinding(Entity, e => e.NewBottlesNeeded, w => w.Active)
				.InitializeFromSource();
			chkNeedNewBottles.Sensitive = CanEdit;

			ycheckSpecialDocuments.Binding
				.AddBinding(Entity, e => e.UseSpecialDocFields, w => w.Active)
				.InitializeFromSource();
			ycheckSpecialDocuments.Sensitive = CanEdit;

			ycheckAlwaysPrintInvoice.Binding
				.AddBinding(Entity, e => e.AlwaysPrintInvoice, w => w.Active)
				.InitializeFromSource();
			ycheckAlwaysPrintInvoice.Sensitive = CanEdit;

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

			ycheckRoboatsExclude.Binding
				.AddBinding(Entity, e => e.RoboatsExclude, w => w.Active)
				.InitializeFromSource();

			ycheckExcludeFromAutoCalls.Binding
				.AddBinding(Entity, e => e.ExcludeFromAutoCalls, w => w.Active)
				.InitializeFromSource();

			ycheckHideDeliveryPointInBill.Binding
				.AddBinding(Entity, e => e.HideDeliveryPointForBill, w => w.Active)
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

			SetVisibilityForCloseDeliveryComments();
			UpdateCounterpartyClassificationValues();

			logisticsRequirementsView.ViewModel = new LogisticsRequirementsViewModel(Entity.LogisticsRequirements ?? new LogisticsRequirements(), _commonServices);
			logisticsRequirementsView.ViewModel.Entity.PropertyChanged += OnLogisticsRequirementsSelectionChanged;
		}

		private void OnPersonTypeChanged()
		{
			if(Entity.Id != 0)
			{
				return;
			}

			Entity.DelayDaysForBuyers = Entity.PersonType == PersonType.legal ? 7 : 0;
		}

		private void UpdateCounterpartyClassificationValues()
		{
			var classification =
				(from c in UoW.GetAll<CounterpartyClassification>()
				 join s in UoW.GetAll<CounterpartyClassificationCalculationSettings>()
				 on c.ClassificationCalculationSettingsId equals s.Id
				 where c.CounterpartyId == Entity.Id
				 orderby c.Id descending
				 select new
				 {
					 ClassificationValue = $"{c.ClassificationByBottlesCount}{c.ClassificationByOrdersCount}",
					 BottlesCount = $"{c.BottlesPerMonthAverageCount} бут/мес",
					 TurnoverSum = $"{c.MoneyTurnoverPerMonthAverageSum} руб/мес",
					 OrdersCount = $"{c.OrdersPerMonthAverageCount} зак/мес",
					 CalculationDate = $"{s.SettingsCreationDate:dd.MM.yyyy}"
				 })
				 .FirstOrDefault();

			ylabelClassificationValue.Text = 
				$"{classification?.ClassificationValue ?? "Новый"}";

			ylabelClassificationBottlesCount.Text = 
				$"Кол-во бут. 19л: {classification?.BottlesCount ?? "не рассчитывалось"}";

			ylabelClassificationTurnoverSum.Text = 
				$"Оборот (инфо): {classification?.TurnoverSum ?? "не рассчитывалось"}";

			ylabelClassificationOrdersCount.Text = 
				$"Частота покупок: {classification?.OrdersCount ?? "не рассчитывалось"}";

			ylabelClassificationCalculationDate.Text =
				$"Дата последнего пересчёта: {classification?.CalculationDate ?? "не рассчитывалось"}";
		}

		private void ConfigureTabContacts()
		{
			_phonesViewModel =
				new PhonesViewModel(
					_commonServices,
					_phoneRepository,
					UoW,
					_contactsSettings,
					_phoneTypeSettings,
					_roboatsJournalsFactory,
					_externalCounterpartyController)
				{
					PhonesList = Entity.ObservablePhones,
					Counterparty = Entity,
					ReadOnly = !CanEdit
				};
			phonesView.ViewModel = _phonesViewModel;

			var emailsViewModel = new EmailsViewModel(
				UoWGeneric,
				Entity.Emails,
				_emailSettings,
				_externalCounterpartyRepository,
				_commonServices.InteractiveService,
				Entity.PersonType);
			emailsView.ViewModel = emailsViewModel;
			emailsView.Sensitive = CanEdit;

			var employeeJournalFactory = _lifetimeScope.Resolve<IEmployeeJournalFactory>();
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
			
			ConfigureTreeExternalCounterparties();
			ConfigureConnectedCustomers();
		}

		private void ConfigureTreeExternalCounterparties()
		{
			GetExternalCounterparties();

			treeExternalCounterparties.ColumnsConfig = FluentColumnsConfig<ExternalCounterpartyNode>.Create()
				.AddColumn("Id внешнего пользователя")
				.AddTextRenderer(node => node.ExternalCounterpartyId.ToString())
				.AddColumn("Номер телефона")
				.AddTextRenderer(node => node.Phone)
				.AddColumn("Откуда")
				.AddTextRenderer(node => node.CounterpartyFrom.GetEnumDisplayName(false))
				.Finish();
			
			treeExternalCounterparties.ItemsDataSource = _externalCounterparties;
		}

		private void ConfigureConnectedCustomers()
		{
			lblConnectedCustomers.LabelProp = Entity.PersonType == PersonType.natural
				? "Клиенты, от имени которых может заказывать в ИПЗ это физическое лицо:"
				: "Клиенты, которые могут заказывать в ИПЗ на это юр лицо:";

			ConfigureTreeConnectedCustomers();

			ActivateConnectConnectedCustomerCommand = new DelegateCommand(
				() =>
				{
					var connectedCustomer = SelectedConnectedCustomer.ConnectedCustomer;
					connectedCustomer.ActivateConnect();
					UpdateStateConnectButtons();
					treeViewConnectedCustomers.QueueDraw();
				},
				() => SelectedConnectedCustomer != null
				      && SelectedConnectedCustomer.ConnectedCustomer.ConnectState != ConnectedCustomerConnectState.Active);
			
			BlockConnectConnectedCustomerCommand = new DelegateCommand(
				() =>
				{
					var connectedCustomer = SelectedConnectedCustomer.ConnectedCustomer;

					if(string.IsNullOrWhiteSpace(connectedCustomer.BlockingReason))
					{
						MessageDialogHelper.RunWarningDialog("Прежде чем блокировать связь, нужно заполнить причину блокировки!");
						return;
					}
					connectedCustomer.BlockConnect();
					UpdateStateConnectButtons();
					treeViewConnectedCustomers.YTreeModel.EmitModelChanged();
				},
				() => SelectedConnectedCustomer != null
				      && SelectedConnectedCustomer.ConnectedCustomer.ConnectState != ConnectedCustomerConnectState.Blocked);
			
			btnActivateConnect.BindCommand(ActivateConnectConnectedCustomerCommand);
			btnBlockConnect.BindCommand(BlockConnectConnectedCustomerCommand);
		}

		private void GetExternalCounterparties()
		{
			_externalCounterparties = new GenericObservableList<ExternalCounterpartyNode>();
			var existingExternalCounterparties =
				_externalCounterpartyController.GetActiveExternalCounterpartiesByCounterparty(UoW, Entity.Id);

			FillExternalCounterparties(existingExternalCounterparties);
		}

		private void UpdateExternalCounterparties()
		{
			_externalCounterparties.Clear();
			var existingExternalCounterparties =
				_externalCounterpartyController.GetActiveExternalCounterpartiesByPhones(UoW, Entity.Phones.Select(x => x.Id));
			
			FillExternalCounterparties(existingExternalCounterparties);
		}

		private void FillExternalCounterparties(IEnumerable<ExternalCounterpartyNode> existingExternalCounterparties)
		{
			foreach(var item in existingExternalCounterparties)
			{
				_externalCounterparties.Add(item);
			}
		}

		private void UpdateStateConnectButtons()
		{
			ActivateConnectConnectedCustomerCommand.RaiseCanExecuteChanged();
			BlockConnectConnectedCustomerCommand.RaiseCanExecuteChanged();
		}

		private void ConfigureTreeConnectedCustomers()
		{
			treeViewConnectedCustomers.ColumnsConfig = FluentColumnsConfig<ConnectedCustomerInfoNode>.Create()
				.AddColumn("Id клиента")
					.AddNumericRenderer(node => node.CounterpartyId)
				.AddColumn("Наименование клиента")
					.AddTextRenderer(node => node.CounterpartyFullName)
				.AddColumn("Привязанный телефон")
					.AddTextRenderer(node => node.PhoneNumber)
				.AddColumn("Состояние связи")
					.AddEnumRenderer(node => node.ConnectState)
				.AddColumn("Причина блокировки")
					.AddTextRenderer(node => node.BlockingReason)
					.Editable()
				.Finish();
			
			GetConnectedCustomers();
			treeViewConnectedCustomers.ItemsDataSource = _connectedCustomers;
			
			treeViewConnectedCustomers.Binding
				.AddBinding(this, dlg => dlg.SelectedConnectedCustomer, w => w.SelectedRow)
				.InitializeFromSource();
		}

		private void GetConnectedCustomers()
		{
			var connectedCustomers = _connectedCustomerRepository.GetConnectedCustomersInfo(UoW, Entity.Id, Entity.PersonType);

			foreach(var connectedCustomer in connectedCustomers)
			{
				_connectedCustomers.Add(connectedCustomer);
			}
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
			_revenueServiceClient = new RevenueServiceClient(_counterpartySettings.RevenueServiceClientAccessToken);

			btnRequestByInn.Binding
				.AddFuncBinding(Entity, e => !string.IsNullOrWhiteSpace(e.INN), w => w.Sensitive)
				.InitializeFromSource();

			btnRequestByInnAndKpp.Binding
				.AddFuncBinding(Entity, e => !string.IsNullOrWhiteSpace(e.INN) && !string.IsNullOrWhiteSpace(e.KPP), w => w.Sensitive)
				.InitializeFromSource();

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

			ybuttonCopyAccountDetails.Clicked += OnButtonCopyAccountDetailsClicked;
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
				.AddSetter((cell, node) => { cell.Markup = $"<span foreground=\"{node.ColorText}\">♥</span>"; })
				.AddColumn("")
				.Finish();

			ytreeviewTags.ItemsDataSource = Entity.ObservableTags;
			buttonAddTag.Sensitive = CanEdit;
			buttonDeleteTag.Sensitive = CanEdit;
		}

		private void UpdateOurOrganizationSpecialAccountItemList()
		{
			if(!Entity.UseSpecialDocFields)
			{
				Entity.OurOrganizationAccountForBills = null;
			}

			var organization = Entity.WorksThroughOrganization ?? _vodovozOrganization;

			speciallistcomboboxSpecialAccount.ShowSpecialStateNot = true;
			speciallistcomboboxSpecialAccount.NameForSpecialStateNot = "По умолчанию";
			speciallistcomboboxSpecialAccount.ItemsList = organization.Accounts;
		}

		private void ConfigureTabSpecialFields()
		{
			enumcomboCargoReceiverSource.ItemsEnum = typeof(CargoReceiverSource);
			enumcomboCargoReceiverSource.Binding
				.AddBinding(Entity, e => e.CargoReceiverSource, w => w.SelectedItem)
				.InitializeFromSource();
			enumcomboCargoReceiverSource.Sensitive = CanEdit;

			yentryCargoReceiver.Binding
				.AddFuncBinding(Entity, e => e.CargoReceiverSource == CargoReceiverSource.Special, w => w.Visible)
				.AddBinding(Entity, e => e.CargoReceiver, w => w.Text)
				.InitializeFromSource();
			yentryCargoReceiver.IsEditable = CanEdit;
			yentryCustomer.Binding
				.AddBinding(Entity, e => e.SpecialCustomer, w => w.Text)
				.InitializeFromSource();
			yentryCustomer.IsEditable = CanEdit;

			UpdateOurOrganizationSpecialAccountItemList();
			speciallistcomboboxSpecialAccount.SetRenderTextFunc<Account>(e => e.Name);
			speciallistcomboboxSpecialAccount.Binding
				.AddBinding(Entity, e => e.OurOrganizationAccountForBills, w => w.SelectedItem)
				.InitializeFromSource();

			#region Особый договор

			entrySpecialContractName.Binding
				.AddBinding(Entity, e => e.SpecialContractName, w => w.Text)
				.InitializeFromSource();
			entrySpecialContractName.IsEditable = CanEdit;

			entrySpecialContractNumber.Binding
				.AddBinding(Entity, e => e.SpecialContractNumber, w => w.Text)
				.InitializeFromSource();
			entrySpecialContractNumber.IsEditable = CanEdit;

			datePickerSpecialContractDate.Binding
				.AddBinding(Entity, e => e.SpecialContractDate, w => w.DateOrNull)
				.InitializeFromSource();
			datePickerSpecialContractDate.IsEditable = CanEdit;

			#endregion

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
				.AddColumn("Код").AddNumericRenderer(node => node.SpecialId).Adjustment(new Adjustment(0, 0, 1000000, 1, 1, 1)).Editing()
				.Finish();
			ytreeviewSpecialNomenclature.Binding
				.AddBinding(Entity, e => e.SpecialNomenclatures, w => w.ItemsDataSource)
				.InitializeFromSource();
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
					NavigationManager as ITdiCompatibilityNavigation);
			supplierPricesWidget.Sensitive = CanEdit;
		}

		private void ConfigureTabFixedPrices()
		{
			var fixedPriceController = _lifetimeScope.Resolve<INomenclatureFixedPriceController>();
			var fixedPricesModel = new CounterpartyFixedPricesModel(UoW, Entity, fixedPriceController);
			var fixedPricesViewModel = new FixedPricesViewModel(UoW, fixedPricesModel, this, NavigationManager, _lifetimeScope);
			fixedpricesview.ViewModel = fixedPricesViewModel;
			SetSensitivityByPermission("can_edit_counterparty_fixed_prices", fixedpricesview);
		}

		private void ConfigureValidationContext()
		{
			_validationContext = _validationContextFactory.CreateNewValidationContext(Entity);
		}

		private void ConfigureTabEmails()
		{
			if(EmailDataLoader != null)
			{
				return;
			}

			_emailLastScrollPosition = 0;
			EmailDataLoader = new ThreadDataLoader<EmailRow>(ServicesConfig.UnitOfWorkFactory) { PageSize = 50 };
			EmailDataLoader.AddQuery(EmailItemsSourceQueryFunction);

			ytreeviewEmails.ColumnsConfig = FluentColumnsConfig<EmailRow>.Create()
				.AddColumn("Дата отправки").AddTextRenderer(x => x.Date.ToString())
				.AddColumn("Тип письма").AddTextRenderer(x => x.Type.GetEnumTitle())
				.AddColumn("Статус").AddTextRenderer(x => x.State.GetEnumTitle())
				.AddColumn("Тема письма").AddTextRenderer(x => x.Subject)
				.Finish();

			EmailDataLoader.ItemsListUpdated += (sender, args) =>
			{
				Gtk.Application.Invoke((s, arg) =>
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

		private void CongigureTabEdo()
		{
			InitializeEdoAccountsWidget();
			
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_choise_other_reason_leaving"))
			{
				yEnumCmbReasonForLeaving.AddEnumToHideList(ReasonForLeaving.Other);
			}

			yEnumCmbReasonForLeaving.ItemsEnum = typeof(ReasonForLeaving);
			yEnumCmbReasonForLeaving.Binding
				.AddBinding(Entity, e => e.ReasonForLeaving, w => w.SelectedItem)
				.InitializeFromSource();

			yEnumCmbReasonForLeaving.ChangedByUser += (s, e) =>
			{
				var isInnRequired = string.IsNullOrWhiteSpace(Entity.INN) &&
									(Entity.ReasonForLeaving == ReasonForLeaving.Resale
									 || Entity.ReasonForLeaving == ReasonForLeaving.Tender
									 || (Entity.ReasonForLeaving == ReasonForLeaving.ForOwnNeeds
										 && Entity.PersonType == PersonType.legal)
									 );

				if(isInnRequired)
				{
					_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Заполните ИНН у контрагента!");
				}

				Entity.IsNotSendDocumentsByEdo = Entity.ReasonForLeaving == ReasonForLeaving.Other;
				_counterpartyEdoAccountsViewModel.RefreshEdoLightsMatrices();
			};

			yChkBtnIsNotSendDocumentsByEdo.Sensitive = false;
			yChkBtnIsNotSendDocumentsByEdo.Binding
				.AddBinding(Entity, e => e.IsNotSendDocumentsByEdo, w => w.Active)
				.InitializeFromSource();

			yChkBtnNeedSendBillByEdo.Binding
				.AddBinding(Entity, e => e.NeedSendBillByEdo, w => w.Active)
				.InitializeFromSource();

			edoValidatedINN.ValidationMode = QSWidgetLib.ValidationType.numeric;
			edoValidatedINN.Binding
				.AddFuncBinding(Entity,
					e => e.PersonType == PersonType.natural && e.ReasonForLeaving == ReasonForLeaving.Resale,
					w => w.Sensitive)
				.AddBinding(Entity, e => e.INN, w => w.Text)
				.InitializeFromSource();

			ybuttonRegistrationInChestnyZnak.Binding
				.AddFuncBinding(Entity,
					e => e.ReasonForLeaving == ReasonForLeaving.Resale && !string.IsNullOrWhiteSpace(e.INN),
					w => w.Sensitive)
				.InitializeFromSource();

			yEnumCmbRegistrationInChestnyZnak.ItemsEnum = typeof(RegistrationInChestnyZnakStatus);
			yEnumCmbRegistrationInChestnyZnak.Binding
				.AddBinding(Entity, e => e.RegistrationInChestnyZnakStatus, w => w.SelectedItem)
				.InitializeFromSource();
			yEnumCmbRegistrationInChestnyZnak.Sensitive = false;

			yEnumCmbSendUpdInOrderStatus.ItemsEnum = typeof(OrderStatusForSendingUpd);
			yEnumCmbSendUpdInOrderStatus.Binding
				.AddFuncBinding(Entity,
					e => e.LegalAndHasAnyDefaultAccountAgreedForEdo || e.ReasonForLeaving == ReasonForLeaving.Tender,
					w => w.Sensitive)
				.AddBinding(Entity, e => e.OrderStatusForSendingUpd, w => w.SelectedItem)
				.InitializeFromSource();

			yChkBtnIsPaperlessWorkflow.Binding
				.AddBinding(Entity, e => e.LegalAndHasAnyDefaultAccountAgreedForEdo, w => w.Sensitive)
				.AddBinding(Entity, e => e.IsPaperlessWorkflow, w => w.Active)
				.InitializeFromSource();

			yChkBtnDoNotMixMarkedAndUnmarkedGoodsInOrder.Binding
				.AddBinding(Entity, e => e.DoNotMixMarkedAndUnmarkedGoodsInOrder, w => w.Active)
				.InitializeFromSource();

			_counterpartyEdoAccountsViewModel.RefreshEdoLightsMatrices();
		}

		private void ConfigureTabEdoContainers()
		{
			treeViewEdoDocumentsContainer.ColumnsConfig = FluentColumnsConfig<EdoDockflowData>.Create()
				.AddColumn("Новый\nдокументооборот")
					.AddToggleRenderer(x => x.IsNewDockflow)
					.Editing(false)
				.AddColumn(" Дата \n создания ")
					.AddTextRenderer(x => x.TaxcomDocflowCreationTime == null ? string.Empty : x.TaxcomDocflowCreationTime.Value.ToString("dd.MM.yyyy\nHH:mm"))
				.AddColumn(" Номер \n заказа ")
					.AddTextRenderer(x => x.OrderId == null ? "" : x.OrderId.ToString())
				.AddColumn(" Номер счета б/о \n на предоплату ")
					.AddTextRenderer(x => x.OrderWithoutShipmentForAdvancePaymentId == null ? "" : x.OrderWithoutShipmentForAdvancePaymentId.ToString())
				.AddColumn(" Номер счета б/о \n на долг ")
					.AddTextRenderer(x => x.OrderWithoutShipmentForDebtId == null ? "" : x.OrderWithoutShipmentForDebtId.ToString())
				.AddColumn(" Номер счета б/о \n на постоплату ")
					.AddTextRenderer(x => x.OrderWithoutShipmentForPaymentId == null ? "" : x.OrderWithoutShipmentForPaymentId.ToString())
				.AddColumn(" Код документооборота ")
					.AddTextRenderer(x => x.DocFlowId.HasValue ? x.DocFlowId.ToString() : string.Empty)
				.AddColumn(" Отправленные \n документы ")
					.AddTextRenderer(x => x.DocumentType)
				.AddColumn(" Статус \n документооборота ")
					.AddTextRenderer(x => x.EdoDocFlowStatusString)
				.AddColumn(" Доставлено \n клиенту? ")
					.AddToggleRenderer(x => x.IsReceived)
					.Editing(false)
				.AddColumn(" Описание ошибки ")
					.AddTextRenderer(x => x.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("Статус задачи\nнового документооборота")
					.AddTextRenderer(x => x.EdoTaskStatus.HasValue ? x.EdoTaskStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("Статус документа\nнового документооборота")
					.AddTextRenderer(x => x.EdoDocumentStatus.HasValue ? x.EdoDocumentStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("")
				.Finish();

			UpdateEdoDocumentDataNodes();
			treeViewEdoDocumentsContainer.ItemsDataSource = _edoEdoDocumentDataNodes;
			ybuttonEdoDocumentsSendAllUnsent.Visible = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_edo_documents");
			ybuttonEdoDocumentsSendAllUnsent.Clicked += OnButtonEdoDocumentsSendAllUnsentClicked;
			ybuttonEdoDocementsUpdate.Clicked += (s, e) => UpdateEdoDocumentDataNodes();
		}

		private void OnButtonEdoDocumentsSendAllUnsentClicked(object sender, EventArgs e)
		{
			if(Entity.Id > 0)
			{
				var resendEdoDocumentsDialog = new ResendCounterpartyEdoDocumentsViewModel(
					EntityUoWBuilder.ForOpen(Entity.Id),
					ServicesConfig.UnitOfWorkFactory,
					_commonServices,
					GetOrderIdsWithoutSuccessfullySentUpd(),
					_edoService);
				TabParent.AddSlaveTab(this, resendEdoDocumentsDialog);
			}
		}

		private void UpdateEdoContainers(IUnitOfWork uow)
		{
			if(Entity.Id < 1)
			{
				return;
			}

			_edoContainers.Clear();

			var containers = _counterpartyRepository.GetEdoContainersByCounterpartyId(uow, Entity.Id);

			foreach(var item in containers)
			{
				if(item.IsIncoming)
				{
					continue;
				}
				_edoContainers.Add(item);
			}
		}

		private void UpdateEdoDocumentDataNodes()
		{
			_edoEdoDocumentDataNodes.Clear();

			var documents = new List<EdoDockflowData>();

			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Отправка документов по ЭДО, диалог заказа"))
			{
				UpdateEdoContainers(uow);

				documents.AddRange(_edoDocflowRepository.GetEdoDocflowDataByClientId(uow, Entity.Id));
				documents.AddRange(_edoContainers.Select(x => new EdoDockflowData(x)));
			}

			documents = documents.OrderByDescending(x => x.OrderId).ToList();

			foreach(var document in documents)
			{
				_edoEdoDocumentDataNodes.Add(document);
			}

			SetEdoDocumentsSendAllUnsentButtonSensitive();
		}

		private void SetEdoDocumentsSendAllUnsentButtonSensitive()
		{

			ybuttonEdoDocumentsSendAllUnsent.Sensitive =
				Entity.Id > 0
				&& GetOrderIdsWithoutSuccessfullySentUpd().Count > 0;
		}

		private List<int> GetOrderIdsWithoutSuccessfullySentUpd()
		{
			var allOrdersIds = _edoContainers.Where(x => EdoContainerSpecification.CreateIsForOrder().IsSatisfiedBy(x)).Select(c => c.Order.Id).Distinct().ToList();

			var orderIdsHavingUpdSentSuccessfully = _edoContainers
				.Where(c => c.Type == DocumentContainerType.Upd
					&& !c.IsIncoming
					&& c.EdoDocFlowStatus == EdoDocFlowStatus.Succeed)
				.Select(c => c.Order.Id)
				.Distinct()
				.ToList();

			var orderIdsWithoutSuccessfullySentUpd = allOrdersIds.Except(orderIdsHavingUpdSentSuccessfully).ToList();

			return orderIdsWithoutSuccessfullySentUpd;
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
					.Select(() => storedEmailAlias.SendDate).WithAlias(() => resultAlias.Date)
					.Select(() => counterpartyEmailAlias.Type).WithAlias(() => resultAlias.Type)
					.Select(() => storedEmailAlias.Subject).WithAlias(() => resultAlias.Subject)
					.Select(() => storedEmailAlias.State).WithAlias(() => resultAlias.State)
				)
				.OrderBy(() => storedEmailAlias.SendDate).Desc
				.TransformUsing(Transformers.AliasToBean<EmailRow>());

			return itemsQuery;
		};

		public IEntityEntryViewModel SubtypeEntryViewModel { get; private set; }
		public INavigationManager NavigationManager => Startup.MainWin.NavigationManager;

		private void CheckIsChainStoreOnToggled(object sender, EventArgs e)
		{
		}

		private void OnButtonLoadFromDeliveryPointClicked(object sender, EventArgs e)
		{
			var filter = new DeliveryPointJournalFilterViewModel
			{
				Counterparty = Entity
			};
			var dpFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>(new TypedParameter(typeof(DeliveryPointJournalFilterViewModel), filter));
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

		public void ActivateEdoTab()
		{
			if(rbnEdo.Sensitive)
			{
				rbnEdo.Active = true;
			}
		}

		public void ActivateDetailsTab()
		{
			if(radioDetails.Sensitive)
			{
				radioDetails.Active = true;
			}
		}

		private void OnAllCounterpartyOrdersActivated(object sender, EventArgs e)
		{
			NavigationManager.OpenViewModel<OrderJournalViewModel, Action<OrderJournalFilterViewModel>>(
				null,
				filter => filter.RestrictCounterparty = Entity,
				OpenPageOptions.IgnoreHash);
		}

		private void OnCounterpartyComplaintsActivated(object sender, EventArgs e)
		{
			NavigationManager.OpenViewModel<ComplaintsJournalsViewModel, Action<ComplaintFilterViewModel>>(
				null,
				filterConfig => filterConfig.Counterparty = Entity,
				OpenPageOptions.IgnoreHash);
		}

		private void OnLogisticsRequirementsSelectionChanged(object sender, PropertyChangedEventArgs e)
		{
			Entity.LogisticsRequirements = logisticsRequirementsView.ViewModel.Entity;
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

				RemoveEmptyEmailsAndPhones();

				if(!ServicesConfig.ValidationService.Validate(Entity, _validationContext))
				{
					return false;
				}

				try
				{
					_counterpartyService.StopShipmentsIfNeeded(Entity, CurrentEmployee, _cancellationTokenCheckLiquidationSource.Token).GetAwaiter().GetResult();
				}
				catch(Exception ex)
				{
					MessageDialogHelper.RunWarningDialog($"Не удалось проверить контрагента в ФНС: {ex.Message}", "Ошибка проверки статуса в ФНС");
					_logger.Warn("Не удалось проверить контрагента в ФНС: {Reason}",
					ex.Message);
				}

				_logger.Info("Сохраняем контрагента...");
				UoW.Save();
				AddAttachedFilesIfNeeded();
				UpdateAttachedFilesIfNeeded();
				DeleteAttachedFilesIfNeeded();
				_attachedFileInformationsViewModel.ClearPersistentInformationCommand.Execute();
				_logger.Info("Ok.");
				return true;
			}
			finally
			{
				SetSensetivity(true);
			}
		}

		private void RemoveEmptyEmailsAndPhones()
		{
			_phonesViewModel.RemoveEmpty();
			emailsView.ViewModel.RemoveEmpty();
		}

		private void AddAttachedFilesIfNeeded()
		{
			var errors = new Dictionary<string, string>();
			var repeat = false;

			if(!_attachedFileInformationsViewModel.FilesToAddOnSave.Any())
			{
				return;
			}

			do
			{
				foreach(var fileName in _attachedFileInformationsViewModel.FilesToAddOnSave)
				{
					var result = _counterpartyFileStorageService.CreateFileAsync(Entity, fileName,
					new MemoryStream(_attachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
						.GetAwaiter()
						.GetResult();

					if(result.IsFailure && !result.Errors.All(x => x.Code == Application.Errors.S3.FileAlreadyExists.ToString()))
					{
						errors.Add(fileName, string.Join(", ", result.Errors.Select(e => e.Message)));
					}
				}

				if(errors.Any())
				{
					repeat = _interactiveService.Question(
						"Не удалось загрузить файлы:\n" +
						string.Join("\n- ", errors.Select(fekv => $"{fekv.Key} - {fekv.Value}")) + "\n" +
						"\n" +
						"Повторить попытку?",
						"Ошибка загрузки файлов");

					errors.Clear();
				}
				else
				{
					repeat = false;
				}
			}
			while(repeat);
		}

		private void UpdateAttachedFilesIfNeeded()
		{
			if(!_attachedFileInformationsViewModel.FilesToUpdateOnSave.Any())
			{
				return;
			}

			foreach(var fileName in _attachedFileInformationsViewModel.FilesToUpdateOnSave)
			{
				_counterpartyFileStorageService.UpdateFileAsync(Entity, fileName, new MemoryStream(_attachedFileInformationsViewModel.AttachedFiles[fileName]), _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
			}
		}

		private void DeleteAttachedFilesIfNeeded()
		{
			if(!_attachedFileInformationsViewModel.FilesToDeleteOnSave.Any())
			{
				return;
			}

			foreach(var fileName in _attachedFileInformationsViewModel.FilesToDeleteOnSave)
			{
				_counterpartyFileStorageService.DeleteFileAsync(Entity, fileName, _cancellationTokenSource.Token)
					.GetAwaiter()
					.GetResult();
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

		protected void OnRadioEdoToggled(object sender, EventArgs e)
		{
			if(rbnEdo.Active)
			{
				notebook1.CurrentPage = 12;
			}
		}

		protected void OnRadioEdoDocumentsToggled(object sender, EventArgs e)
		{
			if(rbnEdoDocuments.Active)
			{
				notebook1.CurrentPage = 13;
			}
		}

		private void OnEnumCounterpartyTypeChanged(object sender, EventArgs e)
		{
			rbnPrices.Visible = Entity.CounterpartyType == CounterpartyType.Supplier;
			validatedOGRN.Visible = labelOGRN.Visible = HasOgrn;
			if(Entity.CounterpartyType == CounterpartyType.Dealer)
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
			labelShort.Visible = labelShort1.Visible = comboboxOpf.Visible = yentryOrganizationName.Visible =
				labelFullName.Visible = entryFullName.Visible =
					entryMainCounterparty.Visible = labelMainCounterparty.Visible =
						radioDetails.Visible = radiobuttonProxies.Visible = lblPaymentType.Visible =
							enumPayment.Visible = (Entity.PersonType == PersonType.legal);

			if(Entity.PersonType != PersonType.legal && Entity.TaxType != TaxType.None)
			{
				Entity.TaxType = TaxType.None;
			}

			if(Entity.PersonType == PersonType.natural)
			{
				var personFullName = GetPersonFullName();
				if(!string.IsNullOrEmpty(personFullName))
				{
					Entity.Name = personFullName;
				}
			}
		}

		protected void OnEnumPaymentEnumItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			enumDefaultDocumentType.Visible = labelDefaultDocumentType.Visible = (PaymentType)e.SelectedItem == PaymentType.Cashless;
		}

		protected void OnEnumPaymentChangedByUser(object sender, EventArgs e)
		{
			if(Entity.PaymentMethod == PaymentType.Cashless)
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
			var personFullName = GetPersonFullName();
			if(Entity.TypeOfOwnership == "ИП" && !string.IsNullOrEmpty(personFullName))
			{
				Entity.Name = Entity.FullName = personFullName;
			}
		}

		protected void OnChkNeedNewBottlesToggled(object sender, EventArgs e)
		{
			Entity.NewBottlesNeeded = chkNeedNewBottles.Active;
		}

		protected void OnYcheckSpecialDocumentsToggled(object sender, EventArgs e)
		{
			radioSpecialDocFields.Visible = ycheckSpecialDocuments.Active;

			UpdateOurOrganizationSpecialAccountItemList();
		}

		private void OnEntryPersonNamePartChanged(object sender, EventArgs e)
		{
			var personFullName = GetPersonFullName();
			if(!string.IsNullOrEmpty(personFullName))
			{
				Entity.Name = Entity.FullName = personFullName;
			}
		}

		private string GetPersonFullName()
		{
			StringBuilder personFullName = new StringBuilder();

			if(Entity.TypeOfOwnership == "ИП" && Entity.PersonType == PersonType.legal)
			{
				personFullName.Append("ИП ");
			}

			personFullName.Append(PersonHelper.PersonFullName(Entity.Surname, Entity.FirstName, Entity.Patronymic));

			return personFullName.ToString();
		}

		#region CloseDelivery //Переделать на PermissionCommentView

		private void SetVisibilityForCloseDeliveryComments()
		{
			labelCloseDelivery.Visible = Entity.IsDeliveriesClosed;
			GtkScrolledWindowCloseDelivery.Visible = Entity.IsDeliveriesClosed;
			ytextviewCloseComment.Buffer.Text = Entity.IsDeliveriesClosed ? Entity.CloseDeliveryComment : String.Empty;
			ytextviewCloseComment.Sensitive = false;

			if(!Entity.IsDeliveriesClosed)
			{
				return;
			}

			labelCloseDelivery.LabelProp = "<b>Поставки закрыты</b>" + Environment.NewLine +
										   "<b>Комментарий по закрытию поставок:</b>";
		}

		#endregion CloseDelivery

		protected void OnYbuttonAddNomClicked(object sender, EventArgs e)
		{
			(NavigationManager as ITdiCompatibilityNavigation).OpenViewModelOnTdi<NomenclaturesJournalViewModel>(
				this,
				OpenPageOptions.AsSlave,
				viewModel =>
				{
					viewModel.SelectionMode = JournalSelectionMode.Single;
					viewModel.OnSelectResult += Journal_OnEntitySelectedResult;
				});
		}

		private void Journal_OnEntitySelectedResult(object sender, JournalSelectedEventArgs e)
		{
			var selectedNode = e.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();
			if(selectedNode == null)
			{
				return;
			}

			if(Entity.SpecialNomenclatures.Any(x => x.Nomenclature.Id == selectedNode.Id))
			{
				return;
			}

			var selectedNomenclature = UoW.GetById<Nomenclature>(selectedNode.Id);
			var specNomenclature = new SpecialNomenclature
			{
				Nomenclature = selectedNomenclature,
				Counterparty = Entity
			};

			Entity.SpecialNomenclatures.Add(specNomenclature);
		}

		private void NomenclatureSelectDlg_ObjectSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			var specNom = new SpecialNomenclature
			{
				Nomenclature = e.Subject as Nomenclature,
				Counterparty = Entity
			};

			if(Entity.SpecialNomenclatures.Any(x => x.Nomenclature.Id == specNom.Nomenclature.Id))
			{
				return;
			}

			Entity.SpecialNomenclatures.Add(specNom);
		}

		protected void OnYbuttonRemoveNomClicked(object sender, EventArgs e)
		{
			Entity.SpecialNomenclatures.Remove(ytreeviewSpecialNomenclature.GetSelectedObject<SpecialNomenclature>());
		}

		protected void OnEnumcomboCargoReceiverSourceChangedByUser(object sender, EventArgs e)
		{
			UpdateCargoReceiver();
		}

		private string _cargoReceiverBackupBuffer;
		private AttachedFileInformationsViewModel _attachedFileInformationsViewModel;

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
		}

		protected void OnButtonUnsubscribeClicked(object sender, EventArgs e)
		{
			var unsubscribingReason = _emailRepository.GetBulkEmailEventOperatorReason(UoW, _emailSettings);

			var unsubscribingEvent = new UnsubscribingBulkEmailEvent
			{
				Reason = unsubscribingReason,
				ReasonDetail = CurrentEmployee.GetPersonNameWithInitials(),
				Counterparty = Entity
			};

			using(var unitOfWork = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Сохранение отписки от массовой рассылки"))
			{
				unitOfWork.Save(unsubscribingEvent);
				unitOfWork.Commit();
			}

			RefreshBulkEmailEventStatus();
		}

		protected void OnButtonSubscribeClicked(object sender, EventArgs e)
		{
			var subscribingReason = _emailRepository.GetBulkEmailEventOperatorReason(UoW, _emailSettings);

			var subscribingEvent = new SubscribingBulkEmailEvent
			{
				Reason = subscribingReason,
				ReasonDetail = CurrentEmployee.GetPersonNameWithInitials(),
				Counterparty = Entity
			};

			using(var unitOfWork = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Сохранение подписки на массовую рассылку"))
			{
				unitOfWork.Save(subscribingEvent);
				unitOfWork.Commit();
			}

			RefreshBulkEmailEventStatus();
		}

		protected void OnYbuttonRegistrationInChestnyZnakClicked(object sender, EventArgs e)
		{
			if(Entity.CheckForINNDuplicate(_counterpartyRepository, UoW))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					"Контрагент с данным ИНН уже существует.\nПроверка в Честном знаке не выполнена.");

				return;
			}

			TrueMarkRegistrationResultDto trueMarkResponse;

			try
			{
				trueMarkResponse = _trueMarkApiClient.GetParticipantRegistrationForWaterStatusAsync(
					_edoSettings.TrueMarkApiParticipantRegistrationForWaterUri, Entity.INN, _cancellationTokenSource.Token)
					.Result;
			}
			catch(Exception ex)
			{
				_logger.Error(ex);

				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					$"Ошибка при проверке в Честном Знаке.\n{ex.Message}");

				return;
			}

			if(!string.IsNullOrWhiteSpace(trueMarkResponse.ErrorMessage))
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					$"Результат проверки в Честном Знаке:\n{trueMarkResponse.ErrorMessage}");

				Entity.RegistrationInChestnyZnakStatus = RegistrationInChestnyZnakStatus.Unknown;

				return;
			}

			var statusConverter = new TrueMarkApiRegistrationStatusConverter();
			var status = statusConverter.ConvertToChestnyZnakStatus(trueMarkResponse.RegistrationStatusString);

			if(status == null)
			{
				_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error,
					$"Такой статус участника в Честном Знаке у нас не используется:\n{trueMarkResponse.RegistrationStatusString}");

				Entity.RegistrationInChestnyZnakStatus = RegistrationInChestnyZnakStatus.Unknown;

				return;
			}

			Entity.RegistrationInChestnyZnakStatus = status.Value;

			_counterpartyEdoAccountsViewModel.RefreshEdoLightsMatrices();

			_commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info,
				$"Статус регистрации в Честном Знаке:\n{trueMarkResponse.RegistrationStatusString}");
		}

		public override void Dispose()
		{
			_cancellationTokenSource.Cancel();
			base.Dispose();
		}

		protected void OnButtonRequestByInnClicked(object sender, EventArgs e)
		{
			var dadataRequestDto = new DadataRequestDto
			{
				Inn = Entity.INN
			};

			OpenRevenueServicePage(dadataRequestDto);
		}

		protected void OnButtonCopyAccountDetailsClicked(object sender, EventArgs e)
		{
			var accountData = $"ИНН: {Entity.INN}\n" +
				$"КПП: {Entity.KPP}\n" +
				$"ЮР. адрес: {Entity.RawJurAddress}\n" +
				$"ФИО: {Entity.SignatoryFIO}\n" +
				$"В лице: {Entity.SignatoryPost}\n" +
				$"На основании:  {Entity.SignatoryBaseOf}";

			GetClipboard(Gdk.Selection.Clipboard).Text = accountData;
		}

		protected void OnButtonRequestByInnAndKppClicked(object sender, EventArgs e)
		{
			var dadataRequestDto = new DadataRequestDto
			{
				Inn = Entity.INN,
				Kpp = Entity.KPP
			};

			OpenRevenueServicePage(dadataRequestDto);
		}

		private void OpenRevenueServicePage(DadataRequestDto dadataRequestDto)
		{
			var revenueServicePage = NavigationManager.OpenViewModel<CounterpartyDetailsFromRevenueServiceViewModel, DadataRequestDto,
				IRevenueServiceClient, CancellationToken>(null, dadataRequestDto, _revenueServiceClient, _cancellationTokenSource.Token);

			revenueServicePage.ViewModel.OnSelectResult += (o, a) =>
			{
				if(a.IsActive
				   || _commonServices.InteractiveService.Question("Вы действительно хотите подгрузить недействующие реквизиты?"))
				{
					FillEntityDetailsFromRevenueService(a);
				}

				if(Entity.IsLiquidating && a.IsActive)
				{
					Entity.IsLiquidating = false;
				}

				if(Entity.IsDeliveriesClosed && !a.IsActive)
				{
					Entity.IsLiquidating = true;
				}

				_counterpartyService.StopShipmentsIfNeeded(Entity, CurrentEmployee, !a.IsActive, a.State.GetUserFriendlyName());
			};
		}

		private void FillEntityDetailsFromRevenueService(CounterpartyRevenueServiceDto revenueServiceRow)
		{
			Entity.KPP = revenueServiceRow.Kpp;
			Entity.Name = revenueServiceRow.ShortName ?? revenueServiceRow.FullName;
			Entity.FullName = revenueServiceRow.FullName ?? Entity.Name;
			Entity.RawJurAddress = revenueServiceRow.Address;

			if((revenueServiceRow.Opf ?? string.Empty).Length > 0 && (revenueServiceRow.OpfFull ?? string.Empty).Length > 0)
			{
				Entity.TypeOfOwnership = revenueServiceRow.Opf;

				Entity.Name = $"{revenueServiceRow.Opf} {Entity.Name}";
				Entity.FullName = $"{revenueServiceRow.Opf} {Entity.FullName}";

				if(!GetAllComboboxOpfValues().Any(t => t == revenueServiceRow.Opf))
				{
					AddNewOrganizationOwnershipType(revenueServiceRow.Opf, revenueServiceRow.OpfFull);
				}
				SetActiveComboboxOpfValue(Entity.TypeOfOwnership);
			}

			if(revenueServiceRow.Opf == "ИП")
			{
				Entity.SignatoryFIO = string.Empty;

				Entity.Surname = revenueServiceRow.PersonSurname ?? string.Empty;
				Entity.FirstName = revenueServiceRow.PersonName ?? string.Empty;
				Entity.Patronymic = revenueServiceRow.PersonPatronymic ?? string.Empty;
			}
			else
			{
				Entity.SignatoryFIO = revenueServiceRow.TitlePersonFullName;

				Entity.Surname = string.Empty;
				Entity.FirstName = string.Empty;
				Entity.Patronymic = string.Empty;
			}

			if(revenueServiceRow.Phones != null)
			{
				var phonesToAdd = revenueServiceRow.Phones
					.Where(number => !Entity.Phones.Any(x => x.Number == number));

				foreach(var number in phonesToAdd)
				{
					_phonesViewModel.PhonesList.Add(new Phone
					{
						Counterparty = Entity,
						Number = number
					});
				}
			}

			if(revenueServiceRow.Emails != null)
			{
				var emailsToAdd = revenueServiceRow.Emails
					.Where(email => Entity.Emails.All(x => x.Address != email));

				foreach(var email in emailsToAdd)
				{
					emailsView.ViewModel.EmailsList.Add(new Email
					{
						Counterparty = Entity,
						Address = email
					});
				}
			}
		}

		private void AddNewOrganizationOwnershipType(string abbreviation, string fullName)
		{
			if(!GetAllOrganizationOwnershipTypes().Any(t => t.Abbreviation == abbreviation))
			{
				var newOrganizationOwnershipType = new OrganizationOwnershipType()
				{
					Abbreviation = abbreviation,
					FullName = fullName,
					IsArchive = false
				};

				using(var uowOrganization = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot(newOrganizationOwnershipType))
				{
					uowOrganization.Save(newOrganizationOwnershipType);
				}
			}
			FillComboboxOpf();
		}

		private List<OrganizationOwnershipType> GetAllOrganizationOwnershipTypes()
		{
			return _organizationRepository
				.GetAllOrganizationOwnershipTypes(UoW)
				.OrderBy(t => t.Id)
				.ToList();
		}

		private List<string> GetAvailableOrganizationOwnershipTypes()
		{
			return GetAllOrganizationOwnershipTypes()
					.Where(t => !t.IsArchive || (Entity.TypeOfOwnership != null && t.Abbreviation == Entity.TypeOfOwnership))
					.Select(t => t.Abbreviation)
					.ToList<string>();
		}

		private void ComboboxOpfChanged(object sender, EventArgs e)
		{
			Entity.TypeOfOwnership = comboboxOpf.ActiveText;
		}

		private void FillComboboxOpf()
		{
			var availableOrganizationOwnershipTypes = GetAvailableOrganizationOwnershipTypes();
			var currentOwnershipType = Entity.TypeOfOwnership;

			while(GetAllComboboxOpfValues().Count() > 0)
			{
				comboboxOpf.RemoveText(0);
			}

			comboboxOpf.AppendText("");

			foreach(var ownershipType in availableOrganizationOwnershipTypes)
			{
				comboboxOpf.AppendText(ownershipType);
			}

			Entity.TypeOfOwnership = SetActiveComboboxOpfValue(currentOwnershipType) ? currentOwnershipType : String.Empty;
		}

		private List<string> GetAllComboboxOpfValues()
		{
			List<string> values = new List<string>();
			TreeIter iter;
			comboboxOpf.Model.GetIterFirst(out iter);
			do
			{
				GLib.Value thisRow = new GLib.Value();
				comboboxOpf.Model.GetValue(iter, 0, ref thisRow);
				if(((thisRow.Val as string) ?? String.Empty).Length > 0)
				{
					values.Add(thisRow.Val as string);
				}
			} while(comboboxOpf.Model.IterNext(ref iter));

			return values;
		}

		private bool SetActiveComboboxOpfValue(string value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return false;
			}

			TreeIter iter;
			comboboxOpf.Model.GetIterFirst(out iter);
			do
			{
				GLib.Value thisRow = new GLib.Value();
				comboboxOpf.Model.GetValue(iter, 0, ref thisRow);
				if(((thisRow.Val as string) ?? String.Empty) == value)
				{
					comboboxOpf.SetActiveIter(iter);
					return true;
				}
			} while(comboboxOpf.Model.IterNext(ref iter));

			return false;
		}

		private void OnEnumPersonTypeChangedByUser(object sender, EventArgs e)
		{
			emailsView.ViewModel.UpdatePersonType(Entity.PersonType);
		}

		private Bank FillBank(NewCounterpartyParameters parameters)
		{
			var bank = BankRepository.GetBankByBik(UoW, parameters.CounterpartyBik);

			if(bank == null)
			{
				bank = new Bank
				{
					Bik = parameters.CounterpartyBik,
					Name = parameters.CounterpartyBank
				};
				var corAcc = new CorAccount { CorAccountNumber = parameters.CounterpartyCorrespondentAcc };
				bank.CorAccounts.Add(corAcc);
				bank.DefaultCorAccount = corAcc;
				UoW.Save(bank);
			}

			return bank;
		}

		public override void Destroy()
		{
			if(_lifetimeScope != null)
			{
				_lifetimeScope.Dispose();
				_lifetimeScope = null;
			}
			base.Destroy();
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
