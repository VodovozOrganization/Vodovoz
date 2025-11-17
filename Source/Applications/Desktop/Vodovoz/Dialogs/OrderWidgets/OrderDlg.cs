using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Bindings.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Autofac;
using Core.Infrastructure;
using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Requests;
using EdoService.Library;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using Gamma.GtkWidgets.Cells;
using Gamma.Utilities;
using Gamma.Widgets;
using Gtk;
using NHibernate.Criterion;
using NLog;
using QS.Dialog;
using QS.Dialog.Gtk;
using QS.Dialog.GtkUI;
using QS.Dialog.GtkUI.FileDialog;
using QS.DocTemplates;
using QS.DomainModel.Entity;
using QS.DomainModel.NotifyChange;
using QS.DomainModel.Tracking;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Print;
using QS.Project.Dialogs;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Report;
using QS.Services;
using QS.Tdi;
using QS.Utilities.Extensions;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Extension;
using QSOrmProject;
using QSProjectsLib;
using QSWidgetLib;
using Vodovoz.Application.Orders;
using Vodovoz.Application.Orders.Services;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Cores;
using Vodovoz.Dialogs;
using Vodovoz.Dialogs.Client;
using Vodovoz.Dialogs.Email;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Sms;
using Vodovoz.Domain.StoredEmails;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.BasicHandbooks;
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
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.EntityRepositories.Payments;
using Vodovoz.EntityRepositories.ServiceClaims;
using Vodovoz.EntityRepositories.Stock;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Errors.Edo;
using Vodovoz.Errors.Logistics;
using Vodovoz.Errors.Orders;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Infrastructure.Print;
using Vodovoz.Journals.Nodes.Rent;
using Vodovoz.JournalViewModels;
using Vodovoz.Models;
using Vodovoz.Models.Orders;
using Vodovoz.Presentation.ViewModels.Controls.EntitySelection;
using Vodovoz.Presentation.ViewModels.Documents;
using Vodovoz.Presentation.ViewModels.PaymentTypes;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using Vodovoz.Settings.Roboats;
using Vodovoz.SidePanel;
using Vodovoz.SidePanel.InfoProviders;
using Vodovoz.SidePanel.InfoViews;
using Vodovoz.Specifications.Orders.EdoContainers;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Validation;
using Vodovoz.ViewModels.Dialogs.Counterparties;
using Vodovoz.ViewModels.Dialogs.Email;
using Vodovoz.ViewModels.Dialogs.Orders;
using Vodovoz.ViewModels.Infrastructure.InfoProviders;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Client;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Nomenclatures;
using Vodovoz.ViewModels.Journals.JournalViewModels.Rent;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.ViewModels.Logistic;
using Vodovoz.ViewModels.Widgets;
using Vodovoz.ViewModels.Widgets.EdoLightsMatrix;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Client;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.EntityRepositories.Edo;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Nodes;
using VodovozBusiness.NotificationSenders;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;
using VodovozInfrastructure.Utils;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using IntToStringConverter = Vodovoz.Infrastructure.Converters.IntToStringConverter;
using LogLevel = NLog.LogLevel;
using Order = Vodovoz.Domain.Orders.Order;
using Selection = Gdk.Selection;

namespace Vodovoz
{
	public partial class OrderDlg : EntityDialogBase<Order>,
		INotifyPropertyChanged,
		ICounterpartyInfoProvider,
		IDeliveryPointInfoProvider,
		ICustomWidthInfoProvider,
		IContractInfoProvider,
		ITdiTabAddedNotifier,
		IEmailsInfoProvider,
		ICallTaskProvider,
		ITDICloseControlTab,
		ISmsSendProvider,
		IFixedPricesHolderProvider,
		IAskSaveOnCloseViewModel,
		IEdoLightsMatrixInfoProvider
	{
		private readonly int? _defaultCallBeforeArrival = null;
		private readonly ITdiCompatibilityNavigation _navigationManager = Startup.MainWin.NavigationManager;

		private ILifetimeScope _lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
		private static Logger _logger = LogManager.GetCurrentClassLogger();
		private CancellationTokenSource _cancellationTokenCheckLiquidationSource;

		private readonly IRouteListService _routeListService = ScopeProvider.Scope.Resolve<IRouteListService>();
		private readonly INomenclatureSettings _nomenclatureSettings = ScopeProvider.Scope.Resolve<INomenclatureSettings>();
		private readonly INomenclatureRepository _nomenclatureRepository = ScopeProvider.Scope.Resolve<INomenclatureRepository>();
		private readonly INomenclatureService _nomenclatureService = ScopeProvider.Scope.Resolve<INomenclatureService>();

		private IFastDeliveryValidator _fastDeliveryValidator;

		private static readonly IDeliveryRulesSettings _deliveryRulesSettings = ScopeProvider.Scope.Resolve<IDeliveryRulesSettings>();

		private static readonly IDeliveryRepository _deliveryRepository = ScopeProvider.Scope.Resolve<IDeliveryRepository>();

		private IEdoService _edoService;
		private IEmailService _emailService;
		private string _lastDeliveryPointComment;

		public event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		public event PropertyChangedEventHandler PropertyChanged;

		private Order _templateOrder;

		private SelectPaymentTypeViewModel _selectPaymentTypeViewModel;

		private int _previousDeliveryPointId;
		private int _paidDeliveryNomenclatureId;
		private int _fastDeliveryNomenclatureId;
		private int _advancedPaymentNomenclatureId;

		private IOrderSettings _orderSettings;
		private IOrganizationSettings _organizationSettings;
		private IPaymentFromBankClientController _paymentFromBankClientController;
		private RouteListAddressKeepingDocumentController _routeListAddressKeepingDocumentController;

		private IGenericRepository<EdoContainer> _edoContainerRepository;

		private readonly IRouteListSettings _routeListSettings = ScopeProvider.Scope.Resolve<IRouteListSettings>();
		private readonly IDocumentPrinter _documentPrinter = ScopeProvider.Scope.Resolve<IDocumentPrinter>();
		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory = ScopeProvider.Scope.Resolve<IEntityDocumentsPrinterFactory>();
		private readonly IEmployeeService _employeeService = ScopeProvider.Scope.Resolve<IEmployeeService>();
		private readonly IEmployeeRepository _employeeRepository = ScopeProvider.Scope.Resolve<IEmployeeRepository>();
		private readonly IUserRepository _userRepository = ScopeProvider.Scope.Resolve<IUserRepository>();
		private readonly IFlyerRepository _flyerRepository = ScopeProvider.Scope.Resolve<IFlyerRepository>();
		private readonly IDocTemplateRepository _docTemplateRepository = ScopeProvider.Scope.Resolve<IDocTemplateRepository>();
		private readonly IServiceClaimRepository _serviceClaimRepository = ScopeProvider.Scope.Resolve<IServiceClaimRepository>();
		private readonly IStockRepository _stockRepository = ScopeProvider.Scope.Resolve<IStockRepository>();
		private readonly IOrderRepository _orderRepository = ScopeProvider.Scope.Resolve<IOrderRepository>();
		private readonly IDiscountReasonRepository _discountReasonRepository = ScopeProvider.Scope.Resolve<IDiscountReasonRepository>();
		private readonly IRouteListItemRepository _routeListItemRepository = ScopeProvider.Scope.Resolve<IRouteListItemRepository>();
		private readonly IEmailRepository _emailRepository = ScopeProvider.Scope.Resolve<IEmailRepository>();
		private readonly ICashRepository _cashRepository = ScopeProvider.Scope.Resolve<ICashRepository>();
		private readonly IPromotionalSetRepository _promotionalSetRepository = ScopeProvider.Scope.Resolve<IPromotionalSetRepository>();
		private readonly IUndeliveredOrdersRepository _undeliveredOrdersRepository = ScopeProvider.Scope.Resolve<IUndeliveredOrdersRepository>();
		private readonly IEdoDocflowRepository _edoDocflowRepository = ScopeProvider.Scope.Resolve<IEdoDocflowRepository>();
		private readonly ICounterpartyRepository _counterpartyRepository = ScopeProvider.Scope.Resolve<ICounterpartyRepository>();
		private readonly IOrganizationRepository _organizationRepository = ScopeProvider.Scope.Resolve<IOrganizationRepository>();
		private readonly IRouteListChangesNotificationSender _routeListChangesNotificationSender = ScopeProvider.Scope.Resolve<IRouteListChangesNotificationSender>();
		private readonly IInteractiveService _interactiveService = ScopeProvider.Scope.Resolve<IInteractiveService>();
		private readonly ICurrentPermissionService _currentPermissionService = ScopeProvider.Scope.Resolve<ICurrentPermissionService>();
		private readonly IUnitOfWorkFactory _unitOfWorkFactory = ScopeProvider.Scope.Resolve<IUnitOfWorkFactory>();

		private IOrderService _orderService => ScopeProvider.Scope
			.Resolve<IOrderService>();
		private IPaymentService _paymentService => ScopeProvider.Scope
			.Resolve<IPaymentService>();

		private ICounterpartyService _counterpartyService;
		private IPartitioningOrderService _partitioningOrderService;

		private readonly IRentPackagesJournalsViewModelsFactory _rentPackagesJournalsViewModelsFactory
			= ScopeProvider.Scope.Resolve<IRentPackagesJournalsViewModelsFactory>();

		private readonly INonSerialEquipmentsForRentJournalViewModelFactory _nonSerialEquipmentsForRentJournalViewModelFactory
			= ScopeProvider.Scope.Resolve<INonSerialEquipmentsForRentJournalViewModelFactory>();

		private readonly IPaymentItemsRepository _paymentItemsRepository = ScopeProvider.Scope.Resolve<IPaymentItemsRepository>();
		private readonly IPaymentsRepository _paymentsRepository = ScopeProvider.Scope.Resolve<IPaymentsRepository>();
		private readonly DateTime _date = new DateTime(2020, 11, 09, 11, 0, 0);

		private readonly bool _canSetOurOrganization =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
				"can_set_organization_from_order_and_counterparty");

		private readonly bool _canResendDocumentsToEdo =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_edo_documents");

		private readonly bool _canEditSealAndSignatureUpd =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_seal_and_signature_UPD");

		private readonly bool _canEditDeliveryDateAfterOrderConfirmation =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission(
				"can_edit_deliverydate_after_order_confirmation");

		private readonly bool _canCreateOrderInAdvance =
			ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_can_create_order_in_advance");

		private bool _isEditOrderClicked;
		private int _treeItemsNomenclatureColumnWidth;
		private IList<DiscountReason> _discountReasons;
		private Employee _currentEmployee;
		private bool _canChangeDiscountValue;
		private bool _canChoosePremiumDiscount;
		private INomenclatureFixedPriceController _nomenclatureFixedPriceController;
		private IOrderDiscountsController _discountsController;
		private IOrderDailyNumberController _dailyNumberController;
		private bool _isNeedSendBillToEmail;
		private Email _emailAddressForBill;
		private DateTime? _previousDeliveryDate;
		private IObservableList<EdoDockflowData> _edoEdoDocumentDataNodes = new ObservableList<EdoDockflowData>();
		private IObservableList<EdoContainer> _edoContainers = new ObservableList<EdoContainer>();
		private string _commentManager;
		private StringBuilder _summaryInfoBuilder = new StringBuilder();
		private EdoDockflowData _selectedEdoDocumentDataNode;
		private IFastDeliveryHandler _fastDeliveryHandler;
		private IFreeLoaderChecker _freeLoaderChecker;
		private IOrderFromOnlineOrderCreator _orderFromOnlineOrderCreator;
		private IBottlesRepository _bottlesRepository;
		private IDeliveryPointRepository _deliveryPointRepository;
		private IOrderContractUpdater _orderContractUpdater;
		private ICounterpartyEdoAccountController _counterpartyEdoAccountController;
		private IUnitOfWorkGeneric<Order> _slaveUnitOfWork = null;
		private OrderDlg _slaveOrderDlg = null;
		private bool _canEditOrderExtraCash;
		private bool _canSetContractCloser;
		private bool _canSetPaymentAfterLoad;
		private bool _canCloseOrders;
		private bool _canAddOnlineStoreNomenclaturesToOrder;
		private bool _canEditOrder;
		private bool _allowLoadSelfDelivery;
		private bool _acceptCashlessPaidSelfDelivery;
		private bool _canEditGoodsInRouteList;
		public bool IsStatusForEditGoodsInRouteList => _orderRepository.GetStatusesForEditGoodsInOrderInRouteList().Contains(Entity.OrderStatus);
		private UndeliveryViewModel _undeliveryViewModel;

		private SendDocumentByEmailViewModel SendDocumentByEmailViewModel { get; set; }

		public ITdiCompatibilityNavigation NavigationManager;

		private bool _justCreated = false;

		private Result _lastSaveResult;

		private readonly IGeneralSettings _generalSettingsSettings = ScopeProvider.Scope.Resolve<IGeneralSettings>();
		public bool IsWaitUntilActive => Entity.OrderStatus.IsIn(
			OrderStatus.Accepted, 
			OrderStatus.OnLoading, 
			OrderStatus.InTravelList, 
			OrderStatus.OnTheWay)
			&& _generalSettingsSettings.GetIsOrderWaitUntilActive;
		private TimeSpan? _lastWaitUntilTime;

		private List<(int Id, decimal Count, decimal Sum)> _orderItemsOriginalValues = new List<(int Id, decimal Count, decimal Sum)>();

		public EdoDockflowData SelectedEdoDocumentDataNode
		{
			get => _selectedEdoDocumentDataNode;
			set
			{
				if(_selectedEdoDocumentDataNode == value)
				{
					return;
				}

				_selectedEdoDocumentDataNode = value;
				CustomizeSendDocumentAgainButton();
			}
		}

		#region Работа с боковыми панелями

		public new int? WidthRequest => 420;

		public PanelViewType[] InfoWidgets
		{
			get
			{
				return new[]
				{
					PanelViewType.FixedPricesPanelView,
					PanelViewType.CounterpartyView,
					PanelViewType.DeliveryPricePanelView,
					PanelViewType.DeliveryPointView,
					PanelViewType.EmailsPanelView,
					PanelViewType.CallTaskPanelView,
					PanelViewType.SmsSendPanelView,
					PanelViewType.EdoLightsMatrixPanelView
				};
			}
		}

		public OrderAddressType? TypeOfAddress => Entity.OrderAddressType;

		public CounterpartyContract Contract => Entity.Contract;

		public bool CanHaveEmails => Entity.Id != 0;

		public Order Order => Entity;

		public List<StoredEmail> GetEmails() => Entity.Id != 0 ? _emailRepository.GetAllEmailsForOrder(UoW, Entity.Id) : null;

		private ICallTaskWorker _callTaskWorker;

		public virtual ICallTaskWorker CallTaskWorker
		{
			get
			{
				if(_callTaskWorker == null)
				{
					_callTaskWorker = ScopeProvider.Scope.Resolve<ICallTaskWorker>();
				}

				return _callTaskWorker;
			}
			set { _callTaskWorker = value; }
		}

		public bool? IsForRetail
		{
			get => _isForRetail;
			set => _isForRetail = value;
		}

		public PaymentType PaymentType
		{
			get => Entity.PaymentType;
			set => Entity.UpdatePaymentType(value, _orderContractUpdater);
		}

		public PaymentFrom PaymentByCardFrom
		{
			get => Entity.PaymentByCardFrom;
			set => Entity.UpdatePaymentByCardFrom(value, _orderContractUpdater);
		}

		public DateTime? DeliveryDate
		{
			get => Entity.DeliveryDate;
			set
			{
				Entity.UpdateDeliveryDate(value, _orderContractUpdater, out var message);

				if(!string.IsNullOrWhiteSpace(message))
				{
					MessageDialogHelper.RunWarningDialog(message);
				}
			}
		}

		public Counterparty Counterparty
		{
			get => Entity.Client;
			set
			{
				Entity.UpdateClient(value, _orderContractUpdater, out var message);

				if(!string.IsNullOrWhiteSpace(message))
				{
					MessageDialogHelper.RunWarningDialog(message);
				}
			}
		}

		public Organization Organization => Contract?.Organization;

		public DeliveryPoint DeliveryPoint
		{
			get => Entity.DeliveryPoint;
			set => Entity.UpdateDeliveryPoint(value, _orderContractUpdater);
		}

		private bool? _isForRetail = null;

		public bool? IsForSalesDepartment;

		public bool AskSaveOnClose => CanEditByPermission;

		#endregion

		#region Конструкторы, настройка диалога

		public override void Destroy()
		{
			if(_undeliveryViewModel != null)
			{
				_undeliveryViewModel.Saved -= OnUndeliveryViewModelSaved;
			}
			NotifyConfiguration.Instance.UnsubscribeAll(this);
			_lifetimeScope?.Dispose();
			_lifetimeScope = null;

			base.Destroy();
		}

		public OrderDlg()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Order>();
			Entity.Author = _currentEmployee = _employeeService.GetEmployeeForUser(UoW, _userRepository.GetCurrentUser(UoW).Id);
			if(Entity.Author == null)
			{
				MessageDialogHelper.RunErrorDialog(
					"Ваш пользователь не привязан к действующему сотруднику, вы не можете создавать заказы, так как некого указывать в качестве автора документа.");
				FailInitialize = true;
				return;
			}

