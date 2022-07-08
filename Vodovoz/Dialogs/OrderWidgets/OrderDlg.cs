using Autofac;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using NLog;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Project.Dialogs;
using QS.Project.Dialogs.GtkUI;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QS.Tdi;
using QSOrmProject;
using QSProjectsLib;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using QS.Navigation;
using QS.Utilities.Extensions;
using QS.ViewModels.Extension;
using SmsPaymentService;
using Vodovoz.Additions.Printing;
using Vodovoz.Controllers;
using Vodovoz.Core;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Sms;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.BasicHandbooks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Nodes;
using Vodovoz.EntityRepositories.Operations;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.ServiceClaims;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalFilters;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Models;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.TempAdapters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using VodovozInfrastructure.Configuration;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Infrastructure.Print;
using CounterpartyContractFactory = Vodovoz.Factories.CounterpartyContractFactory;
using IntToStringConverter = Vodovoz.Infrastructure.Converters.IntToStringConverter;
using IOrganizationProvider = Vodovoz.Models.IOrganizationProvider;
using Vodovoz.Models.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Orders;
using QS.Project.Services.FileDialog;
using QS.Dialog.GtkUI.FileDialog;
using Vodovoz.ViewModels.ViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Orders;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz
{
	public partial class OrderDlg : EntityDialogBase<Order>,
		ICounterpartyInfoProvider,
		Vodovoz.ViewModels.Infrastructure.InfoProviders.IDeliveryPointInfoProvider,
		IContractInfoProvider,
		ITdiTabAddedNotifier,
		IEmailsInfoProvider,
		ICallTaskProvider,
		ITDICloseControlTab,
		ISmsSendProvider,
		IFixedPricesHolderProvider,
		IAskSaveOnCloseViewModel
	{
		static Logger logger = LogManager.GetCurrentClassLogger();
		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly BaseParametersProvider _baseParametersProvider = new BaseParametersProvider(_parametersProvider);
		private static readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider =
			new DeliveryRulesParametersProvider(_parametersProvider);
		private static readonly IDriverApiParametersProvider _driverApiParametersProvider =
			new DriverApiParametersProvider(_parametersProvider);
		private static readonly IDeliveryRepository _deliveryRepository = new DeliveryRepository();

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;

		Order templateOrder;

		private int _previousDeliveryPointId;
		private IOrganizationProvider organizationProvider;
		private ICounterpartyContractRepository counterpartyContractRepository;
		private CounterpartyContractFactory counterpartyContractFactory;
		private IOrderParametersProvider _orderParametersProvider;
		private IPaymentFromBankClientController _paymentFromBankClientController;

		private readonly IDocumentPrinter _documentPrinter = new DocumentPrinter();
		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory = new EntityDocumentsPrinterFactory();
		private readonly IEmployeeService _employeeService = VodovozGtkServicesConfig.EmployeeService;
		private readonly IEmployeeRepository _employeeRepository = new EmployeeRepository();
		private readonly IUserRepository _userRepository = new UserRepository();
		private readonly IFlyerRepository _flyerRepository = new FlyerRepository();
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository = new DeliveryScheduleRepository();
		private readonly IDocTemplateRepository _docTemplateRepository = new DocTemplateRepository();
		private readonly IServiceClaimRepository _serviceClaimRepository = new ServiceClaimRepository();
		private readonly IStockRepository _stockRepository = new StockRepository();
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IDiscountReasonRepository _discountReasonRepository = new DiscountReasonRepository();
		private readonly IRouteListItemRepository _routeListItemRepository = new RouteListItemRepository();
		private readonly IEmailRepository _emailRepository = new EmailRepository();
		private readonly ICashRepository _cashRepository = new CashRepository();
		private readonly IPromotionalSetRepository _promotionalSetRepository = new PromotionalSetRepository();
		private readonly IRentPackagesJournalsViewModelsFactory _rentPackagesJournalsViewModelsFactory
			= new RentPackagesJournalsViewModelsFactory(MainClass.MainWin.NavigationManager);
		private readonly INonSerialEquipmentsForRentJournalViewModelFactory _nonSerialEquipmentsForRentJournalViewModelFactory
			= new NonSerialEquipmentsForRentJournalViewModelFactory();
		private readonly IPaymentItemsRepository _paymentItemsRepository = new PaymentItemsRepository();
		private readonly IPaymentsRepository _paymentsRepository = new PaymentsRepository();
		private readonly DateTime date = new DateTime(2020, 11, 09, 11, 0, 0);
		private readonly bool _canSetOurOrganization =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_organization_from_order_and_counterparty");
		private readonly bool _canEditSealAndSignatureUpd =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_seal_and_signature_UPD");
		private readonly bool _canEditDeliveryDateAfterOrderConfirmation =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_deliverydate_after_order_confirmation");
		private bool isEditOrderClicked;
		private int _treeItemsNomenclatureColumnWidth;
		private IList<DiscountReason> _discountReasons;
		private IList<int> _addedFlyersNomenclaturesIds;
		private Employee _currentEmployee;
		private bool _canChangeDiscountValue;
		private bool _canChoosePremiumDiscount;
		private INomenclatureFixedPriceProvider _nomenclatureFixedPriceProvider;
		private IOrderDiscountsController _discountsController;
		private IOrderDailyNumberController _dailyNumberController;
		private bool _isNeedSendBill;
		private Email _emailAddressForBill;

		private SendDocumentByEmailViewModel SendDocumentByEmailViewModel { get; set; }

		private  INomenclatureRepository nomenclatureRepository;
		public virtual INomenclatureRepository NomenclatureRepository {
			get {
				if (nomenclatureRepository == null) {
					nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(_parametersProvider));
				}
				return nomenclatureRepository;
			}
		}

		private DriverAPIHelper _driverApiHelper;
		public virtual DriverAPIHelper DriverApiHelper
		{
			get
			{
				if(_driverApiHelper == null)
				{
					var driverApiConfig = new DriverApiHelperConfiguration
					{
						ApiBase = _driverApiParametersProvider.ApiBase,
						NotifyOfSmsPaymentStatusChangedURI = _driverApiParametersProvider.NotifyOfSmsPaymentStatusChangedUri,
						NotifyOfFastDeliveryOrderAddedURI = _driverApiParametersProvider.NotifyOfFastDeliveryOrderAddedUri
					};
					_driverApiHelper = new DriverAPIHelper(driverApiConfig);
				}
				return _driverApiHelper;
			}
		}
		
		private ICounterpartyJournalFactory counterpartySelectorFactory;
		public virtual ICounterpartyJournalFactory CounterpartySelectorFactory =>
			counterpartySelectorFactory ?? (counterpartySelectorFactory = new CounterpartyJournalFactory());

		private IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory {
			get {
				if(nomenclatureSelectorFactory == null) {
					nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), CounterpartySelectorFactory,
							NomenclatureRepository, _userRepository);
				}
				return nomenclatureSelectorFactory;
			}
		}

		#region Работа с боковыми панелями

		public PanelViewType[] InfoWidgets {
			get {
				return new[]{
					PanelViewType.FixedPricesPanelView,
					PanelViewType.CounterpartyView,
					PanelViewType.DeliveryPricePanelView,
					PanelViewType.DeliveryPointView,
					PanelViewType.EmailsPanelView,
					PanelViewType.CallTaskPanelView,
					PanelViewType.SmsSendPanelView
				};
			}
		}

		public Counterparty Counterparty => Entity.Client;

		public DeliveryPoint DeliveryPoint => Entity.DeliveryPoint;

		public CounterpartyContract Contract => Entity.Contract;

		public bool CanHaveEmails => Entity.Id != 0;

		public Order Order => Entity;

		public List<StoredEmail> GetEmails() => Entity.Id != 0 ? _emailRepository.GetAllEmailsForOrder(UoW, Entity.Id) : null;

		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						_orderRepository,
						_employeeRepository,
						_baseParametersProvider,
						ServicesConfig.CommonServices.UserService,
						ErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public bool? IsForRetail
		{
			get => isForRetail;
			set {
				isForRetail = value;
			}
		}

		private bool? isForRetail = null;

		public bool AskSaveOnClose => CanEditByPermission;

		#endregion

		#region Конструкторы, настройка диалога

		public override void Destroy()
		{
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			_driverApiHelper?.Dispose();
			base.Destroy();
		}

		public OrderDlg()
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateWithNewRoot<Order>();
			Entity.Author = _currentEmployee = _employeeService.GetEmployeeForUser(UoW, _userRepository.GetCurrentUser(UoW).Id);
			if(Entity.Author == null) {
				MessageDialogHelper.RunErrorDialog("Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать заказы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}
			Entity.OrderStatus = OrderStatus.NewOrder;
			TabName = "Новый заказ";
			ConfigureDlg();
			//по стандарту тип - доставка
			Entity.OrderAddressType = OrderAddressType.Delivery;
		}

		public OrderDlg(Counterparty client) : this()
		{
			Entity.Client = UoW.GetById<Counterparty>(client.Id);
			Entity.PaymentType = Entity.Client.PaymentMethod;
			IsForRetail = Entity.Client.IsForRetail;
			CheckForStopDelivery();
			UpdateOrderAddressTypeWithUI();
		}

		public OrderDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<Order>(id);
			IsForRetail = UoWGeneric.Root.Client.IsForRetail;
			ConfigureDlg();
			UpdateOrderAddressTypeWithUI();
		}

		public OrderDlg(Order sub) : this(sub.Id)
		{ }

		/// <summary>
		/// Конструктор создан изначально для Mango-Интеграции,
		/// </summary>
		/// <param name="copiedOrder">Конструктор копирует заказ по Id заказа</param>
		/// <param name="NeedCopy"><c>true</c> копировать заказ, <c>false</c> работает как обычный конструктор.</param>
		public OrderDlg(Order copiedOrder, bool NeedCopy) : this()
		{
			if(NeedCopy) {
				Entity.Client = UoW.GetById<Counterparty>(copiedOrder.Client.Id);

				if (copiedOrder.DeliveryPoint != null) {
					Entity.DeliveryPoint = UoW.GetById<DeliveryPoint>(copiedOrder.DeliveryPoint.Id);
				}

				Entity.PaymentType = Entity.Client.PaymentMethod;
				var orderOrganizationProviderFactory = new OrderOrganizationProviderFactory();
				var orderOrganizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
				counterpartyContractRepository = new CounterpartyContractRepository(orderOrganizationProvider);
				counterpartyContractFactory = new CounterpartyContractFactory(orderOrganizationProvider, counterpartyContractRepository);
				Entity.UpdateOrCreateContract(UoW, counterpartyContractRepository, counterpartyContractFactory);
				FillOrderItems(copiedOrder);
				CheckForStopDelivery();
				AddCommentFromDeliveryPoint();
			}
			UpdateOrderAddressTypeWithUI();
		}

		public void CopyOrderFrom(int orderId)
		{
			var nomenclatureParameterProvider = new NomenclatureParametersProvider(_parametersProvider);
			var orderCopyModel = new OrderCopyModel(nomenclatureParameterProvider, _flyerRepository);
			var copying = orderCopyModel.StartCopyOrder(UoW, orderId, Entity)
				.CopyFields()
				.CopyStockBottle()
				.CopyPromotionalSets()
				.CopyOrderItems(true, true)
				.CopyPaidDeliveryItem()
				.CopyAdditionalOrderEquipments()
				.CopyOrderDepositItems()
				.CopyAttachedDocuments();
			
			Entity.IsCopiedFromUndelivery = true;
			if(copying.GetCopiedOrder.PaymentType == PaymentType.ByCard
				&& MessageDialogHelper.RunQuestionDialog("Перенести на выбранный заказ Оплату по Карте?"))
			{
				copying.CopyPaymentByCardDataIfPossible();
			}

			Entity.UpdateDocuments();
			CheckForStopDelivery();
			UpdateOrderAddressTypeWithUI();
		}

		//Копирование меньшего количества полей чем в CopyOrderFrom для пункта "Повторить заказ" в журнале заказов
		public void CopyLesserOrderFrom(int orderId)
		{
			var nomenclatureParameterProvider = new NomenclatureParametersProvider(_parametersProvider);
			var orderCopyModel = new OrderCopyModel(nomenclatureParameterProvider, _flyerRepository);
			var copying = orderCopyModel.StartCopyOrder(UoW, orderId, Entity)
				.CopyFields(
					x => x.Client,
					x => x.DeliveryPoint,
					x => x.OrderAddressType,
					x => x.PaymentType
					)
				.CopyPromotionalSets()
				.CopyOrderItems()
				.CopyAdditionalOrderEquipments()
				.CopyOrderDepositItems()
				.CopyAttachedDocuments();

			Entity.UpdateDocuments();
			CheckForStopDelivery();
			UpdateOrderAddressTypeWithUI();
			AddCommentFromDeliveryPoint();
		}

		public void ConfigureDlg()
		{
			if(_currentEmployee == null)
			{
				_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _userRepository.GetCurrentUser(UoW).Id);
			}

			_canChangeDiscountValue = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_direct_discount_value");
			_canChoosePremiumDiscount = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_choose_premium_discount");
			_nomenclatureFixedPriceProvider	=
				new NomenclatureFixedPriceController(
					new NomenclatureFixedPriceFactory(), new WaterFixedPricesGenerator(NomenclatureRepository));
			_discountsController = new OrderDiscountsController(_nomenclatureFixedPriceProvider);
			_paymentFromBankClientController =
				new PaymentFromBankClientController(_paymentItemsRepository, _orderRepository, _paymentsRepository);

			enumDiscountUnit.SetEnumItems((DiscountUnits[])Enum.GetValues(typeof(DiscountUnits)));

			var orderOrganizationProviderFactory = new OrderOrganizationProviderFactory();
			organizationProvider = orderOrganizationProviderFactory.CreateOrderOrganizationProvider();
			counterpartyContractRepository = new CounterpartyContractRepository(organizationProvider);
			counterpartyContractFactory = new CounterpartyContractFactory(organizationProvider, counterpartyContractRepository);
			_orderParametersProvider = new OrderParametersProvider(new ParametersProvider());
			_dailyNumberController = new OrderDailyNumberController(_orderRepository, UnitOfWorkFactory.GetDefaultFactory);

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<NomenclatureFixedPrice>(OnNomenclatureFixedPriceChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<DeliveryPoint, Phone>(OnDeliveryPointChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<Counterparty, Phone>(OnCounterpartyChanged);

			ConfigureTrees();
			ConfigureAcceptButtons();
			ConfigureButtonActions();
			ConfigureSendDocumentByEmailWidget();

			spinDiscount.Adjustment.Upper = 100;

			if(Entity.PreviousOrder != null) {
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
			} else
				labelPreviousOrder.Visible = false;
			hboxStatusButtons.Visible = _orderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus)
				|| Entity.OrderStatus == OrderStatus.Canceled
				|| Entity.OrderStatus == OrderStatus.Closed
				|| Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.OnLoading;

			orderEquipmentItemsView.Configure(UoWGeneric, Entity, _flyerRepository);
			orderEquipmentItemsView.OnDeleteEquipment += OrderEquipmentItemsView_OnDeleteEquipment;

			//Подписывемся на изменения листов для засеривания клиента
			Entity.ObservableOrderDocuments.ListChanged += ObservableOrderDocuments_ListChanged;
			Entity.ObservableOrderDocuments.ElementRemoved += ObservableOrderDocuments_ElementRemoved;
			Entity.ObservableOrderDocuments.ElementAdded += ObservableOrderDocuments_ElementAdded;
			Entity.ObservableOrderDocuments.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableFinalOrderService.ElementAdded += Entity_UpdateClientCanChange;
			Entity.ObservableInitialOrderService.ElementAdded += Entity_UpdateClientCanChange;

			Entity.ObservableOrderItems.ElementAdded += Entity_ObservableOrderItems_ElementAdded;
			Entity.ObservableOrderItems.ElementRemoved += ObservableOrderItems_ElementRemoved;

			//Подписывемся на изменения листа для сокрытия колонки промо-наборов
			Entity.ObservablePromotionalSets.ListChanged += ObservablePromotionalSets_ListChanged;
			Entity.ObservablePromotionalSets.ElementAdded += ObservablePromotionalSets_ElementAdded;
			Entity.ObservablePromotionalSets.ElementRemoved += ObservablePromotionalSets_ElementRemoved;

			//Подписываемся на изменение товара, для обновления количества оборудования в доп. соглашении
			Entity.ObservableOrderItems.ElementChanged += ObservableOrderItems_ElementChanged_ChangeCount;
			Entity.ObservableOrderEquipments.ElementChanged += ObservableOrderEquipments_ElementChanged_ChangeCount;

			Entity.ObservableOrderDepositItems.ElementAdded += ObservableOrderDepositItemsOnElementAdded;
			Entity.ObservableOrderDepositItems.ElementRemoved += ObservableOrderDepositItemsOnElementRemoved;

			Entity.ObservableOrderEquipments.ElementAdded += ObservableOrderEquipmentsOnElementAdded;
			Entity.ObservableOrderEquipments.ElementRemoved += ObservableOrderEquipmentsOnElementRemoved;

			enumSignatureType.ItemsEnum = typeof(OrderSignatureType);

			enumSignatureType.Binding.AddBinding(Entity, s => s.SignatureType, w => w.SelectedItem).InitializeFromSource();

			labelCreationDateValue.Binding.AddFuncBinding(Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp).InitializeFromSource();

			ylabelOrderStatus.Binding.AddFuncBinding(Entity, e => e.OrderStatus.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelOrderAddressType.Binding.AddFuncBinding(Entity, e => "Тип адреса: " + e.OrderAddressType.GetEnumTitle(), w => w.LabelProp).InitializeFromSource();
			ylabelNumber.Binding.AddFuncBinding(Entity, e => e.Code1c + (e.DailyNumber.HasValue ? $" ({e.DailyNumber})" : ""), w => w.LabelProp).InitializeFromSource();

			enumDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDocumentType.Binding.AddBinding(Entity, s => s.DocumentType, w => w.SelectedItem).InitializeFromSource();

			chkContractCloser.Binding.AddBinding(Entity, c => c.IsContractCloser, w => w.Active).InitializeFromSource();

			chkCommentForDriver.Binding.AddBinding(Entity, c => c.HasCommentForDriver, w => w.Active).InitializeFromSource();

			specialListCmbOurOrganization.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOurOrganization.Binding.AddBinding(Entity, o => o.OurOrganization, w => w.SelectedItem).InitializeFromSource();
			specialListCmbOurOrganization.Sensitive = _canSetOurOrganization;
			specialListCmbOurOrganization.ItemSelected += OnOurOrganisationsItemSelected;

			pickerDeliveryDate.Binding.AddBinding(Entity, s => s.DeliveryDate, w => w.DateOrNull).InitializeFromSource();
			pickerDeliveryDate.DateChanged += PickerDeliveryDate_DateChanged;
			pickerBillDate.Visible = labelBillDate.Visible = Entity.PaymentType == PaymentType.cashless;
			pickerBillDate.Binding.AddBinding(Entity, s => s.BillDate, w => w.DateOrNull).InitializeFromSource();

			textComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();
			textCommentsLogistic.Binding.AddBinding(Entity, s => s.CommentLogist, w => w.Buffer.Text).InitializeFromSource();

			checkSelfDelivery.Binding.AddBinding(Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource();
			checkPayAfterLoad.Binding.AddBinding(Entity, s => s.PayAfterShipment, w => w.Active).InitializeFromSource();
			checkDelivered.Binding.AddBinding(Entity, s => s.Shipped, w => w.Active).InitializeFromSource();
			ylabelloadAllowed.Binding.AddFuncBinding(Entity, s => s.LoadAllowedBy != null ? s.LoadAllowedBy.ShortName : string.Empty, w => w.Text).InitializeFromSource();
			entryBottlesToReturn.ValidationMode = ValidationType.numeric;
			entryBottlesToReturn.Binding.AddBinding(Entity, e => e.BottlesReturn, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			entryBottlesToReturn.Changed += OnEntryBottlesToReturnChanged;

			yChkActionBottle.Binding.AddBinding(Entity, e => e.IsBottleStock, w => w.Active).InitializeFromSource();
			yEntTareActBtlFromClient.ValidationMode = ValidationType.numeric;
			yEntTareActBtlFromClient.Binding.AddBinding(Entity, e => e.BottlesByStockCount, w => w.Text, new IntToStringConverter()).InitializeFromSource();
			yEntTareActBtlFromClient.Changed += OnYEntTareActBtlFromClientChanged;

			if(Entity.OrderStatus == OrderStatus.Closed) {
				entryTareReturned.Text = new BottlesRepository().GetEmptyBottlesFromClientByOrder(UoW, NomenclatureRepository, Entity).ToString();
				entryTareReturned.Visible = lblTareReturned.Visible = true;
			}

			entryTrifle.ValidationMode = ValidationType.numeric;
			entryTrifle.Binding.AddBinding(Entity, e => e.Trifle, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

			ylabelContract.Binding.AddFuncBinding(Entity, e => e.Contract != null && e.Contract.Organization != null ? e.Contract.Title + " (" + e.Contract.Organization.FullName + ")" : string.Empty, w => w.Text).InitializeFromSource();

			OldFieldsConfigure();

			entOnlineOrder.ValidationMode = ValidationType.numeric;
			entOnlineOrder.Binding.AddBinding(Entity, e => e.OnlineOrder, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			
			var excludedPaymentFromIds = new[]
			{
				_orderParametersProvider.PaymentByCardFromSmsId,
				_orderParametersProvider.GetPaymentByCardFromAvangardId,
				_orderParametersProvider.GetPaymentByCardFromFastPaymentServiceId,
				_orderParametersProvider.GetPaymentByCardFromSiteByQrCode,
				_orderParametersProvider.PaymentByCardFromOnlineStoreId
			};
			if(Entity.PaymentByCardFrom == null || !excludedPaymentFromIds.Contains(Entity.PaymentByCardFrom.Id))
			{
				ySpecPaymentFrom.ItemsList =
					UoW.Session.QueryOver<PaymentFrom>()
						.WhereRestrictionOn(x => x.Id).Not.IsIn(excludedPaymentFromIds).List();
			}
			else
			{
				ySpecPaymentFrom.ItemsList = UoW.GetAll<PaymentFrom>();
			}

			ySpecPaymentFrom.Binding.AddBinding(Entity, e => e.PaymentByCardFrom, w => w.SelectedItem).InitializeFromSource();

			enumTax.ItemsEnum = typeof(TaxType);
			Enum[] hideTaxTypeEnums = { TaxType.None };
			enumTax.AddEnumToHideList(hideTaxTypeEnums);
			enumTax.ChangedByUser += (sender, args) => { Entity.Client.TaxType = (TaxType)enumTax.SelectedItem; };

			var counterpartyFilter = new CounterpartyJournalFilterViewModel() { IsForRetail = this.IsForRetail, RestrictIncludeArchive = false };
			entityVMEntryClient.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<CounterpartyJournalViewModel>(typeof(Counterparty),
				() => new CounterpartyJournalViewModel(counterpartyFilter, UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices))
			);
			entityVMEntryClient.Binding.AddBinding(Entity, s => s.Client, w => w.Subject).InitializeFromSource();
			entityVMEntryClient.CanEditReference = true;

			var roboatsSettings = new RoboatsSettings(_parametersProvider);
			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			var deliveryScheduleRepository = new DeliveryScheduleRepository();
			var fileDialogService = new FileDialogService();
			var _roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService, ServicesConfig.CommonServices.CurrentPermissionService);
			var deliveryScheduleJournalFactory = new DeliveryScheduleJournalFactory(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices, deliveryScheduleRepository, _roboatsViewModelFactory);
			entryDeliverySchedule.SetEntityAutocompleteSelectorFactory(deliveryScheduleJournalFactory);
			entryDeliverySchedule.Binding.AddBinding(Entity, s => s.DeliverySchedule, w => w.Subject).InitializeFromSource();
			entryDeliverySchedule.CanEditReference = true;

			ybuttonFastDeliveryCheck.Clicked += OnButtonFastDeliveryCheckClicked;

			ycheckFastDelivery.Binding.AddBinding(Entity, e => e.IsFastDelivery, w => w.Active).InitializeFromSource();
			ycheckFastDelivery.Toggled += OnCheckFastDeliveryToggled;

			evmeAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			evmeAuthor.Sensitive = false;

			evmeDeliveryPoint.Binding.AddBinding(Entity, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
			evmeDeliveryPoint.CanEditReference = true;

			evmeDeliveryPoint.ChangedByUser += (s, e) => {
				if(Entity?.DeliveryPoint == null) {
					return;
				}
				if(!Entity.DeliveryPoint.IsActive
					&& !MessageDialogHelper.RunQuestionDialog(
						"Данный адрес деактивирован, вы уверены, что хотите выбрать его?")
				) {
					Entity.DeliveryPoint = null;
				}
			};

			chkContractCloser.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer");

			buttonViewDocument.Sensitive = false;
			btnDeleteOrderItem.Sensitive = false;
			ntbOrderEdit.ShowTabs = false;
			ntbOrderEdit.Page = 0;
			ntbOrder.ShowTabs = false;
			ntbOrder.Page = 0;

			enumPaymentType.ItemsEnum = typeof(PaymentType);
			enumPaymentType.Binding.AddBinding(Entity, s => s.PaymentType, w => w.SelectedItem).InitializeFromSource();
			SetSensitivityOfPaymentType();

			enumAddRentButton.ItemsEnum = typeof(RentType);
			enumAddRentButton.EnumItemClicked += (sender, e) => AddRent((RentType)e.ItemEnum);

			checkSelfDelivery.Toggled += (sender, e) => {
				entryDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active;
				ybuttonFastDeliveryCheck.Sensitive = ycheckFastDelivery.Sensitive = !checkSelfDelivery.Active && Entity.CanChangeFastDelivery;
				lblDeliveryPoint.Sensitive = evmeDeliveryPoint.Sensitive = !checkSelfDelivery.Active;
				buttonAddMaster.Sensitive = !checkSelfDelivery.Active;

				if(Entity.SelfDelivery)
				{
					enumPaymentType.AddEnumToHideList(PaymentType.Terminal);
				}
				else
				{
					enumPaymentType.RemoveEnumFromHideList(PaymentType.Terminal);
				}

				Entity.UpdateClientDefaultParam(UoW, counterpartyContractRepository, organizationProvider, counterpartyContractFactory);
				enumPaymentType.SelectedItem = Entity.PaymentType;

				if((Entity.DeliveryPoint != null || Entity.SelfDelivery) && Entity.OrderStatus == OrderStatus.NewOrder)
				{
					OnFormOrderActions();
				}
				UpdateOrderAddressTypeWithUI();
			};

			dataSumDifferenceReason.Binding.AddBinding(Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource();

			spinSumDifference.Binding.AddBinding(Entity, e => e.ExtraMoney, w => w.ValueAsDecimal).InitializeFromSource();

			labelSum.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderSum), w => w.LabelProp).InitializeFromSource();
			labelCashToReceive.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderCashSum), w => w.LabelProp).InitializeFromSource();

			buttonCopyManagerComment.Clicked += OnButtonCopyManagerCommentClicked;
			textManagerComments.Binding.AddBinding(Entity, e => e.CommentManager, w => w.Buffer.Text).InitializeFromSource();
			textDriverCommentFromMobile.Binding.AddBinding(Entity, e => e.DriverMobileAppComment, w => w.Buffer.Text).InitializeFromSource();

			enumDiverCallType.ItemsEnum = typeof(DriverCallType);
			enumDiverCallType.Binding.AddBinding(Entity, e => e.DriverCallType, w => w.SelectedItem).InitializeFromSource();
			driverCallId.Binding.AddFuncBinding(Entity, e => e.DriverCallId == null ? "" : e.DriverCallId.ToString(), w => w.LabelProp).InitializeFromSource();

			ySpecCmbNonReturnReason.ItemsList = UoW.Session.QueryOver<NonReturnReason>().List();
			ySpecCmbNonReturnReason.Binding.AddBinding(Entity, e => e.TareNonReturnReason, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbNonReturnReason.ItemSelected += (sender, e) => Entity.IsTareNonReturnReasonChangedByUser = true;

			if(Entity.DeliveryPoint == null && !string.IsNullOrWhiteSpace(Entity.Address1c)) {
				var deliveryPoint = Counterparty.DeliveryPoints.FirstOrDefault(d => d.Address1c == Entity.Address1c);
				if(deliveryPoint != null)
					Entity.DeliveryPoint = deliveryPoint;
			}

			OrderItemEquipmentCountHasChanges = false;
			ShowOrderColumnInDocumentsList();

			SetSensitivityOfPaymentType();
			depositrefunditemsview.Configure(UoWGeneric, Entity);
			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = _discountReasons;
			ycomboboxReason.ItemSelected += OnYComboBoxReasonItemSelected;

			yCmbReturnTareReasonCategories.SetRenderTextFunc<ReturnTareReasonCategory>(x => x.Name);
			yCmbReturnTareReasonCategories.ItemsList = UoW.Session.QueryOver<ReturnTareReasonCategory>().List();
			yCmbReturnTareReasonCategories.Binding.AddBinding(Entity, e => e.ReturnTareReasonCategory, w => w.SelectedItem).InitializeFromSource();
			yCmbReturnTareReasonCategories.Changed += YCmbReturnTareReasonCategoriesOnChanged;
			HboxReturnTareReasonCategoriesShow();

			yCmbReturnTareReasons.SetRenderTextFunc<ReturnTareReason>(x => x.Name);

			if(Entity.ReturnTareReasonCategory != null)
				ChangeHboxReasonsVisibility();

			yCmbReturnTareReasons.Binding.AddBinding(Entity, e => e.ReturnTareReason, w => w.SelectedItem).InitializeFromSource();

			yCmbPromoSets.SetRenderTextFunc<PromotionalSet>(x => x.ShortTitle);
			yCmbPromoSets.ItemSelected += YCmbPromoSets_ItemSelected;

			bool showEshop = Entity.EShopOrder == null;
			labelEShop.Visible = !showEshop;
			yvalidatedentryEShopOrder.ValidationMode = ValidationType.numeric;
			yvalidatedentryEShopOrder.Binding.AddBinding(Entity, c => c.EShopOrder, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();
			yvalidatedentryEShopOrder.Visible = !showEshop;

			chkAddCertificates.Binding.AddBinding(Entity, c => c.AddCertificates, w => w.Active).InitializeFromSource();

			ToggleVisibilityOfDeposits(Entity.ObservableOrderDepositItems.Any());
			SetDiscountEditable();
			SetDiscountUnitEditable();

			spinSumDifference.Hide();
			labelSumDifference.Hide();
			dataSumDifferenceReason.Hide();
			labelSumDifferenceReason.Hide();

			UpdateUIState();

			yChkActionBottle.Toggled += (sender, e) => {
				Entity.RecalculateStockBottles(_orderParametersProvider);
				ControlsActionBottleAccessibility();
				ycomboboxReason.Sensitive = !yChkActionBottle.Active;
				SetDiscountUnitEditable();
				SetDiscountEditable();
			};
			ycheckContactlessDelivery.Binding.AddBinding(Entity, e => e.ContactlessDelivery, w => w.Active).InitializeFromSource();
			
			ycheckPaymentBySms.Toggled += OnCheckPaymentBySmsToggled;
			chkPaymentByQr.Toggled += OnCheckPaymentByQrToggled;
			
			ycheckPaymentBySms.Binding.AddBinding(Entity, e => e.PaymentBySms, w => w.Active).InitializeFromSource();
			chkPaymentByQr.Binding.AddBinding(Entity, e => e.PaymentByQr, w => w.Active).InitializeFromSource();

			UpdateOrderAddressTypeWithUI();

			Entity.InteractiveService = ServicesConfig.InteractiveService;

			Entity.PropertyChanged += (sender, args) =>
			{
				switch(args.PropertyName)
				{
					case nameof(Order.OrderStatus):
						CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.OrderStatus));
						break;
					case nameof(Order.Contract):
						CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.Contract));
						OnContractChanged();
						break;
					case nameof(Order.Client):
						UpdateAvailableEnumSignatureTypes();
						if(Entity.Client != null && Entity.Client.IsChainStore && !Entity.OrderItems.Any(x => x.IsMasterNomenclature))
						{
							Entity.OrderAddressType = OrderAddressType.ChainStore;
						}
						UpdateOrderAddressTypeWithUI();
						break;
					case nameof(Entity.OrderAddressType):
						UpdateOrderAddressTypeWithUI();
						break;
					case nameof(Entity.Client.IsChainStore):
						UpdateOrderAddressTypeWithUI();
						break;
				}
			};
			OnContractChanged();

			if(Entity != null && Entity.Id != 0) {
				Entity.CheckDocumentExportPermissions();
			}

			ybuttonToStorageLogicAddressType.Sensitive = ybuttonToDeliveryAddressType.Sensitive =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_order_address_type");

			UpdateAvailableEnumSignatureTypes();
		}

		private void OnCheckPaymentBySmsToggled(object sender, EventArgs e)
		{
			if(Entity.PaymentBySms)
			{
				chkPaymentByQr.Visible = chkPaymentByQr.Active = false;
			}
			else
			{
				chkPaymentByQr.Visible = true;
			}
		}
		
		private void OnCheckPaymentByQrToggled(object sender, EventArgs e)
		{
			if(Entity.PaymentByQr)
			{
				ycheckPaymentBySms.Visible = ycheckPaymentBySms.Active = false;
			}
			else
			{
				ycheckPaymentBySms.Visible = true;
			}
		}

		private void UpdateAvailableEnumSignatureTypes()
		{
			var signatureTranscriptType = new object[] { OrderSignatureType.SignatureTranscript };
			if(Entity.Client?.IsForRetail ?? false)
			{
				while(enumSignatureType.HiddenItems.Contains(OrderSignatureType.SignatureTranscript))
				{
					enumSignatureType.RemoveEnumFromHideList(signatureTranscriptType);
				}
			}
			else
			{
				if(!enumSignatureType.HiddenItems.Contains(OrderSignatureType.SignatureTranscript))
				{
					enumSignatureType.AddEnumToHideList(signatureTranscriptType);
				}
			}
			enumSignatureType.Binding.InitializeFromSource();
		}

		private void OnCheckFastDeliveryToggled(object sender, EventArgs e)
		{
			if(ycheckFastDelivery.Active)
			{
				if(Entity.DeliverySchedule?.Id != _deliveryRulesParametersProvider.FastDeliveryScheduleId)
				{
					Entity.DeliverySchedule = UoW.GetById<DeliverySchedule>(_deliveryRulesParametersProvider.FastDeliveryScheduleId);
				}

				Entity.AddFastDeliveryNomenclatureIfNeeded();
			}

			if(!ycheckFastDelivery.Active)
			{
				if(Entity.DeliverySchedule?.Id == _deliveryRulesParametersProvider.FastDeliveryScheduleId)
				{
					Entity.DeliverySchedule = null;
				}

				Entity.RemoveFastDeliveryNomenclature();
			}
		}

		private void OnButtonFastDeliveryCheckClicked(object sender, EventArgs e)
		{
			if(!Entity.DeliveryDate.HasValue || Entity.DeliveryDate.Value.Date != DateTime.Now.Date)
			{
				MessageDialogHelper.RunWarningDialog("Доставка за час возможна только на текущую дату");
				return;
			}

			if(!Order.PaymentTypesFastDeliveryAvailableFor.Contains(Entity.PaymentType))
			{
				MessageDialogHelper.RunWarningDialog(
					$"Нельзя выбрать доставку за час для заказа с формой оплаты '{Entity.PaymentType.GetEnumTitle()}'");
				return;
			}

			if(Entity.DeliveryPoint == null)
			{
				MessageDialogHelper.RunWarningDialog("Перед выбором доставки за час необходимо выбрать точку доставки");
				return;
			}

			if(Entity.DeliveryPoint.Longitude == null || Entity.DeliveryPoint.Latitude == null)
			{
				MessageDialogHelper.RunWarningDialog("Для выбора доставки за час необходимо корректно заполнить координаты точки доставки");
				return;
			}

			var district = Entity.DeliveryPoint.District;

			if(district == null)
			{
				MessageDialogHelper.RunWarningDialog($"Для точки доставки не указан район");
				return;
			}

			if(district.TariffZone == null)
			{
				MessageDialogHelper.RunWarningDialog($"Для района точки доставки не указана тарифная зона");
				return;
			}

			if(!district.TariffZone.IsFastDeliveryAvailableAtCurrentTime)
			{
				MessageDialogHelper.RunWarningDialog(
					$"По данной тарифной зоне не работает доставка за час либо закончилось время работы - попробуйте в {district.TariffZone.FastDeliveryTimeFrom:hh\\:mm}");
				return;
			}

			if(Entity.Total19LBottlesToDeliver == 0)
			{
				MessageDialogHelper.RunWarningDialog("В доставке за час нет 19л воды!!!");
				return;
			}

			var fastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDelivery(
				UoW,
				(double)Entity.DeliveryPoint.Latitude.Value,
				(double)Entity.DeliveryPoint.Longitude.Value,
				isGetClosestByRoute: false,
				_deliveryRulesParametersProvider,
				Entity.GetAllGoodsToDeliver(),
				Entity
			);

			var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(UnitOfWorkFactory.GetDefaultFactory);
			fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(fastDeliveryAvailabilityHistory);

			var fastDeliveryVerificationViewModel = new FastDeliveryVerificationViewModel(fastDeliveryAvailabilityHistory);
			MainClass.MainWin.NavigationManager.OpenViewModel<FastDeliveryVerificationDetailsViewModel, FastDeliveryVerificationViewModel>(
				null, fastDeliveryVerificationViewModel);
		}

		private void OnOurOrganisationsItemSelected(object sender, ItemSelectedEventArgs e)
		{
			Entity.UpdateOrCreateContract(UoW, counterpartyContractRepository, counterpartyContractFactory);
		}

		private readonly Label torg12OnlyLabel = new Label("Торг12 (2шт.)");

		private void OnContractChanged()
		{
			if(Entity.IsCashlessPaymentTypeAndOrganizationWithoutVAT && hboxDocumentType.Children.Contains(enumDocumentType))
			{
				hboxDocumentType.Remove(enumDocumentType);
				hboxDocumentType.Add(torg12OnlyLabel);
				torg12OnlyLabel.Show();
			}
			else if(!Entity.IsCashlessPaymentTypeAndOrganizationWithoutVAT && hboxDocumentType.Children.Contains(torg12OnlyLabel))
			{
				hboxDocumentType.Remove(torg12OnlyLabel);
				hboxDocumentType.Add(enumDocumentType);
			}
		}

		private void TryAddFlyers()
		{
			if(Entity.SelfDelivery
			   || Entity.OrderStatus != OrderStatus.NewOrder
			   || Entity.DeliveryPoint.District == null)
			{
				return;
			}

			var geographicGroupId = Entity.DeliveryPoint.District.GeographicGroup.Id;
			var activeFlyers = _flyerRepository.GetAllActiveFlyers(UoW);

			if(activeFlyers.Any())
			{
				_addedFlyersNomenclaturesIds = new List<int>();

				foreach(var flyer in activeFlyers)
				{
					if(!_orderRepository.CanAddFlyerToOrder(
						UoW,
						new RouteListParametersProvider(_parametersProvider),
						flyer.FlyerNomenclature.Id,
						geographicGroupId))
					{
						continue;
					}

					Entity.AddFlyerNomenclature(flyer.FlyerNomenclature);
					_addedFlyersNomenclaturesIds.Add(flyer.FlyerNomenclature.Id);
				}
			}
		}

		private void OnDeliveryPointChanged(EntityChangeEvent[] changeevents)
		{
			if(DeliveryPoint == null)
			{
				return;
			}

			var changedDeliveryPoints = changeevents.Select(x => x.Entity).OfType<DeliveryPoint>();
			var changedPhones = changeevents.Select(x => x.Entity).OfType<Phone>();

			if(changedDeliveryPoints.Any(x => x.Id == DeliveryPoint.Id)
				|| changedPhones.Any(x => x.DeliveryPoint?.Id == DeliveryPoint.Id))
			{
				RefreshDeliveryPointWithPhones();
			}
		}
		private void OnCounterpartyChanged(EntityChangeEvent[] changeevents)
		{
			if(Counterparty == null)
			{
				return;
			}

			var changedCounterparties = changeevents.Select(x => x.Entity).OfType<Counterparty>();
			var changedPhones = changeevents.Select(x => x.Entity).OfType<Phone>();

			if(changedCounterparties.Any(x => x.Id == Counterparty.Id))
			{
				RefreshCounterpartyWithPhones();
				UpdateOrderAddressTypeWithUI();
			}
			else if(changedPhones.Any(x => x.Counterparty?.Id == Counterparty.Id))
			{
				RefreshCounterpartyWithPhones();
			}
		}

		private void RefreshEntity<T>(T entity)
		{
			UoW.Session.Refresh(entity);
		}

		private void RefreshCounterpartyWithPhones()
		{
			Counterparty.ReloadChildCollection(x => x.Phones, x => x.Counterparty, UoW.Session);
			RefreshEntity(Counterparty);
		}

		private void RefreshDeliveryPointWithPhones()
		{
			DeliveryPoint.ReloadChildCollection(x => x.Phones, x => x.DeliveryPoint, UoW.Session);
			RefreshEntity(DeliveryPoint);
		}

		private void OnNomenclatureFixedPriceChanged(EntityChangeEvent[] changeevents)
		{
			var changedEntities = changeevents.Select(x => x.Entity).OfType<NomenclatureFixedPrice>();
			if (changedEntities.Any(x => x.DeliveryPoint != null && DeliveryPoint != null && x.DeliveryPoint.Id == DeliveryPoint.Id))
			{
				DeliveryPoint.ReloadChildCollection(x => x.NomenclatureFixedPrices, x => x.DeliveryPoint, UoW.Session);
				RefreshEntity(DeliveryPoint);
				CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
				return;
			}

			if(changedEntities.Any(x => x.Counterparty != null && Counterparty != null && x.Counterparty.Id == Counterparty.Id))
			{
				Counterparty.ReloadChildCollection(x => x.NomenclatureFixedPrices, x => x.Counterparty, UoW.Session);
				RefreshEntity(Counterparty);
				CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity));
				return;
			}

			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Counterparty));
		}

		void ControlsActionBottleAccessibility()
		{
			bool canAddAction = Entity.CanAddStockBottle(_orderRepository) || Entity.IsBottleStock;
			hboxBottlesByStock.Visible = canAddAction;
			lblActionBtlTareFromClient.Visible = yEntTareActBtlFromClient.Visible = yChkActionBottle.Active;
			hboxReturnTare.Visible = !canAddAction;
			yEntTareActBtlFromClient.Sensitive = canAddAction;
		}

		private void ConfigureTrees()
		{
			var colorBlack = new Gdk.Color(0, 0, 0);
			var colorBlue = new Gdk.Color(0, 0, 0xff);
			var colorGreen = new Gdk.Color(0, 0xff, 0);
			var colorWhite = new Gdk.Color(0xff, 0xff, 0xff);
			var colorLightYellow = new Gdk.Color(0xe1, 0xd6, 0x70);
			var colorLightRed = new Gdk.Color(0xff, 0x66, 0x66);

			_discountReasons = _canChoosePremiumDiscount
				? _discountReasonRepository.GetActiveDiscountReasons(UoW)
				: _discountReasonRepository.GetActiveDiscountReasonsWithoutPremiums(UoW);

			treeItems.CreateFluentColumnsConfig<OrderItem>()
				.AddColumn("№")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => Entity.OrderItems.IndexOf(node) + 1)
				.AddColumn("Номенклатура")
					.SetTag(nameof(Nomenclature))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.NomenclatureString)
				.AddColumn(!_orderRepository.GetStatusesForActualCount(Entity).Contains(Entity.OrderStatus) ? "Кол-во" : "Кол-во [Факт]")
					.SetTag("Count")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Count)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) => c.Editable = node.CanEditAmount).WidthChars(10)
					.EditedEvent(OnCountEdited)
					.AddTextRenderer(node => node.ActualCount.HasValue
						? string.Format("[{0:" + $"F{(node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)}" + "}]",
							node.ActualCount)
						: string.Empty)
					.AddTextRenderer(node => node.CanShowReturnedCount
						? string.Format("({0:" + $"F{(node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)}" + "})",
							node.ReturnedCount)
						: string.Empty)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Аренда")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.RentCount).Editing().Digits(0)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Visible = node.RentVisible)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice)
					.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) => {
						if(Entity.OrderStatus == OrderStatus.NewOrder || (Entity.OrderStatus == OrderStatus.WaitForPayment && !Entity.SelfDelivery))//костыль. на Win10 не видна цветная цена, если виджет засерен
						{
							c.ForegroundGdk = colorBlack;
							var fixedPrice = Order.GetFixedPriceOrNull(node.Nomenclature);
							if(fixedPrice != null) {
								c.ForegroundGdk = colorGreen;
							} else if(node.IsUserPrice && Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category)) {
								c.ForegroundGdk = colorBlue;
							}
						}
					})
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS ?? 0))
					.AddSetter((c, n) => c.Visible = Entity.PaymentType == PaymentType.cashless)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
					.AddSetter((c, n) => {
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							c.Text = CurrencyWorks.GetShortCurrencyString(n.OriginalSum);
						}
					)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ManualChangingDiscount)
					.AddSetter((c, n) => c.Editable = _canChangeDiscountValue)
					.AddSetter(
						(c, n) => c.Adjustment = n.IsDiscountInMoney
									? new Adjustment(0, 0, (double)(n.Price * n.CurrentCount), 1, 100, 1)
									: new Adjustment(0, 0, 100, 1, 100, 1)
					)
					.AddSetter((c, n) =>
					{
						if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
						{
							c.Text = n.ManualChangingOriginalDiscount.ToString();
						}
					})
					.Digits(2)
					.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
					.AddSetter((c, n) => c.Activatable = _canChangeDiscountValue)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(x => x.DiscountReason)
					.SetDisplayFunc(x => x.Name)
					.DynamicFillListFunc(item =>
						{
							var list = _discountReasons.Where(
								dr => _discountsController.IsApplicableDiscount(dr, item.Nomenclature)).ToList();
							return list;
						})
					.EditedEvent(OnDiscountReasonComboEdited)
					.AddSetter((cell, node) => cell.Editable = node.DiscountByStock == 0)
					.AddSetter(
						(c, n) =>
							c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null && n.PromoSet == null ? colorLightRed : colorWhite
					)
					.AddSetter((c, n) =>
						{
							if(n.PromoSet != null && n.DiscountReason == null && n.Discount > 0)
							{
								c.Text = n.PromoSet.DiscountReasonInfo;
							}
							else if(Entity.OrderStatus == OrderStatus.DeliveryCanceled || Entity.OrderStatus == OrderStatus.NotDelivered)
							{
								c.Text = n.OriginalDiscountReason?.Name ?? n.DiscountReason?.Name;
							}
						})
				.AddColumn("Промо-наборы").SetTag(nameof(Entity.PromotionalSets))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.PromoSet == null ? "" : node.PromoSet.Name)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = Entity.ObservableOrderItems;
			treeItems.Selection.Changed += TreeItems_Selection_Changed;
			treeItems.ColumnsConfig.GetColumnsByTag(nameof(Entity.PromotionalSets)).FirstOrDefault().Visible = Entity.PromotionalSets.Count > 0;

			treeDocuments.ColumnsConfig = ColumnsConfigFactory.Create<OrderDocument>()
				.AddColumn("Документ").AddTextRenderer(node => node.Name)
				.AddColumn("Дата документа").AddTextRenderer(node => node.DocumentDateText)
				.AddColumn("Заказ №").SetTag("OrderNumberColumn").AddTextRenderer(node => node.Order.Id != node.AttachedToOrder.Id ? node.Order.Id.ToString() : "")
				.AddColumn("Без рекламы").AddToggleRenderer(x => x is IAdvertisable && (x as IAdvertisable).WithoutAdvertising)
					.ChangeSetProperty(PropertyUtil.GetPropertyInfo<IAdvertisable>(x => x.WithoutAdvertising))
					.AddSetter((c, n) => c.Visible = n.Type == OrderDocumentType.Invoice || n.Type == OrderDocumentType.InvoiceContractDoc)
					.AddSetter((c, n) => c.Activatable = CanEditByPermission)
				.AddColumn("Без подписей и печати").AddToggleRenderer(x => x is ISignableDocument && (x as ISignableDocument).HideSignature)
				.Editing().ChangeSetProperty(PropertyUtil.GetPropertyInfo<ISignableDocument>(x => x.HideSignature))
				.AddSetter((c, n) => c.Visible = n is ISignableDocument)
				.AddSetter((toggle, document) =>
				{
					if (document.Type == OrderDocumentType.UPD)
					{
						toggle.Activatable = CanEditByPermission && _canEditSealAndSignatureUpd;
					}
					else
					{
						toggle.Activatable = CanEditByPermission;
					}
				}) // Сделать только для  ISignableDocument и UDP
				.AddColumn("")
				.RowCells().AddSetter<CellRenderer>((c, n) => {
					c.CellBackgroundGdk = colorWhite;
					if(n.Order.Id != n.AttachedToOrder.Id && !(c is CellRendererToggle)) {
						c.CellBackgroundGdk = colorLightYellow;
					}
				})
				.Finish();
			treeDocuments.Selection.Mode = SelectionMode.Multiple;
			treeDocuments.ItemsDataSource = Entity.ObservableOrderDocuments;
			treeDocuments.Selection.Changed += Selection_Changed;

			treeDocuments.RowActivated += (o, args) => OrderDocumentsOpener();

			treeServiceClaim.ColumnsConfig = ColumnsConfigFactory.Create<ServiceClaim>()
				.AddColumn("Статус заявки").SetDataProperty(node => node.Status.GetEnumTitle())
				.AddColumn("Номенклатура оборудования").SetDataProperty(node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn("Серийный номер").SetDataProperty(node => node.Equipment != null && node.Equipment.Nomenclature.IsSerial ? node.Equipment.Serial : "-")
				.AddColumn("Причина").SetDataProperty(node => node.Reason)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.Foreground = n.RowColor)
				.Finish();

			treeServiceClaim.ItemsDataSource = Entity.ObservableInitialOrderService;
			treeServiceClaim.Selection.Changed += TreeServiceClaim_Selection_Changed;
		}

		private void OnCountEdited(object o, EditedArgs args)
		{
			var path = new TreePath(args.Path);
			treeItems.YTreeModel.GetIter(out var iter, path);
			treeItems.YTreeModel.Adapter.EmitRowChanged(path, iter);
		}

		private void OnDiscountReasonComboEdited(object o, EditedArgs args)
		{
			var index = int.Parse(args.Path);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			var previousDiscountReason = orderItem.DiscountReason;

			Application.Invoke((sender, eventArgs) =>
			{
				//Дополнительно проверяем основание скидки на null, т.к при двойном щелчке
				//комбо-бокс не откроется, но событие сработает и прилетит null
				if(orderItem.DiscountReason != null)
				{
					if(!_discountsController.SetDiscountFromDiscountReasonForOrderItem(
						orderItem.DiscountReason, orderItem, _canChangeDiscountValue, out string message))
					{
						orderItem.DiscountReason = previousDiscountReason;
					}

					if(message != null)
					{
						ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
							$"На позицию:\n№{index + 1} {message}нельзя применить скидку," +
							" т.к. она из промо-набора или на нее есть фикса.\nОбратитесь к руководителю");
					}
				}
			});
		}

		private void ConfigureAcceptButtons()
		{
			buttonAcceptOrderWithClose.Clicked += OnButtonAcceptOrderWithCloseClicked;
			buttonAcceptAndReturnToOrder.Clicked += OnButtonAcceptAndReturnToOrderClicked;
		}

		MenuItem menuItemCloseOrder = null;
		MenuItem menuItemSelfDeliveryToLoading = null;
		MenuItem menuItemSelfDeliveryPaid = null;
		MenuItem menuItemReturnToAccepted = null;

		/// <summary>
		/// Конфигурирование меню кнопок с дополнительными действиями заказа
		/// </summary>
		public void ConfigureButtonActions()
		{
			menubuttonActions.MenuAllocation = ButtonMenuAllocation.Top;
			menubuttonActions.MenuAlignment = ButtonMenuAlignment.Right;
			Menu menu = new Menu();

			menuItemCloseOrder = new MenuItem("Закрыть без доставки");
			menuItemCloseOrder.Activated += OnButtonCloseOrderClicked;
			menu.Add(menuItemCloseOrder);

			menuItemReturnToAccepted = new MenuItem("Вернуть в Принят");
			menuItemReturnToAccepted.Activated += OnButtonReturnToAcceptedClicked;
			menu.Add(menuItemReturnToAccepted);

			menuItemSelfDeliveryToLoading = new MenuItem("Самовывоз на погрузку");
			menuItemSelfDeliveryToLoading.Activated += OnButtonSelfDeliveryToLoadingClicked;
			menu.Add(menuItemSelfDeliveryToLoading);

			menuItemSelfDeliveryPaid = new MenuItem("Принять оплату самовывоза");
			menuItemSelfDeliveryPaid.Activated += OnButtonSelfDeliveryAcceptPaidClicked;
			menu.Add(menuItemSelfDeliveryPaid);

			menubuttonActions.Menu = menu;
			menubuttonActions.LabelXAlign = 0.5f;
			menu.ShowAll();
		}

		private void ConfigureSendDocumentByEmailWidget()
		{
			SendDocumentByEmailViewModel =
				new SendDocumentByEmailViewModel(_emailRepository,  new EmailParametersProvider(new ParametersProvider()), _currentEmployee, ServicesConfig.InteractiveService);
			var sendEmailView = new SendDocumentByEmailView(SendDocumentByEmailViewModel);
			hbox19.Add(sendEmailView);
			sendEmailView.Show();
		}

		/// <summary>
		/// Старые поля, оставлены для отображения информации в старых заказах. В новых скрыты.
		/// Не удаляем полностью а только скрываем, чтобы можно было увидеть адрес в старых заказах, загруженных из 1с.
		/// </summary>
		private void OldFieldsConfigure()
		{
			textTaraComments.Binding.AddBinding(Entity, e => e.InformationOnTara, w => w.Buffer.Text).InitializeFromSource();
			var tareVisible = !string.IsNullOrWhiteSpace(Entity.InformationOnTara);
			textTaraComments.Sensitive = CanEditByPermission && !string.IsNullOrWhiteSpace(Entity.InformationOnTara);

			labelTaraComments.Visible = tareVisible;
			textTaraComments.Visible = tareVisible;
			GtkScrolledWindow4.Visible = tareVisible;

			if (Entity.Client != null)
			{
				if (Entity.Client.IsChainStore)
				{
					textODZComments.Binding.AddBinding(Entity, e => e.ODZComment, w => w.Buffer.Text)
						.InitializeFromSource();
					textOPComments.Binding.AddBinding(Entity, e => e.OPComment, w => w.Buffer.Text)
						.InitializeFromSource();
				}
				else
				{
					textOPComments.Visible = false;
					labelOPComments.Visible = false;
					GtkScrolledWindow6.Visible = false;
					labelODZComments.Visible = false;
					textODZComments.Visible = false;
					GtkScrolledWindow8.Visible = false;
				}
			}

			int currentUserId = _userRepository.GetCurrentUser(UoW).Id;
			bool canChangeCommentOdz = CanEditByPermission &&
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_odz_op_comment", currentUserId);
			bool canChangeSalesDepartmentComment = CanEditByPermission &&
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_sales_department_comment",
					currentUserId);
			textODZComments.Sensitive = canChangeCommentOdz;
			textOPComments.Sensitive = canChangeSalesDepartmentComment;
		}

		#endregion

		#region Сохранение, закрытие заказа

		bool SaveOrderBeforeContinue<T>()
		{
			if(UoWGeneric.IsNew) {
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(EntityObject.GetType(), typeof(T))) {
					if(!Save())
						return false;
				} else
					return false;
			}
			return true;
		}

		private bool canClose = true;
		public bool CanClose()
		{
			if(!canClose)
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения задачи и повторите");
			return canClose;
		}

		private void SetSensitivity(bool isSensitive)
		{
			canClose = isSensitive;
			buttonSave.Sensitive = CanEditByPermission && isSensitive;
			buttonCancel.Sensitive = isSensitive;
		}

		protected bool Validate(ValidationContext validationContext)
		{
			validationContext.ServiceContainer.AddService(_orderParametersProvider);
			validationContext.ServiceContainer.AddService(_deliveryRulesParametersProvider);
			return ServicesConfig.ValidationService.Validate(Entity, validationContext);
		}

		public override bool Save()
		{
			try {
				SetSensitivity(false);
				Entity.CheckAndSetOrderIsService();

				ValidationContext validationContext = new ValidationContext(Entity);

				if(!Validate(validationContext))
				{
					return false;
				}

				if(Entity.OrderStatus == OrderStatus.NewOrder) {
					if(!MessageDialogHelper.RunQuestionDialog("Вы не подтвердили заказ. Вы уверены что хотите оставить его в качестве черновика?"))
					{
						return false;
					}
				}

				if (Entity.Id == 0 &&
					Entity.PaymentType == PaymentType.cashless) {
					Entity.OrderPaymentStatus = OrderPaymentStatus.UnPaid;
				}

				if(OrderItemEquipmentCountHasChanges) {
					MessageDialogHelper.RunInfoDialog("Было изменено количество оборудования в заказе, оно также будет изменено в дополнительном соглашении");
				}

				logger.Info("Сохраняем заказ...");

				Entity.SaveEntity(UoWGeneric, _currentEmployee, _dailyNumberController, _paymentFromBankClientController);

				if(_isNeedSendBill)
				{
					SendBillByEmail(_emailAddressForBill);
				}

				logger.Info("Ok.");
				UpdateUIState();
				return true;
			} finally {
				SetSensitivity(true);
			}
		}

		protected void OnBtnSaveCommentClicked(object sender, EventArgs e)
		{
			Entity.SaveOrderComment();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			isEditOrderClicked = true;
			EditOrder();
		}

		private void EditOrder()
		{
			if(!Entity.CanSetOrderAsEditable) {
				return;
			}
			Entity.EditOrder(CallTaskWorker);
			UpdateUIState();
		}

		private bool AcceptOrder()
		{
			if(!Entity.CanSetOrderAsAccepted)
			{
				return false;
			}

			var canContinue = Entity.DefaultWaterCheck(ServicesConfig.InteractiveService);
			if(canContinue.HasValue && !canContinue.Value)
			{
				toggleGoods.Activate();
				return false;
			}

			if(!ValidateAndFormOrder() || !CheckCertificates(canSaveFromHere: true))
			{
				return false;
			}

			PromosetDuplicateFinder promosetDuplicateFinder = new PromosetDuplicateFinder(ServicesConfig.InteractiveService);
			List<Phone> phones = new List<Phone>();
			phones.AddRange(Entity.Client.Phones);
			if(Entity.DeliveryPoint != null) {
				phones.AddRange(Entity.DeliveryPoint.Phones);
			}

			bool hasPromoInOrders = Entity.PromotionalSets.Count != 0;
			bool canBeReorderedWithoutRestriction = Entity.PromotionalSets.Any(x => x.CanBeReorderedWithoutRestriction);

			if(!canBeReorderedWithoutRestriction && Entity.OrderItems.Any(x => x.PromoSet != null))
			{
				if(!promosetDuplicateFinder.RequestDuplicatePromosets(UoW, Entity.DeliveryPoint, phones))
				{
					return false;
				}
			}
			if( hasPromoInOrders
			    && !canBeReorderedWithoutRestriction 
				&& Entity.CanUsedPromo(_promotionalSetRepository))
			{
				string message = "По этому адресу уже была ранее отгрузка промонабора на другое физ.лицо.\n" +
								 "Пожалуйста удалите промо набор или поменяйте адрес доставки.";
				MessageDialogHelper.RunWarningDialog( message );
				return false;
			}

			PrepareSendBillInformation();

			if(_emailAddressForBill == null
			   && Entity.NeedSendBill(_emailRepository)
			   && !MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?"))
			{
				return false;
			}

			RouteList routeListToAddOrderTo = null;

			if(Entity.IsFastDelivery)
			{
				if(!Entity.DeliveryDate.HasValue || Entity.DeliveryDate.Value.Date != DateTime.Now.Date)
				{
					throw new InvalidOperationException("Доставка за час возможна только на текущую дату");
				}

				if(Entity.DeliveryPoint?.Latitude == null || Entity.DeliveryPoint.Longitude == null)
				{
					throw new InvalidOperationException(
						"В доставке за час обязательно должна быть точка доставки с заполненными координатами");
				}

				var district = Entity.DeliveryPoint.District;

				if(district == null)
				{
					throw new InvalidOperationException($"Для точки доставки не указан район");
				}

				if(district.TariffZone == null)
				{
					throw new InvalidOperationException($"Для района точки доставки не указана тарифная зона");
				}

				if(!district.TariffZone.IsFastDeliveryAvailableAtCurrentTime)
				{
					MessageDialogHelper.RunWarningDialog(
						$"По данной тарифной зоне не работает доставка за час либо закончилось время работы - попробуйте в {district.TariffZone.FastDeliveryTimeFrom:hh\\:mm}");
					
					return false;
				}

				if(Entity.Total19LBottlesToDeliver == 0)
				{
					throw new InvalidOperationException("В доставке за час обязательно должна быть 19л вода");
				}

				var fastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDelivery(
					UoW,
					(double)Entity.DeliveryPoint.Latitude.Value,
					(double)Entity.DeliveryPoint.Longitude.Value,
					isGetClosestByRoute: true,
					_deliveryRulesParametersProvider,
					Entity.GetAllGoodsToDeliver(),
					Entity
				);

				var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(UnitOfWorkFactory.GetDefaultFactory);
				fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(fastDeliveryAvailabilityHistory);

				routeListToAddOrderTo = fastDeliveryAvailabilityHistory.Items
					.FirstOrDefault(x => x.IsValidToFastDelivery)
					?.RouteList;

				if(routeListToAddOrderTo == null)
				{
					var fastDeliveryVerificationViewModel = new FastDeliveryVerificationViewModel(fastDeliveryAvailabilityHistory);
					MainClass.MainWin.NavigationManager.OpenViewModel<FastDeliveryVerificationDetailsViewModel, IUnitOfWork, FastDeliveryVerificationViewModel>(
						null, UoW, fastDeliveryVerificationViewModel);

					return false;
				}
			}

			if(Contract == null && !Entity.IsLoadedFrom1C) {
				Entity.UpdateOrCreateContract(UoW, counterpartyContractRepository, counterpartyContractFactory);
			}

			Entity.AcceptOrder(_currentEmployee, CallTaskWorker);
			treeItems.Selection.UnselectAll();

			if(routeListToAddOrderTo != null)
			{
				routeListToAddOrderTo.AddAddressFromOrder(Entity);
				Entity.ChangeStatusAndCreateTasks(OrderStatus.OnTheWay, CallTaskWorker);
				Entity.UpdateDocuments();
			}

			if(!Save())
			{
				return false;
			}
			if(routeListToAddOrderTo != null && DriverApiParametersProvider.NotificationsEnabled)
			{
				NotifyDriver();
			}
			ProcessSmsNotification();
			UpdateUIState();

			return true;
		}

		private void NotifyDriver()
		{
			try
			{
				DriverApiHelper.NotifyOfFastDeliveryOrderAdded(Entity.Id);
			}
			catch(Exception e)
			{
				logger.Error(e, "Не удалось уведомить водителя о добавлении заказа с быстрой доставкой в МЛ");
			}
		}

		private void PrepareSendBillInformation()
		{
			_emailAddressForBill = Entity.GetEmailAddressForBill();

			if(_emailAddressForBill != null && Entity.NeedSendBill(_emailRepository))
			{
				_isNeedSendBill = true;
			}
			else
			{
				_isNeedSendBill = false;
			}
		}

		private void OnButtonAcceptOrderWithCloseClicked(object sender, EventArgs e)
		{
			if(AcceptOrder())
			{
				OnCloseTab(false, CloseSource.Save);
			}
		}

		private void OnButtonAcceptAndReturnToOrderClicked(object sender, EventArgs e)
		{
			AcceptOrder();
			ReturnToEditTab();
		}

		private void ProcessSmsNotification()
		{
			SmsNotifier smsNotifier = new SmsNotifier(_baseParametersProvider);
			smsNotifier.NotifyIfNewClient(Entity);
		}

		private bool ValidateAndFormOrder()
		{
			Entity.CheckAndSetOrderIsService();

			ILifetimeScope autofacScope = MainClass.AppDIContainer.BeginLifetimeScope();
			var uowFactory = autofacScope.Resolve<IUnitOfWorkFactory>();

			ValidationContext validationContext = new ValidationContext(Entity, null, new Dictionary<object, object>
			{
				{ "NewStatus", OrderStatus.Accepted },
				{ "uowFactory", uowFactory }
			});

			if(!Validate(validationContext))
			{
				autofacScope.Dispose();
				return false;
			}

			if(Entity.DeliveryPoint != null && !Entity.DeliveryPoint.CalculateDistricts(UoW).Any())
				MessageDialogHelper.RunWarningDialog("Точка доставки не попадает ни в один из наших районов доставки. Пожалуйста, согласуйте стоимость доставки с руководителем и клиентом.");

			OnFormOrderActions();
			autofacScope.Dispose();
			return true;
		}

		/// <summary>
		/// Действия обрабатываемые при формировании заказа
		/// </summary>
		private void OnFormOrderActions()
		{
			//проверка и добавление платной доставки в товары
			Entity.CalculateDeliveryPrice();
		}

		/// <summary>
		/// Ручное закрытие заказа
		/// </summary>
		protected void OnButtonCloseOrderClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Accepted && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_orders")) {
				if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите закрыть заказ?")) {
					return;
				}

				Entity.UpdateBottlesMovementOperationWithoutDelivery(
					UoW, _baseParametersProvider, new RouteListItemRepository(), new CashRepository());
				Entity.UpdateDepositOperations(UoW);

				Entity.ChangeStatusAndCreateTasks(OrderStatus.Closed, CallTaskWorker);
				foreach(OrderItem i in Entity.ObservableOrderItems) {
					i.ActualCount = i.Count;
				}
			}
			UpdateUIState();
		}
		/// <summary>
		/// Возврат в принят из ручного закрытия
		/// </summary>
		protected void OnButtonReturnToAcceptedClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted) {
				if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите вернуть заказ в статус \"Принят\"?")) {
					return;
				}
				Entity.ChangeStatusAndCreateTasks(OrderStatus.Accepted, CallTaskWorker);
			}
			UpdateUIState();
		}

		/// <summary>
		/// Отправка самовывоза на погрузку
		/// </summary>
		protected void OnButtonSelfDeliveryToLoadingClicked(object sender, EventArgs e)
		{
			Entity.SelfDeliveryToLoading(_currentEmployee, ServicesConfig.CommonServices.CurrentPermissionService, CallTaskWorker);
			UpdateUIState();
		}

		/// <summary>
		/// Принятие оплаты самовывоза
		/// </summary>
		protected void OnButtonSelfDeliveryAcceptPaidClicked(object sender, EventArgs e)
		{
			Entity.SelfDeliveryAcceptCashlessPaid(CallTaskWorker);
			UpdateUIState();
		}

		#endregion

		#region Документы заказа

		protected void OnBtnRemExistingDocumentClicked(object sender, EventArgs e)
		{
			if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите удалить выделенные документы?")) return;
			var documents = treeDocuments.GetSelectedObjects<OrderDocument>();
			var notDeletedDocs = Entity.RemoveAdditionalDocuments(documents);
			if(notDeletedDocs != null && notDeletedDocs.Any()) {
				string strDocuments = "";
				foreach(OrderDocument doc in notDeletedDocs) {
					strDocuments += string.Format("\n\t{0}", doc.Name);
				}
				MessageDialogHelper.RunWarningDialog(string.Format("Документы{0}\nудалены не были, так как относятся к текущему заказу.", strDocuments));
			}
		}

		protected void OnBtnAddM2ProxyForThisOrderClicked(object sender, EventArgs e)
		{
			ValidationContext validationContext = new ValidationContext(Entity);
			
			if(Validate(validationContext) && SaveOrderBeforeContinue<M2ProxyDocument>())
			{
				TabParent.OpenTab(
					DialogHelper.GenerateDialogHashName<M2ProxyDocument>(0),
					() => OrmMain.CreateObjectDialog(typeof(M2ProxyDocument), Entity)
				);
			}
		}

		protected void OnButtonAddExistingDocumentClicked(object sender, EventArgs e)
		{
			if(Entity.Client == null) {
				MessageDialogHelper.RunWarningDialog("Для добавления дополнительных документов должен быть выбран клиент.");
				return;
			}

			TabParent.AddSlaveTab(this, new AddExistingDocumentsDlg(UoWGeneric, Entity.Client)
			);
		}

		protected void OnButtonViewDocumentClicked(object sender, EventArgs e)
		{
			OrderDocumentsOpener();
		}

		/// <summary>
		/// Открытие соответствующего документу заказа окна.
		/// </summary>
		void OrderDocumentsOpener()
		{
			if(!treeDocuments.GetSelectedObjects().Any())
				return;

			var rdlDocs = treeDocuments.GetSelectedObjects()
									   .OfType<PrintableOrderDocument>()
									   .Where(d => d.PrintType == PrinterType.RDL)
									   .ToList();

			if(rdlDocs.Any()) {
				string whatToPrint = rdlDocs.ToList().Count > 1
											? "документов"
											: "документа \"" + rdlDocs.Cast<OrderDocument>().First().Type.GetEnumTitle() + "\"";
				if(CanEditByPermission && UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
				{
					UoWGeneric.Save();
				}
				rdlDocs.ForEach(
					doc => {
						if(doc is IPrintableRDLDocument)
							TabParent.AddTab(QSReport.DocumentPrinter.GetPreviewTab(doc as IPrintableRDLDocument), this, false);
					}
				);
			}

			var odtDocs = treeDocuments.GetSelectedObjects()
									   .OfType<PrintableOrderDocument>()
									   .Where(d => d.PrintType == PrinterType.ODT)
									   .ToList();
			if(odtDocs.Any())
				foreach(var doc in odtDocs) {
					if(doc is OrderContract)
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<CounterpartyContract>((doc as OrderContract).Contract.Id),
							() => {
								var dialog = OrmMain.CreateObjectDialog((doc as OrderContract).Contract);
								if(dialog != null)
									(dialog as IEditableDialog).IsEditable = false;
								return dialog;
							}
						);
					else if(doc is OrderM2Proxy) {
						if(doc.Id == 0) {
							MessageDialogHelper.RunInfoDialog("Перед просмотром документа необходимо сохранить заказ");
							return;
						}
						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<M2ProxyDocument>((doc as OrderM2Proxy).M2Proxy.Id),
							() => {
								var dialog = OrmMain.CreateObjectDialog((doc as OrderM2Proxy).M2Proxy);
								if(dialog != null)
									(dialog as IEditableDialog).IsEditable = false;
								return dialog;
							}
						);
					}
				}
		}

		/// <summary>
		/// Распечатать документы.
		/// </summary>
		/// <param name="docList">Лист документов.</param>
		private void PrintDocuments(IList<OrderDocument> docList)
		{
			if(docList.Any())
			{
				_documentPrinter.PrintAllDocuments(docList.OfType<PrintableOrderDocument>());
			}
		}

		/// <summary>
		/// Добавление сертификатов, проверка наличия актуального и, в случае провала при проверке, выдача предупреждения.
		/// </summary>
		/// <returns><c>true</c>, если все сертификаты актуальны, либо не найден ни один,
		/// <c>false</c> если все найденные сертификаты архивные, либо просрочены</returns>
		/// <param name="canSaveFromHere"><c>true</c>Если вызывается проверка при сохранении заказа
		/// и <c>false</c> если после проверки нужно только предупреждение</param>
		bool CheckCertificates(bool canSaveFromHere = false)
		{
			Entity.UpdateCertificates(out List<Nomenclature> needUpdateCertificatesFor);
			if(needUpdateCertificatesFor.Any()) {
				string msg = "Для следующих номенклатур устарели сертификаты продукции и добавлены в список документов не были:\n\n";
				msg += string.Join(
					"\t*",
					needUpdateCertificatesFor.Select(
						n => string.Format(
							" - {0} (код номенклатуры: {1})",
							n.Name,
							n.Id
						)
					)
				);
				msg += "\n\nПожалуйста обновите.";
				ButtonsType btns = ButtonsType.Ok;
				if(canSaveFromHere) {
					msg += "\nПродолжить сохранение заказа?";
					btns = ButtonsType.YesNo;
				}
				return MessageDialogHelper.RunWarningDialog("Сертификаты не добавлены", msg, btns);
			}
			return true;
		}

		#endregion

		#region Toggle buttons

		protected void OnToggleInformationToggled(object sender, EventArgs e)
		{
			if(toggleInformation.Active)
				ntbOrderEdit.CurrentPage = 0;
		}

		protected void OnToggleTareControlToggled(object sender, EventArgs e)
		{
			if(toggleTareControl.Active)
				ntbOrderEdit.CurrentPage = 1;
		}

		protected void OnToggleGoodsToggled(object sender, EventArgs e)
		{
			if(toggleGoods.Active)
				ntbOrderEdit.CurrentPage = 2;
		}

		protected void OnToggleEquipmentToggled(object sender, EventArgs e)
		{
			if(toggleEquipment.Active)
				ntbOrderEdit.CurrentPage = 3;
		}

		protected void OnToggleServiceToggled(object sender, EventArgs e)
		{
			if(toggleService.Active)
				ntbOrderEdit.CurrentPage = 4;
		}

		protected void OnToggleDocumentsToggled(object sender, EventArgs e)
		{
			if(toggleDocuments.Active)
				ntbOrderEdit.CurrentPage = 5;
			btnOpnPrnDlg.Sensitive = Entity.OrderDocuments
				.OfType<PrintableOrderDocument>()
				.Any(doc => doc.PrintType == PrinterType.RDL || doc.PrintType == PrinterType.ODT);
		}

		#endregion

		#region Сервисный ремонт

		protected void OnTreeServiceClaimRowActivated(object o, RowActivatedArgs args)
		{
			ITdiTab mytab = DialogHelper.FindParentTab(this);
			if(mytab == null)
				return;

			ServiceClaimDlg dlg = new ServiceClaimDlg((treeServiceClaim.GetSelectedObjects()[0] as ServiceClaim).Id);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonAddServiceClaimClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue<ServiceClaim>())
				return;
			var dlg = new ServiceClaimDlg(Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonAddDoneServiceClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue<ServiceClaim>())
				return;
			OrmReference SelectDialog = new OrmReference(
				typeof(ServiceClaim),
				UoWGeneric,
				_serviceClaimRepository
					.GetDoneClaimsForClient(Entity)
					.GetExecutableQueryOver(UoWGeneric.Session)
					.RootCriteria
			) {
				Mode = OrmReferenceMode.Select,
				ButtonMode = ReferenceButtonMode.CanEdit
			};
			SelectDialog.ObjectSelected += DoneServiceSelected;

			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void DoneServiceSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(!(e.Subject is ServiceClaim selectedServiceClaim)) {
				return;
			}
			var serviceClaim = UoW.GetById<ServiceClaim>(selectedServiceClaim.Id);
			serviceClaim.FinalOrder = Entity;
			Entity.ObservableFinalOrderService.Add(serviceClaim);
			//TODO Add service nomenclature with price.
		}

		void TreeServiceClaim_Selection_Changed(object sender, EventArgs e)
		{
			buttonOpenServiceClaim.Sensitive = treeServiceClaim.Selection.CountSelectedRows() > 0;
		}

		protected void OnButtonOpenServiceClaimClicked(object sender, EventArgs e)
		{
			var claim = treeServiceClaim.GetSelectedObject<ServiceClaim>();
			OpenTab(
				EntityDialogBase<ServiceClaim>.GenerateHashName(claim.Id),
				() => new ServiceClaimDlg(claim)
			);
		}

		#endregion

		#region Добавление номенклатур

		void YCmbPromoSets_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(!(e.SelectedItem is PromotionalSet proSet))
			{
				return;
			}

			if(CanAddNomenclaturesToOrder() && Entity.CanAddPromotionalSet(proSet, _promotionalSetRepository))
			{
				ActivatePromotionalSet(proSet);
			}

			if(!yCmbPromoSets.IsSelectedNot)
			{
				yCmbPromoSets.SelectedItem = SpecialComboState.Not;
			}
		}

		bool CanAddNomenclaturesToOrder()
		{
			if(Entity.Client == null) {
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return false;
			}

			if(Entity.DeliveryPoint == null && !Entity.SelfDelivery) {
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должна быть выбрана точка доставки.");
				return false;
			}

			return true;
		}

		protected void OnButtonAddMasterClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = new [] { NomenclatureCategory.master },
				x => x.RestrictCategory = NomenclatureCategory.master,
				x => x.RestrictArchive = false
			);

			NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_employeeService,
				new NomenclatureJournalFactory(),
				CounterpartySelectorFactory,
				NomenclatureRepository,
				_userRepository
			) {
				SelectionMode = JournalSelectionMode.Single,
			};
			journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
			journalViewModel.TabName = "Выезд мастера";
			journalViewModel.CalculateQtyOnStock = true;
			journalViewModel.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.FirstOrDefault();
				if(selectedNode == null)
					return;
				TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
			};
			this.TabParent.AddSlaveTab(this, journalViewModel);
		}

		protected void OnButtonAddForSaleClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var defaultCategory = NomenclatureCategory.water;
			if(CurrentUserSettings.Settings.DefaultSaleCategory.HasValue)
				defaultCategory = CurrentUserSettings.Settings.DefaultSaleCategory.Value;

			var nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.SelectCategory = defaultCategory,
				x => x.SelectSaleCategory = SaleCategory.forSale,
				x => x.RestrictArchive = false
			);

			NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				_employeeService,
				new NomenclatureJournalFactory(),
				CounterpartySelectorFactory,
				NomenclatureRepository,
				_userRepository
			) {
				SelectionMode = JournalSelectionMode.Single,
			};
			journalViewModel.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
			journalViewModel.TabName = "Номенклатура на продажу";
			journalViewModel.CalculateQtyOnStock = true;
			journalViewModel.OnEntitySelectedResult += (s, ea) => {
				var selectedNode = ea.SelectedNodes.FirstOrDefault();
				if(selectedNode == null)
					return;
				TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
			};
			this.TabParent.AddSlaveTab(this, journalViewModel);
		}

		#region Рекламные наборы

		void ActivatePromotionalSet(PromotionalSet proSet)
		{
			//Добавление спец. действий промо-набора
			foreach(var action in proSet.PromotionalSetActions) {
				action.Activate(Entity);
			}
			//Добавление номенклатур из промо-набора
			TryAddNomenclatureFromPromoSet(proSet);

			Entity.ObservablePromotionalSets.Add(proSet);
		}

		#endregion

		void NomenclatureSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			TryAddNomenclature(e.Subject as Nomenclature);
		}

		void TryAddNomenclature(Nomenclature nomenclature, decimal count = 0, decimal discount = 0, DiscountReason discountReason = null)
		{
			if(Entity.IsLoadedFrom1C)
				return;

			if(Entity.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
			   && nomenclature.Category == NomenclatureCategory.master) {
				MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
			   && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category)) {
				MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}
			if(nomenclature.OnlineStore != null && !ServicesConfig.CommonServices.CurrentPermissionService
				.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order")) {
				MessageDialogHelper.RunWarningDialog("У вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
				return;
			}

			Entity.AddNomenclature(nomenclature, count, discount, false, discountReason);
		}

		void TryAddNomenclatureFromPromoSet(PromotionalSet proSet)
		{
			if(Entity.IsLoadedFrom1C)
			{
				return;
			}

			if(proSet != null && !proSet.IsArchive && proSet.PromotionalSetItems.Any())
			{
				foreach(var proSetItem in proSet.PromotionalSetItems)
				{
					var nomenclature = proSetItem.Nomenclature;
					if(Entity.OrderItems.Any(x =>
							!Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
						&& nomenclature.Category == NomenclatureCategory.master) {
						MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
						return;
					}

					if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
						&& !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category)) {
						MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
						return;
					}

					Entity.AddNomenclature(
						proSetItem.Nomenclature,
						proSetItem.Count,
						proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
						proSetItem.IsDiscountInMoney,
						null,
						proSetItem.PromoSet
					);
				}

				OnFormOrderActions();
			}
		}

		protected void OnButtonbuttonAddEquipmentToClientClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.equipment
			);
			PermissionControlledRepresentationJournal SelectDialog = new PermissionControlledRepresentationJournal(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter)) {
				Mode = JournalSelectMode.Single,
				ShowFilter = true
			};
			SelectDialog.CustomTabName("Оборудование к клиенту");
			SelectDialog.ObjectSelected += NomenclatureToClient;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureToClient(object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedId = e.GetSelectedIds().FirstOrDefault();
			if(selectedId == 0) {
				return;
			}
			AddNomenclatureToClient(UoWGeneric.Session.Get<Nomenclature>(selectedId));
		}

		void AddNomenclatureToClient(Nomenclature nomenclature)
		{
			Entity.AddEquipmentNomenclatureToClient(nomenclature, UoWGeneric);
		}

		protected void OnButtonAddEquipmentFromClientClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
				return;

			var nomenclatureFilter = new NomenclatureRepFilter(UoWGeneric);
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods(),
				x => x.DefaultSelectedCategory = NomenclatureCategory.equipment
			);
			PermissionControlledRepresentationJournal SelectDialog = new PermissionControlledRepresentationJournal(new ViewModel.NomenclatureForSaleVM(nomenclatureFilter)) {
				Mode = JournalSelectMode.Single,
				ShowFilter = true
			};
			SelectDialog.CustomTabName("Оборудование от клиента");
			SelectDialog.ObjectSelected += NomenclatureFromClient;
			TabParent.AddSlaveTab(this, SelectDialog);
		}

		void NomenclatureFromClient(object sender, JournalObjectSelectedEventArgs e)
		{
			var selectedId = e.GetSelectedIds().FirstOrDefault();
			if(selectedId == 0) {
				return;
			}
			AddNomenclatureFromClient(UoWGeneric.Session.Get<Nomenclature>(selectedId));
		}

		void AddNomenclatureFromClient(Nomenclature nomenclature)
		{
			Entity.AddEquipmentNomenclatureFromClient(nomenclature, UoWGeneric);
		}

		public void FillOrderItems(Order order)
		{

			if(Entity.OrderStatus != OrderStatus.NewOrder
			   || Entity.ObservableOrderItems.Any() && !MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите удалить все позиции текущего из заказа и заполнить его позициями из выбранного?")) {
				return;
			}

			Entity.ClearOrderItemsList();
			foreach(OrderItem orderItem in order.OrderItems) {
				switch(orderItem.Nomenclature.Category) {
					case NomenclatureCategory.additional:
						Entity.AddNomenclatureForSaleFromPreviousOrder(orderItem, UoWGeneric);
						continue;
					case NomenclatureCategory.water:
						TryAddNomenclature(orderItem.Nomenclature, orderItem.Count);
						continue;
					default:
						//Entity.AddAnyGoodsNomenclatureForSaleFromPreviousOrder(orderItem);
						continue;
				}
			}
			Entity?.RecalculateItemsPrice();
		}
		#endregion

		#region Удаление номенклатур

		private void RemoveOrderItem(OrderItem item)
		{
			var orderEquipment = Entity.OrderEquipments.FirstOrDefault(x => x.OrderRentDepositItem == item || x.OrderRentServiceItem == item);

			if(orderEquipment != null)
			{
				var existingRentDepositItem = orderEquipment.OrderRentDepositItem;
				var existingNonFreeRentServiceItem = orderEquipment.OrderRentServiceItem;
			
				if(existingRentDepositItem != null || existingNonFreeRentServiceItem != null)
				{
					MessageDialogHelper.RunWarningDialog(
						$"Нельзя удалить строку заказа. Сначала удалите связанную с ней строку оборудования {orderEquipment.FullNameString}");
					return;
				}
			}

			Entity.RemoveItem(item);
		}

		void OrderEquipmentItemsView_OnDeleteEquipment(object sender, OrderEquipment e)
		{
			if(e.OrderItem != null) {
				RemoveOrderItem(e.OrderItem);
			} else {
				Entity.RemoveEquipment(e);
			}
		}

		protected void OnBtnDeleteOrderItemClicked(object sender, EventArgs e)
		{
			if(treeItems.GetSelectedObject() is OrderItem orderItem) {
				RemoveOrderItem(orderItem);
				Entity.TryToRemovePromotionalSet(orderItem);
				//при удалении номенклатуры выделение снимается и при последующем удалении exception
				//для исправления делаем кнопку удаления не активной, если объект не выделился в списке
				btnDeleteOrderItem.Sensitive = treeItems.GetSelectedObject() != null;
			}
		}
		#endregion

		#region Создание договоров, доп соглашений

		CounterpartyContract GetActualInstanceContract(CounterpartyContract anotherSessionContract)
		{
			return UoW.GetById<CounterpartyContract>(anotherSessionContract.Id);
		}

		protected void OnReferenceContractChanged(object sender, EventArgs e)
		{
			OnReferenceDeliveryPointChanged(sender, e);
		}

		#endregion

		#region Изменение диалога

		/// <summary>
		/// Активирует редактирование ячейки количества
		/// </summary>
		private void EditGoodsCountCellOnAdd(yTreeView treeView)
		{
			try {
				int index = treeView.Model.IterNChildren() - 1;
				TreePath path;

				treeView.Model.IterNthChild(out TreeIter iter, index);
				path = treeView.Model.GetPath(iter);

				var column = treeView.ColumnsConfig.GetColumnsByTag("Count").FirstOrDefault();
				if(column == null) {
					return;
				}
				var renderer = column.CellRenderers.First();
				Application.Invoke(delegate {
					treeView.SetCursorOnCell(path, column, renderer, true);
				});
				treeView.GrabFocus();
			} catch(Exception ex) {
				logger.Error(ex, "Ошибка при попытке установки состояния редактирования на ячейку");
				return;
			}

		}

		#endregion

		#region Методы событий виджетов

		void PickerDeliveryDate_DateChanged(object sender, EventArgs e)
		{
			if(pickerDeliveryDate.Date < DateTime.Today && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_can_create_order_in_advance"))
				pickerDeliveryDate.ModifyBase(StateType.Normal, new Gdk.Color(255, 0, 0));
			else
				pickerDeliveryDate.ModifyBase(StateType.Normal, new Gdk.Color(255, 255, 255));

			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		protected void OnEntityVMEntryClientChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(entityVMEntryClient.Subject));
			if(Entity.Client != null)
			{
				var filter = new DeliveryPointJournalFilterViewModel() {Counterparty = Entity.Client, HidenByDefault = true};
				evmeDeliveryPoint.SetEntityAutocompleteSelectorFactory(new DeliveryPointJournalFactory(filter)
					.CreateDeliveryPointByClientAutocompleteSelectorFactory());
				evmeDeliveryPoint.Sensitive = Entity.OrderStatus == OrderStatus.NewOrder;

				PaymentType? previousPaymentType = enumPaymentType.SelectedItem as PaymentType?;

				if(Entity.Client.PersonType == PersonType.natural) {
					chkContractCloser.Active = false;
					chkContractCloser.Visible = false;
					enumPaymentType.AddEnumToHideList(PaymentType.cashless);
				} else {
					chkContractCloser.Visible = true;
					enumPaymentType.RemoveEnumFromHideList( PaymentType.cashless);
				}

				var promoSets = UoW.Session.QueryOver<PromotionalSet>().Where(s => !s.IsArchive).List();
				yCmbPromoSets.ItemsList = promoSets.Where(s => s.IsValidForOrder(Entity, _baseParametersProvider));

				if(previousPaymentType.HasValue) {
					if(previousPaymentType.Value == Entity.PaymentType) {
						enumPaymentType.SelectedItem = previousPaymentType.Value;
					} else if(Entity.Id == 0 || Entity.PaymentType == PaymentType.cashless) {
						enumPaymentType.SelectedItem = Entity.Client.PaymentMethod;
						OnEnumPaymentTypeChanged(null, e);
					} else {
						enumPaymentType.SelectedItem = Entity.PaymentType;
					}
				}

				enumTax.SelectedItem = Entity.Client.TaxType;
				enumTax.Visible = lblTax.Visible = IsEnumTaxVisible();
			} else {
				evmeDeliveryPoint.Sensitive = false;
			}
			Entity.SetProxyForOrder();
			UpdateProxyInfo();

			SetSensitivityOfPaymentType();
		}

		private bool IsEnumTaxVisible() => Entity.Client != null &&
										   (!Entity.CreateDate.HasValue || Entity.CreateDate > date) &&
										   Entity.Client.PersonType == PersonType.legal &&
										   Entity.Client.TaxType == TaxType.None;

		protected void OnSpinSumDifferenceValueChanged(object sender, EventArgs e)
		{
			string text;
			if(spinSumDifference.Value > 0)
				text = "Сумма <b>переплаты</b>/недоплаты:";
			else if(spinSumDifference.Value < 0)
				text = "Сумма переплаты/<b>недоплаты</b>:";
			else
				text = "Сумма переплаты/недоплаты:";
			labelSumDifference.Markup = text;
		}

		protected void OnEnumSignatureTypeChanged(object sender, EventArgs e)
		{
			UpdateProxyInfo();
		}

		protected void OnReferenceDeliveryPointChanged(object sender, EventArgs e)
		{
			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(evmeDeliveryPoint.Subject));

			if(Entity.DeliveryPoint != null)
			{
				UpdateProxyInfo();
				Entity.SetProxyForOrder();
			}

			if(Entity.DeliveryDate.HasValue && Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}

			if(Entity.DeliveryPoint != null)
			{
				TryAddFlyers();
			}
			else
			{
				if(_addedFlyersNomenclaturesIds != null &&_addedFlyersNomenclaturesIds.Any())
				{
					foreach(var flyerNomenclatureId in _addedFlyersNomenclaturesIds)
					{
						Entity.ObservableOrderEquipments.Remove(Entity.ObservableOrderEquipments.SingleOrDefault(
							x => x.Nomenclature.Id == flyerNomenclatureId));
					}
				}
			}
		}

		protected void OnReferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			CheckSameOrders();

			if(Entity.DeliveryDate.HasValue && Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();

			AddCommentFromDeliveryPoint();
		}

		private void AddCommentFromDeliveryPoint()
		{
			if(DeliveryPoint != null)
			{
				if(string.IsNullOrWhiteSpace(Entity.Comment))
				{
					Entity.Comment = DeliveryPoint.Comment;
				}
				else
				{
					if(!string.IsNullOrWhiteSpace(DeliveryPoint.Comment) && DeliveryPoint.Id != _previousDeliveryPointId)
					{
						Entity.Comment = string.Join("\n", DeliveryPoint.Comment, $"Предыдущий комментарий: {Entity.Comment}");
					}
				}

				_previousDeliveryPointId = DeliveryPoint.Id;
			}
		}

		protected void OnButtonPrintSelectedClicked(object c, EventArgs args)
		{
			try {
				SetSensitivity(false);
				var allList = treeDocuments.GetSelectedObjects().Cast<OrderDocument>().ToList();
				if(allList.Count <= 0)
					return;

				allList.OfType<ITemplateOdtDocument>().ToList().ForEach(x => x.PrepareTemplate(UoW, _docTemplateRepository));

				string whatToPrint = allList.Count > 1
					? "документов"
					: "документа \"" + allList.First().Type.GetEnumTitle() + "\"";
				if(CanEditByPermission && UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
				{
					UoWGeneric.Save();
				}

				var selectedPrintableRDLDocuments = treeDocuments.GetSelectedObjects().OfType<PrintableOrderDocument>()
					.Where(doc => doc.PrintType == PrinterType.RDL).ToList();
				if(selectedPrintableRDLDocuments.Any())
				{
					_documentPrinter.PrintAllDocuments(selectedPrintableRDLDocuments);
				}

				var selectedPrintableODTDocuments = treeDocuments.GetSelectedObjects()
					.OfType<IPrintableOdtDocument>().ToList();

				if(selectedPrintableODTDocuments.Any())
				{
					_documentPrinter.PrintAllODTDocuments(selectedPrintableODTDocuments);
				}
			} finally {
				SetSensitivity(true);
			}
		}

		protected void OnBtnOpnPrnDlgClicked(object sender, EventArgs e)
		{
			if(Entity.OrderDocuments.OfType<PrintableOrderDocument>().Any(
				doc => doc.PrintType == PrinterType.RDL || doc.PrintType == PrinterType.ODT))
			{
				TabParent.AddSlaveTab(this, new DocumentsPrinterViewModel(
					_entityDocumentsPrinterFactory,
					ServicesConfig.InteractiveService,
					MainClass.MainWin.NavigationManager,
					Entity));
			}
		}

		protected void OnEnumPaymentTypeChanged(object sender, EventArgs e)
		{
			//при изменении типа платежа вкл/откл кнопку "ожидание оплаты"
			buttonWaitForPayment.Sensitive = CanEditByPermission && IsPaymentTypeBarterOrCashless();

			//при изменении типа платежа вкл/откл кнопку "закрывашка по контракту"
			chkContractCloser.Visible = IsPaymentTypeCashless();

			checkDelivered.Visible = enumDocumentType.Visible = labelDocumentType.Visible = IsPaymentTypeCashless();

			if (Entity.PaymentType != PaymentType.cash) {
				ycheckPaymentBySms.Visible = ycheckPaymentBySms.Active = false;
				chkPaymentByQr.Visible = chkPaymentByQr.Active = false;
			}
			else {
				ycheckPaymentBySms.Visible = true;
				chkPaymentByQr.Visible = true;
			}

			if (Entity.PaymentType == PaymentType.Terminal) {
				checkSelfDelivery.Visible = checkSelfDelivery.Active = false;
			}
			else {
				checkSelfDelivery.Visible = true;
			}

			enumSignatureType.Visible = labelSignatureType.Visible =
				Entity.Client != null && (Entity.Client.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless);

			hbxOnlineOrder.Visible = UpdateVisibilityHboxOnlineOrder();
			ySpecPaymentFrom.Visible = Entity.PaymentType == PaymentType.ByCard;

			if(treeItems.Columns.Any())
				treeItems.Columns.First(x => x.Title == "В т.ч. НДС").Visible = Entity.PaymentType == PaymentType.cashless;
			spinSumDifference.Visible = labelSumDifference.Visible = labelSumDifferenceReason.Visible =
				dataSumDifferenceReason.Visible = (Entity.PaymentType == PaymentType.cash);
			spinSumDifference.Visible = spinSumDifference.Visible && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_order_extra_cash");
			pickerBillDate.Visible = labelBillDate.Visible = Entity.PaymentType == PaymentType.cashless;
			Entity.SetProxyForOrder();
			UpdateProxyInfo();
			UpdateUIState();
		}

		private bool UpdateVisibilityHboxOnlineOrder() {
			switch (Entity.PaymentType) {
				case PaymentType.ByCard:
					return true;
				case PaymentType.Terminal:
					return Entity.OnlineOrder != null;
				default:
					return false;
			}
		}

		protected void OnPickerDeliveryDateDateChanged(object sender, EventArgs e)
		{
			Entity.SetProxyForOrder();
			UpdateProxyInfo();
		}

		protected void OnPickerDeliveryDateDateChangedByUser(object sender, EventArgs e)
		{
			if(Entity.DeliveryDate.HasValue) {
				if(Entity.DeliveryDate.Value.Date != DateTime.Today.Date || MessageDialogHelper.RunWarningDialog("Подтвердите дату доставки", "Доставка сегодня? Вы уверены?", ButtonsType.YesNo)) {
					CheckSameOrders();
					return;
				}
				Entity.DeliveryDate = null;
			}
		}

		protected void OnEntityVMEntryClientChangedByUser(object sender, EventArgs e)
		{
			chkContractCloser.Active = false;
			CheckForStopDelivery();

			Entity.UpdateClientDefaultParam(UoW, counterpartyContractRepository, organizationProvider, counterpartyContractFactory);

			if(DeliveryPoint != null)
			{
				AddCommentFromDeliveryPoint();
			}

			//Проверяем возможность добавления Акции "Бутыль"
			ControlsActionBottleAccessibility();
			UpdateOnlineOrderText();
		}

		protected void CheckForStopDelivery()
		{
			if (Entity?.Client != null && Entity.Client.IsDeliveriesClosed)
			{
				string message = "Стоп отгрузки!!!" + Environment.NewLine + "Комментарий от фин.отдела: " + Entity.Client?.CloseDeliveryComment;
				MessageDialogHelper.RunInfoDialog(message);
				Enum[] hideEnums = {
					PaymentType.barter,
					PaymentType.ContractDoc,
					PaymentType.cashless
				};
				enumPaymentType.AddEnumToHideList(hideEnums);
			}
		}

		protected void OnButtonCancelOrderClicked(object sender, EventArgs e) {

			bool isShipped = !_orderRepository.IsSelfDeliveryOrderWithoutShipment(UoW, Entity.Id);
			bool orderHasIncome = _cashRepository.OrderHasIncome(UoW, Entity.Id);

			if (Entity.SelfDelivery && (orderHasIncome || isShipped)) {
				MessageDialogHelper.RunErrorDialog(
					"Вы не можете отменить отгруженный или оплаченный самовывоз. " +
					"Для продолжения необходимо удалить отгрузку или приходник.");
				return;
			}

			ValidationContext validationContext = new ValidationContext(Entity,null, new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.Canceled }
			});

			if(!Validate(validationContext))
			{
				return;
			}

			OpenDlgToCreateNewUndeliveredOrder();
		}

		/// <summary>
		/// Открытие окна создания нового недовоза при отмене заказа
		/// </summary>
		void OpenDlgToCreateNewUndeliveredOrder()
		{
			var dlg = new UndeliveryOnOrderCloseDlg(Entity, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (sender, e) =>
			{
				Entity.SetUndeliveredStatus(UoW, _baseParametersProvider, CallTaskWorker);

				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity);
				if(routeListItem != null)
				{
					routeListItem.StatusLastUpdate = DateTime.Now;
					routeListItem.SetOrderActualCountsToZeroOnCanceled();
					UoW.Save(routeListItem);
				}
				else
				{
					Entity.SetActualCountsToZeroOnCanceled();
				}

				UpdateUIState();

				if(Save())
				{
					OnCloseTab(false);
				}
			};
		}

		protected void OnEnumPaymentTypeChangedByUser(object sender, EventArgs e)
		{
			UpdateOnlineOrderText();
		}

		private void UpdateOnlineOrderText()
		{
			if(Entity.PaymentType != PaymentType.ByCard)
				entOnlineOrder.Text = string.Empty; //костыль, т.к. Entity.OnlineOrder = null не убирает почему-то текст из виджета
		}

		protected void OnButtonWaitForPaymentClicked(object sender, EventArgs e)
		{
			ValidationContext validationContext = new ValidationContext(Entity, null, new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.WaitForPayment }
			});

			if(!Validate(validationContext))
			{
				return ;
			}

			PrepareSendBillInformation();

			if(_emailAddressForBill == null
			   && Entity.NeedSendBill(_emailRepository)
			   && !MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить смену статуса заказа без дальнейшей отправки почты?"))
			{
				return;
			}

			Entity.ChangeStatusAndCreateTasks(OrderStatus.WaitForPayment, CallTaskWorker);
			UpdateUIState();
		}

		protected void OnEnumDiverCallTypeChanged(object sender, EventArgs e)
		{
			var listDriverCallType = UoW.Session.QueryOver<Order>()
										.Where(x => x.Id == Entity.Id)
										.Select(x => x.DriverCallType).List<DriverCallType>().FirstOrDefault();

			if(listDriverCallType != (DriverCallType)enumDiverCallType.SelectedItem) {
				var max = UoW.Session.QueryOver<Order>().Select(NHibernate.Criterion.Projections.Max<Order>(x => x.DriverCallId)).SingleOrDefault<int>();
				Entity.DriverCallId = max != 0 ? max + 1 : 1;
			}
		}

		protected void OnYEntTareActBtlFromClientChanged(object sender, EventArgs e)
		{
			Entity.CalculateBottlesStockDiscounts(_orderParametersProvider);
		}

		protected void OnEntryTrifleChanged(object sender, EventArgs e)
		{
			if(int.TryParse(entryTrifle.Text, out int result)) {
				Entity.Trifle = result;
			}
		}

		protected void OnShown(object sender, EventArgs e)
		{
			//Скрывает журнал заказов при открытии заказа, чтобы все элементы умещались на экране
			if(TabParent is TdiSliderTab slider)
				slider.IsHideJournal = true;
		}

		protected void OnButtonDepositsClicked(object sender, EventArgs e)
		{
			ToggleVisibilityOfDeposits();
		}

		protected void OnChkContractCloserToggled(object sender, EventArgs e)
		{
			SetSensitivityOfPaymentType();
		}

		protected void OnSpinDiscountValueChanged(object sender, EventArgs e)
		{
			if(spinDiscount.ValueAsDecimal != default(decimal))
			{
				SetDiscount();
			}
		}

		private void OnYComboBoxReasonItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(ycomboboxReason.SelectedItem != null)
			{
				SetDiscount();
			}
			else
			{
				SetDiscountUnitEditable();
				spinDiscount.ValueAsDecimal = default(decimal);
				SetDiscountEditable();
				_discountsController.RemoveDiscountFromOrder(Entity.ObservableOrderItems);
			}
		}

		protected void OnEnumDiscountUnitEnumItemSelected(object sender, EnumItemClickedEventArgs e)
		{
			SetDiscountEditable();
			var sum = Entity.ObservableOrderItems.Sum(i => i.CurrentCount * i.Price);
			var unit = (DiscountUnits)e.ItemEnum;
			spinDiscount.Adjustment.Upper = unit == DiscountUnits.money ? (double)sum : 100d;

			if(unit == DiscountUnits.percent && spinDiscount.Value > 100)
			{
				spinDiscount.Value = 100;
			}
			if(spinDiscount.ValueAsDecimal == default(decimal))
			{
				return;
			}

			SetDiscount();
		}

		private void OnEntryBottlesToReturnChanged(object sender, EventArgs e)
		{
			HboxReturnTareReasonCategoriesShow();

			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void HboxReturnTareReasonCategoriesShow()
		{
			if (Entity.BottlesReturn.HasValue && Entity.BottlesReturn > 0)
			{
				hboxReturnTareReason.Visible = Entity.GetTotalWater19LCount() == 0;

				if(!hboxReturnTareReason.Visible) {
					hboxReasons.Visible = false;
					Entity.RemoveReturnTareReason();
				}
			}
			else
			{
				hboxReturnTareReason.Visible = hboxReasons.Visible = false;
				Entity.RemoveReturnTareReason();
			}
		}

		private void YCmbReturnTareReasonCategoriesOnChanged(object sender, EventArgs e)
		{
			ChangeHboxReasonsVisibility();
		}

		private void ChangeHboxReasonsVisibility()
		{
			if (yCmbReturnTareReasonCategories.SelectedItem is ReturnTareReasonCategory category)
			{
				if (!hboxReasons.Visible)
					hboxReasons.Visible = true;

				yCmbReturnTareReasons.ItemsList = category.ChildReasons;
			}
		}

		private void OnButtonCopyManagerCommentClicked(object sender, EventArgs e)
		{
			var cb = textManagerComments.GetClipboard(Gdk.Selection.Clipboard);
			cb.Text = textManagerComments.Buffer.Text;
		}

		#endregion

		#region Service functions

		/// <summary>
		/// Is the payment type barter or cashless?
		/// </summary>
		private bool IsPaymentTypeBarterOrCashless() => Entity.PaymentType == PaymentType.barter || Entity.PaymentType == PaymentType.cashless;

		/// <summary>
		/// Is the payment type cashless?
		/// </summary>
		private bool IsPaymentTypeCashless() => Entity.PaymentType == PaymentType.cashless;
		#endregion

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			//если новый заказ и не создан из недовоза (templateOrder заполняется только из недовоза)
			if(UoW.IsNew && templateOrder == null && Entity.Client == null)
				//открыть окно выбора контрагента
				entityVMEntryClient.OpenSelectDialog();
		}

		public virtual bool HideItemFromDirectionReasonComboInEquipment(OrderEquipment node, DirectionReason item)
		{
			switch(item) {
				case DirectionReason.None:
					return true;
				case DirectionReason.Rent:
					return node.Direction == Domain.Orders.Direction.Deliver;
				case DirectionReason.Repair:
				case DirectionReason.Cleaning:
				case DirectionReason.RepairAndCleaning:
				default:
					return false;
			}
		}

		void Entity_UpdateClientCanChange(object aList, int[] aIdx)
		{
			entityVMEntryClient.IsEditable = Entity.CanChangeContractor();
		}

		void Entity_ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			FixPrice(aIdx[0]);

			if (Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder) {
				Entity.CheckAndSetOrderIsService();
				OnFormOrderActions();
			}
			_treeItemsNomenclatureColumnWidth = treeItems.ColumnsConfig.GetColumnsByTag(nameof(Nomenclature)).First().Width;
			treeItems.ExposeEvent += TreeItemsOnExposeEvent;
		}

		private void TreeItemsOnExposeEvent(object o, ExposeEventArgs args)
		{
			if(_treeItemsNomenclatureColumnWidth != ((yTreeView)o).ColumnsConfig.GetColumnsByTag(nameof(Nomenclature)).First().Width)
			{
				EditGoodsCountCellOnAdd((yTreeView)o);
				treeItems.ExposeEvent -= TreeItemsOnExposeEvent;
			}
		}

		void ObservableOrderItems_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			var items = aList as GenericObservableList<OrderItem>;
			for(var i = 0; i < items?.Count; i++)
			{
				FixPrice(i);
			}

			HboxReturnTareReasonCategoriesShow();

			if (Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder) {
				Entity.CheckAndSetOrderIsService();
				OnFormOrderActions();
			}

			Entity.AddFastDeliveryNomenclatureIfNeeded();
		}

		void ObservableOrderDocuments_ListChanged(object aList)
		{
			ShowOrderColumnInDocumentsList();
		}

		void ObservableOrderDocuments_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ShowOrderColumnInDocumentsList();
		}

		void ObservableOrderDocuments_ElementAdded(object aList, int[] aIdx)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ShowOrderColumnInDocumentsList()
		{
			var column = treeDocuments.ColumnsConfig.GetColumnsByTag("OrderNumberColumn").FirstOrDefault();
			column.Visible = Entity.ObservableOrderDocuments.Any(x => x.Order.Id != x.AttachedToOrder.Id);
		}

		void FixPrice(int id)
		{
			OrderItem item = Entity.ObservableOrderItems[id];
			if(item.Nomenclature.Category == NomenclatureCategory.deposit && item.Price != 0)
				return;
			item.RecalculatePrice();
		}

		void TreeItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeItems.GetSelectedObjects();

			btnDeleteOrderItem.Sensitive = items.Any();
		}

		/// <summary>
		/// Для хранения состояния, было ли изменено количество оборудования в товарах,
		/// для информирования пользователя о том, что изменения сохранятся также и в
		/// дополнительном соглашении
		/// </summary>
		private bool OrderItemEquipmentCountHasChanges;

		/// <summary>
		/// При изменении количества оборудования в списке товаров меняет его
		/// также в доп. соглашении и списке оборудования заказа
		/// + если меняем количество у 19л бут, то меняем видимость у hboxReturnTareReason
		/// </summary>
		void ObservableOrderItems_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<OrderItem>) {
				foreach(var i in aIdx) {
					OrderItem oItem = (aList as GenericObservableList<OrderItem>)[aIdx] as OrderItem;

					FixPrice(aIdx[0]);

					if(oItem != null && oItem.Nomenclature.IsWater19L)
						HboxReturnTareReasonCategoriesShow();

					if(oItem != null && oItem.Count > 0 && Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
						OnFormOrderActions();

					if(oItem == null) {
						return;
					}

					if(oItem.Nomenclature.Category == NomenclatureCategory.equipment) {
						ChangeEquipmentsCount(oItem, (int)oItem.Count);
					}
				}
			}
		}

		/// <summary>
		/// При изменении количества оборудования в списке оборудования меняет его
		/// также в доп. соглашении и списке товаров заказа
		/// </summary>
		void ObservableOrderEquipments_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<OrderEquipment>) {
				foreach(var i in aIdx) {
					OrderEquipment oEquip = (aList as GenericObservableList<OrderEquipment>)[aIdx] as OrderEquipment;
					if(oEquip == null || oEquip.OrderItem == null) {
						return;
					}
					if(oEquip.Count != oEquip.OrderItem.Count) {
						ChangeEquipmentsCount(oEquip.OrderItem, oEquip.Count);
					}
				}
			}
		}

		private void ObservableOrderDepositItemsOnElementRemoved(object alist, int[] aidx, object aobject) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void ObservableOrderDepositItemsOnElementAdded(object alist, int[] aidx) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void ObservableOrderEquipmentsOnElementRemoved(object alist, int[] aidx, object aobject) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		private void ObservableOrderEquipmentsOnElementAdded(object alist, int[] aidx) {
			if(Entity.DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
				OnFormOrderActions();
		}

		/// <summary>
		/// Меняет количество оборудования в списке оборудования заказа, в списке
		/// товаров заказа, в списке оборудования дополнительного соглашения и
		/// меняет количество залогов за оборудование в списке товаров заказа
		/// </summary>
		void ChangeEquipmentsCount(OrderItem orderItem, int newCount)
		{
			orderItem.Count = newCount;

			OrderEquipment orderEquip = Entity.OrderEquipments.FirstOrDefault(x => x.OrderItem == orderItem);
			if(orderEquip != null) {
				orderEquip.Count = newCount;
			}
		}

		private bool CanEditByPermission => permissionResult.CanUpdate || permissionResult.CanCreate && Entity.Id == 0;

		private void UpdateUIState()
		{
			bool val = Entity.CanEditByStatus && CanEditByPermission;
			enumPaymentType.Sensitive = (Entity.Client != null) && val && !chkContractCloser.Active;
			evmeDeliveryPoint.IsEditable = entityVMEntryClient.IsEditable = val;
			entryDeliverySchedule.Sensitive = labelDeliverySchedule.Sensitive = !checkSelfDelivery.Active && val;
			ybuttonFastDeliveryCheck.Sensitive = ycheckFastDelivery.Sensitive = !checkSelfDelivery.Active && val && Entity.CanChangeFastDelivery;
			lblDeliveryPoint.Sensitive = evmeDeliveryPoint.Sensitive = !checkSelfDelivery.Active && val && Entity.Client != null;
			buttonAddMaster.Sensitive = !checkSelfDelivery.Active && val && !Entity.IsLoadedFrom1C;
			enumAddRentButton.Sensitive = enumSignatureType.Sensitive =
				enumDocumentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive =
				buttonAddForSale.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			//pickerDeliveryDate.Sensitive = val; // оно повторно устанавливается в ChangeOrderEditable(val) -> SetPadInfoSensitive(val)
			dataSumDifferenceReason.Sensitive = val;
			ycheckContactlessDelivery.Sensitive = val;
			ycheckPaymentBySms.Sensitive = val;
			chkPaymentByQr.Sensitive = val;
			enumDiscountUnit.Visible = spinDiscount.Visible = labelDiscont.Visible = vseparatorDiscont.Visible = val;
			ChangeOrderEditable(val);
			checkPayAfterLoad.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_payment_after_load") && checkSelfDelivery.Active && val;
			buttonAddForSale.Sensitive = enumAddRentButton.Sensitive = !Entity.IsLoadedFrom1C;
			UpdateButtonState();
			ControlsActionBottleAccessibility();
			chkContractCloser.Sensitive = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer") && val && !Entity.SelfDelivery;
			hbxTareNonReturnReason.Sensitive = val;
			lblTax.Visible = enumTax.Visible = val && IsEnumTaxVisible();

			if(Entity != null)
				yCmbPromoSets.Sensitive = val;
		}

		void ChangeOrderEditable(bool val)
		{
			SetPadInfoSensitive(val);
			ChangeGoodsTabSensitive(val);
			buttonAddExistingDocument.Sensitive = val;
			btnAddM2ProxyForThisOrder.Sensitive = val;
			btnRemExistingDocument.Sensitive = val;
			RouteListStatus? rlStatus = null;
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot()) {
				if(Entity.Id != 0)
					rlStatus = _orderRepository.GetAllRLForOrder(uow, Entity).FirstOrDefault()?.Status;
				var sensitive = rlStatus.HasValue && CanEditByPermission
					&& !new[] { RouteListStatus.MileageCheck, RouteListStatus.OnClosing, RouteListStatus.Closed }.Contains(rlStatus.Value);
				textManagerComments.Editable = sensitive;
				enumDiverCallType.Sensitive = sensitive;
			}
		}

		private void ChangeGoodsTabSensitive(bool sensitive)
		{
			treeItems.Sensitive = sensitive;
			hbox12.Sensitive = sensitive;
			hbox10.Sensitive = sensitive;
			hboxReturnTareReason.Sensitive = sensitive;
			orderEquipmentItemsView.Sensitive = sensitive;
			hbox11.Sensitive = sensitive;
			depositrefunditemsview.Sensitive = sensitive;
			table2.Sensitive = sensitive;
		}

		void SetPadInfoSensitive(bool value)
		{
			foreach(var widget in table1.Children)
				widget.Sensitive = widget.Name == vboxOrderComment.Name || value;

			if(chkContractCloser.Active)
				enumPaymentType.Sensitive = false;

			if(isEditOrderClicked)
			{
				pickerDeliveryDate.Sensitive =
					Order.OrderStatus == OrderStatus.NewOrder
					&& Order.Id != 0
					&& _canEditDeliveryDateAfterOrderConfirmation;
			}
			else
			{
				if(Order.OrderStatus == OrderStatus.NewOrder && Order.Id != 0)
				{
					pickerDeliveryDate.Sensitive = _canEditDeliveryDateAfterOrderConfirmation;
				}
			}
		}

		void SetSensitivityOfPaymentType()
		{
			if(chkContractCloser.Active) {
				Entity.PaymentType = PaymentType.cashless;
				UpdateUIState();
			} else {
				UpdateUIState();
			}
		}

		public void SetDlgToReadOnly()
		{
			buttonSave.Sensitive = buttonCancel.Sensitive =
			hboxStatusButtons.Visible = false;
		}

		void UpdateButtonState()
		{
			if(!CanEditByPermission || !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_order")) {
				buttonEditOrder.Sensitive = false;
				buttonEditOrder.TooltipText = "Нет права на редактирование";
			}

			buttonSave.Sensitive = CanEditByPermission;
			btnForm.Sensitive = CanEditByPermission;
			menubuttonActions.Sensitive = CanEditByPermission;
			yBtnAddCurrentContract.Sensitive = CanEditByPermission;

			if(Entity.CanSetOrderAsAccepted) {
				btnForm.Visible = true;
				buttonEditOrder.Visible = false;
			} else if(Entity.CanSetOrderAsEditable) {
				buttonEditOrder.Visible = true;
				btnForm.Visible = false;
			} else {
				btnForm.Visible = false;
				buttonEditOrder.Visible = false;
			}

			textComments.Editable = CanEditByPermission;
			btnSaveComment.Sensitive = CanEditByPermission && Entity.OrderStatus != OrderStatus.NewOrder;

			//если новый заказ и тип платежа бартер или безнал, то вкл кнопку
			buttonWaitForPayment.Sensitive = CanEditByPermission && Entity.OrderStatus == OrderStatus.NewOrder && IsPaymentTypeBarterOrCashless() && !Entity.SelfDelivery;

			buttonCancelOrder.Sensitive = CanEditByPermission &&
				(_orderRepository.GetStatusesForOrderCancelation().Contains(Entity.OrderStatus)
					|| (Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.OnLoading)) && Entity.OrderStatus != OrderStatus.NewOrder;

			menuItemSelfDeliveryToLoading.Sensitive = Entity.SelfDelivery
				&& Entity.OrderStatus == OrderStatus.Accepted
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("allow_load_selfdelivery");
			menuItemSelfDeliveryPaid.Sensitive = Entity.SelfDelivery
				&& (Entity.PaymentType == PaymentType.cashless || Entity.PaymentType == PaymentType.ByCard)
				&& Entity.OrderStatus == OrderStatus.WaitForPayment
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("accept_cashless_paid_selfdelivery");

			menuItemCloseOrder.Sensitive = Entity.OrderStatus == OrderStatus.Accepted && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_close_orders") && !Entity.SelfDelivery;
			menuItemReturnToAccepted.Sensitive = Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted;
		}

		void UpdateProxyInfo()
		{
			bool canShow = Entity.Client != null && Entity.DeliveryDate.HasValue &&
								 (Entity.Client?.PersonType == PersonType.legal || Entity.PaymentType == PaymentType.cashless);

			labelProxyInfo.Visible = canShow;

			DBWorks.SQLHelper text = new DBWorks.SQLHelper("");
			if(canShow) {
				var proxies = Entity.Client.Proxies.Where(p => p.IsActiveProxy(Entity.DeliveryDate.Value) && (p.DeliveryPoints == null || !p.DeliveryPoints.Any() || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, Entity.DeliveryPoint))));
				foreach(var proxy in proxies) {
					if(!string.IsNullOrWhiteSpace(text.Text))
						text.Add("\n");
					text.Add(string.Format("Доверенность{2} №{0} от {1:d}", proxy.Number, proxy.IssueDate,
						proxy.DeliveryPoints == null ? "(общая)" : ""));
					text.StartNewList(": ");
					foreach(var pers in proxy.Persons) {
						text.AddAsList(pers.NameWithInitials);
					}
				}
			}
			if(string.IsNullOrWhiteSpace(text.Text))
				labelProxyInfo.Markup = "<span foreground=\"red\">Нет активной доверенности</span>";
			else
				labelProxyInfo.LabelProp = text.Text;
		}

		private void CheckSameOrders()
		{
			if(!Entity.DeliveryDate.HasValue || Entity.DeliveryPoint == null) {
				return;
			}

			var sameOrder = _orderRepository.GetOrderOnDateAndDeliveryPoint(UoW, Entity.DeliveryDate.Value, Entity.DeliveryPoint);
			if(sameOrder != null && templateOrder == null) {
				MessageDialogHelper.RunWarningDialog("На выбранную дату и точку доставки уже есть созданный заказ!");
			}
		}

		void SetDiscountEditable(bool? canEdit = null)
		{
			spinDiscount.Sensitive = canEdit ?? enumDiscountUnit.SelectedItem != null && _canChangeDiscountValue && !Entity.IsBottleStock;
		}

		void SetDiscountUnitEditable() => enumDiscountUnit.Sensitive = _canChangeDiscountValue && !Entity.IsBottleStock;

		/// <summary>
		/// Переключает видимость элементов управления депозитами
		/// </summary>
		/// <param name="visibly"><see langword="true"/>если хотим принудительно сделать видимым;
		/// <see langword="false"/>если хотим принудительно сделать невидимым;
		/// <see langword="null"/>переключает видимость с невидимого на видимый и обратно.</param>
		private void ToggleVisibilityOfDeposits(bool? visibly = null)
		{
			depositrefunditemsview.Visible = visibly ?? !depositrefunditemsview.Visible;
			labelDeposit1.Visible = visibly ?? !labelDeposit1.Visible;
		}

		private void SetDiscount()
		{
			var reason = ycomboboxReason.SelectedItem as DiscountReason;
			if(decimal.TryParse(spinDiscount.Text, out decimal discount))
			{
				if(reason == null && discount > 0) {
					MessageDialogHelper.RunErrorDialog("Необходимо выбрать основание для скидки");
					return;
				}

				if(discount > 0)
				{
					var unit = (DiscountUnits)enumDiscountUnit.SelectedItem;
					_discountsController.SetCustomDiscountForOrder(reason, discount, unit, Entity.ObservableOrderItems);
				}
				else
				{
					_discountsController.SetDiscountFromDiscountReasonForOrder(
						reason, Entity.ObservableOrderItems, _canChangeDiscountValue, out string messages);

					if(messages?.Length > 0)
					{
						ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
							"На следующие позиции не применилась скидка," +
							$" т.к. они из промо-набора или на них есть фикса:\n{messages}Обратитесь к руководителю");
					}
				}
			}
		}

		private bool HaveEmailForBill()
		{
			Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
			return clientEmail != null || MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?");
		}

		private void SendBillByEmail(Email emailAddressForBill)
		{
			if(emailAddressForBill == null)
			{
				throw new ArgumentNullException(nameof(emailAddressForBill));
			}

			if(_emailRepository.HaveSendedEmailForBill(Entity.Id))
			{
				return;
			}

			var document = Entity.OrderDocuments.FirstOrDefault(x => x.Type == OrderDocumentType.Bill || x.Type == OrderDocumentType.SpecialBill);

			if(document == null)
			{
				MessageDialogHelper.RunErrorDialog("Невозможно отправить счет по электронной почте. Счет не найден.");
				return;
			}

			using(var uow = UnitOfWorkFactory.CreateWithoutRoot($"Добавление записи о письме со счетом"))
			{
				var configuration = uow.GetAll<InstanceMailingConfiguration>().FirstOrDefault();

				Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);

				var storedEmail = new StoredEmail
				{
					SendDate = DateTime.Now,
					StateChangeDate = DateTime.Now,
					State = StoredEmailStates.PreparingToSend,
					RecipientAddress = clientEmail.Address,
					ManualSending = false,
					Subject = document.Name,
					Author = _employeeRepository.GetEmployeeForCurrentUser(uow)
				};

				try
				{
					uow.Save(storedEmail);

					OrderDocumentEmail orderDocumentEmail = new OrderDocumentEmail
					{
						StoredEmail = storedEmail,
						Counterparty = Counterparty,
						OrderDocument = document
					};

					uow.Save(orderDocumentEmail);

					uow.Commit();
				}

				catch(Exception ex)
				{
					logger.Debug($"Ошибка при сохранении. Ошибка: { ex.Message }");
					throw ex;
				}
			}
		}

		void Selection_Changed(object sender, EventArgs e)
		{
			buttonViewDocument.Sensitive = treeDocuments.Selection.CountSelectedRows() > 0;

			var selectedDoc = treeDocuments.GetSelectedObjects().Cast<OrderDocument>().FirstOrDefault();
			if(selectedDoc == null) {
				return;
			}
			string email = "";
			if(!Entity.Client.Emails.Any()) {
				email = "";
			} else {
				Email clientEmail = Entity.Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
				if(clientEmail == null) {
					clientEmail = Entity.Client.Emails.FirstOrDefault();
				}
				email = clientEmail.Address;
			}

			SendDocumentByEmailViewModel.Update(selectedDoc as IEmailableDocument, email);
		}

		protected void OnCheckSelfDeliveryToggled(object sender, EventArgs e)
		{
			UpdateUIState();

			if(!checkSelfDelivery.Active) {
				checkPayAfterLoad.Active = false;
			}
		}

		void ObservablePromotionalSets_ListChanged(object aList)
		{
			ShowPromoSetsColumn();
		}

		void ObservablePromotionalSets_ElementAdded(object aList, int[] aIdx)
		{
			ShowPromoSetsColumn();
		}

		void ObservablePromotionalSets_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ShowPromoSetsColumn();
		}

		private void ShowPromoSetsColumn()
		{
			var promoSetColumn = treeItems.ColumnsConfig.GetColumnsByTag(nameof(Entity.PromotionalSets)).FirstOrDefault();
			promoSetColumn.Visible = Entity.PromotionalSets.Count > 0;
		}

		protected void OnYBtnAddCurrentContractClicked(object sender, EventArgs e)
		{
			Order.AddContractDocument(Order.Contract);
		}

		protected void OnBtnFormClicked(object sender, EventArgs e)
		{
			ylblCounterpartyFIO.Text = Entity.Client.FullName;
			ylblDeliveryAddress.Text = Entity.DeliveryPoint?.CompiledAddress ?? "";

			ylblPhoneNumber.Text = Entity.DeliveryPoint?.Phones.Count > 0
				? string.Join(", ", Entity.DeliveryPoint.Phones.Select(p => p.DigitsNumber))
				: string.Join(", ", Entity.Client.Phones.Select(p => p.DigitsNumber));

			ylblDeliveryDate.Text = Entity.DeliveryDate?.ToString("dd.MM.yyyy, dddd") ?? "";
			ylblDeliveryInterval.Text = Entity.DeliverySchedule?.DeliveryTime;

			var isPaymentTypeCashless = Entity.PaymentType == PaymentType.cashless;
			ylblDocumentSigning.Visible = isPaymentTypeCashless;
			lblDocumentSigning.Visible = isPaymentTypeCashless;
			ylblDocumentSigning.Text = isPaymentTypeCashless
				? Entity.SignatureType?.GetEnumTitle() ?? ""
				: "";

			var hasOrderItems = Entity.OrderItems.Count > 0;
			ylblGoods.Visible = hasOrderItems;
			lblGoods.Visible = hasOrderItems;
			ylblGoods.Text = hasOrderItems
				? string.Join("\n",
					Entity.OrderItems.Select(oi => $"{ oi.Nomenclature.Name } - { oi.Count.ToString("F" + (oi.Nomenclature.Unit?.Digits ?? 0).ToString()) }{ oi.Nomenclature.Unit?.Name }"))
				: "";

			var hasOrderEquipments = Entity.OrderEquipments.Count > 0;
			ylblEquipment.Visible = hasOrderEquipments;
			lblEquipment1.Visible = hasOrderEquipments;
			ylblEquipment.Text = hasOrderEquipments
				? string.Join("\n",
					Entity.OrderEquipments.Select(oe => $"{ oe.Nomenclature.Name } - { oe.Count.ToString("F" + (oe.Nomenclature.Unit?.Digits ?? 0).ToString()) }{ oe.Nomenclature.Unit?.Name ?? "шт" }"))
				: "";

			var hasDepositItems = Entity.OrderDepositItems.Count > 0;

			ylblReturns.Visible = hasDepositItems;
			lblReturns.Visible = hasDepositItems;
			ylblReturns.Text = hasDepositItems
				? string.Join("\n",
					Entity.OrderDepositItems.Select(odi =>
					{
						if(odi.EquipmentNomenclature != null)
						{
							return $"{ odi.EquipmentNomenclature.Name } - { odi.Count }{ odi.EquipmentNomenclature.Unit.Name }";
						}
						else
						{
							return $"{ odi.DepositTypeString } - { odi.Count }";
						}
					}))
				: "";

			ylblBottlesPlannedToReturn.Text = $"{ Entity.BottlesReturn ?? 0 } бут.";

			var isIncorrectLegalClientPaymentType = Entity.Client.PersonType == PersonType.legal && Entity.PaymentType != Entity.Client.PaymentMethod;
			ylblPaymentType.LabelProp = isIncorrectLegalClientPaymentType
				? $"<span foreground='red'>{ Entity.PaymentType.GetEnumTitle() }</span>"
				: Entity.PaymentType.GetEnumTitle();

			ylblPlannedSum.Text = $"{ Entity.OrderPositiveSum } руб.";

			var isPaymentTypeCash = Entity.PaymentType == PaymentType.cash;
			ylblTrifleFrom.Visible = isPaymentTypeCash;
			lblTrifleFrom.Visible = isPaymentTypeCash;
			ylblTrifleFrom.Text = isPaymentTypeCash
								? $"{ Entity.Trifle ?? 0 } руб."
								: "";

			ylblCommentForDriver.Text = Entity.HasCommentForDriver ? Entity.Comment : "";

			ylblCommentForLogist.Text = Entity.CommentLogist;

			ntbOrder.GetNthPage(1).Hide();
			ntbOrder.GetNthPage(1).Show();

			ntbOrder.CurrentPage = 1;
		}

		protected void OnBtnReturnToEditClicked(object sender, EventArgs e)
		{
			ReturnToEditTab();
		}

		private void ReturnToEditTab()
		{
			ntbOrder.CurrentPage = 0;
		}

		#region Аренда

		private void AddRent(RentType rentType)
		{
			if (Entity.OrderAddressType == OrderAddressType.Service) {
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Нельзя добавлять аренду в сервисный заказ",
					"Ошибка"
				);
				return;
			}
			switch (rentType) {
				case RentType.NonfreeRent:
					SelectPaidRentPackage(RentType.NonfreeRent);
					break;
				case RentType.DailyRent:
					SelectPaidRentPackage(RentType.DailyRent);
					break;
				case RentType.FreeRent:
					SelectFreeRentPackage();
					break;
			}
		}

		#region PaidRent

		private void SelectPaidRentPackage(RentType rentType)
		{
			var paidRentJournal = _rentPackagesJournalsViewModelsFactory.CreatePaidRentPackagesJournalViewModel(false, false, false, false);

			paidRentJournal.OnSelectResult += (sender, e) =>
			{
				var selectedRent = e.GetSelectedObjects<PaidRentPackagesJournalNode>().FirstOrDefault();

				if (selectedRent == null)
				{
					return;
				}

				var paidRentPackage = UoW.GetById<PaidRentPackage>(selectedRent.Id);
				SelectEquipmentForPaidRentPackage(rentType, paidRentPackage);
			};
			TabParent.AddTab(paidRentJournal, this);
		}

		private void SelectEquipmentForPaidRentPackage(RentType rentType, PaidRentPackage paidRentPackage)
		{
			if (ServicesConfig.InteractiveService.Question("Подобрать оборудование автоматически по типу?")) {
				var existingItems = Entity.OrderEquipments
					.Where(x => x.OrderRentDepositItem != null || x.OrderRentServiceItem != null)
					.Select(x => x.Nomenclature.Id)
					.Distinct()
					.ToArray();

				var anyNomenclature = NomenclatureRepository.GetAvailableNonSerialEquipmentForRent(UoW, paidRentPackage.EquipmentKind, existingItems);
				AddPaidRent(rentType, paidRentPackage, anyNomenclature);
			}
			else
			{
				var equipmentForRentJournal =
					_nonSerialEquipmentsForRentJournalViewModelFactory.CreateNonSerialEquipmentsForRentJournalViewModel(paidRentPackage.EquipmentKind);

				equipmentForRentJournal.OnSelectResult += (sender, e) =>
				{
					var nomenclature = TryGetSelectedNomenclature(e);

					if(nomenclature == null)
					{
						return;
					}

					AddPaidRent(rentType, paidRentPackage, nomenclature);
				};
				TabParent.AddSlaveTab(this, equipmentForRentJournal);
			}
		}

		private void AddPaidRent(RentType rentType, PaidRentPackage paidRentPackage, Nomenclature equipmentNomenclature)
		{
			if(rentType == RentType.FreeRent)
			{
				throw new InvalidOperationException($"Не правильный тип аренды {RentType.FreeRent}, возможен только {RentType.NonfreeRent} или {RentType.DailyRent}");
			}
			var interactiveService = ServicesConfig.InteractiveService;
			if(equipmentNomenclature == null) {
				interactiveService.ShowMessage(ImportanceLevel.Error, "Для выбранного типа оборудования нет оборудования в справочнике номенклатур.");
				return;
			}

			var stock = _stockRepository.GetStockForNomenclature(UoW, equipmentNomenclature.Id);
			if(stock <= 0) {
				if(!interactiveService.Question($"На складах не найдено свободного оборудования\n({equipmentNomenclature.Name})\nДобавить принудительно?")) {
					return;
				}
			}

			switch(rentType)
			{
				case RentType.NonfreeRent:
					Entity.AddNonFreeRent(paidRentPackage, equipmentNomenclature);
					break;
				case RentType.DailyRent:
					Entity.AddDailyRent(paidRentPackage, equipmentNomenclature);
					break;
			}
		}

		#endregion PaidRent

		#region FreeRent

		private void SelectFreeRentPackage()
		{
			var freeRentJournal = _rentPackagesJournalsViewModelsFactory.CreateFreeRentPackagesJournalViewModel(false, false, false, false);

			freeRentJournal.OnSelectResult += (sender, e) =>
			{
				var selectedRent = e.GetSelectedObjects<FreeRentPackagesJournalNode>().FirstOrDefault();

				if (selectedRent == null)
				{
					return;
				}

				var freeRentPackage = UoW.GetById<FreeRentPackage>(selectedRent.Id);
				SelectEquipmentForFreeRentPackage(freeRentPackage);
			};
			TabParent.AddTab(freeRentJournal, this);
		}

		private void SelectEquipmentForFreeRentPackage(FreeRentPackage freeRentPackage)
		{
			if(ServicesConfig.InteractiveService.Question("Подобрать оборудование автоматически по типу?"))
			{
				var existingItems = Entity.OrderEquipments
					.Where(x => x.OrderRentDepositItem != null || x.OrderRentServiceItem != null)
					.Select(x => x.Nomenclature.Id)
					.Distinct()
					.ToArray();

				var anyNomenclature = NomenclatureRepository.GetAvailableNonSerialEquipmentForRent(UoW, freeRentPackage.EquipmentKind, existingItems);
				AddFreeRent(freeRentPackage, anyNomenclature);
			}
			else
			{
				var equipmentForRentJournal =
					_nonSerialEquipmentsForRentJournalViewModelFactory.CreateNonSerialEquipmentsForRentJournalViewModel(freeRentPackage.EquipmentKind);

				equipmentForRentJournal.OnSelectResult += (sender, e) =>
				{
					var nomenclature = TryGetSelectedNomenclature(e);

					if(nomenclature == null)
					{
						return;
					}

					AddFreeRent(freeRentPackage, nomenclature);
				};
				TabParent.AddSlaveTab(this, equipmentForRentJournal);
			}
		}

		private void AddFreeRent(FreeRentPackage freeRentPackage, Nomenclature equipmentNomenclature)
		{
			var interactiveService = ServicesConfig.InteractiveService;
			if(equipmentNomenclature == null)
			{
				interactiveService.ShowMessage(ImportanceLevel.Error, "Для выбранного типа оборудования нет оборудования в справочнике номенклатур.");
				return;
			}

			var stock = _stockRepository.GetStockForNomenclature(UoW, equipmentNomenclature.Id);
			if(stock <= 0)
			{
				if(!interactiveService.Question($"На складах не найдено свободного оборудования\n({equipmentNomenclature.Name})\nДобавить принудительно?"))
				{
					return;
				}
			}

			Entity.AddFreeRent(freeRentPackage, equipmentNomenclature);
		}

		protected void OnYbuttonToStorageLogicAddressTypeClicked(object sender, EventArgs e)
		{
			if(Entity.OrderAddressType == OrderAddressType.Delivery
			   && !Entity.Client.IsChainStore
			   && !Entity.OrderItems.Any(x => x.IsMasterNomenclature))
			{
				Entity.OrderAddressType = OrderAddressType.StorageLogistics;
			}
		}

		protected void OnYbuttonToDeliveryAddressTypeClicked(object sender, EventArgs e)
		{
			if(Entity.OrderAddressType == OrderAddressType.StorageLogistics
			   && !Entity.Client.IsChainStore
			   && !Entity.OrderItems.Any(x => x.IsMasterNomenclature))
			{
				Entity.OrderAddressType = OrderAddressType.Delivery;
			}
		}

		private void UpdateOrderAddressTypeWithUI()
		{
			Entity.UpdateAddressType();

			if(Entity.SelfDelivery)
			{
				ylabelOrderAddressType.Visible = false;
				ybuttonToDeliveryAddressType.Visible = false;
				ybuttonToStorageLogicAddressType.Visible = false;
				return;
			}

			switch(Entity.OrderAddressType)
			{
				case OrderAddressType.Delivery:
					ybuttonToDeliveryAddressType.Visible = false;
					ybuttonToStorageLogicAddressType.Visible = true;
					break;
				case OrderAddressType.StorageLogistics:
					ybuttonToDeliveryAddressType.Visible = true;
					ybuttonToStorageLogicAddressType.Visible = false;
					break;
				case OrderAddressType.ChainStore:
				case OrderAddressType.Service:
					ybuttonToDeliveryAddressType.Visible = false;
					ybuttonToStorageLogicAddressType.Visible = false;
					break;
			}
			ylabelOrderAddressType.Visible = true;
		}

		#endregion FreeRent

		private Nomenclature TryGetSelectedNomenclature(JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<NomenclatureForRentNode>().FirstOrDefault();

			return selectedNode == null ? null : UoW.GetById<Nomenclature>(selectedNode.Id);
		}

		#endregion
	}
}