			Entity.OrderStatus = OrderStatus.NewOrder;
			TabName = "Новый заказ";
			ConfigureDlg();
			//по стандарту тип - доставка
			Entity.OrderAddressType = OrderAddressType.Delivery;
		}

		public OrderDlg(OnlineOrder onlineOrder) : this()
		{
			var thisSessionOnlineOrder = UoW.GetById<OnlineOrder>(onlineOrder.Id);
			_orderFromOnlineOrderCreator.FillOrderFromOnlineOrder(UoW, Entity, thisSessionOnlineOrder, manualCreation: true);

			UpdateCallBeforeArrivalMinutesSelectedItem();
			Entity.UpdateDocuments();
			CheckForStopDelivery();
			UpdateOrderAddressTypeWithUI();
			AddCommentsFromDeliveryPoint();
			SetLogisticsRequirementsCheckboxes();
		}

		public OrderDlg(IUnitOfWorkGeneric<Order> unitOfWork)
		{
			Build();
			UoWGeneric = unitOfWork;
			Entity.OrderStatus = OrderStatus.NewOrder;
			TabName = "Новый заказ на забор оборудования";
			Entity.OrderAddressType = OrderAddressType.Delivery;
			ConfigureDlg();
		}

		public OrderDlg(Counterparty client, Phone contactPhone) : this()
		{
			Entity.UpdateClient(UoW.GetById<Counterparty>(client.Id), _orderContractUpdater, out var updateClientMessage);
			_phonesJournal.FilterViewModel.Counterparty = Counterparty;
			Entity.UpdatePaymentType(Counterparty.PaymentMethod, _orderContractUpdater);
			IsForRetail = Counterparty.IsForRetail;
			IsForSalesDepartment = Counterparty.IsForSalesDepartment;

			if(contactPhone != null)
			{
				Entity.ContactPhone = UoW.GetById<Phone>(contactPhone.Id);

				if(contactPhone.DeliveryPoint != null)
				{
					Entity.UpdateDeliveryPoint(UoW.GetById<DeliveryPoint>(contactPhone.DeliveryPoint.Id), _orderContractUpdater);
				}
			}

			AddCommentsFromDeliveryPoint();
			CheckForStopDelivery();
		}

		public OrderDlg(int id)
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Order>(id);
			IsForRetail = UoWGeneric.Root.Client.IsForRetail;
			IsForSalesDepartment = UoWGeneric.Root.Client.IsForSalesDepartment;
			ConfigureDlg();
		}

		public OrderDlg(Order sub) : this(sub.Id)
		{
		}

		/// <summary>
		/// Конструктор создан изначально для Mango-Интеграции,
		/// </summary>
		/// <param name="copiedOrder">Конструктор копирует заказ по Id заказа</param>
		/// <param name="NeedCopy"><c>true</c> копировать заказ, <c>false</c> работает как обычный конструктор.</param>
		public OrderDlg(int orderId, bool NeedCopy) : this()
		{
			if(NeedCopy)
			{
				var copiedOrder = UoW.GetById<Order>(orderId);
				Entity.UpdateClient(
					UoW.GetById<Counterparty>(copiedOrder.Client.Id), _orderContractUpdater, out var updateClientMessage);

				if(copiedOrder.DeliveryPoint != null)
				{
					Entity.UpdateDeliveryPoint(UoW.GetById<DeliveryPoint>(copiedOrder.DeliveryPoint.Id), _orderContractUpdater);
				}

				Entity.UpdatePaymentType(Counterparty.PaymentMethod, _orderContractUpdater);
				_orderContractUpdater.UpdateOrCreateContract(UoW, Entity);
				FillOrderItems(copiedOrder);
				CheckForStopDelivery();
				AddCommentsFromDeliveryPoint();
			}
		}

		public void CopyOrderFrom(int orderId)
		{
			Entity.IsCopiedFromUndelivery = true;

			var orderCopyModel = new OrderCopyModel(_nomenclatureSettings, _flyerRepository, _orderContractUpdater);
			var copying = orderCopyModel.StartCopyOrder(UoW, orderId, Entity)
				.CopyFields()
				.CopyStockBottle()
				.CopyPromotionalSets()
				.CopyOrderItems(false, true, true)
				.CopyPaidDeliveryItem()
				.CopyAdditionalOrderEquipments()
				.CopyOrderDepositItems()
				.CopyAttachedDocuments();

			if(copying.GetCopiedOrder.PaymentType == PaymentType.PaidOnline)
			{
				var currentPaymentFromTypes = ySpecPaymentFrom.ItemsList.Cast<PaymentFrom>().ToList();

				var copiedPaymentByCardFrom = copying.GetCopiedOrder.PaymentByCardFrom;

				if(!currentPaymentFromTypes
						.Contains(copiedPaymentByCardFrom))
				{
					currentPaymentFromTypes.Add(copiedPaymentByCardFrom);
					ySpecPaymentFrom.ItemsList = currentPaymentFromTypes;
				}

				copying.CopyPaymentByCardDataIfPossible();
			}

			if(copying.GetCopiedOrder.PaymentType == PaymentType.DriverApplicationQR
				|| copying.GetCopiedOrder.PaymentType == PaymentType.SmsQR)
			{
				copying.CopyPaymentByQrDataIfPossible();
				hbxOnlineOrder.Visible = UpdateVisibilityHboxOnlineOrder();
			}

			Entity.UpdateDocuments();
			CheckForStopDelivery();
			UpdateOrderAddressTypeWithUI();
			SetLogisticsRequirementsCheckboxes();

			var isCopiedOrderHasDiscountsWithoutPromosets =
				copying.OrderItemsFromCopiedOrderHavingDiscountsWithoutPromosets
				.Any(x => x.Nomenclature.Id != _paidDeliveryNomenclatureId);

			if(isCopiedOrderHasDiscountsWithoutPromosets)
			{
				MessageDialogHelper.RunWarningDialog(
					"Внимание!\nСкидки из исходного заказа не были скопированы.\nПри необходимости установите скидки заново");
			}
		}

		//Копирование меньшего количества полей чем в CopyOrderFrom для пункта "Повторить заказ" в журнале заказов
		public void CopyLesserOrderFrom(int orderId)
		{
			var orderCopyModel = new OrderCopyModel(_nomenclatureSettings, _flyerRepository, _orderContractUpdater);
			var copying = orderCopyModel.StartCopyOrder(UoW, orderId, Entity)
				.CopyFields(
					x => x.Client,
					x => x.DeliveryPoint,
					x => x.PaymentType,
					x => x.OrderAddressType,
					x => x.ContactPhone)
				.CopyPromotionalSets()
				.CopyOrderItemsExceptEquipmentReferenced();

			if(Counterparty.PersonType == PersonType.legal)
			{
				Entity.UpdatePaymentType(Counterparty.PaymentMethod, _orderContractUpdater);
			}

			Entity.UpdateDocuments();
			CheckForStopDelivery();
			UpdateOrderAddressTypeWithUI();
			AddCommentsFromDeliveryPoint();
			SetLogisticsRequirementsCheckboxes();
		}

		public void ConfigureDlg()
		{
			SetPermissions();

			_paidDeliveryNomenclatureId = _nomenclatureSettings.PaidDeliveryNomenclatureId;
			_fastDeliveryNomenclatureId = _nomenclatureSettings.FastDeliveryNomenclatureId;
			_advancedPaymentNomenclatureId = _nomenclatureSettings.AdvancedPaymentNomenclatureId;
			_fastDeliveryHandler = _lifetimeScope.Resolve<IFastDeliveryHandler>();
			_fastDeliveryValidator = _lifetimeScope.Resolve<IFastDeliveryValidator>();
			_counterpartyService = _lifetimeScope.Resolve<ICounterpartyService>();
			_edoService = _lifetimeScope.Resolve<IEdoService>();
			_emailService = _lifetimeScope.Resolve<IEmailService>();
			NavigationManager = Startup.MainWin.NavigationManager;
			_selectPaymentTypeViewModel = new SelectPaymentTypeViewModel(NavigationManager);
			_lastDeliveryPointComment = DeliveryPoint?.Comment.Trim('\n').Trim(' ') ?? string.Empty;
			_counterpartyService = _lifetimeScope.Resolve<ICounterpartyService>();
			_orderFromOnlineOrderCreator = _lifetimeScope.Resolve<IOrderFromOnlineOrderCreator>();
			_bottlesRepository = _lifetimeScope.Resolve<IBottlesRepository>();
			_deliveryPointRepository = _lifetimeScope.Resolve<IDeliveryPointRepository>();
			_orderContractUpdater = _lifetimeScope.Resolve<IOrderContractUpdater>();

			_edoContainerRepository = _lifetimeScope.Resolve<IGenericRepository<EdoContainer>>();
			_freeLoaderChecker = _lifetimeScope.Resolve<IFreeLoaderChecker>();
			_partitioningOrderService = _lifetimeScope.Resolve<IPartitioningOrderService>();
			_counterpartyEdoAccountController = _lifetimeScope.Resolve<ICounterpartyEdoAccountController>();
			_organizationSettings = _lifetimeScope.Resolve<IOrganizationSettings>();
			_paymentFromBankClientController = _lifetimeScope.Resolve<IPaymentFromBankClientController>();

			_justCreated = UoWGeneric.IsNew;

			if(_currentEmployee == null)
			{
				_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _userRepository.GetCurrentUser(UoW).Id);
			}

			_previousDeliveryDate = DeliveryDate;

			_nomenclatureFixedPriceController = _lifetimeScope.Resolve<INomenclatureFixedPriceController>();
			_discountsController = new OrderDiscountsController(_nomenclatureFixedPriceController);
			_routeListAddressKeepingDocumentController = new RouteListAddressKeepingDocumentController(_employeeRepository, _nomenclatureRepository);

			enumDiscountUnit.SetEnumItems((DiscountUnits[])Enum.GetValues(typeof(DiscountUnits)));

			_orderSettings = ScopeProvider.Scope.Resolve<IOrderSettings>();
			_dailyNumberController = new OrderDailyNumberController(_orderRepository, ServicesConfig.UnitOfWorkFactory);

			NotifyConfiguration.Instance.BatchSubscribeOnEntity<NomenclatureFixedPrice>(OnNomenclatureFixedPriceChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<DeliveryPoint, Phone>(OnDeliveryPointChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<Counterparty, Phone>(OnCounterpartyChanged);
			NotifyConfiguration.Instance.BatchSubscribeOnEntity<LogisticsRequirements>(OnLogisticsRequirementsChanged);

			ConfigureTrees();
			ConfigureAcceptButtons();
			ConfigureButtonActions();
			ConfigureSendDocumentByEmailWidget();

			btnCancel.Clicked += (sender, args) => OnCloseTab(CanEditByPermission, CloseSource.Cancel);

			spinDiscount.Adjustment.Upper = 100;
			_commentManager = Entity.CommentManager ?? string.Empty;

			if(Entity.PreviousOrder != null)
			{
				labelPreviousOrder.Text = "Посмотреть предыдущий заказ";
			}
			else
			{
				labelPreviousOrder.Visible = false;
			}

			hboxStatusButtons.Visible = _orderRepository
				.GetStatusesForOrderCancelation()
				.Contains(Entity.OrderStatus)
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

			//Подписывемся на изменения листа для сокрытия колонки промонаборов
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

			enumSignatureType.Binding.AddBinding(Entity, s => s.SignatureType, w => w.SelectedItemOrNull).InitializeFromSource();

			labelCreationDateValue.Binding
				.AddFuncBinding(Entity, s => s.CreateDate.HasValue ? s.CreateDate.Value.ToString("dd.MM.yyyy HH:mm") : "", w => w.LabelProp)
				.InitializeFromSource();

			ylabelOrderStatus.Binding
				.AddFuncBinding(Entity, e => e.OrderStatus.GetEnumTitle(), w => w.LabelProp)
				.InitializeFromSource();
			ylabelNumber.Binding
				.AddFuncBinding(Entity, e => e.Code1c + (e.DailyNumber.HasValue ? $" ({e.DailyNumber})" : ""), w => w.LabelProp)
				.InitializeFromSource();

			enumDocumentType.ItemsEnum = typeof(DefaultDocumentType);
			enumDocumentType.Binding.AddBinding(Entity, s => s.DocumentType, w => w.SelectedItem).InitializeFromSource();

			chkContractCloser.Binding.AddBinding(Entity, c => c.IsContractCloser, w => w.Active).InitializeFromSource();

			chkCommentForDriver.Binding.AddBinding(Entity, c => c.HasCommentForDriver, w => w.Active).InitializeFromSource();

			speciallistcomboboxCallBeforeArrivalMinutes.ShowSpecialStateNot = true;
			speciallistcomboboxCallBeforeArrivalMinutes.NameForSpecialStateNot = "Не нужен";

			speciallistcomboboxCallBeforeArrivalMinutes.ItemsList = new int?[] { null, 15, 30, 60 };

			speciallistcomboboxCallBeforeArrivalMinutes.Binding
				.AddBinding(Entity, e => e.CallBeforeArrivalMinutes, w => w.SelectedItem)
				.InitializeFromSource();

			if(UoWGeneric.IsNew
				|| (Entity.CallBeforeArrivalMinutes is null && Entity.IsDoNotMakeCallBeforeArrival != true))
			{
				//Для новых и кривых заказов добавляем и выставляем пустое значение, чтобы пользователь вручную выбрал нужный вариант
				speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = _defaultCallBeforeArrival;
			}

			speciallistcomboboxCallBeforeArrivalMinutes.ItemSelected += (s, e) =>
			{
				Entity.IsDoNotMakeCallBeforeArrival = !Entity.CallBeforeArrivalMinutes.HasValue;
			};

			specialListCmbOurOrganization.ItemsList = UoW.GetAll<Organization>();
			specialListCmbOurOrganization.Binding.AddBinding(Entity, o => o.OurOrganization, w => w.SelectedItem).InitializeFromSource();
			specialListCmbOurOrganization.Sensitive = _canSetOurOrganization;
			specialListCmbOurOrganization.ItemSelected += OnOurOrganisationsItemSelected;

			pickerDeliveryDate.Binding
				.AddBinding(this, dlg => dlg.DeliveryDate, w => w.DateOrNull)
				.InitializeFromSource();

			pickerDeliveryDate.DateChanged += PickerDeliveryDate_DateChanged;
			pickerDeliveryDate.DateChangedByUser += OnPickerDeliveryDateDateChangedByUser;

			pickerBillDate.Visible = labelBillDate.Visible = PaymentType == PaymentType.Cashless;
			pickerBillDate.Binding.AddBinding(Entity, s => s.BillDate, w => w.DateOrNull).InitializeFromSource();

			textComments.Binding.AddBinding(Entity, s => s.Comment, w => w.Buffer.Text).InitializeFromSource();

			checkSelfDelivery.Binding.AddBinding(Entity, s => s.SelfDelivery, w => w.Active).InitializeFromSource();
			checkPayAfterLoad.Binding.AddBinding(Entity, s => s.PayAfterShipment, w => w.Active).InitializeFromSource();
			checkDelivered.Binding.AddBinding(Entity, s => s.Shipped, w => w.Active).InitializeFromSource();
			ylabelloadAllowed.Binding
				.AddFuncBinding(Entity, s => s.LoadAllowedBy != null ? s.LoadAllowedBy.ShortName : string.Empty, w => w.Text)
				.InitializeFromSource();
			entryBottlesToReturn.ValidationMode = ValidationType.numeric;
			entryBottlesToReturn.Binding.AddBinding(Entity, e => e.BottlesReturn, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			entryBottlesToReturn.Changed += OnEntryBottlesToReturnChanged;

			ylabelGeoGroup.Binding
				.AddBinding(Entity, e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();

			specialListCmbSelfDeliveryGeoGroup.ItemsList = GetSelfDeliveryGeoGroups();
			specialListCmbSelfDeliveryGeoGroup.Binding
				.AddSource(Entity)
				.AddBinding(e => e.SelfDeliveryGeoGroup, w => w.SelectedItem)
				.AddBinding(e => e.SelfDelivery, w => w.Visible)
				.InitializeFromSource();

			yChkActionBottle.Binding.AddBinding(Entity, e => e.IsBottleStock, w => w.Active).InitializeFromSource();
			yEntTareActBtlFromClient.ValidationMode = ValidationType.numeric;
			yEntTareActBtlFromClient.Binding.AddBinding(Entity, e => e.BottlesByStockCount, w => w.Text, new IntToStringConverter())
				.InitializeFromSource();
			yEntTareActBtlFromClient.Changed += OnYEntTareActBtlFromClientChanged;

			if(Entity.OrderStatus == OrderStatus.Closed)
			{
				entryTareReturned.Text = ScopeProvider.Scope.Resolve<IBottlesRepository>().GetEmptyBottlesFromClientByOrder(UoW, _nomenclatureRepository, Entity)
					.ToString();
				entryTareReturned.Visible = lblTareReturned.Visible = true;
			}

			entryTrifle.ValidationMode = ValidationType.numeric;
			entryTrifle.Binding.AddBinding(Entity, e => e.Trifle, w => w.Text, new NullableIntToStringConverter()).InitializeFromSource();

			ylabelContract.Binding.AddFuncBinding(Entity,
				e => e.Contract != null && e.Contract.Organization != null
					? e.Contract.Title + " (" + e.Contract.Organization.FullName + ")"
					: string.Empty, w => w.Text).InitializeFromSource();

			OldFieldsConfigure();

			entOnlineOrder.ValidationMode = ValidationType.numeric;
			entOnlineOrder.Binding.AddBinding(Entity, e => e.OnlinePaymentNumber, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			var excludedPaymentFromIds = new[]
			{
				_orderSettings.PaymentByCardFromSmsId,
				_orderSettings.GetPaymentByCardFromAvangardId,
				_orderSettings.GetPaymentByCardFromFastPaymentServiceId,
				_orderSettings.PaymentByCardFromOnlineStoreId
			};

			var paymentFromItemsQuery = UoW.Session.QueryOver<PaymentFrom>();
			if(PaymentByCardFrom == null || !excludedPaymentFromIds.Contains(PaymentByCardFrom.Id))
			{
				paymentFromItemsQuery.WhereRestrictionOn(x => x.Id).Not.IsIn(excludedPaymentFromIds);
			}

			if(PaymentByCardFrom == null || !PaymentByCardFrom.IsArchive)
			{
				paymentFromItemsQuery.Where(p => !p.IsArchive);
			}

			ySpecPaymentFrom.ItemsList = paymentFromItemsQuery.List();
			ySpecPaymentFrom.Binding
				.AddBinding(this, e => e.PaymentByCardFrom, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Sensitive = false;
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(Entity)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItemOrNull)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();

			enumTax.ItemsEnum = typeof(TaxType);
			Enum[] hideTaxTypeEnums = { TaxType.None };
			enumTax.AddEnumToHideList(hideTaxTypeEnums);
			enumTax.ChangedByUser += (sender, args) => { Counterparty.TaxType = (TaxType)enumTax.SelectedItem; };

			entityselectionDeliverySchedule.ViewModel = CreateEntityselectionDeliveryScheduleViewModel();
			entityselectionDeliverySchedule.ViewModel.Changed += (s, e) => UpdateClientSecondOrderDiscount();

			var counterpartyFilter = _lifetimeScope.Resolve<CounterpartyJournalFilterViewModel>();

			entityVMEntryClient.SetEntityAutocompleteSelectorFactory(
				new EntityAutocompleteSelectorFactory<CounterpartyJournalViewModel>(typeof(Counterparty),
					() => new CounterpartyJournalViewModel(counterpartyFilter, ServicesConfig.UnitOfWorkFactory,
						ServicesConfig.CommonServices, Startup.MainWin.NavigationManager, filter =>
						{
							filter.IsForRetail = IsForRetail;
							filter.IsForSalesDepartment = IsForSalesDepartment;
							filter.RestrictIncludeArchive = false;
						}))
			);
			entityVMEntryClient.Binding.AddBinding(this, dlg => dlg.Counterparty, w => w.Subject).InitializeFromSource();
			entityVMEntryClient.CanEditReference = true;
			entityVMEntryClient.BeforeChangeByUser += OnClientBeforeChangeByUser;

			evmeContactPhone.SetObjectDisplayFunc<Phone>((phone) => phone.ToString());
			evmeContactPhone.Binding.AddSource(Entity)
				.AddBinding(e => e.ContactPhone, w => w.Subject)
				.AddFuncBinding(e => e.Client != null, w => w.Sensitive)
				.InitializeFromSource();

			var roboatsSettings = _lifetimeScope.Resolve<IRoboatsSettings>();
			var roboatsFileStorageFactory = new RoboatsFileStorageFactory(roboatsSettings, ServicesConfig.CommonServices.InteractiveService, ErrorReporter.Instance);
			var deliveryScheduleRepository = ScopeProvider.Scope.Resolve<IDeliveryScheduleRepository>();
			var fileDialogService = new FileDialogService();
			var _roboatsViewModelFactory = new RoboatsViewModelFactory(roboatsFileStorageFactory, fileDialogService,
				ServicesConfig.CommonServices.CurrentPermissionService);

			ybuttonFastDeliveryCheck.Clicked += OnButtonFastDeliveryCheckClicked;

			ycheckFastDelivery.Binding.AddBinding(Entity, e => e.IsFastDelivery, w => w.Active).InitializeFromSource();
			ycheckFastDelivery.Toggled += OnCheckFastDeliveryToggled;

			chkDontArriveBeforeInterval.Binding
				.AddBinding(Entity, e => e.DontArriveBeforeInterval, w => w.Active)
				.InitializeFromSource();

			evmeAuthor.Binding.AddBinding(Entity, s => s.Author, w => w.Subject).InitializeFromSource();
			evmeAuthor.Sensitive = false;

			entryDeliveryPoint.ViewModel = new LegacyEEVMBuilderFactory<OrderDlg>(this, this, UoW, NavigationManager, _lifetimeScope)
				.ForProperty(dlg => dlg.DeliveryPoint)
				.UseViewModelJournalAndAutocompleter<DeliveryPointByClientJournalViewModel, DeliveryPointJournalFilterViewModel>(filter =>
				{
					filter.Counterparty = Counterparty;
				})
				.UseViewModelDialog<DeliveryPointViewModel>()
				.Finish();

			entryDeliveryPoint.ViewModel.Changed += OnReferenceDeliveryPointChanged;
			entryDeliveryPoint.ViewModel.ChangedByUser += OnReferenceDeliveryPointChangedByUser;

			_phonesJournal = ScopeProvider.Scope.Resolve<PhonesJournalViewModel>();

			var phoneSelectoFactory = new EntityAutocompleteSelectorFactory<PhonesJournalViewModel>(typeof(Phone),
				() => _phonesJournal);
			evmeContactPhone.SetEntitySelectorFactory(phoneSelectoFactory);

			_phonesJournal.FilterViewModel.Counterparty = Counterparty;
			_phonesJournal.FilterViewModel.DeliveryPoint = DeliveryPoint;

			entryDeliveryPoint.ViewModel.ChangedByUser += (s, e) =>
			{
				if(Entity?.DeliveryPoint == null)
				{
					return;
				}

				if(!DeliveryPoint.IsActive
					&& !MessageDialogHelper.RunQuestionDialog("Данный адрес деактивирован, вы уверены, что хотите выбрать его?")
				)
				{
					Entity.UpdateDeliveryPoint(null, _orderContractUpdater);
				}
				UpdateOrderItemsPrices();
			};

			chkContractCloser.Sensitive = _canSetContractCloser;

			buttonViewDocument.Sensitive = false;
			btnDeleteOrderItem.Sensitive = false;
			ntbOrderEdit.ShowTabs = false;
			ntbOrderEdit.Page = 0;
			ntbOrder.ShowTabs = false;
			ntbOrder.Page = 0;

			buttonSelectPaymentType.Clicked += OnSelectPaymentTypeClicked;
			buttonSelectPaymentType.Sensitive = true;

			yentryPaymentType.Binding
				.AddFuncBinding(Entity, e => e.PaymentType.GetEnumTitle(), w => w.Text)
				.InitializeFromSource();

			SetSensitivityOfPaymentType();

			enumAddRentButton.ItemsEnum = typeof(RentType);
			enumAddRentButton.EnumItemClicked += (sender, e) => AddRent((RentType)e.ItemEnum);

			checkSelfDelivery.Toggled += (sender, e) =>
			{
				if(checkSelfDelivery.Active)
				{
					if(!_selectPaymentTypeViewModel.ExcludedPaymentTypes.Contains(PaymentType.DriverApplicationQR))
					{
						_selectPaymentTypeViewModel.AddExcludedPaymentTypes(PaymentType.DriverApplicationQR);
					}

					if(PaymentType == PaymentType.DriverApplicationQR)
					{
						if(Counterparty?.PaymentMethod != PaymentType.DriverApplicationQR)
						{
							Entity.UpdatePaymentType(Counterparty.PaymentMethod, _orderContractUpdater);
						}
						else
						{
							MessageDialogHelper.RunWarningDialog("Не возможно определить тип оплаты автоматически", "Тип оплаты был сброшен!");
							Entity.UpdatePaymentType(PaymentType.Cash, _orderContractUpdater);
						}
					}
				}
				else
				{
					_selectPaymentTypeViewModel.RemoveExcludedPaymentTypes(PaymentType.DriverApplicationQR);

					Entity.SelfDeliveryGeoGroup = null;
					specialListCmbSelfDeliveryGeoGroup.ShowSpecialStateNot = true;
				}

				SetDeliveryScheduleSelectionEditable();

				ybuttonFastDeliveryCheck.Sensitive =
					ycheckFastDelivery.Sensitive = !checkSelfDelivery.Active && Entity.CanChangeFastDelivery;
				lblDeliveryPoint.Sensitive = entryDeliveryPoint.Sensitive = !checkSelfDelivery.Active;
				buttonAddMaster.Sensitive = !checkSelfDelivery.Active;

				UpdateClientDefaultParam();

				if((DeliveryPoint != null || Entity.SelfDelivery) && Entity.OrderStatus == OrderStatus.NewOrder)
				{
					OnFormOrderActions();
				}

				UpdateOrderAddressTypeUI();
				_orderContractUpdater.UpdateOrCreateContract(UoW, Entity);
				UpdateOrderItemsPrices();
				SetLogisticsRequirementsCheckboxes();

				speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = null;
			};

			dataSumDifferenceReason.Binding.AddBinding(Entity, s => s.SumDifferenceReason, w => w.Text).InitializeFromSource();

			spinSumDifference.Binding.AddBinding(Entity, e => e.ExtraMoney, w => w.ValueAsDecimal).InitializeFromSource();

			labelSum.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderSum), w => w.LabelProp)
				.InitializeFromSource();
			labelCashToReceive.Binding.AddFuncBinding(Entity, e => CurrencyWorks.GetShortCurrencyString(e.OrderCashSum), w => w.LabelProp)
				.InitializeFromSource();

			buttonCopyManagerComment.Clicked += OnButtonCopyManagerCommentClicked;
			textManagerComments.Binding.AddBinding(Entity, e => e.CommentManager, w => w.Buffer.Text).InitializeFromSource();
			lastComment.Binding
				.AddBinding(Entity, e => e.LastOPCommentUpdate, w => w.Text)
				.InitializeFromSource();

			textDriverCommentFromMobile.Binding.AddBinding(Entity, e => e.DriverMobileAppComment, w => w.Buffer.Text)
				.InitializeFromSource();

			enumDiverCallType.ItemsEnum = typeof(DriverCallType);
			enumDiverCallType.Binding.AddBinding(Entity, e => e.DriverCallType, w => w.SelectedItem).InitializeFromSource();
			driverCallId.Binding.AddFuncBinding(Entity, e => e.DriverCallId == null ? "" : e.DriverCallId.ToString(), w => w.LabelProp)
				.InitializeFromSource();

			ySpecCmbNonReturnReason.ItemsList = UoW.Session.QueryOver<NonReturnReason>().List();
			ySpecCmbNonReturnReason.Binding.AddBinding(Entity, e => e.TareNonReturnReason, w => w.SelectedItem).InitializeFromSource();
			ySpecCmbNonReturnReason.ItemSelected += (sender, e) => Entity.IsTareNonReturnReasonChangedByUser = true;
			ySpecCmbNonReturnReason.Sensitive = CanEditByPermission;

			if(DeliveryPoint == null && !string.IsNullOrWhiteSpace(Entity.Address1c))
			{
				var deliveryPoint = Counterparty.DeliveryPoints.FirstOrDefault(d => d.Address1c == Entity.Address1c);
				Entity.UpdateDeliveryPoint(deliveryPoint, _orderContractUpdater);
			}

			_orderItemEquipmentCountHasChanges = false;
			ShowOrderColumnInDocumentsList();

			SetSensitivityOfPaymentType();
			depositrefunditemsview.Configure(UoWGeneric, Entity);
			ycomboboxReason.SetRenderTextFunc<DiscountReason>(x => x.Name);
			ycomboboxReason.ItemsList = _discountReasons;
			ycomboboxReason.ItemSelected += OnYComboBoxReasonItemSelected;

			yCmbReturnTareReasonCategories.SetRenderTextFunc<ReturnTareReasonCategory>(x => x.Name);
			yCmbReturnTareReasonCategories.ItemsList = UoW.Session.QueryOver<ReturnTareReasonCategory>().List();
			yCmbReturnTareReasonCategories.Binding.AddBinding(Entity, e => e.ReturnTareReasonCategory, w => w.SelectedItem)
				.InitializeFromSource();
			yCmbReturnTareReasonCategories.Changed += YCmbReturnTareReasonCategoriesOnChanged;
			HboxReturnTareReasonCategoriesShow();

			yCmbReturnTareReasons.SetRenderTextFunc<ReturnTareReason>(x => x.Name);

			if(Entity.ReturnTareReasonCategory != null)
			{
				ChangeHboxReasonsVisibility();
			}

			yCmbReturnTareReasons.Binding.AddBinding(Entity, e => e.ReturnTareReason, w => w.SelectedItem).InitializeFromSource();

			yCmbPromoSets.SetRenderTextFunc<PromotionalSet>(x => x.ShortTitle);
			yCmbPromoSets.ItemSelected += YCmbPromoSets_ItemSelected;

			bool showEshop = Entity.EShopOrder == null;
			labelEShop.Visible = !showEshop;
			yvalidatedentryEShopOrder.ValidationMode = ValidationType.numeric;
			yvalidatedentryEShopOrder.Binding.AddBinding(Entity, c => c.EShopOrder, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();
			yvalidatedentryEShopOrder.Visible = !showEshop;

			yhboxCounterpartyExternalOrderId.Binding
				.AddFuncBinding(Entity, c => c.Client != null && c.Client.UseSpecialDocFields, w => w.Visible)
				.InitializeFromSource();

			yvalidatedentryCounterpartyExternalOrderId.Binding
				.AddBinding(Entity, o => o.CounterpartyExternalOrderId, w => w.Text)
				.InitializeFromSource();

			chkAddCertificates.Binding.AddBinding(Entity, c => c.AddCertificates, w => w.Active).InitializeFromSource();

			ToggleVisibilityOfDeposits(Entity.ObservableOrderDepositItems.Any());
			SetDiscountEditable();
			SetDiscountUnitEditable();

			spinSumDifference.Hide();
			labelSumDifference.Hide();
			dataSumDifferenceReason.Hide();
			labelSumDifferenceReason.Hide();

			UpdateUIState();

			yChkActionBottle.Toggled += (sender, e) =>
			{
				Entity.RecalculateStockBottles(_orderSettings);
				ControlsActionBottleAccessibility();
				ycomboboxReason.Sensitive = !yChkActionBottle.Active;
				SetDiscountUnitEditable();
				SetDiscountEditable();
			};
			ycheckContactlessDelivery.Binding.AddBinding(Entity, e => e.ContactlessDelivery, w => w.Active).InitializeFromSource();

			UpdateOrderAddressTypeUI();

			Entity.InteractiveService = new CastomInteractiveService();

			Entity.PropertyChanged += OnEntityPropertyChanged;

			if(Entity != null && Entity.Id != 0)
			{
				Entity.CheckDocumentExportPermissions();
			}

			ylabelOrderAddressType.Binding
				.AddFuncBinding(Entity, e => "Тип адреса: " + e.OrderAddressType.GetEnumTitle(), w => w.LabelProp)
				.InitializeFromSource();
			var canChangeOrderAddressType =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_change_order_address_type");
			ybuttonToStorageLogicAddressType.Sensitive = canChangeOrderAddressType;

			UpdateAvailableEnumSignatureTypes();

			btnUpdateEdoDocFlowStatus.Clicked += (sender, args) =>
			{
				UpdateEdoDocumentDataNodes();
				CustomizeSendDocumentAgainButton();
			};

			ybuttonSendDocumentAgain.Visible = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_edo_documents");
			ybuttonSendDocumentAgain.Clicked += OnButtonSendDocumentAgainClicked;
			CustomizeSendDocumentAgainButton();

			btnCopyEntityId.Sensitive = Entity.Id > 0;
			btnCopySummaryInfo.Clicked += OnBtnCopySummaryInfoClicked;

			logisticsRequirementsView.ViewModel = new LogisticsRequirementsViewModel(Entity.LogisticsRequirements ?? _orderService.GetLogisticsRequirements(Entity), ServicesConfig.CommonServices);
			UpdateEntityLogisticsRequirements();
			logisticsRequirementsView.ViewModel.Entity.PropertyChanged += OnLogisticsRequirementsSelectionChanged;

			timepickerWaitUntil.Binding.AddBinding(Entity, e => e.WaitUntilTime, w => w.TimeOrNull).InitializeFromSource();
			timepickerWaitUntil.GetDefaultTime = () => DateTime.Now.AddHours(1).TimeOfDay;

			_lastWaitUntilTime = Entity.WaitUntilTime;

			ybuttonSaveWaitUntil.Visible = Entity.Id != 0;

			OnEnumPaymentTypeChanged(null, EventArgs.Empty);
			UpdateCallBeforeArrivalVisibility();
			SetNearestDeliveryDateLoaderFunc();

			UpdateOrderItemsOriginalValues();

			RefreshBottlesDebtNotifier();

			RefreshDebtorDebtNotifier();
		}

		private void OnYbuttonSaveWaitUntilClicked(object sender, EventArgs e)
		{
			if(!IsWaitUntilActive)
			{
				return;
			}

			Entity.WaitUntilTime = timepickerWaitUntil.TimeOrNull;
			if(Save())
			{
				MessageDialogHelper.RunInfoDialog("Поле \"Ожидает до\" успешно сохранено.");
			}
			else
			{
				MessageDialogHelper.RunWarningDialog("Не удалось сохранить");
			}
		}

		private void UpdateOrderItemsOriginalValues()
		{
			_orderItemsOriginalValues.Clear();

			_orderItemsOriginalValues.AddRange(GetOrderItemsSmallNodes());
		}

		private IEnumerable<(int Id, decimal Count, decimal Sum)> GetOrderItemsSmallNodes()
			=> Entity.OrderItems.Select(oi => (oi.Id, oi.Count, oi.Sum));

		public void UpdateClientDefaultParam()
		{
			Entity.UpdateClientDefaultParam();
		}

		private void OnClientBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(Counterparty != null && Entity.IsOrderCashlessAndPaid)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					OrderErrors.PaidCashlessOrderClientReplacementError.Message);

				e.CanChange = false;
				return;
			}
			e.CanChange = true;
		}

		private void UpdateCallBeforeArrivalMinutesSelectedItem()
		{
			if(Entity.CallBeforeArrivalMinutes is null)
			{
				speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = SpecialComboState.Not;
			}
			else
			{
				speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = Entity.CallBeforeArrivalMinutes;
			}
		}

		private void SetPermissions()
		{
			var currentPermissionService = ServicesConfig.CommonServices.CurrentPermissionService;

			CanFormOrderWithLiquidatedCounterparty = currentPermissionService.ValidatePresetPermission(
				OrderPermissions.CanFormOrderWithLiquidatedCounterparty);

			_canChangeDiscountValue = currentPermissionService.ValidatePresetPermission("can_set_direct_discount_value");
			_canChoosePremiumDiscount = currentPermissionService.ValidatePresetPermission("can_choose_premium_discount");
			_canEditOrderExtraCash = currentPermissionService.ValidatePresetPermission("can_edit_order_extra_cash");
			_canSetContractCloser = currentPermissionService.ValidatePresetPermission("can_set_contract_closer");
			_canSetPaymentAfterLoad = currentPermissionService.ValidatePresetPermission("can_set_payment_after_load");
			_canCloseOrders = currentPermissionService.ValidatePresetPermission("can_close_orders");
			_canAddOnlineStoreNomenclaturesToOrder =
				currentPermissionService.ValidatePresetPermission("can_add_online_store_nomenclatures_to_order");
			_canEditOrder = currentPermissionService.ValidatePresetPermission("can_edit_order");
			_allowLoadSelfDelivery = currentPermissionService.ValidatePresetPermission(StorePermissions.Documents.CanLoadSelfDeliveryDocument);
			_acceptCashlessPaidSelfDelivery = currentPermissionService.ValidatePresetPermission("accept_cashless_paid_selfdelivery");
			_canEditGoodsInRouteList = currentPermissionService.ValidatePresetPermission(OrderPermissions.CanEditGoodsInRouteList);
		}

		private void OnSelectPaymentTypeClicked(object sender, EventArgs e)
		{
			NavigationManager.OpenViewModel<SelectPaymentTypeViewModel>(null, addingRegistrations: containerBuilder =>
			{
				containerBuilder.Register((cb) => _selectPaymentTypeViewModel);
			});

			_selectPaymentTypeViewModel.PaymentTypeSelected += OnPaymentTypeSelected;
		}

		private void OnPaymentTypeSelected(object sender, SelectPaymentTypeViewModel.PaymentTypeSelectedEventArgs e)
		{
			Entity.UpdatePaymentType(e.PaymentType, _orderContractUpdater);

			OnEnumPaymentTypeChangedByUser(this, EventArgs.Empty);
			_selectPaymentTypeViewModel.PaymentTypeSelected -= OnPaymentTypeSelected;
		}

		private IEnumerable<GeoGroup> GetSelfDeliveryGeoGroups()
		{
			var currentGeoGroupId = Entity?.SelfDeliveryGeoGroup?.Id;

			var geoGroups = UoW.GetAll<GeoGroup>()
				.Where(g => !g.IsArchived || g.Id == currentGeoGroupId)
				.ToList();

			return geoGroups;
		}

		private void UpdateCallBeforeArrivalVisibility()
		{
			var isNotFastDeliveryOrSelfDelivery = !(Entity.SelfDelivery || Entity.IsFastDelivery);

			hboxCallBeforeArrival.Visible = isNotFastDeliveryOrSelfDelivery;
		}

		private void OnEntityPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			switch(args.PropertyName)
			{
				case nameof(Order.OrderStatus):
					CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.OrderStatus));
					entityVMEntryClient.IsEditable = Entity.CanEditByStatus && CanEditByPermission;
					break;
				case nameof(Order.Contract):
					CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.Contract));
					break;
				case nameof(Order.Client):
					if(Counterparty != null)
					{
						try
						{
							_cancellationTokenCheckLiquidationSource = new CancellationTokenSource();
							_counterpartyService.StopShipmentsIfNeeded(Counterparty.Id, _currentEmployee.Id, _cancellationTokenCheckLiquidationSource.Token).GetAwaiter().GetResult();
						}
						catch(Exception e)
						{
							MessageDialogHelper.RunWarningDialog($"Не удалось проверить статус контрагента в ФНС. {e.Message}", "Ошибка проверки статуса контрагента в ФНС");
						}

						if (Counterparty.PersonType == PersonType.legal)
						{
							try
							{
								var totalDebt = _counterpartyRepository.GetTotalDebt(UoW, Counterparty.Id);
								var organizations = _organizationRepository.GetOrganizations(UoW);
								var orgDebts = new StringBuilder();

								foreach(var org in organizations)
								{
									var orgDebt = _counterpartyRepository.GetDebtByOrganization(UoW, Counterparty.Id, org.Id);
									if(orgDebt > 0)
									{
										orgDebts.AppendLine($"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">{org.FullName}: {orgDebt} руб.</span>");
									}
								}
								if(totalDebt > 0)
								{
									var message = $"У клиента имеется задолженность в размере {totalDebt} руб.";
									if(orgDebts.Length > 0)
									{
										message += "\nЗадолженность по следующим организациям:\n" + orgDebts.ToString();
									}
									message += "Пожалуйста, уведомите клиента о задолженности";
									_interactiveService.ShowMessage(
										ImportanceLevel.Warning,
										message,
										"Уведомление о задолженности клиента");
								}
							}
							catch(Exception ex)
							{
								_logger.Error(ex, $"Ошибка при получении задолженности по клиенту {Counterparty.Id}");
							}
						}

						if(!Entity.IsCopiedFromUndelivery)
						{
							_orderService.CheckAndAddBottlesToReferrerByReferFriendPromo(UoW, Entity, _canChangeDiscountValue);
						}
					}
					UpdateAvailableEnumSignatureTypes();
					UpdateOrderAddressTypeWithUI();
					SetLogisticsRequirementsCheckboxes(true);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Counterparty)));
					break;
				case nameof(Entity.OrderAddressType):
					UpdateOrderAddressTypeUI();
					CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(Entity.OrderAddressType));
					Entity.UpdateMasterCallNomenclatureIfNeeded(UoW, _orderContractUpdater);
					break;
				case nameof(Counterparty.IsChainStore):
					UpdateOrderAddressTypeWithUI();
					break;
				case nameof(Order.SelfDelivery):
					Entity.UpdateMasterCallNomenclatureIfNeeded(UoW, _orderContractUpdater);
					UpdateCallBeforeArrival();
					break;
				case nameof(Order.IsFastDelivery):
					UpdateCallBeforeArrival();
					break;
				case nameof(Order.PaymentType):
					OnEnumPaymentTypeChanged(null, EventArgs.Empty);
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PaymentType)));
					break;
				case nameof(Order.DeliveryPoint):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeliveryPoint)));
					break;
				case nameof(Order.PaymentByCardFrom):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PaymentByCardFrom)));
					break;
				case nameof(Order.DeliveryDate):
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeliveryDate)));
					break;
			}
		}

		private void UpdateCallBeforeArrival()
		{
			var isNotFastDeliveryOrSelfDelivery = !(Entity.SelfDelivery || Entity.IsFastDelivery);

			UpdateCallBeforeArrivalVisibility();

			if(isNotFastDeliveryOrSelfDelivery)
			{
				Entity.CallBeforeArrivalMinutes = _defaultCallBeforeArrival;
			}
		}

		private List<EdoDockflowData> GetEdoOutgoingDocuments()
		{
			var orderDocuments = new List<EdoDockflowData>();

			if(Entity.Id == 0)
			{
				return orderDocuments;
			}

			return _edoEdoDocumentDataNodes.ToList();
		}

		private void CustomizeSendDocumentAgainButton()
		{
			if(!_canResendDocumentsToEdo)
			{
				ybuttonSendDocumentAgain.Sensitive = false;
				ybuttonSendDocumentAgain.Label = "Отсутствуют права для повторной отправки";

				return;
			}

			if(SelectedEdoDocumentDataNode is null)
			{
				ybuttonSendDocumentAgain.Label = "Не выбран документ для повторной отправки";
				ybuttonSendDocumentAgain.Sensitive = false;

				return;
			}

			if(SelectedEdoDocumentDataNode.IsNewDockflow || SelectedEdoDocumentDataNode.OldEdoDocumentType is null)
			{
				ybuttonSendDocumentAgain.Label = "Документы по новому документообороту недоступны для повторной отправки";
				ybuttonSendDocumentAgain.Sensitive = false;

				return;
			}

			var selectedType = SelectedEdoDocumentDataNode.OldEdoDocumentType.Value;

			OrderEdoTrueMarkDocumentsActions resendAction;

			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				var resendActionQuery = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
						.Where(x => x.Order.Id == Entity.Id);

				switch(selectedType)
				{
					case DocumentContainerType.Upd:
						resendActionQuery.Where(x => x.IsNeedToResendEdoUpd);
						break;
					case DocumentContainerType.Bill:
						resendActionQuery.Where(x => x.IsNeedToResendEdoBill);
						break;
				}

				resendAction = resendActionQuery.FirstOrDefault();
			}

			var alreadyInProcess = resendAction != null
				&& (
						(resendAction.IsNeedToResendEdoUpd && selectedType == DocumentContainerType.Upd)
						|| (resendAction.IsNeedToResendEdoBill && selectedType == DocumentContainerType.Bill)
					);

			if(alreadyInProcess)
			{
				ybuttonSendDocumentAgain.Sensitive = false;
				ybuttonSendDocumentAgain.Label = $"Идет подготовка {selectedType.GetEnumTitle()}";

				return;
			}

			var outgoingEdoDocuments = GetEdoOutgoingDocuments();
			var canResendUpd = selectedType is DocumentContainerType.Upd && outgoingEdoDocuments.Any(x => !x.IsNewDockflow && x.OldEdoDocumentType == DocumentContainerType.Upd);
			var canResendBill = selectedType is DocumentContainerType.Bill && outgoingEdoDocuments.Any(x => !x.IsNewDockflow && x.OldEdoDocumentType == DocumentContainerType.Bill);

			if(canResendUpd || canResendBill)
			{
				ybuttonSendDocumentAgain.Sensitive = true;
				ybuttonSendDocumentAgain.Label = $"Отправить повторно {selectedType.GetEnumDisplayName()}";

				return;
			}

			ybuttonSendDocumentAgain.Sensitive = false;
			ybuttonSendDocumentAgain.Label = "Отправить повторно";
		}

		private void OnButtonSendDocumentAgainClicked(object sender, EventArgs e)
		{
			ResendUpd();
			CustomizeSendDocumentAgainButton();
		}

		private void ResendUpd()
		{
			if(SelectedEdoDocumentDataNode is null
				|| SelectedEdoDocumentDataNode.OldEdoDocumentType is null)
			{
				return;
			}

			var type = SelectedEdoDocumentDataNode.OldEdoDocumentType.Value;

			var edoValidateDocumentResult = _edoService.ValidateOrderForDocument(Entity, type);

			var outgoingEdoContainers = _edoContainers.Where(x => x.Type == type).ToList();

			var edoValidateContainerResult = _edoService.ValidateEdoContainers(outgoingEdoContainers);

			var edoValidateResult = edoValidateDocumentResult.Errors.Concat(edoValidateContainerResult.Errors);

			var isValidateFailure = edoValidateDocumentResult.IsFailure || edoValidateContainerResult.IsFailure;

			var errorMessages = edoValidateDocumentResult.Errors.Select(x => x.Message)
				.Concat(edoValidateContainerResult.Errors.Select(x => x.Message))
				.ToArray();

			if(isValidateFailure)
			{
				if(edoValidateResult.Any(error => error.Code == EdoErrors.AlreadyPaidUpd || error.Code == EdoErrors.AlreadySuccefullSended)
					&& !ServicesConfig.InteractiveService.Question(
						"Вы уверены, что хотите отправить повторно?\n" +
						string.Join("\n - ", errorMessages),
						"Требуется подтверждение!"))
				{
					return;
				}
			}

			_edoService.SetNeedToResendEdoDocumentForOrder(Entity, type);
		}

		private void OnLogisticsRequirementsSelectionChanged(object sender, PropertyChangedEventArgs e)
		{
			if(sender is LogisticsRequirements requirements)
			{
				if(e.PropertyName == nameof(LogisticsRequirements.ForwarderRequired))
				{
					var requirementComment = "Требуется экспедитор на адресе\n";
					if(requirements.ForwarderRequired)
					{
						AddLogisticsRequirementsCommentToOrderComment(requirementComment);
					}
					else
					{
						RemoveLogisticsRequirementsCommentFromOrderComment(requirementComment);
					}
				}

				if(e.PropertyName == nameof(LogisticsRequirements.DocumentsRequired))
				{
					var requirementComment = "Наличие паспорта/документов у водителя\n";
					if(requirements.DocumentsRequired)
					{
						AddLogisticsRequirementsCommentToOrderComment(requirementComment);
					}
					else
					{
						RemoveLogisticsRequirementsCommentFromOrderComment(requirementComment);
					}
				}

				if(e.PropertyName == nameof(LogisticsRequirements.RussianDriverRequired))
				{
					var requirementComment = "Требуется русский водитель\n";
					if(requirements.RussianDriverRequired)
					{
						AddLogisticsRequirementsCommentToOrderComment(requirementComment);
					}
					else
					{
						RemoveLogisticsRequirementsCommentFromOrderComment(requirementComment);
					}
				}

				if(e.PropertyName == nameof(LogisticsRequirements.PassRequired))
				{
					var requirementComment = "Требуется пропуск\n";
					if(requirements.PassRequired)
					{
						AddLogisticsRequirementsCommentToOrderComment(requirementComment);
					}
					else
					{
						RemoveLogisticsRequirementsCommentFromOrderComment(requirementComment);
					}
				}

				if(e.PropertyName == nameof(LogisticsRequirements.LargusRequired))
				{
					var requirementComment = "Только ларгус (газель не проедет)\n";
					if(requirements.LargusRequired)
					{
						AddLogisticsRequirementsCommentToOrderComment(requirementComment);
					}
					else
					{
						RemoveLogisticsRequirementsCommentFromOrderComment(requirementComment);
					}
				}
			}

			UpdateEntityLogisticsRequirements();
		}

		private void AddLogisticsRequirementsCommentToOrderComment(string comment)
		{
			if(!Entity.Comment.Contains(comment))
			{
				Entity.Comment = comment + Entity.Comment;
			}
		}

		private void RemoveLogisticsRequirementsCommentFromOrderComment(string comment)
		{
			if(Entity.Comment.Contains(comment))
			{
				Entity.Comment = Entity.Comment.Replace(comment, "");
			}
		}

		private void UpdateEntityLogisticsRequirements()
		{
			Entity.LogisticsRequirements = logisticsRequirementsView.ViewModel.Entity;
		}

		private void SetLogisticsRequirementsCheckboxes(bool clearCheckedCheckboxes = false)
		{
			if(logisticsRequirementsView.ViewModel != null)
			{
				var requirements =
					clearCheckedCheckboxes
					? new LogisticsRequirements()
					: _orderService.GetLogisticsRequirements(Entity);

				logisticsRequirementsView.ViewModel.Entity.CopyRequirementPropertiesValues(requirements);
				UpdateEntityLogisticsRequirements();
			}
		}

		private void UpdateAvailableEnumSignatureTypes()
		{
			var signatureTranscriptType = new object[] { OrderSignatureType.SignatureTranscript };
			if(Counterparty?.IsForRetail ?? false)
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
				if(Entity.IsNeedIndividualSetOnLoad(_counterpartyEdoAccountController) || Entity.IsNeedIndividualSetOnLoadForTender)
				{
					ResetFastDeliveryForNetworkClient();

					return;
				}

				if(Entity.DeliverySchedule?.Id != _deliveryRulesSettings.FastDeliveryScheduleId)
				{
					Entity.DeliverySchedule = UoW.GetById<DeliverySchedule>(_deliveryRulesSettings.FastDeliveryScheduleId);
				}

				Entity.AddFastDeliveryNomenclatureIfNeeded(UoW, _orderContractUpdater);
				return;
			}

			if(Entity.DeliverySchedule?.Id == _deliveryRulesSettings.FastDeliveryScheduleId)
			{
				Entity.DeliverySchedule = null;
			}

			Entity.RemoveFastDeliveryNomenclature(UoW, _orderContractUpdater);

			speciallistcomboboxCallBeforeArrivalMinutes.SelectedItem = null;
		}

		private void ResetFastDeliveryForNetworkClient()
		{
			ycheckFastDelivery.Active = false;

			ServicesConfig.InteractiveService.ShowMessage(
				ImportanceLevel.Error,
				"Нельзя выбрать доставку за час для сетевого клиента и клиента с целью покупки - Тендер");
		}

		private void OnButtonFastDeliveryCheckClicked(object sender, EventArgs e)
		{
			var fastDeliveryValidationResult = _fastDeliveryValidator.ValidateOrder(Entity);

			if(fastDeliveryValidationResult.IsFailure)
			{
				ShowErrorsWindow(fastDeliveryValidationResult.Errors);
				return;
			}

			var fastDeliveryAvailabilityHistory = _deliveryRepository.GetRouteListsForFastDeliveryForOrder(
				UoW,
				(double)DeliveryPoint.Latitude.Value,
				(double)DeliveryPoint.Longitude.Value,
				isGetClosestByRoute: false,
				Entity.GetAllGoodsToDeliver(),
				DeliveryPoint.District.TariffZone.Id,
				fastDeliveryOrder: Entity
			);

			var fastDeliveryAvailabilityHistoryModel = new FastDeliveryAvailabilityHistoryModel(ServicesConfig.UnitOfWorkFactory);
			fastDeliveryAvailabilityHistoryModel.SaveFastDeliveryAvailabilityHistory(fastDeliveryAvailabilityHistory);

			var fastDeliveryVerificationViewModel = new FastDeliveryVerificationViewModel(fastDeliveryAvailabilityHistory);
			Startup.MainWin.NavigationManager.OpenViewModel<FastDeliveryVerificationDetailsViewModel, FastDeliveryVerificationViewModel>(
				null, fastDeliveryVerificationViewModel);
		}

		private void OnOurOrganisationsItemSelected(object sender, ItemSelectedEventArgs e)
		{
			_orderContractUpdater.UpdateOrCreateContract(UoW, Entity);
			UpdateOrderItemsPrices();
		}

		private void TryAddFlyers()
		{
			if(Entity.SelfDelivery
				|| (Entity.OrderStatus != OrderStatus.NewOrder && Entity.OrderStatus != OrderStatus.WaitForPayment)
				|| DeliveryPoint?.District == null
				|| !DeliveryDate.HasValue)
			{
				return;
			}

			var geographicGroupId = DeliveryPoint.District.GeographicGroup.Id;
			var activeFlyers = _flyerRepository.GetAllActiveFlyersByDate(UoW, DeliveryDate.Value);

			if(!activeFlyers.Any())
			{
				if(Entity.ObservableOrderEquipments.Any())
				{
					RemoveFlyers();
				}
				return;
			}

			RemoveFlyers();

			if(Entity.Contract?.Organization?.Id == _organizationSettings.KulerServiceOrganizationId)
			{
				return;
			}

			foreach(var flyer in activeFlyers)
			{
				if(!_orderRepository.HasFlyersOnStock(UoW, _routeListSettings, flyer.FlyerNomenclature.Id, geographicGroupId))
				{
					continue;
				}

				if(!flyer.IsForFirstOrder)
				{
					Entity.AddFlyerNomenclature(flyer.FlyerNomenclature);
				}
				else
				{
					if((Entity.Id == 0 && Counterparty != null && Counterparty.FirstOrder == null) || Entity.IsFirstOrder)
					{
						Entity.AddFlyerNomenclature(flyer.FlyerNomenclature);
					}
				}
			}
			orderEquipmentItemsView.UpdateActiveFlyersNomenclaturesIds();
			_previousDeliveryDate = DeliveryDate;
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

		private void OnLogisticsRequirementsChanged(EntityChangeEvent[] changeevents)
		{
			if(Counterparty != null && (Entity.SelfDelivery || DeliveryPoint != null))
			{
				foreach(var changeevent in changeevents)
				{
					if(changeevent.Entity is LogisticsRequirements newRequirements)
					{
						if(Entity.LogisticsRequirements?.Id == newRequirements.Id)
						{
							logisticsRequirementsView.ViewModel.Entity.CopyRequirementPropertiesValues(newRequirements);
							UpdateEntityLogisticsRequirements();
						}

						if(newRequirements.Id == Counterparty?.LogisticsRequirements?.Id || newRequirements.Id == DeliveryPoint?.LogisticsRequirements?.Id)
						{
							SetLogisticsRequirementsCheckboxes();
						}
					}
				}
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
			RefreshContactPhone();
		}

		private void RefreshDeliveryPointWithPhones()
		{
			DeliveryPoint.ReloadChildCollection(x => x.Phones, x => x.DeliveryPoint, UoW.Session);
			RefreshEntity(DeliveryPoint);
			RefreshContactPhone();
		}

		private void RefreshContactPhone()
		{
			if(Entity.ContactPhone == null)
			{
				return;
			}

			if(Counterparty.Phones.All(p => p.Number != Entity.ContactPhone.Number)
			   && DeliveryPoint.Phones.All(p => p.Number != Entity.ContactPhone.Number))
			{
				ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Контактный номер телефона, указанный в открытом заказе для связи, больше не существует.\nВыберите новый номер телефона для связи в заказе.");
				Entity.ContactPhone = null;
			}
		}

		private void OnNomenclatureFixedPriceChanged(EntityChangeEvent[] changeevents)
		{
			var changedEntities = changeevents.Select(x => x.Entity).OfType<NomenclatureFixedPrice>();
			if(changedEntities.Any(x => x.DeliveryPoint != null && DeliveryPoint != null && x.DeliveryPoint.Id == DeliveryPoint.Id))
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

		private void ControlsActionBottleAccessibility()
		{
			bool canAddAction = Entity.CanAddStockBottle(_orderRepository) || Entity.IsBottleStock;
			hboxBottlesByStock.Visible = canAddAction;
			lblActionBtlTareFromClient.Visible = yEntTareActBtlFromClient.Visible = yChkActionBottle.Active;
			hboxReturnTare.Visible = !canAddAction;
			yEntTareActBtlFromClient.Sensitive = canAddAction;
		}

		private void ConfigureTrees()
		{
			var colorPrimaryText = GdkColors.PrimaryText;
			var colorBlue = GdkColors.InfoText;
			var colorGreen = GdkColors.SuccessText;
			var colorPrimaryBase = GdkColors.PrimaryBase;
			var colorLightYellow = GdkColors.WarningBase;
			var colorLightRed = GdkColors.DangerBase;

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
					.AddNumericRenderer(node => node.Count, OnCountEdited)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0))
					.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddSetter((c, node) =>
					{
						bool result = true;

						if(node.RentType != OrderRentType.None)
						{
							result = false;
						}

						if(node.Nomenclature.Id == _paidDeliveryNomenclatureId)
						{
							result = false;
						}

						if(node.PromoSet != null && !node.PromoSet.CanEditNomenclatureCount)
						{
							result = false;
						}

						if(node.Nomenclature.Id == _fastDeliveryNomenclatureId)
						{
							result = false;
						}

						c.Editable = result;
					}).WidthChars(10)
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
					.EditedEvent(OnRentEdited)
				.AddColumn("Цена")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
					.Adjustment(new Adjustment(0, 0, 1000000, 1, 100, 0)).Editing(true)
					.AddSetter((c, node) => c.Editable = node.CanEditPrice)
					.EditedEvent(OnSpinPriceEdited)
					.AddSetter((NodeCellRendererSpin<OrderItem> c, OrderItem node) =>
					{
						if(Entity.OrderStatus == OrderStatus.NewOrder || (Entity.OrderStatus == OrderStatus.WaitForPayment && !Entity.SelfDelivery))//костыль. на Win10 не видна цветная цена, если виджет засерен
						{
							c.ForegroundGdk = colorPrimaryText;
							var fixedPrice = Order.GetFixedPriceOrNull(node.Nomenclature, node.TotalCountInOrder);
							if(fixedPrice != null && node.PromoSet == null && node.CopiedFromUndelivery == null)
							{
								c.ForegroundGdk = colorGreen;
							}
							else if(node.IsUserPrice && Nomenclature.GetCategoriesWithEditablePrice().Contains(node.Nomenclature.Category))
							{
								c.ForegroundGdk = colorBlue;
							}
						}
					})
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Альтерн.\nцена")
					.AddToggleRenderer(x => x.IsAlternativePrice).Editing(false)
				.AddColumn("В т.ч. НДС")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => CurrencyWorks.GetShortCurrencyString(x.IncludeNDS ?? 0))
					.AddSetter((c, n) => c.Visible = PaymentType == PaymentType.Cashless)
				.AddColumn("Сумма")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => CurrencyWorks.GetShortCurrencyString(node.ActualSum))
					.AddSetter((c, n) =>
					{
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
							c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null && n.PromoSet == null ? colorLightRed : colorPrimaryBase
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
				.AddColumn("Промонаборы").SetTag(nameof(Entity.PromotionalSets))
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.PromoSet == null ? "" : node.PromoSet.Name)
				.RowCells()
					.XAlign(0.5f)
				.Finish();
			treeItems.ItemsDataSource = Entity.ObservableOrderItems;
			treeItems.Selection.Mode = SelectionMode.Multiple;
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
					if(document.Type == OrderDocumentType.UPD
						|| document.Type == OrderDocumentType.SpecialUPD)
					{
						toggle.Activatable = CanEditByPermission && _canEditSealAndSignatureUpd;
					}
					else
					{
						toggle.Activatable = CanEditByPermission;
					}
				}) // Сделать только для  ISignableDocument, UDP и SpecialUPD
				.AddColumn("")
				.RowCells().AddSetter<CellRenderer>((c, n) =>
				{
					c.CellBackgroundGdk = colorPrimaryBase;
					if(n.Order.Id != n.AttachedToOrder.Id && !(c is CellRendererToggle))
					{
						c.CellBackgroundGdk = colorLightYellow;
					}
				})
				.Finish();
			treeDocuments.Selection.Mode = SelectionMode.Multiple;
			treeDocuments.ItemsDataSource = Entity.ObservableOrderDocuments;
			treeDocuments.Selection.Changed += Selection_Changed;

			treeDocuments.RowActivated += OnButtonViewDocumentClicked;

			treeViewEdoContainers.ColumnsConfig = FluentColumnsConfig<EdoDockflowData>.Create()
				.AddColumn("Новый\nдокументооборот")
					.AddToggleRenderer(x => x.IsNewDockflow)
					.Editing(false)
				.AddColumn("Код\nдокументооборота")
					.AddTextRenderer(x => x.DocFlowId.HasValue ? x.DocFlowId.ToString() : string.Empty)
				.AddColumn("Отправленные\nдокументы")
					.AddTextRenderer(x => x.DocumentType)
				.AddColumn("Статус\nдокументооборота")
					.AddTextRenderer(x => x.EdoDocFlowStatusString)
				.AddColumn("Доставлено\nклиенту?")
					.AddToggleRenderer(x => x.IsReceived)
					.Editing(false)
				.AddColumn("Описание ошибки")
					.AddTextRenderer(x => x.ErrorDescription)
					.WrapWidth(500)
				.AddColumn("Статус задачи\nнового документооборота")
					.AddTextRenderer(x => x.EdoTaskStatus.HasValue ? x.EdoTaskStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("Статус документа\nнового документооборота")
					.AddTextRenderer(x => x.EdoDocumentStatus.HasValue ? x.EdoDocumentStatus.Value.GetEnumTitle() : string.Empty)
				.AddColumn("")
				.Finish();

			treeViewEdoContainers.Binding
				.AddBinding(this, vm => vm.SelectedEdoDocumentDataNode, w => w.SelectedRow)
				.InitializeFromSource();

			if(Entity.Id != 0)
			{
				UpdateEdoDocumentDataNodes();
				CustomizeSendDocumentAgainButton();
			}

			treeViewEdoContainers.ItemsDataSource = _edoEdoDocumentDataNodes;

			treeServiceClaim.ColumnsConfig = ColumnsConfigFactory.Create<ServiceClaim>()
				.AddColumn("Статус заявки").AddTextRenderer(node => node.Status.GetEnumTitle())
				.AddColumn("Номенклатура оборудования").AddTextRenderer(node => node.Nomenclature != null ? node.Nomenclature.Name : "-")
				.AddColumn("Серийный номер").AddTextRenderer(node => node.Equipment != null && node.Equipment.Nomenclature.IsSerial ? node.Equipment.Serial : "-")
				.AddColumn("Причина").AddTextRenderer(node => node.Reason)
				.RowCells().AddSetter<CellRendererText>((c, n) =>
				{
					c.ForegroundGdk = n.RepeatedService ? GdkColors.DangerText : GdkColors.PrimaryText;
				})
				.Finish();

			treeServiceClaim.ItemsDataSource = Entity.ObservableInitialOrderService;
			treeServiceClaim.Selection.Changed += TreeServiceClaim_Selection_Changed;
		}

		private void OnSpinPriceEdited(object o, EditedArgs args)
		{
			decimal.TryParse(args.NewText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newPrice);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			orderItem.SetPrice(newPrice);
		}

		private void OnCountEdited(object o, EditedArgs args)
		{
			decimal.TryParse(args.NewText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newCount);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is OrderItem orderItem))
			{
				return;
			}

			Entity.SetOrderItemCount(orderItem, newCount);
			var path = new TreePath(args.Path);
			treeItems.YTreeModel.GetIter(out var iter, path);
			treeItems.YTreeModel.Adapter.EmitRowChanged(path, iter);
		}

		private void OnRentEdited(object o, EditedArgs args)
		{
			int.TryParse(args.NewText, out var newRentCount);
			var node = treeItems.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			orderItem.UpdateRentCount(newRentCount);
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

			Gtk.Application.Invoke((sender, eventArgs) =>
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
							" т.к. она из промонабора или на нее есть фикса.\nОбратитесь к руководителю");
					}
				}

				Order?.RecalculateItemsPrice();
			});
		}

		private void OnOpCommentChanged(object o, EventArgs args)
		{
			Entity.UpdateCommentManagerInfo(_currentEmployee);
		}

		private void UpdateEdoContainers(IUnitOfWork uow)
		{
			_edoContainers.Clear();

			var containers = _edoContainerRepository.Get(uow, EdoContainerSpecification.CreateForOrderId(Entity.Id));

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

				documents.AddRange(_edoDocflowRepository.GetEdoDocflowDataByOrderId(uow, Entity.Id));
				documents.AddRange(_edoContainers.Select(x => new EdoDockflowData(x)));
			}

			documents = documents
				.OrderByDescending(x => x.TaxcomDocflowCreationTime == null ? x.EdoRequestCreationTime : x.TaxcomDocflowCreationTime)
				.ToList();

			foreach(var document in documents)
			{
				_edoEdoDocumentDataNodes.Add(document);
			}
		}

		private void ConfigureAcceptButtons()
		{
			buttonAcceptOrderWithClose.Clicked += OnButtonAcceptOrderWithCloseClicked;
			buttonAcceptAndReturnToOrder.Clicked += OnButtonAcceptAndReturnToOrderClicked;
		}

		private MenuItem _menuItemCloseOrder = null;
		private MenuItem _menuItemSelfDeliveryToLoading = null;
		private MenuItem _menuItemSelfDeliveryPaid = null;
		private MenuItem _menuItemReturnToAccepted = null;

		/// <summary>
		/// Конфигурирование меню кнопок с дополнительными действиями заказа
		/// </summary>
		public void ConfigureButtonActions()
		{
			menubuttonActions.MenuAllocation = ButtonMenuAllocation.Top;
			menubuttonActions.MenuAlignment = ButtonMenuAlignment.Right;
			Menu menu = new Menu();

			_menuItemCloseOrder = new MenuItem("Закрыть без доставки");
			_menuItemCloseOrder.Activated += OnButtonCloseOrderClicked;
			menu.Add(_menuItemCloseOrder);

			_menuItemReturnToAccepted = new MenuItem("Вернуть в Принят");
			_menuItemReturnToAccepted.Activated += OnButtonReturnToAcceptedClicked;
			menu.Add(_menuItemReturnToAccepted);

			_menuItemSelfDeliveryToLoading = new MenuItem("Самовывоз на погрузку");
			_menuItemSelfDeliveryToLoading.Activated += OnButtonSelfDeliveryToLoadingClicked;
			menu.Add(_menuItemSelfDeliveryToLoading);

			_menuItemSelfDeliveryPaid = new MenuItem("Принять оплату самовывоза");
			_menuItemSelfDeliveryPaid.Activated += OnButtonSelfDeliveryAcceptPaidClicked;
			menu.Add(_menuItemSelfDeliveryPaid);

			menubuttonActions.Menu = menu;
			menubuttonActions.LabelXAlign = 0.5f;
			menu.ShowAll();
		}

		private void ConfigureSendDocumentByEmailWidget()
		{
			SendDocumentByEmailViewModel =
				new SendDocumentByEmailViewModel(
					ServicesConfig.UnitOfWorkFactory,
					_emailRepository,
					_lifetimeScope.Resolve<IEmailSettings>(),
					_currentEmployee,
					ServicesConfig.CommonServices);
			var sendEmailView = new SendDocumentByEmailView(SendDocumentByEmailViewModel);
			hbox20.Add(sendEmailView);
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

			if(Counterparty != null)
			{
				if(Counterparty.IsChainStore)
				{
					textODZComments.Binding.AddBinding(Entity, e => e.ODZComment, w => w.Buffer.Text)
						.InitializeFromSource();

				}
				else
				{
					labelODZComments.Visible = false;
					textODZComments.Visible = false;
					GtkScrolledWindow8.Visible = false;
				}
			}

			int currentUserId = _userRepository.GetCurrentUser(UoW).Id;
			bool canChangeCommentOdz = CanEditByPermission &&
				ServicesConfig.CommonServices.PermissionService.ValidateUserPresetPermission("can_change_odz_op_comment", currentUserId);

			textODZComments.Sensitive = canChangeCommentOdz;

			textOPComments.Binding.AddBinding(Entity, e => e.OPComment, w => w.Buffer.Text)
				.InitializeFromSource();
			textOPComments.Sensitive = CanEditByPermission;
			textOPComments.Buffer.Changed += OnOpCommentChanged;
		}

		#endregion

		#region Сохранение, закрытие заказа

		private bool SaveOrderBeforeContinue<T>()
		{
			if(UoWGeneric.IsNew)
			{
				if(CommonDialogs.SaveBeforeCreateSlaveEntity(EntityObject.GetType(), typeof(T)))
				{
					if(!Save())
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		private bool _canClose = true;

		public bool CanClose()
		{
			if(!_canClose)
			{
				MessageDialogHelper.RunInfoDialog("Дождитесь завершения задачи и повторите");
			}

			return _canClose;
		}

		private void SetSensitivity(bool isSensitive)
		{
			_canClose = isSensitive;
			buttonSave.Sensitive = CanEditByPermission && isSensitive;
			btnCancel.Sensitive = isSensitive;
		}

		protected bool Validate(ValidationContext validationContext)
		{
			validationContext.ServiceContainer.AddService(_orderSettings);
			validationContext.ServiceContainer.AddService(_deliveryRulesSettings);
			return ServicesConfig.ValidationService.Validate(Entity, validationContext);
		}

		public override bool Save()
		{
			try
			{
				SetSensitivity(false);

				if(_orderRepository.GetStatusesForFreeBalanceOperations().Contains(Entity.OrderStatus))
				{
					CreateDeliveryFreeBalanceOperations();
				}

				_lastSaveResult = null;

				Entity.CheckAndSetOrderIsService();

				ValidationContext validationContext = new ValidationContext(Entity);

				if(!Validate(validationContext))
				{
					_lastSaveResult = Result.Failure(OrderErrors.Validation);

					return false;
				}

				using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Обновление статуса оплаты из карточки заказа"))
				{
					_orderService.UpdatePaymentStatus(uow, Entity);
				}

				if(_orderItemEquipmentCountHasChanges)
				{
					MessageDialogHelper.RunInfoDialog("Было изменено количество оборудования в заказе, оно также будет изменено в дополнительном соглашении");
				}

				var canSendOrResendBillToEmail = (Entity.OrderStatus == OrderStatus.Accepted || Entity.OrderStatus == OrderStatus.WaitForPayment)
					|| (Entity.IsFastDelivery && Entity.OrderStatus == OrderStatus.OnTheWay);

				if(canSendOrResendBillToEmail)
				{
					PrepareSendBillInformation();
					var needToResendBill = CheckNeedBillResend();

					if(_isNeedSendBillToEmail)
					{
						_emailService.SendBillToEmail(UoW, Entity);
					}
					else if(needToResendBill)
					{
						if(_emailService.NeedResendBillToEmail(UoW, Entity))
						{
							_emailService.SendBillToEmail(UoW, Entity);
						}
						else if(_orderService.NeedResendByEdo(UoW, Entity))
						{
							_edoService.CancelOldEdoOffers(UoW, Entity);
							_edoService.SetNeedToResendEdoDocumentForOrder(Entity, DocumentContainerType.Bill);
						}
					}
				}

				_logger.Info("Сохраняем заказ...");

				Entity.SaveEntity(UoW, _orderContractUpdater, _currentEmployee, _dailyNumberController, _paymentFromBankClientController);

				if(Entity.WaitUntilTime != _lastWaitUntilTime)
				{
					// Пока нет доработки в мобильном тут оставим заглушенным
					// NotifyDriverAboutWaitingTimeChangedAsync();
				}

				_logger.Info("Ok.");
				UpdateUIState();
				btnCopyEntityId.Sensitive = true;
				TabName = typeof(Order).GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName;

				_lastSaveResult = Result.Success();

				return true;
			}
			catch(Exception e)
			{
				_lastSaveResult = Result.Failure(OrderErrors.Save);

				_logger.Log(LogLevel.Error, e);

				return false;
			}
			finally
			{
				SetSensitivity(true);
			}
		}

		private bool CheckNeedBillResend()
		{
			var currentOrderItemsValues = new List<(int Id, decimal Count, decimal Sum)>();

			currentOrderItemsValues.AddRange(GetOrderItemsSmallNodes());

			return !_orderItemsOriginalValues
				.All(eoiov => currentOrderItemsValues
					.Any(coiov => coiov.Id == eoiov.Id
						&& coiov.Count == eoiov.Count
						&& coiov.Sum == eoiov.Sum))
				|| !currentOrderItemsValues.All(coiov => _orderItemsOriginalValues
					.Any(eoiov => eoiov.Id == coiov.Id
						&& eoiov.Count == coiov.Count
						&& eoiov.Sum == coiov.Sum));
		}

		private void CreateDeliveryFreeBalanceOperations()
		{
			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity);

			if(routeListItem == null)
			{
				return;
			}

			_routeListAddressKeepingDocumentController.CreateOrUpdateRouteListKeepingDocumentByDiscrepancy(UoW, ServicesConfig.UnitOfWorkFactory, routeListItem, forceUsePlanCount: true);
		}

		protected void OnBtnSaveCommentClicked(object sender, EventArgs e)
		{
			Entity.SaveOrderComment();
		}

		protected void OnButtonEditClicked(object sender, EventArgs e)
		{
			_isEditOrderClicked = true;
			EditOrder();
		}

		private void EditOrder()
		{
			if(!Entity.CanSetOrderAsEditable)
			{
				return;
			}
			Entity.EditOrder(CallTaskWorker);
			UpdateUIState();
		}

		private Result AcceptOrder()
		{
			if(!Save())
			{
				return _lastSaveResult;
			}

			var possibleConfirmation = CheckPossibilityConfirmation();

			if(possibleConfirmation.IsFailure)
			{
				return possibleConfirmation;
			}

			using(var transaction = UoW.Session.BeginTransaction())
			{
				try
				{
					var acceptResult = TryAcceptOrder();

					if(!acceptResult.SplitedOrder && acceptResult.Result.IsSuccess)
					{
						transaction.Commit();
						GlobalUowEventsTracker.OnPostCommit((IUnitOfWorkTracked)UoW);
						Save();
					}

					return acceptResult.Result;
				}
				catch(Exception e)
				{
					if(!transaction.WasCommitted
						&& !transaction.WasRolledBack
						&& transaction.IsActive
						&& UoW.Session.Connection.State == ConnectionState.Open)
					{
						try
						{
							transaction.Rollback();
						}
						catch { }
					}

					transaction.Dispose();
					OnCloseTab(false);

					TabParent.OpenTab(() => new OrderDlg(Entity.Id));

					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						"Возникла ошибка при подтверждении заказа, заказ был сохранён в виде черновика, вкладка переоткрыта.");

					return Result.Failure(OrderErrors.AcceptException);
				}
			}
		}

		private Result CheckPossibilityConfirmation()
		{
			if(!Entity.CanSetOrderAsAccepted)
			{
				return Result.Failure(OrderErrors.CantEdit);
			}

			var canContinue = Entity.DefaultWaterCheck(ServicesConfig.InteractiveService);

			if(canContinue.HasValue && !canContinue.Value)
			{
				toggleGoods.Activate();
				return Result.Failure(OrderErrors.Accept.HasNoDefaultWater);
			}

			var validationResult = ValidateAndFormOrder();

			if(validationResult.IsFailure)
			{
				return Result.Failure(validationResult.Errors);
			}

			if(!CheckCertificates(canSaveFromHere: true))
			{
				return Result.Failure(OrderErrors.HasNoValidCertificates);
			}

			var promosetDuplicateFinder = new PromosetDuplicateFinder(_freeLoaderChecker, new CastomInteractiveService());

			var phones = new List<string>();

			phones.AddRange(Entity.Client.Phones.Select(x => x.DigitsNumber));

			if(DeliveryPoint != null)
			{
				phones.AddRange(Entity.DeliveryPoint.Phones.Select(x => x.DigitsNumber));
			}

			var hasPromoSetForNewClients = Entity.PromotionalSets.Any(x => x.PromotionalSetForNewClients);
			var hasOtherFirstRealOrder = _orderRepository.HasCounterpartyOtherFirstRealOrder(UoW, Counterparty, Entity.Id);

			if(hasPromoSetForNewClients && hasOtherFirstRealOrder)
			{
				if(!MessageDialogHelper.RunQuestionDialog(
					"В заказ добавлен промонабор для новых клиентов, но это не первый заказ клиента\n" +
					"Хотите продолжить сохранение?"))
				{
					return Result.Failure(OrderErrors.AcceptAbortedByUser);
				}
			}

			if(hasPromoSetForNewClients && Entity.OrderItems.Any(x => x.PromoSet != null))
			{
				if(!promosetDuplicateFinder.RequestDuplicatePromosets(UoW, Entity.Id, DeliveryPoint, phones))
				{
					return Result.Failure(OrderErrors.AcceptAbortedByUser);
				}
			}
			if(hasPromoSetForNewClients && _freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(
				UoW, Entity.SelfDelivery, Counterparty, DeliveryPoint))
			{
				return Result.Failure(OrderErrors.UnableToShipPromoSet);
			}

			PrepareSendBillInformation();

			if(_emailAddressForBill == null
			   && _emailService.NeedSendBillToEmail(UoW, Entity)
			   && (!Counterparty.NeedSendBillByEdo || CurrentCounterpartyEdoAccount().ConsentForEdoStatus != ConsentForEdoStatus.Agree)
			   && !MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?"))
			{
				return Result.Failure(OrderErrors.AcceptAbortedByUser);
			}

			var fastDeliveryResult = _fastDeliveryHandler.CheckFastDelivery(UoW, Entity);

			if(fastDeliveryResult.IsFailure)
			{
				if(fastDeliveryResult.Errors.Any(x => x.Code == nameof(FastDeliveryErrors.RouteListForFastDeliveryIsMissing)))
				{
					var fastDeliveryVerificationViewModel =
						new FastDeliveryVerificationViewModel(_fastDeliveryHandler.FastDeliveryAvailabilityHistory);
					NavigationManager.OpenViewModel<FastDeliveryVerificationDetailsViewModel, IUnitOfWork, FastDeliveryVerificationViewModel>(
						null, UoW, fastDeliveryVerificationViewModel);
				}

				return fastDeliveryResult;
			}

			var edoLightsMatrixViewModel = !(Startup.MainWin.InfoPanel.GetWidget(typeof(EdoLightsMatrixPanelView)) is EdoLightsMatrixPanelView edoLightsMatrixPanelView)
				? new EdoLightsMatrixViewModel()
				: edoLightsMatrixPanelView.ViewModel.EdoLightsMatrixViewModel;

			edoLightsMatrixViewModel.RefreshLightsMatrix(CurrentCounterpartyEdoAccount());

			var edoLightsMatrixPaymentType = PaymentType == PaymentType.Cashless
				? EdoLightsMatrixPaymentType.Cashless
				: EdoLightsMatrixPaymentType.Receipt;

			var isAccountableInChestniyZnak = Entity.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);

			if(isAccountableInChestniyZnak
			   && Entity.DeliveryDate >= new DateTime(2022, 11, 01)
			   && !edoLightsMatrixViewModel.IsPaymentAllowed(Entity.Client, edoLightsMatrixPaymentType)
			   && Counterparty.ReasonForLeaving != ReasonForLeaving.Tender)
			{
				if(ServicesConfig.InteractiveService.Question($"Данному контрагенту запрещено отгружать товары по выбранному типу оплаты\n" +
															  $"Оставить черновик заказа в статусе \"Новый\"?"))
				{
					if(Save())
					{
						return Result.Success();
					}

					return Result.Failure(OrderErrors.Save);
				}

				return Result.Failure(OrderErrors.AcceptAbortedByUser);
			}

			if(PaymentType == PaymentType.Cashless)
			{
				var hasUnknownEdoLightsType = edoLightsMatrixViewModel.HasUnknown();

				if(hasUnknownEdoLightsType
				   && !ServicesConfig.InteractiveService.Question(
					   $"Вы уверены, что клиент не работает с ЭДО и хотите отправить заказ без формирования электронной УПД?\nПродолжить?"))
				{
					return Result.Failure(OrderErrors.AcceptAbortedByUser);
				}
			}

			return Result.Success();
		}

		private CounterpartyEdoAccount CurrentCounterpartyEdoAccount()
		{
			var currentCounterpartyEdoAccount =
				_counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(Counterparty, Organization?.Id);
			return currentCounterpartyEdoAccount;
		}

		private (bool SplitedOrder, Result Result) TryAcceptOrder()
		{
			var orderPartsByOrganizations =
				_lifetimeScope.Resolve<IOrderOrganizationManager>()
					.GetOrderPartsByOrganizations(UoW, DateTime.Now.TimeOfDay, OrderOrganizationChoice.Create(Entity));

			if(!orderPartsByOrganizations.CanSplitOrderWithDeposits && Entity.ObservableOrderDepositItems.Any())
			{
				MessageDialogHelper.RunWarningDialog("Данный заказ содержит возврат залогов." +
					" И т.к. он содержит позиции, продаваемые от разных организаций," +
					" то сумма каждого отдельного заказа меньше возвращаемого залога, что не позволяет разбить его вместе с залогом");

				return (false, Result.Failure(OrderErrors.UnableToPartitionOrderWithBigDeposit));
			}

			var partsOrder = orderPartsByOrganizations.OrderParts.Count();

			if(partsOrder == 1)
			{
				var set = orderPartsByOrganizations.OrderParts.First();

				if(set.Goods != null && set.Goods.Any() && set.Goods.Count() != Entity.OrderItems.Count)
				{
					throw new InvalidOperationException(
						"Неправильное разбиение заказа. Несоответствие количества товаров в разбиении и начальном заказе");
				}
			}
			else if(partsOrder > 1)
			{
				if(!MessageDialogHelper.RunQuestionDialog(
					"Данный заказ содержит товары, продаваемые от нескольких организаций." +
					$" Будет произведено автоматическое разбиение на {partsOrder} заказа(ов), с последующим сохранением." +
					" Продолжаем?"))
				{
					return (false, Result.Failure(OrderErrors.AcceptAbortedByUser));
				}

				SplitOrder(orderPartsByOrganizations);
				OnCloseTab(false);

				return (true, Result.Success());
			}

			if(Contract == null && !Entity.IsLoadedFrom1C)
			{
				_orderContractUpdater.UpdateOrCreateContract(UoW, Entity);
			}

			Entity.AcceptOrder(_currentEmployee, CallTaskWorker);
			treeItems.Selection.UnselectAll();

			var addingToRouteListResult = _fastDeliveryHandler.TryAddOrderToRouteListAndNotifyDriver(UoW, Entity, _routeListService, CallTaskWorker);
			
			if(addingToRouteListResult.IsFailure)
			{
				return (false, addingToRouteListResult);
			}

			OpenNewOrderForDailyRentEquipmentReturnIfNeeded();
			ProcessSmsNotification();
			UpdateUIState();

			return (false, Result.Success());
		}

		private bool SplitOrder(PartitionedOrderByOrganizations orderPartsByOrganizations)
		{
			var needOpenSavedOrders = false;
			Result<IEnumerable<int>> result = null;

			result = _partitioningOrderService.CreatePartOrdersAndSave(Entity.Id, Entity.Author, orderPartsByOrganizations);

			if(result.IsFailure)
			{
				MessageDialogHelper.RunErrorDialog($"{result.Errors.First().Message}. Переоткройте заново заказ и повторите попытку");
				return false;
			}

			if(MessageDialogHelper.RunQuestionDialog("После сохранения открыть итоговые заказы?"))
			{
				foreach(var orderId in result.Value)
				{
					_navigationManager.OpenTdiTabOnTdi<OrderDlg, int>(null, orderId);
				}
			}

			return true;
		}

		private void PrepareSendBillInformation()
		{
			_emailAddressForBill = _emailService.GetEmailAddressForBill(Entity);

			if(_emailService.NeedSendBillToEmail(UoW, Entity)
			   && _emailAddressForBill != null)
			{
				_isNeedSendBillToEmail = true;
			}
			else
			{
				_isNeedSendBillToEmail = false;
			}
		}

		private void OnButtonAcceptOrderWithCloseClicked(object sender, EventArgs e) =>
			AcceptOrder()
				.Match(
					() =>
					{
						if(!NeedToCreateOrderForDailyRentEquipmentReturn)
						{
							OnCloseTab(false, CloseSource.Save);
						}
					},
					ReturnToNew);

		private void OnButtonAcceptAndReturnToOrderClicked(object sender, EventArgs e) =>
			AcceptOrder()
				.Match(
					ReturnToEditTab,
					ReturnToNew);

		private void ReturnToNew(IEnumerable<Error> errors)
		{
			if(errors.All(x => x == OrderErrors.AcceptException))
			{
				return;
			}

			if(errors.All(x => x != OrderErrors.AcceptException))
			{
				EditOrder();
			}

			ShowErrorsWindow(errors);
		}

		private void ShowErrorsWindow(IEnumerable<Error> errors)
		{
			if(errors.All(x => x == OrderErrors.Validation
				|| x == OrderErrors.AcceptAbortedByUser))
			{
				return;
			}

			var errorsStrings = errors.Select(x => $"{x.Message} : {x.Code}");

			MessageDialogHelper.RunErrorDialog(
				string.Join("\n", errorsStrings),
				"Ошибка подтверждения заказа");
		}

		private void ProcessSmsNotification()
		{
			var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			var smsNotifierSettings = _lifetimeScope.Resolve<ISmsNotifierSettings>();
			var smsNotifier = new SmsNotifier(uowFactory, smsNotifierSettings);
			smsNotifier.NotifyIfNewClient(Entity);
		}

		private Result ValidateAndFormOrder()
		{
			Entity.CheckAndSetOrderIsService();

			var uowFactory = _lifetimeScope.Resolve<IUnitOfWorkFactory>();
			var validationContext = new ValidationContext(Entity, null, new Dictionary<object, object>
			{
				{ "NewStatus", OrderStatus.Accepted },
				{ "uowFactory", uowFactory }
			});

			if(!Validate(validationContext))
			{
				return Result.Failure(OrderErrors.Validation);
			}

			if(DeliveryPoint != null && !DeliveryPoint.CalculateDistricts(UoW, _deliveryRepository).Any())
			{
				MessageDialogHelper.RunWarningDialog("Точка доставки не попадает ни в один из наших районов доставки. Пожалуйста, согласуйте стоимость доставки с руководителем и клиентом.");
			}

			OnFormOrderActions();
			return Result.Success();
		}

		/// <summary>
		/// Действия обрабатываемые при формировании заказа
		/// </summary>
		private void OnFormOrderActions()
		{
			_orderService.UpdateDeliveryCost(UoW, Entity);
		}

		/// <summary>
		/// Ручное закрытие заказа
		/// </summary>
		protected void OnButtonCloseOrderClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Accepted && _canCloseOrders)
			{
				if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите закрыть заказ?"))
				{
					return;
				}

				Entity.UpdateBottlesMovementOperationWithoutDelivery(
					UoW, _nomenclatureSettings, ScopeProvider.Scope.Resolve<IRouteListItemRepository>(), ScopeProvider.Scope.Resolve<ICashRepository>());
				Entity.UpdateDepositOperations(UoW);

				Entity.ChangeStatusAndCreateTasks(OrderStatus.Closed, CallTaskWorker);

				Entity.ResetOrderItemsActualCounts();
			}
			UpdateUIState();
		}
		/// <summary>
		/// Возврат в принят из ручного закрытия
		/// </summary>
		protected void OnButtonReturnToAcceptedClicked(object sender, EventArgs e)
		{
			if(Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted)
			{
				if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите вернуть заказ в статус \"Принят\"?"))
				{
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

			OrderDocumentsOpener(Entity.OrderDocuments
				.Where(x => x.Type == OrderDocumentType.Invoice
					|| x.Type == OrderDocumentType.InvoiceBarter
					|| x.Type == OrderDocumentType.InvoiceContractDoc)
				.OfType<PrintableOrderDocument>()
				.ToArray());
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
			if(!MessageDialogHelper.RunQuestionDialog("Вы уверены, что хотите удалить выделенные документы?"))
			{
				return;
			}

			var documents = treeDocuments.GetSelectedObjects<OrderDocument>();
			var notDeletedDocs = Entity.RemoveAdditionalDocuments(documents);

			if(notDeletedDocs != null && notDeletedDocs.Any())
			{
				string strDocuments = "";

				foreach(OrderDocument doc in notDeletedDocs)
				{
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
			if(Counterparty == null)
			{
				MessageDialogHelper.RunWarningDialog("Для добавления дополнительных документов должен быть выбран клиент.");
				return;
			}

			TabParent.AddSlaveTab(this, new AddExistingDocumentsDlg(UoWGeneric, Counterparty)
			);
		}

		protected void OnButtonViewDocumentClicked(object sender, EventArgs e)
		{
			var selectedObjects = treeDocuments.GetSelectedObjects().OfType<PrintableOrderDocument>().ToArray();

			OrderDocumentsOpener(selectedObjects);
		}

		/// <summary>
		/// Открытие соответствующего документу заказа окна.
		/// </summary>
		private void OrderDocumentsOpener(PrintableOrderDocument[] printableOrderDocuments)
		{
			_logger.Info("Открытие документа заказа");

			if(!printableOrderDocuments.Any())
			{
				return;
			}

			var rdlDocs =
				printableOrderDocuments
					.Where(d => d.PrintType == PrinterType.RDL)
					.ToArray();

			if(rdlDocs.Any())
			{
				var whatToPrint =
					rdlDocs.Length > 1
						? "документов"
						: "документа \"" + rdlDocs.Cast<OrderDocument>().First().Type.GetEnumTitle() + "\"";

				if(CanEditByPermission && UoWGeneric.HasChanges && CommonDialogs.SaveBeforePrint(typeof(Order), whatToPrint))
				{
					UoWGeneric.Save();
				}

				foreach(var doc in rdlDocs)
				{
					if(doc is IPrintableRDLDocument document)
					{
						NavigationManager
							.OpenViewModelOnTdi<PrintableRdlDocumentViewModel<IPrintableRDLDocument>, IPrintableRDLDocument>(this, document, OpenPageOptions.AsSlave);
					}
				}
			}

			var odtDocs =
				printableOrderDocuments
					.Where(d => d.PrintType == PrinterType.ODT)
					.ToArray();

			if(odtDocs.Any())
			{
				foreach(var doc in odtDocs)
				{
					if(doc is OrderContract orderContract)
					{
						if(orderContract.Id == 0)
						{
							MessageDialogHelper.RunInfoDialog("Перед просмотром документа необходимо сохранить заказ");
							return;
						}

						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<CounterpartyContract>(orderContract.Contract.Id),
							() =>
							{
								var dialog = OrmMain.CreateObjectDialog(orderContract.Contract);

								if(dialog != null)
								{
									(dialog as IEditableDialog).IsEditable = false;
								}

								return dialog;
							}
						);
					}
					else if(doc is OrderM2Proxy orderM2Proxy)
					{
						if(orderM2Proxy.Id == 0)
						{
							MessageDialogHelper.RunInfoDialog("Перед просмотром документа необходимо сохранить заказ");
							return;
						}

						TabParent.OpenTab(
							DialogHelper.GenerateDialogHashName<M2ProxyDocument>(orderM2Proxy.M2Proxy.Id),
							() =>
							{
								var dialog = OrmMain.CreateObjectDialog(orderM2Proxy.M2Proxy);

								if(dialog != null)
								{
									(dialog as IEditableDialog).IsEditable = false;
								}

								return dialog;
							}
						);
					}
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
		private bool CheckCertificates(bool canSaveFromHere = false)
		{
			Entity.UpdateCertificates(out List<Nomenclature> needUpdateCertificatesFor);
			if(needUpdateCertificatesFor.Any())
			{
				string msg = "Для следующих номенклатур устарели сертификаты продукции и добавлены в список документов не были:\n\n";
				msg += string.Join(
					"\t*",
					needUpdateCertificatesFor.Select(
						n => $" - {n.Name} (код номенклатуры: {n.Id})"
					)
				);
				msg += "\n\nПожалуйста обновите.";
				ButtonsType btns = ButtonsType.Ok;
				if(canSaveFromHere)
				{
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
			{
				ntbOrderEdit.CurrentPage = 0;
			}
		}

		protected void OnToggleTareControlToggled(object sender, EventArgs e)
		{
			if(toggleTareControl.Active)
			{
				ntbOrderEdit.CurrentPage = 1;
			}
		}

		protected void OnToggleGoodsToggled(object sender, EventArgs e)
		{
			if(toggleGoods.Active)
			{
				ntbOrderEdit.CurrentPage = 2;
			}
		}

		protected void OnToggleEquipmentToggled(object sender, EventArgs e)
		{
			if(toggleEquipment.Active)
			{
				ntbOrderEdit.CurrentPage = 3;
			}
		}

		protected void OnToggleServiceToggled(object sender, EventArgs e)
		{
			if(toggleService.Active)
			{
				ntbOrderEdit.CurrentPage = 4;
			}
		}

		protected void OnToggleDocumentsToggled(object sender, EventArgs e)
		{
			if(toggleDocuments.Active)
			{
				ntbOrderEdit.CurrentPage = 5;
			}

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
			{
				return;
			}

			ServiceClaimDlg dlg = new ServiceClaimDlg((treeServiceClaim.GetSelectedObjects()[0] as ServiceClaim).Id);
			mytab.TabParent.AddSlaveTab(mytab, dlg);
		}

		protected void OnButtonAddServiceClaimClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue<ServiceClaim>())
			{
				return;
			}

			var dlg = new ServiceClaimDlg(Entity);
			TabParent.AddSlaveTab(this, dlg);
		}

		protected void OnButtonAddDoneServiceClicked(object sender, EventArgs e)
		{
			if(!SaveOrderBeforeContinue<ServiceClaim>())
			{
				return;
			}

			OrmReference SelectDialog = new OrmReference(
				typeof(ServiceClaim),
				UoWGeneric,
				_serviceClaimRepository
					.GetDoneClaimsForClient(Entity)
					.GetExecutableQueryOver(UoWGeneric.Session)
					.RootCriteria
			)
			{
				Mode = OrmReferenceMode.Select,
				ButtonMode = ReferenceButtonMode.CanEdit
			};
			SelectDialog.ObjectSelected += DoneServiceSelected;

			TabParent.AddSlaveTab(this, SelectDialog);
		}

		private void DoneServiceSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			if(!(e.Subject is ServiceClaim selectedServiceClaim))
			{
				return;
			}
			var serviceClaim = UoW.GetById<ServiceClaim>(selectedServiceClaim.Id);
			serviceClaim.FinalOrder = Entity;
			Entity.ObservableFinalOrderService.Add(serviceClaim);
			//TODO: Add service nomenclature with price.
		}

		private void TreeServiceClaim_Selection_Changed(object sender, EventArgs e)
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

		private void YCmbPromoSets_ItemSelected(object sender, ItemSelectedEventArgs e)
		{
			if(!(e.SelectedItem is PromotionalSet proSet))
			{
				return;
			}

			if(CanAddNomenclaturesToOrder() && Entity.CanAddPromotionalSet(proSet, _freeLoaderChecker, _promotionalSetRepository))
			{
				ActivatePromotionalSet(proSet);
			}

			if(!yCmbPromoSets.IsSelectedNot)
			{
				yCmbPromoSets.SelectedItem = SpecialComboState.Not;
			}
		}

		private bool CanAddNomenclaturesToOrder()
		{
			if(Counterparty == null)
			{
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должен быть выбран клиент.");
				return false;
			}

			if(DeliveryPoint == null && !Entity.SelfDelivery)
			{
				MessageDialogHelper.RunWarningDialog("Для добавления товара на продажу должна быть выбрана точка доставки.");
				return false;
			}

			return true;
		}

		protected void OnButtonAddMasterClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
			{
				return;
			}

			var journalViewModel =
				_navigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
					this,
					f =>
					{
						f.AvailableCategories = new[] { NomenclatureCategory.master };
						f.RestrictCategory = NomenclatureCategory.master;
						f.RestrictArchive = false;
						f.CanChangeOnlyOnlineNomenclatures = false;
					},
					OpenPageOptions.AsSlave,
					vm =>
					{
						vm.SelectionMode = JournalSelectionMode.Single;
						vm.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
						vm.TabName = "Выезд мастера";
						vm.CalculateQuantityOnStock = true;
					})
				.ViewModel;

			journalViewModel.OnSelectResult += (s, ea) =>
			{
				var selectedNode = ea.SelectedObjects.Cast<NomenclatureJournalNode>().FirstOrDefault();

				if(selectedNode == null)
				{
					return;
				}

				TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(selectedNode.Id));
			};
		}

		protected void OnButtonAddForSaleClicked(object sender, EventArgs e)
		{
			if(!CanAddNomenclaturesToOrder())
			{
				return;
			}

			var defaultCategory = NomenclatureCategory.water;
			if(CurrentUserSettings.Settings.DefaultSaleCategory.HasValue)
			{
				defaultCategory = CurrentUserSettings.Settings.DefaultSaleCategory.Value;
			}

			var journalViewModel =
				_navigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(
					this,
					f =>
					{
						f.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
						f.SelectCategory = defaultCategory;
						f.SelectSaleCategory = SaleCategory.forSale;
						f.RestrictArchive = false;
						f.CanChangeShowArchive = false;
						f.CanChangeOnlyOnlineNomenclatures = false;
					},
					OpenPageOptions.AsSlaveIgnoreHash,
					vm =>
					{
						vm.SelectionMode = JournalSelectionMode.Single;
						vm.AdditionalJournalRestriction = new NomenclaturesForOrderJournalRestriction(ServicesConfig.CommonServices);
						vm.TabName = "Номенклатура на продажу";
						vm.CalculateQuantityOnStock = true;
					})
				.ViewModel;

			journalViewModel.SelectionMode = JournalSelectionMode.Multiple;

			journalViewModel.OnSelectResult += (s, ea) =>
			{
				var selectedNodes = ea.SelectedObjects.Cast<NomenclatureJournalNode>().ToList();
				if(selectedNodes == null || !selectedNodes.Any())
				{
					return;
				}

				foreach(var node in selectedNodes)
				{
					TryAddNomenclature(UoWGeneric.Session.Get<Nomenclature>(node.Id));
				}
			};
		}

		#region Промонаборы

		private void ActivatePromotionalSet(PromotionalSet proSet)
		{
			//Добавление спец. действий промонабора
			foreach(var action in proSet.PromotionalSetActions)
			{
				action.Activate(Entity);
			}
			//Добавление номенклатур из промонабора
			TryAddNomenclatureFromPromoSet(proSet);

			Entity.ObservablePromotionalSets.Add(proSet);
		}

		#endregion

		private void NomenclatureSelected(object sender, OrmReferenceObjectSectedEventArgs e)
		{
			TryAddNomenclature(e.Subject as Nomenclature);
		}

		private void TryAddNomenclature(
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			DiscountReason discountReason = null)
		{
			if(Entity.IsLoadedFrom1C)
			{
				return;
			}

			if(Entity.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
			   && nomenclature.Category == NomenclatureCategory.master)
			{
				MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
			   && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category))
			{
				MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}
			if(nomenclature.OnlineStore != null && !_canAddOnlineStoreNomenclaturesToOrder)
			{
				MessageDialogHelper.RunWarningDialog("У вас недостаточно прав для добавления на продажу номенклатуры интернет магазина");
				return;
			}

			Entity.AddNomenclature(UoW, _orderContractUpdater, nomenclature, count, discount, false, discountReason: discountReason);
		}

		private void TryAddNomenclatureFromPromoSet(PromotionalSet proSet)
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
						&& nomenclature.Category == NomenclatureCategory.master)
					{
						MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
						return;
					}

					if(Entity.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
						&& !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category))
					{
						MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
						return;
					}

					Entity.AddNomenclature(
						UoW,
						_orderContractUpdater,
						proSetItem.Nomenclature,
						proSetItem.Count,
						proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
						proSetItem.IsDiscountInMoney,
						true,
						null,
						proSetItem.PromoSet
					);
				}

				OnFormOrderActions();
			}
		}

		public void FillOrderItems(Order order)
		{
			if(Entity.OrderStatus != OrderStatus.NewOrder
				|| Entity.ObservableOrderItems.Any() && !MessageDialogHelper.RunQuestionDialog(
					"Вы уверены, что хотите удалить все позиции из текущего заказа и заполнить его позициями из выбранного?"))
			{
				return;
			}

			Entity.ClearOrderItemsList();
			foreach(OrderItem orderItem in order.OrderItems)
			{
				switch(orderItem.Nomenclature.Category)
				{
					case NomenclatureCategory.additional:
						Entity.AddNomenclatureForSaleFromPreviousOrder(UoW, _orderContractUpdater, orderItem);
						continue;
					case NomenclatureCategory.water:
						TryAddNomenclature(orderItem.Nomenclature, orderItem.Count);
						continue;
					default:
						//Entity.AddAnyGoodsNomenclatureForSaleFromPreviousOrder(orderItem);
						continue;
				}
			}
			Entity.RecalculateItemsPrice();
			UpdateOrderAddressTypeWithUI();
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

			var isMovedToNewOrder = _orderRepository.IsMovedToTheNewOrder(UoW, item);
			if(isMovedToNewOrder)
			{
				MessageDialogHelper.RunWarningDialog(
					$"Нельзя удалить строку заказа, т.к. данная позиция была перенесена в другой заказ.");
				return;
			}

			Entity.RemoveItem(UoW, _orderContractUpdater, item);
		}

		private void OrderEquipmentItemsView_OnDeleteEquipment(object sender, OrderEquipment e)
		{
			if(e.OrderItem != null)
			{
				RemoveOrderItem(e.OrderItem);
			}
			else
			{
				Entity.RemoveEquipment(UoW, _orderContractUpdater, e);
			}
		}

		protected void OnBtnDeleteOrderItemClicked(object sender, EventArgs e)
		{
			var selectedRows = treeItems.GetSelectedObjects<OrderItem>();

			foreach(var orderItem in selectedRows)
			{
				RemoveOrderItem(orderItem);
				Entity.TryToRemovePromotionalSet(orderItem);
				//при удалении номенклатуры выделение снимается и при последующем удалении exception
				//для исправления делаем кнопку удаления не активной, если объект не выделился в списке
				btnDeleteOrderItem.Sensitive = treeItems.GetSelectedObjects<OrderItem>().Any();
			}
		}
		#endregion

		#region Создание договоров, доп соглашений

		private CounterpartyContract GetActualInstanceContract(CounterpartyContract anotherSessionContract)
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
			try
			{
				int index = treeView.Model.IterNChildren() - 1;
				if(index == -1)
				{
					return;
				}

				TreePath path;

				treeView.Model.IterNthChild(out TreeIter iter, index);
				path = treeView.Model.GetPath(iter);

				var column = treeView.ColumnsConfig.GetColumnsByTag("Count").FirstOrDefault();
				if(column == null)
				{
					return;
				}
				var renderer = column.CellRenderers.First();
				Gtk.Application.Invoke(delegate
				{
					treeView.SetCursorOnCell(path, column, renderer, true);
				});
				treeView.GrabFocus();
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Ошибка при попытке установки состояния редактирования на ячейку");
				return;
			}

		}

		#endregion

		#region Методы событий виджетов

		private void PickerDeliveryDate_DateChanged(object sender, EventArgs e)
		{
			if(pickerDeliveryDate.Date < DateTime.Today && !_canCreateOrderInAdvance)
			{
				pickerDeliveryDate.ModifyBase(StateType.Normal, GdkColors.DangerText);
			}
			else
			{
				pickerDeliveryDate.ModifyBase(StateType.Normal, GdkColors.PrimaryBase);
			}

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}

			if(DeliveryPoint != null
				&& (Entity.OrderStatus == OrderStatus.NewOrder || Entity.OrderStatus == OrderStatus.WaitForPayment))
			{
				TryAddFlyers();
			}

			OnPickerDeliveryDateDateChanged(sender, e);

			ResetSelectedDeliverySchedule();
			SetDeliveryScheduleSelectionEditable();
		}

		protected void OnEntityVMEntryClientChanged(object sender, EventArgs e)
		{
			RefreshDebtorDebtNotifier();
			UpdateContactPhoneFilter();

			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(entityVMEntryClient.Subject));
			if(Counterparty != null)
			{
				(entryDeliveryPoint.ViewModel as DeliveryPointByClientJournalViewModel)?.FilterViewModel.SetAndRefilterAtOnce(dpf =>
				{
					dpf.Counterparty = Counterparty;
					dpf.HidenByDefault = true;
				});

				entryDeliveryPoint.Sensitive = Entity.OrderStatus == OrderStatus.NewOrder;

				if(Counterparty.PersonType == PersonType.natural)
				{
					chkContractCloser.Active = false;
					chkContractCloser.Visible = false;
					_selectPaymentTypeViewModel.AddExcludedPaymentTypes(PaymentType.Cashless);
				}
				else
				{
					chkContractCloser.Visible = true;
					_selectPaymentTypeViewModel.RemoveExcludedPaymentTypes(PaymentType.Cashless);
				}

				var promoSets = UoW.Session.QueryOver<PromotionalSet>().Where(s => !s.IsArchive).List();
				yCmbPromoSets.ItemsList = promoSets.Where(s => s.IsValidForOrder(Entity, _nomenclatureSettings));

				if(Entity.Id == 0
					&& PaymentType == PaymentType.Cashless)
				{
					Entity.UpdatePaymentType(Counterparty.PaymentMethod, _orderContractUpdater);
					OnEnumPaymentTypeChanged(null, e);
				}

				enumTax.SelectedItem = Counterparty.TaxType;
				enumTax.Visible = lblTax.Visible = IsEnumTaxVisible();

				UpdateBarterPaymentTypeVisible();
			}
			else
			{
				entryDeliveryPoint.Sensitive = false;
			}
			Entity.SetProxyForOrder();
			UpdateProxyInfo();

			SetSensitivityOfPaymentType();

			UpdateClientSecondOrderDiscount();
		}

		private void UpdateBarterPaymentTypeVisible()
		{
			if(Counterparty.CounterpartyType is CounterpartyType.AdvertisingDepartmentClient)
			{
				_selectPaymentTypeViewModel.RemoveExcludedPaymentTypes(PaymentType.Barter);
			}
			else
			{
				_selectPaymentTypeViewModel.AddExcludedPaymentTypes(PaymentType.Barter);
			}
		}

		private bool IsEnumTaxVisible() =>
			Counterparty != null
			&& (!Entity.CreateDate.HasValue
				|| Entity.CreateDate > _date)
			&& Counterparty.PersonType == PersonType.legal
			&& Counterparty.TaxType == TaxType.None;

		protected void OnSpinSumDifferenceValueChanged(object sender, EventArgs e)
		{
			string text;
			if(spinSumDifference.Value > 0)
			{
				text = "Сумма <b>переплаты</b>/недоплаты:";
			}
			else if(spinSumDifference.Value < 0)
			{
				text = "Сумма переплаты/<b>недоплаты</b>:";
			}
			else
			{
				text = "Сумма переплаты/недоплаты:";
			}

			labelSumDifference.Markup = text;
		}

		protected void OnEnumSignatureTypeChanged(object sender, EventArgs e)
		{
			UpdateProxyInfo();
		}

		protected void OnReferenceDeliveryPointChanged(object sender, EventArgs e)
		{
			UpdateContactPhoneFilter();

			CurrentObjectChanged?.Invoke(this, new CurrentObjectChangedArgs(DeliveryPoint));

			ResetSelectedDeliveryDate();

			if(DeliveryPoint != null)
			{
				UpdateProxyInfo();
				Entity.SetProxyForOrder();
			}

			if(DeliveryDate.HasValue && DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}

			if(DeliveryPoint != null && DeliveryDate.HasValue)
			{
				TryAddFlyers();
			}
			else
			{
				RemoveFlyers();
			}

			SetDeliveryDatePickerSensetive();
			SetNearestDeliveryDateLoaderFunc();

			RefreshBottlesDebtNotifier();
		}

		private void RemoveFlyers()
		{
			var activeFlyersNomenclaturesByDate = _flyerRepository.GetAllActiveFlyersNomenclaturesIdsByDate(UoW, _previousDeliveryDate);

			foreach(var flyerNomenclatureId in activeFlyersNomenclaturesByDate)
			{
				Entity.ObservableOrderEquipments.Remove(Entity.ObservableOrderEquipments.SingleOrDefault(
					x => x.Nomenclature.Id == flyerNomenclatureId));
			}
		}

		protected void OnReferenceDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			CheckSameOrders();

			if(DeliveryDate.HasValue
				&& DeliveryPoint != null
				&& Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}

			AddCommentsFromDeliveryPoint();

			SetLogisticsRequirementsCheckboxes();
		}

		private void RefreshBottlesDebtNotifier()
		{
			ylabelBottlesDebtAtDeliveryPoint.UseMarkup = true;
			if(DeliveryPoint is null)
			{
				ylabelBottlesDebtAtDeliveryPoint.Visible = false;
				return;
			}

			var bottlesAtDeliveryPoint = _bottlesRepository.GetBottlesDebtAtDeliveryPoint(UoW, DeliveryPoint.Id);
			var bottlesAvgDeliveryPoint = _deliveryPointRepository.GetAvgBottlesOrdered(UoW, DeliveryPoint, 5);

			if(bottlesAtDeliveryPoint > bottlesAvgDeliveryPoint)
			{
				ylabelBottlesDebtAtDeliveryPoint.Visible = Entity.OrderAddressType != OrderAddressType.Service;
				ylabelBottlesDebtAtDeliveryPoint.LabelProp = $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">Долг бутылей по адресу: {bottlesAtDeliveryPoint} бут.</span>";
			}
			else
			{
				ylabelBottlesDebtAtDeliveryPoint.Visible = false;
			}
		}
		private void RefreshDebtorDebtNotifier()
		{
			ylabelTotalDebt.UseMarkup = true;

			if(Counterparty != null && Counterparty.PersonType == PersonType.legal)
			{
				try
				{
					var totalDebt = _counterpartyRepository.GetTotalDebt(UoW, Counterparty.Id);
					if(totalDebt > 0)
					{
						ylabelTotalDebt.Visible = true;
						ylabelTotalDebt.LabelProp = $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">Долг по безналу: {totalDebt} руб.</span>";
					}
				}
				catch(Exception ex)
				{
					_logger.Error(ex, $"Ошибка при получении задолженности по клиенту {Counterparty.Id}");
				}
			}
			else
			{
				ylabelTotalDebt.Visible = false;
			}
		}

		private void AddCommentsFromDeliveryPoint()
		{
			if(DeliveryPoint == null)
			{
				return;
			}

			UoW.Session.Refresh(DeliveryPoint);

			AddCommentFromDeliveryPoint();

			_previousDeliveryPointId = DeliveryPoint.Id;
		}

		private void AddCommentFromDeliveryPoint()
		{
			if(DeliveryPoint.Id == _previousDeliveryPointId)
			{
				return;
			}

			const string previousCommentPrefix = "Предыдущий комментарий:";

			string trimmedCurrentComment = Entity.Comment.Trim('\n').Trim(' ');

			string trimmedNewDeliveryPointComment = DeliveryPoint.Comment.Trim('\n').Trim(' ');

			var firstPreviousCommentIndex = trimmedCurrentComment.IndexOf(previousCommentPrefix);

			if(!string.IsNullOrWhiteSpace(_lastDeliveryPointComment))
			{
				if(firstPreviousCommentIndex >= 0)
				{
					trimmedCurrentComment = trimmedCurrentComment
						.Substring(0, firstPreviousCommentIndex)
						.Replace(_lastDeliveryPointComment, "")
						.Trim('\n')
						.Trim(' ') +
						trimmedCurrentComment.Substring(firstPreviousCommentIndex);
				}
				else
				{
					trimmedCurrentComment = trimmedCurrentComment.Replace(_lastDeliveryPointComment, "");
				}
			}

			_lastDeliveryPointComment = trimmedNewDeliveryPointComment;

			if(string.IsNullOrWhiteSpace(trimmedCurrentComment))
			{
				Entity.Comment = $"{trimmedNewDeliveryPointComment}\n";
				return;
			}

			if(string.IsNullOrWhiteSpace(trimmedNewDeliveryPointComment))
			{
				Entity.Comment = $"{trimmedCurrentComment}\n";
				return;
			}

			if(trimmedCurrentComment.StartsWith(previousCommentPrefix))
			{
				Entity.Comment = $"{trimmedNewDeliveryPointComment}\n{trimmedCurrentComment}\n";
				return;
			}

			Entity.Comment = $"{trimmedNewDeliveryPointComment}\n{previousCommentPrefix}{trimmedCurrentComment}\n";
		}

		protected void OnButtonPrintSelectedClicked(object c, EventArgs args)
		{
			_logger.Info("Открываем печать документов заказа");
			try
			{
				SetSensitivity(false);
				var allList = treeDocuments.GetSelectedObjects().Cast<OrderDocument>().ToList();
				if(allList.Count <= 0)
				{
					return;
				}

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
			}
			finally
			{
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
					Startup.MainWin.NavigationManager,
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

			var isCashless = PaymentType == PaymentType.Cashless;

			if(!isCashless)
			{
				Entity.SignatureType = null;
			}

			enumSignatureType.Visible = labelSignatureType.Visible = isCashless;

			hbxOnlineOrder.Visible = UpdateVisibilityHboxOnlineOrder();
			ySpecPaymentFrom.Visible = PaymentType == PaymentType.PaidOnline;

			if(treeItems.Columns.Any())
			{
				treeItems.Columns.First(x => x.Title == "В т.ч. НДС").Visible = PaymentType == PaymentType.Cashless;
			}

			spinSumDifference.Visible = labelSumDifference.Visible = labelSumDifferenceReason.Visible =
				dataSumDifferenceReason.Visible = (PaymentType == PaymentType.Cash);
			spinSumDifference.Visible = spinSumDifference.Visible && _canEditOrderExtraCash;
			pickerBillDate.Visible = labelBillDate.Visible = PaymentType == PaymentType.Cashless;
			Entity.SetProxyForOrder();
			UpdateProxyInfo();
			UpdateUIState();
		}

		private bool UpdateVisibilityHboxOnlineOrder()
		{
			switch(PaymentType)
			{
				case PaymentType.PaidOnline:
					return true;
				case PaymentType.Terminal:
				case PaymentType.SmsQR:
				case PaymentType.DriverApplicationQR:
					return Entity.OnlinePaymentNumber != null;
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
			if(DeliveryDate.HasValue)
			{
				if(DeliveryDate.Value.Date != DateTime.Today.Date || MessageDialogHelper.RunWarningDialog("Подтвердите дату доставки", "Доставка сегодня? Вы уверены?", ButtonsType.YesNo))
				{
					CheckSameOrders();
					return;
				}
				Entity.UpdateDeliveryPoint(null, _orderContractUpdater);
			}
		}

		protected void OnEntityVMEntryClientChangedByUser(object sender, EventArgs e)
		{
			chkContractCloser.Active = false;
			CheckForStopDelivery();

			UpdateClientDefaultParam();

			if(DeliveryPoint != null)
			{
				AddCommentsFromDeliveryPoint();
			}

			SetLogisticsRequirementsCheckboxes();

			//Проверяем возможность добавления Акции "Бутыль"
			ControlsActionBottleAccessibility();
			UpdateOnlineOrderText();

			if(ycheckFastDelivery.Active
				&& (Entity.IsNeedIndividualSetOnLoad(_counterpartyEdoAccountController) || Entity.IsNeedIndividualSetOnLoadForTender))
			{
				ResetFastDeliveryForNetworkClient();
			}
		}

		private void UpdateContactPhoneFilter()
		{
			if(_phonesJournal != null)
			{
				_phonesJournal.FilterViewModel.Counterparty = Counterparty;
				_phonesJournal.FilterViewModel.DeliveryPoint = DeliveryPoint;
			}
		}

		protected void CheckForStopDelivery()
		{
			if(Entity?.Client != null && Counterparty.IsDeliveriesClosed)
			{
				string message = "Стоп отгрузки!!!" + Environment.NewLine + "Комментарий от фин.отдела: " + Counterparty?.CloseDeliveryComment;
				MessageDialogHelper.RunInfoDialog(message);
				PaymentType[] hideEnums = {
					PaymentType.Barter,
					PaymentType.ContractDocumentation,
					PaymentType.Cashless
				};
				_selectPaymentTypeViewModel.AddExcludedPaymentTypes(hideEnums);
			}
		}

		protected void OnButtonCancelOrderClicked(object sender, EventArgs e)
		{

			bool isShipped = !_orderRepository.IsSelfDeliveryOrderWithoutShipment(UoW, Entity.Id);
			bool orderHasIncome = _cashRepository.OrderHasIncome(UoW, Entity.Id);

			if(Entity.SelfDelivery && (orderHasIncome || isShipped))
			{
				MessageDialogHelper.RunErrorDialog(
					"Вы не можете отменить отгруженный или оплаченный самовывоз. " +
					"Для продолжения необходимо удалить отгрузку или приходник.");
				return;
			}

			ValidationContext validationContext = new ValidationContext(Entity, null, new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.Canceled }
			});

			if(!Validate(validationContext))
			{
				return;
			}

			OpenUndelivery();
		}

		/// <summary>
		/// Открытие окна недовоза при отмене заказа
		/// </summary>
		private void OpenUndelivery()
		{
			_undeliveryViewModel = NavigationManager.OpenViewModelOnTdi<UndeliveryViewModel>(
				this,
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.Saved += OnUndeliveryViewModelSaved;
					vm.Initialize(UoW, Entity.Id);
				}
			).ViewModel;
		}

		private void OnUndeliveryViewModelSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			Entity.SetUndeliveredStatus(UoW, _routeListService, _nomenclatureSettings, CallTaskWorker, 
				needCreateDeliveryFreeBalanceOperation: true);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, Entity);
			if(routeListItem != null)
			{
				routeListItem.StatusLastUpdate = DateTime.Now;
				routeListItem.SetOrderActualCountsToZeroOnCanceled();
				UoW.Save(routeListItem);

				var notificationRequest = new NotificationRouteListChangesRequest
				{
					OrderId = e.UndeliveredOrder.OldOrder.Id,
					PushNotificationDataEventType = PushNotificationDataEventType.RouteListContentChanged
				};

				var result = _routeListChangesNotificationSender.NotifyOfRouteListChanged(notificationRequest).GetAwaiter().GetResult();

				if(!result.IsSuccess)
				{
					ServicesConfig.InteractiveService.ShowMessage(
						ImportanceLevel.Error,
						string.Join(", ",
						result.Errors
						.Where(x => x.Code == RouteListErrors.RouteListItem.TransferTypeNotSet)
						.Select(x => x.Message))
						);
				}
			}
			else
			{
				Entity.SetActualCountsToZeroOnCanceled();
			}

			UpdateUIState();

			if(Save() && e.NeedClose)
			{
				OnCloseTab(false);
			}
		}

		protected void OnEnumPaymentTypeChangedByUser(object sender, EventArgs e)
		{
			UpdateOnlineOrderText();

			if(ycheckFastDelivery.Active
				&& (Entity.IsNeedIndividualSetOnLoad(_counterpartyEdoAccountController) || Entity.IsNeedIndividualSetOnLoadForTender))
			{
				ResetFastDeliveryForNetworkClient();
			}
		}

		private void UpdateOnlineOrderText()
		{
			if(PaymentType != PaymentType.PaidOnline
				&& PaymentType != PaymentType.DriverApplicationQR
				&& PaymentType != PaymentType.SmsQR)
			{
				entOnlineOrder.Text = string.Empty; //костыль, т.к. Entity.OnlineOrder = null не убирает почему-то текст из виджета
			}
		}

		protected void OnButtonWaitForPaymentClicked(object sender, EventArgs e)
		{
			ValidationContext validationContext = new ValidationContext(Entity, null, new Dictionary<object, object> {
				{ "NewStatus", OrderStatus.WaitForPayment }
			});

			if(!Validate(validationContext))
			{
				return;
			}

			PrepareSendBillInformation();

			if(_emailAddressForBill == null
			   && _emailService.NeedSendBillToEmail(UoW, Entity)
			   && (!Counterparty.NeedSendBillByEdo || CurrentCounterpartyEdoAccount().ConsentForEdoStatus != ConsentForEdoStatus.Agree)
			   && !MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?"))
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

			if(listDriverCallType != (DriverCallType)enumDiverCallType.SelectedItem)
			{
				var max = UoW.Session.QueryOver<Order>().Select(Projections.Max<Order>(x => x.DriverCallId)).SingleOrDefault<int>();
				Entity.DriverCallId = max != 0 ? max + 1 : 1;
			}
		}

		protected void OnYEntTareActBtlFromClientChanged(object sender, EventArgs e)
		{
			Entity.CalculateBottlesStockDiscounts(_orderSettings);
		}

		protected void OnEntryTrifleChanged(object sender, EventArgs e)
		{
			if(int.TryParse(entryTrifle.Text, out int result))
			{
				Entity.Trifle = result;
			}
		}

		protected void OnShown(object sender, EventArgs e)
		{
			//Скрывает журнал заказов при открытии заказа, чтобы все элементы умещались на экране
			if(TabParent is TdiSliderTab slider)
			{
				slider.IsHideJournal = true;
			}
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

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}
		}

		private void HboxReturnTareReasonCategoriesShow()
		{
			if(Entity.BottlesReturn.HasValue && Entity.BottlesReturn > 0)
			{
				hboxReturnTareReason.Visible = Entity.GetTotalWater19LCount() == 0;
				if(!hboxReturnTareReason.Visible)
				{
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
			if(yCmbReturnTareReasonCategories.SelectedItem is ReturnTareReasonCategory category)
			{
				if(!hboxReasons.Visible)
				{
					hboxReasons.Visible = true;
				}

				yCmbReturnTareReasons.ItemsList = category.ChildReasons;
			}
		}

		private void OnButtonCopyManagerCommentClicked(object sender, EventArgs e)
		{
			var cb = textManagerComments.GetClipboard(Selection.Clipboard);
			cb.Text = textManagerComments.Buffer.Text;
		}

		#endregion

		#region Service functions

		/// <summary>
		/// Is the payment type barter or cashless?
		/// </summary>
		private bool IsPaymentTypeBarterOrCashless() => PaymentType == PaymentType.Barter || PaymentType == PaymentType.Cashless;

		/// <summary>
		/// Is the payment type cashless?
		/// </summary>
		private bool IsPaymentTypeCashless() => PaymentType == PaymentType.Cashless;
		#endregion

		//реализация метода интерфейса ITdiTabAddedNotifier
		public void OnTabAdded()
		{
			//если новый заказ и не создан из недовоза (templateOrder заполняется только из недовоза)
			if(Entity.Id == 0 && _templateOrder == null && Counterparty == null)
			{
				//открыть окно выбора контрагента
				entityVMEntryClient.OpenSelectDialog();
			}
		}

		public virtual bool HideItemFromDirectionReasonComboInEquipment(OrderEquipment node, DirectionReason item)
		{
			switch(item)
			{
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

		private void UpdateClientSecondOrderDiscount()
		{
			Entity.UpdateClientSecondOrderDiscount(_discountsController);
		}

		private void Entity_UpdateClientCanChange(object aList, int[] aIdx)
		{
			entityVMEntryClient.IsEditable = Entity.CanChangeContractor();
		}

		private void Entity_ObservableOrderItems_ElementAdded(object aList, int[] aIdx)
		{
			FixPrice(aIdx[0]);

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				Entity.CheckAndSetOrderIsService();
				OnFormOrderActions();
			}
			_treeItemsNomenclatureColumnWidth = treeItems.ColumnsConfig.GetColumnsByTag(nameof(Nomenclature)).First().Width;
			treeItems.ExposeEvent += TreeItemsOnExposeEvent;

			UpdateClientSecondOrderDiscount();
		}

		private void TreeItemsOnExposeEvent(object o, ExposeEventArgs args)
		{
			if(_treeItemsNomenclatureColumnWidth != ((yTreeView)o).ColumnsConfig.GetColumnsByTag(nameof(Nomenclature)).First().Width)
			{
				EditGoodsCountCellOnAdd((yTreeView)o);
				treeItems.ExposeEvent -= TreeItemsOnExposeEvent;
			}
		}

		private void ObservableOrderItems_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			var items = aList as GenericObservableList<OrderItem>;
			for(var i = 0; i < items?.Count; i++)
			{
				FixPrice(i);
			}

			HboxReturnTareReasonCategoriesShow();

			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				Entity.CheckAndSetOrderIsService();
				OnFormOrderActions();
			}

			Entity.AddFastDeliveryNomenclatureIfNeeded(UoW, _orderContractUpdater);
			Entity.UpdateMasterCallNomenclatureIfNeeded(UoW, _orderContractUpdater);

			UpdateClientSecondOrderDiscount();
		}

		private void ObservableOrderDocuments_ListChanged(object aList)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ObservableOrderDocuments_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ObservableOrderDocuments_ElementAdded(object aList, int[] aIdx)
		{
			ShowOrderColumnInDocumentsList();
		}

		private void ShowOrderColumnInDocumentsList()
		{
			var column = treeDocuments.ColumnsConfig.GetColumnsByTag("OrderNumberColumn").FirstOrDefault();
			column.Visible = Entity.ObservableOrderDocuments.Any(x => x.Order.Id != x.AttachedToOrder.Id);
		}

		private void UpdateOrderItemsPrices()
		{
			for(int i = 0; i < Entity.ObservableOrderItems.Count; i++)
			{
				FixPrice(i);
			}
		}

		private void FixPrice(int id)
		{
			OrderItem item = Entity.ObservableOrderItems[id];
			if(item.Nomenclature.Category == NomenclatureCategory.deposit && item.Price != 0)
			{
				return;
			}

			item.RecalculatePrice();
		}

		private void TreeItems_Selection_Changed(object sender, EventArgs e)
		{
			object[] items = treeItems.GetSelectedObjects();

			btnDeleteOrderItem.Sensitive = items.Any();
		}

		/// <summary>
		/// Для хранения состояния, было ли изменено количество оборудования в товарах,
		/// для информирования пользователя о том, что изменения сохранятся также и в
		/// дополнительном соглашении
		/// </summary>
		private bool _orderItemEquipmentCountHasChanges;
		private bool _showBottlesDebtNotifier;
		private PhonesJournalViewModel _phonesJournal;

		/// <summary>
		/// При изменении количества оборудования в списке товаров меняет его
		/// также в доп. соглашении и списке оборудования заказа
		/// + если меняем количество у 19л бут, то меняем видимость у hboxReturnTareReason
		/// </summary>
		private void ObservableOrderItems_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<OrderItem>)
			{
				foreach(var i in aIdx)
				{
					OrderItem oItem = (aList as GenericObservableList<OrderItem>)[aIdx] as OrderItem;

					FixPrice(aIdx[0]);

					if(oItem?.CopiedFromUndelivery == null)
					{
						var curCount = oItem.Nomenclature.IsWater19L ? Order.GetTotalWater19LCount(true, true) : oItem.Count;
						oItem.IsAlternativePrice = Entity.HasPermissionsForAlternativePrice
												   && oItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount)
												   && oItem.GetWaterFixedPrice() == null;
					}

					if(oItem != null && oItem.Nomenclature.IsWater19L)
					{
						HboxReturnTareReasonCategoriesShow();
					}

					if(oItem != null && oItem.Count > 0 && DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
					{
						OnFormOrderActions();
					}

					if(oItem == null)
					{
						return;
					}

					if(oItem.Nomenclature.Category == NomenclatureCategory.equipment)
					{
						ChangeEquipmentsCount(oItem, (int)oItem.Count);
					}

					UpdateClientSecondOrderDiscount();
				}
			}
		}

		/// <summary>
		/// При изменении количества оборудования в списке оборудования меняет его
		/// также в доп. соглашении и списке товаров заказа
		/// </summary>
		private void ObservableOrderEquipments_ElementChanged_ChangeCount(object aList, int[] aIdx)
		{
			if(aList is GenericObservableList<OrderEquipment>)
			{
				foreach(var i in aIdx)
				{
					if(!((aList as GenericObservableList<OrderEquipment>)[aIdx] is OrderEquipment oEquip) || oEquip.OrderItem == null)
					{
						return;
					}
					if(oEquip.Count != oEquip.OrderItem.Count)
					{
						ChangeEquipmentsCount(oEquip.OrderItem, oEquip.Count);
					}
				}
			}
		}

		private void ObservableOrderDepositItemsOnElementRemoved(object alist, int[] aidx, object aobject)
		{
			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}
		}

		private void ObservableOrderDepositItemsOnElementAdded(object alist, int[] aidx)
		{
			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}
		}

		private void ObservableOrderEquipmentsOnElementRemoved(object alist, int[] aidx, object aobject)
		{
			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}
		}

		private void ObservableOrderEquipmentsOnElementAdded(object alist, int[] aidx)
		{
			if(DeliveryPoint != null && Entity.OrderStatus == OrderStatus.NewOrder)
			{
				OnFormOrderActions();
			}
		}

		/// <summary>
		/// Меняет количество оборудования в списке оборудования заказа, в списке
		/// товаров заказа, в списке оборудования дополнительного соглашения и
		/// меняет количество залогов за оборудование в списке товаров заказа
		/// </summary>
		private void ChangeEquipmentsCount(OrderItem orderItem, int newCount)
		{
			Entity.SetOrderItemCount(orderItem, newCount);

			OrderEquipment orderEquip = Entity.OrderEquipments.FirstOrDefault(x => x.OrderItem == orderItem);
			if(orderEquip != null)
			{
				orderEquip.Count = newCount;
			}
		}

		public bool CanFormOrderWithLiquidatedCounterparty { get; private set; }

		public bool CanEditByPermission => permissionResult.CanUpdate || (permissionResult.CanCreate && Entity.Id == 0);

		private void UpdateUIState()
		{
			bool val = Entity.CanEditByStatus && CanEditByPermission;
			buttonSelectPaymentType.Sensitive = (Counterparty != null) && val && !chkContractCloser.Active && !Entity.IsOrderCashlessAndPaid;
			if(entryDeliveryPoint.ViewModel != null)
			{
				entryDeliveryPoint.ViewModel.IsEditable = val;
			}

			SetDeliveryScheduleSelectionEditable(val);

			ybuttonFastDeliveryCheck.Sensitive = ycheckFastDelivery.Sensitive = !checkSelfDelivery.Active && val && Entity.CanChangeFastDelivery;
			lblDeliveryPoint.Sensitive = entryDeliveryPoint.Sensitive = !checkSelfDelivery.Active && val && Counterparty != null;
			buttonAddMaster.Sensitive = !checkSelfDelivery.Active && val && !Entity.IsLoadedFrom1C;
			enumSignatureType.Sensitive =
				enumDocumentType.Sensitive = val;
			buttonAddDoneService.Sensitive = buttonAddServiceClaim.Sensitive =
				buttonAddForSale.Sensitive = val;
			checkDelivered.Sensitive = checkSelfDelivery.Sensitive = val;
			dataSumDifferenceReason.Sensitive = val;
			ycheckContactlessDelivery.Sensitive = val;
			enumDiscountUnit.Visible = spinDiscount.Visible = labelDiscont.Visible = vseparatorDiscont.Visible = val;
			ChangeOrderEditable(val);

			ChangeGoodsSensitive(val
				|| (IsStatusForEditGoodsInRouteList && _canEditGoodsInRouteList));

			enumAddRentButton.Sensitive = val && !Entity.IsLoadedFrom1C;

			checkPayAfterLoad.Sensitive = _canSetPaymentAfterLoad && checkSelfDelivery.Active && val;
			buttonAddForSale.Sensitive = !Entity.IsLoadedFrom1C;
			UpdateButtonState();
			ControlsActionBottleAccessibility();
			chkContractCloser.Sensitive = _canSetContractCloser && val && !Entity.SelfDelivery;
			lblTax.Visible = enumTax.Visible = val && IsEnumTaxVisible();

			if(Entity != null)
			{
				yCmbPromoSets.Sensitive = val;
			}

			var canChangeSelfDeliveryGeoGroup = val
				|| (Entity.SelfDelivery && Entity.OrderStatus == OrderStatus.WaitForPayment && Entity.SelfDeliveryGeoGroup == null);

			ylabelGeoGroup.Sensitive = canChangeSelfDeliveryGeoGroup;
			specialListCmbSelfDeliveryGeoGroup.Sensitive = canChangeSelfDeliveryGeoGroup;
			ybuttonSaveWaitUntil.Visible = Entity.Id != 0;
		}

		private void ChangeOrderEditable(bool val)
		{
			ChangeGoodsTabSensitiveWithoutGoods(val);
			SetPadInfoSensitive(val);
			buttonAddExistingDocument.Sensitive = val;
			btnAddM2ProxyForThisOrder.Sensitive = val;
			btnRemExistingDocument.Sensitive = val;
			RouteListStatus? rlStatus = null;
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot())
			{
				if(Entity.Id != 0)
				{
					rlStatus = _orderRepository.GetAllRLForOrder(uow, Entity).FirstOrDefault()?.Status;
				}

				var sensitive = rlStatus.HasValue && CanEditByPermission
					&& !new[] { RouteListStatus.MileageCheck, RouteListStatus.OnClosing, RouteListStatus.Closed }.Contains(rlStatus.Value);
				textManagerComments.Editable = sensitive;
				enumDiverCallType.Sensitive = sensitive;
			}

			SetDeliveryDatePickerSensetive();
		}

		private void ChangeGoodsTabSensitiveWithoutGoods(bool sensitive)
		{
			hbox11.Sensitive = sensitive;
			hboxReturnTareReason.Sensitive = sensitive;
			orderEquipmentItemsView.Sensitive = sensitive;
			hbox13.Sensitive = sensitive;
			depositrefunditemsview.Sensitive = sensitive;
			table2.Sensitive = sensitive;
		}

		private void ChangeGoodsSensitive(bool sensitive)
		{
			treeItems.Sensitive = sensitive;
			hbox12.Sensitive = sensitive;
		}

		private void SetPadInfoSensitive(bool value)
		{
			foreach(var widget in table1.Children)
			{
				if(widget.Name == yhbox4.Name)
				{
					widget.Sensitive = IsWaitUntilActive;
				}
				else
				{
					widget.Sensitive = widget.Name == vboxOrderComment.Name || value;
				}
			}

			if(chkContractCloser.Active)
			{
				buttonSelectPaymentType.Sensitive = false;
			}
		}

		private void SetDeliveryDatePickerSensetive()
		{
			pickerDeliveryDate.Sensitive =
				((Order.OrderStatus == OrderStatus.WaitForPayment && !Order.SelfDelivery)
				|| (Order.OrderStatus == OrderStatus.NewOrder && Order.Id == 0)
				|| (Order.OrderStatus == OrderStatus.NewOrder && Order.Id != 0 && _canEditDeliveryDateAfterOrderConfirmation))
				&& (DeliveryPoint != null || Entity.SelfDelivery);
		}

		private void SetSensitivityOfPaymentType()
		{
			if(chkContractCloser.Active)
			{
				Entity.UpdatePaymentType(PaymentType.Cashless, _orderContractUpdater);
				UpdateUIState();
			}
			else
			{
				UpdateUIState();
			}
		}

		public void SetDlgToReadOnly()
		{
			buttonSave.Sensitive = btnCancel.Sensitive =
			hboxStatusButtons.Visible = false;
		}

		private void UpdateButtonState()
		{
			if(!CanEditByPermission || !_canEditOrder)
			{
				buttonEditOrder.Sensitive = false;
				buttonEditOrder.TooltipText = "Нет права на редактирование";
			}

			buttonSave.Sensitive = CanEditByPermission;
			menubuttonActions.Sensitive = CanEditByPermission;
			yBtnAddCurrentContract.Sensitive = CanEditByPermission;

			if(Entity.CanSetOrderAsAccepted
				&& (CanFormOrderWithLiquidatedCounterparty || Counterparty?.RevenueStatus == RevenueStatus.Active))
			{
				btnForm.Visible = true;
				buttonEditOrder.Visible = false;
			}
			else if(Entity.CanSetOrderAsEditable && !Entity.IsOldServiceOrder)
			{
				buttonEditOrder.Visible = true;
				btnForm.Visible = false;
			}
			else
			{
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

			_menuItemSelfDeliveryToLoading.Sensitive = Entity.SelfDelivery
				&& Entity.OrderStatus == OrderStatus.Accepted
				&& _allowLoadSelfDelivery;
			_menuItemSelfDeliveryPaid.Sensitive = Entity.SelfDelivery
				&& (PaymentType == PaymentType.Cashless || PaymentType == PaymentType.PaidOnline)
				&& Entity.OrderStatus == OrderStatus.WaitForPayment
				&& _acceptCashlessPaidSelfDelivery;

			_menuItemCloseOrder.Sensitive = Entity.OrderStatus == OrderStatus.Accepted && _canCloseOrders && !Entity.SelfDelivery;
			_menuItemReturnToAccepted.Sensitive = Entity.OrderStatus == OrderStatus.Closed && Entity.CanBeMovedFromClosedToAcepted;
		}

		private void UpdateProxyInfo()
		{
			bool canShow =
				Counterparty != null
				&& DeliveryDate.HasValue
				&& (Counterparty?.PersonType == PersonType.legal
					|| PaymentType == PaymentType.Cashless);

			labelProxyInfo.Visible = canShow;

			DBWorks.SQLHelper text = new DBWorks.SQLHelper("");
			if(canShow)
			{
				var proxies = Counterparty.Proxies.Where(p => p.IsActiveProxy(DeliveryDate.Value) && (p.DeliveryPoints == null || !p.DeliveryPoints.Any() || p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, DeliveryPoint))));
				foreach(var proxy in proxies)
				{
					if(!string.IsNullOrWhiteSpace(text.Text))
					{
						text.Add("\n");
					}

					text.Add(string.Format("Доверенность{2} №{0} от {1:d}", proxy.Number, proxy.IssueDate,
						proxy.DeliveryPoints == null ? "(общая)" : ""));
					text.StartNewList(": ");
					foreach(var pers in proxy.Persons)
					{
						text.AddAsList(pers.NameWithInitials);
					}
				}
			}
			if(string.IsNullOrWhiteSpace(text.Text))
			{
				labelProxyInfo.Markup = $"<span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">Нет активной доверенности</span>";
			}
			else
			{
				labelProxyInfo.LabelProp = text.Text;
			}
		}

		private void CheckSameOrders()
		{
			if(!DeliveryDate.HasValue || DeliveryPoint == null)
			{
				return;
			}

			var sameOrder = _orderRepository.GetOrderOnDateAndDeliveryPoint(UoW, DeliveryDate.Value, DeliveryPoint);
			if(sameOrder != null && _templateOrder == null)
			{
				MessageDialogHelper.RunWarningDialog("На выбранную дату и точку доставки уже есть созданный заказ!");
			}
		}

		private void SetDiscountEditable(bool? canEdit = null)
		{
			spinDiscount.Sensitive = canEdit ?? enumDiscountUnit.SelectedItem != null && _canChangeDiscountValue && !Entity.IsBottleStock;
		}

		private void SetDiscountUnitEditable() => enumDiscountUnit.Sensitive = _canChangeDiscountValue && !Entity.IsBottleStock;

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
				if(reason == null && discount > 0)
				{
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
							$" т.к. они из промонабора или на них есть фикса:\n{messages}Обратитесь к руководителю");
					}
				}
			}
		}

		private bool HaveEmailForBill()
		{
			Email clientEmail = Counterparty.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
			return clientEmail != null || MessageDialogHelper.RunQuestionDialog("Не найден адрес электронной почты для отправки счетов, продолжить сохранение заказа без отправки почты?");
		}

		private void Selection_Changed(object sender, EventArgs e)
		{
			buttonViewDocument.Sensitive = treeDocuments.Selection.CountSelectedRows() > 0;

			var selectedDoc = treeDocuments.GetSelectedObjects().Cast<OrderDocument>().FirstOrDefault();
			if(selectedDoc == null)
			{
				return;
			}

			var email = string.Empty;

			var clientEmail =
				Counterparty.Emails.FirstOrDefault(x => x.EmailType?.EmailPurpose == EmailPurpose.ForBills)
					?? Counterparty.Emails.FirstOrDefault(x => x.EmailType == null)
						?? Counterparty.Emails.FirstOrDefault();

			if(clientEmail != null)
			{
				email = clientEmail.Address;
			}

			SendDocumentByEmailViewModel.Update(selectedDoc as IEmailableDocument, email);
		}

		protected void OnCheckSelfDeliveryToggled(object sender, EventArgs e)
		{
			UpdateUIState();

			if(!checkSelfDelivery.Active)
			{
				checkPayAfterLoad.Active = false;
			}
			UpdateOrderItemsPrices();
		}

		private void ObservablePromotionalSets_ListChanged(object aList)
		{
			ShowPromoSetsColumn();
		}

		private void ObservablePromotionalSets_ElementAdded(object aList, int[] aIdx)
		{
			ShowPromoSetsColumn();
		}

		private void ObservablePromotionalSets_ElementRemoved(object aList, int[] aIdx, object aObject)
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
			if(Counterparty is null)
			{
				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Не выбран контрагент в заказе!");
				return;
			}

			if(Counterparty.PaymentMethod != PaymentType
				&& !MessageDialogHelper.RunQuestionDialog($"Вы выбрали форму оплаты &lt;{PaymentType.GetEnumTitle()}&gt;." +
				$" У клиента по умолчанию установлено &lt;{Counterparty.PaymentMethod.GetEnumTitle()}&gt;. Вы уверены, что хотите продолжить?"))
			{
				return;
			}

			if(!CheckDepositOrderCanBeFormed())
			{
				return;
			}

			_summaryInfoBuilder.Clear();

			var clientFIO = Counterparty.FullName.ToUpper();
			ylblCounterpartyFIO.Text = clientFIO;

			_summaryInfoBuilder.AppendLine($"{lblCounterpartyFIO.Text} {clientFIO}").AppendLine();

			var deliveryAddress = DeliveryPoint?.CompiledAddress.ToUpper() ?? "";
			ylblDeliveryAddress.Text = deliveryAddress;

			_summaryInfoBuilder.AppendLine($"{lblDeliveryAddress.Text} {deliveryAddress}").AppendLine();

			var phone = Entity.ContactPhone != null ? $"+7 {Entity.ContactPhone.Number}" : "";
			ylblPhoneNumber.Text = phone;

			_summaryInfoBuilder.AppendLine($"{lblPhoneNumber.Text} {phone}").AppendLine();

			var todayTommorowLable = string.Empty;
			if(DeliveryDate?.Date == DateTime.Today.Date)
			{
				todayTommorowLable = "Сегодня, ";
			}
			if(DeliveryDate?.Date == DateTime.Today.Date.AddDays(1))
			{
				todayTommorowLable = "Завтра, ";
			}

			var deliveryDate = todayTommorowLable + DeliveryDate?.ToString("dd.MM.yyyy, dddd") ?? "";
			ylblDeliveryDate.Text = deliveryDate;

			_summaryInfoBuilder.AppendLine($"{lblDeliveryDate.Text} {deliveryDate}").AppendLine();

			var deliveryTime = Entity.DeliverySchedule?.DeliveryTime;
			ylblDeliveryInterval.Text = deliveryTime;

			_summaryInfoBuilder.AppendLine($"{lblDeliveryInterval.Text} {deliveryTime}").AppendLine();

			var isPaymentTypeCashless = PaymentType == PaymentType.Cashless;
			var documentSigning = isPaymentTypeCashless
				? Entity.SignatureType?.GetEnumTitle().ToUpper() ?? ""
				: "";
			lblDocumentSigning.Visible = ylblDocumentSigning.Visible = isPaymentTypeCashless;
			ylblDocumentSigning.Text = documentSigning;

			if(lblDocumentSigning.Visible)
			{
				_summaryInfoBuilder.AppendLine($"{lblDocumentSigning.Text} {documentSigning}").AppendLine();
			}

			var hasOrderItems = Entity.OrderItems.Count > 0;
			var goods = hasOrderItems
				? string.Join("\n",
					Entity.OrderItems.Select(oi =>
						$"{oi.Nomenclature.Name.ToUpper()} - {oi.Count.ToString("F" + (oi.Nomenclature.Unit?.Digits ?? 0).ToString())}{oi.Nomenclature.Unit?.Name}"))
				: "";
			lblGoods.Visible = ylblGoods.Visible = hasOrderItems;
			ylblGoods.Text = goods;

			if(lblGoods.Visible)
			{
				_summaryInfoBuilder.AppendLine($"{lblGoods.Text} {goods}").AppendLine();
			}

			var hasOrderEquipments = Entity.OrderEquipments.Count > 0;
			var equipments = hasOrderEquipments
				? string.Join("\n",
					Entity.OrderEquipments.Select(oe =>
						$"{oe.Nomenclature.Name.ToUpper()} - {oe.Count.ToString("F" + (oe.Nomenclature.Unit?.Digits ?? 0).ToString())}{oe.Nomenclature.Unit?.Name ?? "шт"}"))
				: "";
			lblEquipment1.Visible = ylblEquipment.Visible = hasOrderEquipments;
			ylblEquipment.Text = equipments;

			if(lblEquipment1.Visible)
			{
				_summaryInfoBuilder.AppendLine($"{lblEquipment1.Text} {equipments}").AppendLine();
			}

			var hasDepositItems = Entity.OrderDepositItems.Count > 0;
			var deposits = hasDepositItems
				? string.Join("\n",
					Entity.OrderDepositItems.Select(odi =>
					{
						if(odi.EquipmentNomenclature != null)
						{
							return $"{odi.EquipmentNomenclature.Name.ToUpper()} - {odi.Count}{odi.EquipmentNomenclature.Unit.Name}";
						}
						else
						{
							return $"{odi.DepositTypeString.ToUpper()} - {odi.Count}";
						}
					}))
				: "";
			lblReturns.Visible = ylblReturns.Visible = hasDepositItems;
			ylblReturns.Text = deposits;

			if(lblReturns.Visible)
			{
				_summaryInfoBuilder.AppendLine($"{lblReturns.Text} {deposits}").AppendLine();
			}

			var bottlesToReturn = $"{Entity.BottlesReturn ?? 0} бут.";
			ylblBottlesPlannedToReturn.Text = bottlesToReturn;

			if(lblBottlesPlannedToReturn.Visible)
			{
				_summaryInfoBuilder.AppendLine($"{lblBottlesPlannedToReturn.Text} {bottlesToReturn}").AppendLine();
			}

			var isPaymentTypeCash = PaymentType == PaymentType.Cash;
			var paymentType = PaymentType.GetEnumTitle().ToUpper();
			var isIncorrectLegalClientPaymentType =
				Counterparty.PersonType == PersonType.legal
					&& PaymentType != Counterparty.PaymentMethod;

			ylblPaymentType.LabelProp = isIncorrectLegalClientPaymentType
				? $"<span foreground='{GdkColors.DangerText.ToHtmlColor()}'>{paymentType}</span>"
				: paymentType;

			_summaryInfoBuilder.AppendLine($"{lblPaymentType.Text} {paymentType}").AppendLine();

			var plannedSum = $"{Entity.OrderPositiveSum} руб.";
			ylblPlannedSum.Text = plannedSum;

			_summaryInfoBuilder.AppendLine($"{lblPlannedSum.Text} {plannedSum}").AppendLine();

			lblTrifleFrom.Visible = isPaymentTypeCash;
			ylblTrifleFrom.Visible = isPaymentTypeCash;

			var trifle = isPaymentTypeCash
				? $"{Entity.Trifle ?? 0} руб."
				: "";

			ylblTrifleFrom.Text = trifle;

			if(lblTrifleFrom.Visible)
			{
				_summaryInfoBuilder.AppendLine($"{lblTrifleFrom.Text} {trifle}").AppendLine();
			}

			var contactlessDelivery = Entity.ContactlessDelivery ? "Да".ToUpper() : "Нет".ToUpper();
			lblContactlessDelivery.Visible = lblContactlessDeliveryText.Visible = Entity.ContactlessDelivery;
			lblContactlessDeliveryText.Text = contactlessDelivery;

			if(Entity.ContactlessDelivery)
			{
				_summaryInfoBuilder.AppendLine($"{lblContactlessDelivery.Text} {contactlessDelivery}").AppendLine();
			}

			var commentForDriver = Entity.HasCommentForDriver ? Entity.Comment?.ToUpper() : "";
			ylblCommentForDriver.Text = commentForDriver;

			_summaryInfoBuilder.AppendLine($"{lblCommentForDriver.Text} {commentForDriver}").AppendLine();

			var logisticsRequirementsSummary = Entity.LogisticsRequirements?.GetSummaryString().ToUpper();
			ylblResumeLogisticsRequirementsSummary.Text = logisticsRequirementsSummary;

			_summaryInfoBuilder.Append($"{lblResumeLogisticsRequirements.Text} {logisticsRequirementsSummary}");

			ntbOrder.GetNthPage(1).Hide();
			ntbOrder.GetNthPage(1).Show();

			ntbOrder.CurrentPage = 1;
		}

		private bool CheckDepositOrderCanBeFormed()
		{
			if(Entity.OrderItems == null ||
				_currentPermissionService.ValidatePresetPermission(
					OrderPermissions.CanFormOrderWithDepositWithoutPayment))
			{
				return true;
			}

			bool hasDepositForEquipment = Entity.OrderItems.Any(oi =>
				oi.Nomenclature.Category == NomenclatureCategory.deposit &&
				oi.Nomenclature.TypeOfDepositCategory == TypeOfDepositCategory.EquipmentDeposit);

			bool isCashless = Entity.PaymentType == PaymentType.Cashless;
			bool isPaidAndOrderItemsMatch = false;

			if(Order.Id != 0)
			{
				// Нужна новая сессия, чтобы получить изначальную коллекцию товаров заказа
				using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
				{
					var dbOrderItems = _orderRepository.GetOrderItems(uow, Order.Id)
					.Select(oi => new { oi.Nomenclature.Id, oi.Count, oi.Sum })
					.ToList();

					var entityOrderItems = Entity.OrderItems
					.Select(oi => new { oi.Nomenclature.Id, oi.Count, oi.Sum })
					.ToList();

					var dbOrderSum = dbOrderItems.Sum(x => x.Sum);
					var entityOrderSum = entityOrderItems.Sum(x => x.Sum);

					isPaidAndOrderItemsMatch = Entity.OrderPaymentStatus == OrderPaymentStatus.Paid
						&& (!dbOrderItems.Except(entityOrderItems).Any()
						&& !entityOrderItems.Except(dbOrderItems).Any()
						|| entityOrderSum <= dbOrderSum);
				}
			}

			if(hasDepositForEquipment 
				&& !isPaidAndOrderItemsMatch 
				&& isCashless)
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Невозможно сформировать.\nЗаказ с залогом должен быть в статусе \"Оплачен\"",
					"Проверка корректности данных");
				return false;
			}

			return true;
		}

		private void OpenNewOrderForDailyRentEquipmentReturnIfNeeded()
		{
			if(NeedToCreateOrderForDailyRentEquipmentReturn)
			{
				_slaveUnitOfWork = CreateNewOrderForDailyRentEquipmentReturn(Entity);
			}

			if(_slaveOrderDlg != null)
			{
				TabParent.SwitchOnTab(_slaveOrderDlg);
			}
			else if(_slaveUnitOfWork != null)
			{
				_slaveOrderDlg = new OrderDlg(_slaveUnitOfWork);

				TabParent.AddSlaveTab(this, _slaveOrderDlg);

				_slaveOrderDlg.TabClosed += OnSlaveOrderClosed;
			}
		}

		private bool NeedToCreateOrderForDailyRentEquipmentReturn =>
			_justCreated &&
			Entity.ObservableOrderItems.Any(orderItem =>
				orderItem.Nomenclature.Id == _nomenclatureSettings.DailyCoolerRentNomenclatureId);

		private void OnSlaveOrderClosed(object sender, EventArgs e)
		{
			_slaveOrderDlg.TabClosed -= OnSlaveOrderClosed;
			SaveAndClose();
			_justCreated = false;
		}

		private IUnitOfWorkGeneric<Order> CreateNewOrderForDailyRentEquipmentReturn(Order sourceOrder)
		{
			var result = ServicesConfig.UnitOfWorkFactory.CreateWithNewRoot<Order>();

			result.Root.UpdateClient(sourceOrder.Client, _orderContractUpdater, out var updateClientMessage);
			result.Root.Author = sourceOrder.Author;
			result.Root.UpdateDeliveryPoint(sourceOrder.DeliveryPoint, _orderContractUpdater);
			result.Root.UpdatePaymentType(sourceOrder.PaymentType, _orderContractUpdater);
			result.Root.ContactPhone = sourceOrder.ContactPhone;
			result.Root.SignatureType = sourceOrder.SignatureType;

			var equipmentItems = sourceOrder.OrderEquipments
				.Where(oe => oe.OwnType == OwnTypes.Rent
					&& oe.Reason == Reason.Rent
					&& oe.Direction == Domain.Orders.Direction.Deliver
					&& oe.OrderRentDepositItem?.RentType == OrderRentType.DailyRent)
				.Select(oe => (oe.Nomenclature, oe.Count)).ToList();

			foreach(var equipmentItem in equipmentItems)
			{
				result.Root.AddEquipmentNomenclatureFromClient(
					equipmentItem.Nomenclature,
					result,
					equipmentItem.Count,
					Domain.Orders.Direction.PickUp,
					DirectionReason.Rent,
					OwnTypes.Rent,
					Reason.Rent);
			}

			result.Root.UpdateDocuments();

			return result;
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
			if(Entity.OrderAddressType == OrderAddressType.Service)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Error,
					"Нельзя добавлять аренду в сервисный заказ",
					"Ошибка"
				);
				return;
			}
			switch(rentType)
			{
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

				if(selectedRent == null)
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
			if(ServicesConfig.InteractiveService.Question("Подобрать оборудование автоматически по типу?"))
			{
				var existingItems = Entity.OrderEquipments
					.Where(x => x.OrderRentDepositItem != null || x.OrderRentServiceItem != null)
					.Select(x => x.Nomenclature.Id)
					.Distinct()
					.ToArray();

				var anyNomenclature = _nomenclatureRepository.GetAvailableNonSerialEquipmentForRent(UoW, paidRentPackage.EquipmentKind, existingItems);
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

			switch(rentType)
			{
				case RentType.NonfreeRent:
					Entity.AddNonFreeRent(UoW, _orderContractUpdater, paidRentPackage, equipmentNomenclature);
					break;
				case RentType.DailyRent:
					Entity.AddDailyRent(UoW, _orderContractUpdater, paidRentPackage, equipmentNomenclature);
					break;
			}
		}

		#endregion PaidRent

		#region FreeRent

		private void SelectFreeRentPackage()
		{
			var freeRentJournal = _rentPackagesJournalsViewModelsFactory.CreateFreeRentPackagesJournalViewModel(false, false, false, false);

			freeRentJournal.JournalFilter.SetAndRefilterAtOnce<FreeRentPackagesFilterViewModel>(filter =>
			{
				filter.RestrictArchieved = false;
			});

			freeRentJournal.OnSelectResult += (sender, e) =>
			{
				var selectedRent = e.GetSelectedObjects<FreeRentPackagesJournalNode>().FirstOrDefault();

				if(selectedRent == null)
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

				var anyNomenclature = _nomenclatureRepository.GetAvailableNonSerialEquipmentForRent(UoW, freeRentPackage.EquipmentKind, existingItems);
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

			Entity.AddFreeRent(UoW, _orderContractUpdater, freeRentPackage, equipmentNomenclature);
		}

		protected void OnYbuttonToStorageLogicAddressTypeClicked(object sender, EventArgs e)
		{
			if((Entity.OrderAddressType == OrderAddressType.Delivery
				|| Entity.OrderAddressType == OrderAddressType.Service)
			   && !Counterparty.IsChainStore
			   && !Entity.OrderItems.Any(x => x.IsMasterNomenclature && x.Nomenclature.Id != _nomenclatureSettings.MasterCallNomenclatureId))
			{
				Entity.OrderAddressType = OrderAddressType.StorageLogistics;
				Entity.UpdateDeliveryPoint(null, _orderContractUpdater);
				Entity.DeliverySchedule = null;
			}
		}

		protected void OnYbuttonToDeliveryAddressTypeClicked(object sender, EventArgs e)
		{
			if((Entity.OrderAddressType == OrderAddressType.StorageLogistics
				|| Entity.OrderAddressType == OrderAddressType.Service)
			   && !Counterparty.IsChainStore
			   && !Entity.OrderItems.Any(x => x.IsMasterNomenclature && x.Nomenclature.Id != _nomenclatureSettings.MasterCallNomenclatureId))
			{
				Entity.OrderAddressType = OrderAddressType.Delivery;
				Entity.UpdateDeliveryDate(null, _orderContractUpdater, out var updateDeliveryDateMessage);
				Entity.DeliverySchedule = null;

				if(!string.IsNullOrWhiteSpace(updateDeliveryDateMessage))
				{
					MessageDialogHelper.RunWarningDialog(updateDeliveryDateMessage);
				}
			}
		}

		protected void OnYbuttonToServiceTypeClicked(object sender, EventArgs e)
		{
			Entity.OrderAddressType = OrderAddressType.Service;
			Entity.UpdateDeliveryDate(null, _orderContractUpdater, out var updateDeliveryDateMessage);
			Entity.DeliverySchedule = null;

			if(!string.IsNullOrWhiteSpace(updateDeliveryDateMessage))
			{
				MessageDialogHelper.RunWarningDialog(updateDeliveryDateMessage);
			}
		}

		private void UpdateOrderAddressTypeWithUI()
		{
			Entity.UpdateAddressType();
			UpdateOrderAddressTypeUI();
		}

		private void UpdateOrderAddressTypeUI()
		{
			if(Entity.SelfDelivery)
			{
				ylabelOrderAddressType.Visible = false;
				ybuttonToDeliveryAddressType.Visible = false;
				ybuttonToStorageLogicAddressType.Visible = false;
				ybuttonToServiceType.Visible = false;
				return;
			}

			switch(Entity.OrderAddressType)
			{
				case OrderAddressType.Delivery:
					ybuttonToDeliveryAddressType.Visible = false;
					ybuttonToStorageLogicAddressType.Visible = true;
					ybuttonToServiceType.Visible = true;

					entryBottlesToReturn.Visible = true;
					label7.Visible = true;
					lblBottlesPlannedToReturn.Visible = true;
					ylblBottlesPlannedToReturn.Visible = true;
					ylabelBottlesDebtAtDeliveryPoint.Visible = true;
					break;
				case OrderAddressType.StorageLogistics:
					ybuttonToDeliveryAddressType.Visible = true;
					ybuttonToStorageLogicAddressType.Visible = false;
					ybuttonToServiceType.Visible = true;

					entryBottlesToReturn.Visible = true;
					label7.Visible = true;
					lblBottlesPlannedToReturn.Visible = true;
					ylblBottlesPlannedToReturn.Visible = true;
					ylabelBottlesDebtAtDeliveryPoint.Visible = true;
					break;
				case OrderAddressType.ChainStore:
					ybuttonToDeliveryAddressType.Visible = false;
					ybuttonToStorageLogicAddressType.Visible = false;
					ybuttonToServiceType.Visible = false;

					entryBottlesToReturn.Visible = true;
					label7.Visible = true;
					lblBottlesPlannedToReturn.Visible = true;
					ylblBottlesPlannedToReturn.Visible = true;
					ylabelBottlesDebtAtDeliveryPoint.Visible = true;
					break;
				case OrderAddressType.Service:
					ybuttonToDeliveryAddressType.Visible = true;
					ybuttonToStorageLogicAddressType.Visible = true;
					ybuttonToServiceType.Visible = false;

					entryBottlesToReturn.Visible = false;
					label7.Visible = false;
					lblBottlesPlannedToReturn.Visible = false;
					ylblBottlesPlannedToReturn.Visible = false;
					ylabelBottlesDebtAtDeliveryPoint.Visible = false;
					break;
			}
			ylabelOrderAddressType.Visible = true;
		}

		#endregion FreeRent

		private EntitySelectionViewModel<DeliverySchedule> CreateEntityselectionDeliveryScheduleViewModel()
		{
			var builder = ScopeProvider.Scope.Resolve<LegacyEntitySelectionViewModelBuilder<DeliverySchedule>>();

			var viewModel = builder
				.SetDialogTab(() => this)
				.SetUnitOfWork(UoW)
				.ForProperty(Entity, e => e.DeliverySchedule)
				.UseViewModelJournalSelector<DeliveryScheduleJournalViewModel, DeliveryScheduleFilterViewModel>(filter => filter.RestrictIsNotArchive = true)
				.UseSelectionDialogAndAutocompleteSelector(
					() =>
					{
						var isForMasterCall = Entity.OrderAddressType == OrderAddressType.Service;
						return Entity.GetAvailableDeliveryScheduleIds(isForMasterCall);
					},
					(searchText) => DeliverySchedule.GetNameCompareExpression(searchText),
					(entity) => entity.OrderBy(e => e.Name.Length).ThenBy(e => e.Name),
					() => GetSelectionDialogSettings())
				.Finish();

			return viewModel;
		}

		private SelectionDialogSettings GetSelectionDialogSettings()
		{
			string deliveryDate = string.Empty;

			if(DeliveryDate.HasValue)
			{
				deliveryDate = $"<b>На {GeneralUtils.GetDayNameByDate(DeliveryDate.Value)}</b> {DeliveryDate.Value: dd.MM.yyyy}";
			}

			var selectionDialogSettings = new SelectionDialogSettings
			{
				Title = "Время доставки",
				TopLabelText = deliveryDate,
				NoEntitiesMessage = "На данный день\nинтервалы\nдоставки\nотсутствуют",
				SelectFromJournalButtonLabelText = "Выбрать интервал вручную",
				IsCanOpenJournal = true
			};

			return selectionDialogSettings;
		}

		private void SetDeliveryScheduleSelectionEditable(bool isCanEditOrder = true)
		{
			var isEditable =
				isCanEditOrder
				&& !checkSelfDelivery.Active
				&& DeliveryDate.HasValue
				&& DeliveryPoint != null;

			if(entityselectionDeliverySchedule.ViewModel != null)
			{
				entityselectionDeliverySchedule.ViewModel.IsEditable = isEditable;
			}

			labelDeliverySchedule.Sensitive = isEditable;
		}

		private void ResetSelectedDeliveryDate()
		{
			Entity.UpdateDeliveryDate(null, _orderContractUpdater, out var message);

			if(!string.IsNullOrWhiteSpace(message))
			{
				MessageDialogHelper.RunWarningDialog(message);
			}
		}

		private void ResetSelectedDeliverySchedule()
		{
			Entity.DeliverySchedule = null;
		}

		private void SetNearestDeliveryDateLoaderFunc()
		{
			pickerDeliveryDate.ButtonsDatesLoaderFunc =
				() =>
				{
					if(Entity.SelfDelivery)
					{
						return new List<DateTime> { DateTime.Today, DateTime.Today.AddDays(1) };
					}

					if(Entity.OrderAddressType == OrderAddressType.Service)
					{
						if(DeliveryPoint?.Latitude is null || DeliveryPoint?.Longitude is null)
						{
							return null;
						}

						var serviceDistrict = _deliveryRepository.GetServiceDistrictByCoordinates(UoW, DeliveryPoint.Latitude.Value, DeliveryPoint.Longitude.Value);

						return serviceDistrict?.GetNearestDatesWhenDeliveryIsPossible();
					}

					return DeliveryPoint?.District?.GetNearestDatesWhenDeliveryIsPossible();
				};
		}

		private Nomenclature TryGetSelectedNomenclature(JournalSelectedEventArgs e)
		{
			var selectedNode = e.GetSelectedObjects<NomenclatureForRentNode>().FirstOrDefault();

			return selectedNode == null ? null : UoW.GetById<Nomenclature>(selectedNode.Id);
		}

		#endregion

		protected void OnBtnCopyEntityIdClicked(object sender, EventArgs e)
		{
			if(Entity.Id > 0)
			{
				GetClipboard(Selection.Clipboard).Text = Entity.Id.ToString();
			}
		}

		private void OnBtnCopySummaryInfoClicked(object sender, EventArgs e)
		{
			GetClipboard(Selection.Clipboard).Text = _summaryInfoBuilder.ToString();
		}

		#region CustomCancellationConfirmationDialog
		public override bool HasCustomCancellationConfirmationDialog => UoW.IsNew;
		public override Func<int> CustomCancellationConfirmationDialogFunc => ShowOrderCancellationAdditionalConfirmationDialog;

		private int ShowOrderCancellationAdditionalConfirmationDialog()
		{
			var dlg = new OrderCancellationConfirmationDlg();
			dlg.SetPosition(WindowPosition.CenterAlways);
			dlg.Title = "Подтверждение отмены заказа";
			var result = dlg.Run();
			dlg.Destroy();

			return result;
		}
		#endregion CustomCancellationConfirmationDialog
	}
}
