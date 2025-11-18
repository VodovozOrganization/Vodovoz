using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using NHibernate.Criterion;
using QS.Dialog;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using QS.Tdi;
using QS.Utilities.Extensions;
using QS.ViewModels.Control.EEVM;
using QSProjectsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using Vodovoz.Application.Orders;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Filters.ViewModels;
using Vodovoz.Infrastructure;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.JournalViewModels;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.Orders;
using Vodovoz.ViewModels.TempAdapters;
using VodovozBusiness.Services.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz
{
	public partial class OrderReturnsView : QS.Dialog.Gtk.TdiTabBase, ITDICloseControlTab, ISingleUoWDialog, INotifyPropertyChanged
	{
		#region Поля и свойства

		private readonly ILifetimeScope _lifetimeScope;
		private readonly IOrderContractUpdater _contractUpdater;
		private readonly IRouteListService _routeListService;
		private readonly ICounterpartyService _counterpartyService;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly IDeliveryRulesSettings _deliveryRulesSettings;
		private readonly IFlyerRepository _flyerRepository;
		private readonly ITdiCompatibilityNavigation _tdiNavigationManager;
		private readonly IOrderSettings _orderSettings;
		private readonly IOrderRepository _orderRepository;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IWageParameterService _wageParameterService;
		private readonly IOrderDiscountsController _discountsController;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly INomenclatureFixedPriceController _nomenclatureFixedPriceController;

		private List<OrderItemReturnsNode> _itemsToClient;

		public event PropertyChangedEventHandler PropertyChanged;

		private Employee _currentEmployee;

		private Counterparty _lastCounterparty = null;

		private RouteListItem _routeListItem;
		private bool _canEditPrices;
		private bool _canEditOrderAfterRecieptCreated;
		private RouteListItemStatus _routeListItemStatusToChange;
		private UndeliveryViewModel _undeliveryViewModel;
		private int? _oldDeliveryPointId;
		private int? _oldCounterpartyId;

		public IUnitOfWork UoW { get; }
		
		private Order BaseOrder { get; set; }
		
		public Counterparty Client
		{
			get => BaseOrder.Client;
			private set
			{
				BaseOrder.UpdateClient(value, _contractUpdater, out var message);

				if(!string.IsNullOrWhiteSpace(message))
				{
					_interactiveService.ShowMessage(ImportanceLevel.Warning, message);
				}
			}
		}

		public DeliveryPoint DeliveryPoint
		{
			get => BaseOrder.DeliveryPoint;
			private set => BaseOrder.UpdateDeliveryPoint(value, _contractUpdater);
		}
		
		public PaymentType PaymentType
		{
			get => BaseOrder.PaymentType;
			private set => BaseOrder.UpdatePaymentType(value, _contractUpdater);
		}
		
		public PaymentFrom PaymentByCardFrom
		{
			get => BaseOrder.PaymentByCardFrom;
			private set => BaseOrder.UpdatePaymentByCardFrom(value, _contractUpdater);
		}
		
		public ChangedType CompletedChange
		{
			get
			{
				if(!_oldCounterpartyId.HasValue || !_oldDeliveryPointId.HasValue)
				{
					return ChangedType.None;
				}

				if(Client.Id == _oldCounterpartyId && DeliveryPoint.Id != _oldDeliveryPointId)
				{
					return ChangedType.DeliveryPoint;
				}

				if(Client.Id != _oldCounterpartyId)
				{
					return ChangedType.Both;
				}

				return ChangedType.None;
			}
		}

		#endregion

		public OrderReturnsView(
			IUnitOfWork unitOfWork,
			IOrderDiscountsController orderDiscountsController,
			ICallTaskWorker callTaskWorker,
			ICounterpartyService counterpartyService,
			ICurrentPermissionService currentPermissionService,
			IInteractiveService interactiveService,
			IEmployeeService employeeService,
			IUserRepository userRepository,
			IOrderRepository orderRepository,
			IDiscountReasonRepository discountReasonRepository,
			IWageParameterService wageParameterService,
			IOrderSettings orderSettings,
			INomenclatureOnlineSettings nomenclatureOnlineSettings,
			IDeliveryRulesSettings deliveryRulesSettings,
			IFlyerRepository flyerRepository,
			ITdiCompatibilityNavigation tdiNavigationManager,
			ILifetimeScope lifetimeScope,
			IOrderContractUpdater orderContractUpdater,
			IRouteListService routeListService
			)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			CanFormOrderWithLiquidatedCounterparty = currentPermissionService
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanFormOrderWithLiquidatedCounterparty);

			_canEditPrices = currentPermissionService
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanEditPriceDiscountFromRouteListAndSelfDelivery);

			_canEditOrderAfterRecieptCreated = currentPermissionService
				.ValidatePresetPermission(Vodovoz.Core.Domain.Permissions.OrderPermissions.CanChangeOrderAfterRecieptCreated);

			Build();

			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_discountsController = orderDiscountsController ?? throw new ArgumentNullException(nameof(orderDiscountsController));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_counterpartyService = counterpartyService ?? throw new ArgumentNullException(nameof(counterpartyService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_deliveryRulesSettings = deliveryRulesSettings ?? throw new ArgumentNullException(nameof(deliveryRulesSettings));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_tdiNavigationManager = tdiNavigationManager ?? throw new ArgumentNullException(nameof(tdiNavigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			_contractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));;
		}

		public bool CanFormOrderWithLiquidatedCounterparty { get; }

		public bool IgnoreReceipt { get; private set; } = false;

		public int? OrderId => _routeListItem?.Order?.Id;
		private bool IsClientSelectedAndOrderCashlessAndPaid =>
			Client != null && _routeListItem?.Order?.IsOrderCashlessAndPaid == true;

		public void ConfigureForRouteListAddress(RouteListItem routeListItem)
		{
			_routeListItem = routeListItem ?? throw new ArgumentNullException(nameof(routeListItem));
			TabName = "Изменение заказа №" + routeListItem.Order.Id;

			depositrefunditemsview1.Configure(UoW, _routeListItem.Order, true);

			UpdateListsSentivity();
			entryTotal.Sensitive = yenumcomboOrderPayment.Sensitive = routeListItem.Status != RouteListItemStatus.Transfered;

			orderEquipmentItemsView.OnDeleteEquipment += OrderEquipmentItemsView_OnDeleteEquipment;
			Configure();
			UpdateItemsList();
			UpdateButtonsState();
		}

		private void UpdateListsSentivity()
		{
			ytreeToClient.Sensitive =
				orderEquipmentItemsView.Sensitive =
					depositrefunditemsview1.Sensitive = _routeListItem.IsDelivered();
		}

		private void UpdateItemsList()
		{
			_itemsToClient = new List<OrderItemReturnsNode>();
			var nomenclatures = _routeListItem.Order.OrderItems
				.Where(item => !item.Nomenclature.IsSerial).ToList();
			foreach(var item in nomenclatures)
			{
				_itemsToClient.Add(new OrderItemReturnsNode(item));
				item.PropertyChanged += OnOrderChanged;
			}

			entryTotal.Text = CurrencyWorks.GetShortCurrencyString(_routeListItem.Order.ActualGoodsTotalSum ?? 0);

			ytreeToClient.ItemsDataSource = _itemsToClient;
		}

		private void OpenSelectNomenclatureDlg()
		{
			var journalViewModel = _tdiNavigationManager.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(this, filter =>
			{
				filter.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
				filter.SelectCategory = NomenclatureCategory.deposit;
				filter.SelectSaleCategory = SaleCategory.forSale;
				filter.RestrictArchive = false;
				filter.CanChangeOnlyOnlineNomenclatures = false;
			},
			OpenPageOptions.AsSlave,
			viewModel =>
			{
				viewModel.SelectionMode = JournalSelectionMode.Single;
				viewModel.TabName = "Номенклатура на продажу";
				viewModel.CalculateQuantityOnStock = true;
				viewModel.OnSelectResult += OnNomenclatureSelected;
			});
		}

		private void OnNomenclatureSelected(object sender, JournalSelectedEventArgs e)
		{
			var selectedNodes = e.SelectedObjects.Cast<NomenclatureJournalNode>();

			if(!selectedNodes.Any())
			{
				return;
			}

			Nomenclature nomenclature = UoW.Session.Get<Nomenclature>(selectedNodes.First().Id);
			CounterpartyContract contract = _routeListItem.Order.Contract;

			if(_routeListItem.Order.IsLoadedFrom1C
				|| nomenclature is null
				|| contract is null)
			{
				return;
			}

			if(_routeListItem.Order.OrderItems.Any(x => !Nomenclature.GetCategoriesForMaster().Contains(x.Nomenclature.Category))
			   && nomenclature.Category == NomenclatureCategory.master)
			{
				MessageDialogHelper.RunInfoDialog("В не сервисный заказ нельзя добавить сервисную услугу");
				return;
			}

			if(_routeListItem.Order.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master)
			   && !Nomenclature.GetCategoriesForMaster().Contains(nomenclature.Category))
			{
				MessageDialogHelper.RunInfoDialog("В сервисный заказ нельзя добавить не сервисную услугу");
				return;
			}

			switch(nomenclature.Category)
			{
				case NomenclatureCategory.water:
					_routeListItem.Order.AddWaterForSale(UoW, _contractUpdater, nomenclature, 0, 0);
					break;
				case NomenclatureCategory.master:
					_routeListItem.Order.AddMasterNomenclature(UoW, _contractUpdater, nomenclature, 0);
					break;
				default:
					_routeListItem.Order.AddAnyGoodsNomenclatureForSale(UoW, _contractUpdater, nomenclature, true);
					break;
			}

			UpdateItemsList();
		}

		public void OnOrderChanged(object sender, PropertyChangedEventArgs args)
		{
			entryTotal.Text = CurrencyWorks.GetShortCurrencyString(_routeListItem.Order.ActualGoodsTotalSum ?? 0);
		}

		protected void Configure()
		{
			if(_currentEmployee == null)
			{
				_currentEmployee = _employeeService.GetEmployeeForUser(UoW, _userRepository.GetCurrentUser(UoW).Id);
			}

			Initialize(_routeListItem.Order);

			clientEntry.ViewModel = GetClientEntityEntryViewModel();

			orderEquipmentItemsView.Configure(UoW, _routeListItem.Order, _flyerRepository);
			ConfigureDeliveryPointRefference(Client);

			var discountReasons = _discountReasonRepository.GetActiveDiscountReasons(UoW);

			ytreeToClient.ColumnsConfig = ColumnsConfigFactory.Create<OrderItemReturnsNode>()
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Кол-во")
					.AddNumericRenderer(node => node.Count)
						.AddSetter((c, node) => c.Digits = node.Nomenclature.Unit == null ? 0 : (uint)node.Nomenclature.Unit.Digits)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Кол-во по факту")
					.AddToggleRenderer(node => node.IsDelivered, false)
						.AddSetter((cell, node) => cell.Visible = node.IsSerialEquipment)
					.AddNumericRenderer(node => node.ActualCount, false)
						.AddSetter((cell, node) => cell.Editable = node.Nomenclature.Category != NomenclatureCategory.deposit)
						.Adjustment(new Adjustment(0, 0, 9999, 1, 1, 0))
						.AddSetter((cell, node) => cell.Editable = !node.IsEquipment)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Цена")
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
						.Adjustment(new Adjustment(0, 0, 99999, 1, 100, 0))
						.AddSetter((cell, node) => cell.Editable = node.HasPrice && _canEditPrices)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName, false)
				.AddColumn("Альтерн.\nцена")
					.AddToggleRenderer(x => x.OrderItem.IsAlternativePrice).Editing(false)
				.AddColumn("Скидка")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(node => node.ManualChangingDiscount)
						.AddSetter((cell, node) => cell.Editable = _canEditPrices)
						.AddSetter(
							(c, n) => c.Adjustment = n.IsDiscountInMoney
								? new Adjustment(0, 0, (double)(n.Price * n.ActualCount), 1, 100, 1)
								: new Adjustment(0, 0, 100, 1, 100, 1)
						)
						.Digits(2)
						.WidthChars(10)
					.AddTextRenderer(n => n.IsDiscountInMoney ? CurrencyWorks.CurrencyShortName : "%", false)
				.AddColumn("Скидка \nв рублях?")
					.AddToggleRenderer(x => x.IsDiscountInMoney)
						.AddSetter((c, n) => c.Activatable = _canEditPrices)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddComboRenderer(node => node.DiscountReason)
						.SetDisplayFunc(x => x.Name)
						.DynamicFillListFunc(item =>
						{
							var list = discountReasons.Where(
								dr => _discountsController.IsApplicableDiscount(dr, item.Nomenclature)).ToList();
							return list;
						})
						.EditedEvent(OnDiscountReasonComboEdited)
						.AddSetter((c, n) => c.Editable = _canEditPrices)
						.AddSetter(
							(c, n) =>
								c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null && n.OrderItem?.PromoSet == null
									? GdkColors.DangerBase
									: GdkColors.PrimaryBase
						)
						.AddSetter((c, n) =>
							{
								if(n.OrderItem?.PromoSet != null && n.DiscountReason == null && n.Discount > 0)
								{
									c.Text = n.OrderItem.PromoSet.DiscountReasonInfo;
								}
							})
				.AddColumn("Стоимость")
					.AddNumericRenderer(node => node.Sum).Digits(2)
					.AddTextRenderer(node => CurrencyWorks.CurrencyShortName)
				.AddColumn("Промонаборы")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(node => node.PromoSetName)
				.AddColumn("")
				.Finish();

			yenumcomboOrderPayment.ItemsEnum = typeof(PaymentType);
			yenumcomboOrderPayment.Binding
				.AddBinding(this, o => o.PaymentType, w => w.SelectedItem)
				.InitializeFromSource();

			ySpecPaymentFrom.ItemsList =
				PaymentType == PaymentType.PaidOnline
					? GetActivePaymentFromWithSelected(PaymentByCardFrom)
					: UoW.Session.QueryOver<PaymentFrom>().Where(p => !p.IsArchive).List();

			ySpecPaymentFrom.Binding
				.AddFuncBinding(this, e => e.PaymentType == PaymentType.PaidOnline, w => w.Visible)
				.AddBinding(this, e => e.PaymentByCardFrom, w => w.SelectedItem)
				.InitializeFromSource();

			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(_routeListItem.Order)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItemOrNull)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();

			entryOnlineOrder.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(_routeListItem.Order, e => e.OnlinePaymentNumber, w => w.Text, new NullableIntToStringConverter())
				.InitializeFromSource();

			_routeListItem.Order.ObservableOrderItems.ListContentChanged += (sender, e) => { UpdateItemsList(); };

			_routeListItem.Order.ObservableOrderItems.ElementAdded += (aList, aIdx) => ActualCountsOfOrderItemsFromNullToZero();
			_routeListItem.Order.ObservableOrderEquipments.ElementAdded += (aList, aIdx) => ActualCountsOfOrderEqupmentFromNullToZero();
			_routeListItem.Order.ObservableOrderDepositItems.ElementAdded += (aList, aIdx) => ActualCountsOfOrderDepositsFromNullToZero();

			yspinbuttonBottlesByStockCount.Binding.AddBinding(_routeListItem.Order, e => e.BottlesByStockCount, w => w.ValueAsInt)
				.InitializeFromSource();
			yspinbuttonBottlesByStockActualCount.Binding
				.AddBinding(_routeListItem.Order, e => e.BottlesByStockActualCount, w => w.ValueAsInt).InitializeFromSource();
			yspinbuttonBottlesByStockActualCount.ValueChanged += OnYspinbuttonBottlesByStockActualCountChanged;
			hboxBottlesByStock.Visible = _routeListItem.Order.IsBottleStock;

			OnlineOrderVisible();
			OnClientEntryViewModelChanged(null, null);
		}

		private void Initialize(Order order)
		{
			if(BaseOrder != null)
			{
				UnsubscribeBaseOrder();
			}
			
			BaseOrder = order;
			BaseOrder.PropertyChanged += OnOrderPropertyChanged;

			_oldDeliveryPointId = order.DeliveryPoint?.Id;
			_oldCounterpartyId = order.Client?.Id;
		}

		private void OnOrderPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(Order.DeliveryPoint))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeliveryPoint)));
			}
			
			if(e.PropertyName == nameof(Order.Client))
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Client)));
			}
		}

		private IEntityEntryViewModel GetClientEntityEntryViewModel()
		{
			var builder = new LegacyEEVMBuilderFactory<OrderReturnsView>(this, this, UoW, _tdiNavigationManager, _lifetimeScope);

			var viewModel = builder.ForProperty(x => x.Client)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();

			viewModel.Changed += OnClientEntryViewModelChanged;
			viewModel.ChangedByUser += OnClientEntryViewModelChangedByUser;
			viewModel.BeforeChangeByUser += OnClientBeforeChangeByUser;

			return viewModel;
		}

		private IEnumerable<PaymentFrom> GetActivePaymentFromWithSelected(IDomainObject selectedPaymentFrom)
		{
			var selectedPaymentFromId = selectedPaymentFrom?.Id ?? 0; 
			PaymentFrom paymentFromAlias = null;
			
			return UoW.Session.QueryOver(() => paymentFromAlias)
				.Where(Restrictions.Or(
					Restrictions.WhereNot(() => paymentFromAlias.IsArchive),
					Restrictions.IdEq(selectedPaymentFromId)))
				.List();
		}

		private void OnDiscountReasonComboEdited(object o, EditedArgs args)
		{
			var index = int.Parse(args.Path);
			var node = ytreeToClient.YTreeModel.NodeAtPath(new TreePath(args.Path));

			if(!(node is OrderItemReturnsNode orderItemNode))
			{
				return;
			}

			var previousDiscountReason = orderItemNode.OrderItem.DiscountReason;

			Gtk.Application.Invoke((sender, eventArgs) =>
			{
				//Дополнительно проверяем основание скидки на null, т.к при двойном щелчке
				//комбо-бокс не откроется, но событие сработает и прилетит null
				if(orderItemNode.OrderItem != null && orderItemNode.DiscountReason != null)
				{
					if(!_discountsController.SetDiscountFromDiscountReasonForOrderItem(
						orderItemNode.DiscountReason, orderItemNode.OrderItem, _canEditPrices, out string message))
					{
						orderItemNode.OrderItem.DiscountReason = previousDiscountReason;
					}

					if(message != null)
					{
						_interactiveService.ShowMessage(ImportanceLevel.Warning,
							$"На позицию:\n№{index + 1} {message}нельзя применить скидку," +
							" т.к. она из промонабора или на нее есть фикса.\nОбратитесь к руководителю");
					}
				}
			});
		}

		public void FixActualCounts()
		{
			ActualCountsOfOrderItemsFromNullToZero();
			ActualCountsOfOrderEqupmentFromNullToZero();
			ActualCountsOfOrderDepositsFromNullToZero();
		}

		void ActualCountsOfOrderDepositsFromNullToZero()
		{
			foreach(var dep in _routeListItem.Order.OrderDepositItems)
			{
				if(dep.ActualCount == null)
				{
					dep.ActualCount = 0;
				}
			}
		}

		void ActualCountsOfOrderEqupmentFromNullToZero()
		{
			foreach(var equip in _routeListItem.Order.OrderEquipments)
			{
				if(equip.ActualCount == null)
				{
					equip.ActualCount = 0;
				}
			}
		}

		void ActualCountsOfOrderItemsFromNullToZero()
		{
			foreach(var item in _routeListItem.Order.OrderItems)
			{
				if(item.ActualCount == null)
				{
					item.SetActualCountZero();
				}
			}
		}

		private void ConfigureDeliveryPointRefference(Counterparty client = null)
		{
			var deliveryPointFilter = new DeliveryPointJournalFilterViewModel
			{
				Counterparty = client
			};

			var deliveryPointJournalFactory = _lifetimeScope.Resolve<IDeliveryPointJournalFactory>();
			deliveryPointJournalFactory.SetDeliveryPointJournalFilterViewModel(deliveryPointFilter);
			entityVMEntryDeliveryPoint
				.SetEntityAutocompleteSelectorFactory(deliveryPointJournalFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			entityVMEntryDeliveryPoint.Binding
				.AddBinding(this, s => s.DeliveryPoint, w => w.Subject)
				.InitializeFromSource();
		}

		protected void OnButtonNotDeliveredClicked(object sender, EventArgs e)
		{
			OpenOrCreateUndelivery(RouteListItemStatus.Overdue);
		}

		protected void OnButtonDeliveryCanceledClicked(object sender, EventArgs e)
		{
			OpenOrCreateUndelivery(RouteListItemStatus.Canceled);
		}

		private void OpenOrCreateUndelivery(RouteListItemStatus routeListItemStatusToChange)
		{
			_routeListItemStatusToChange = routeListItemStatusToChange;
			_undeliveryViewModel = _tdiNavigationManager.OpenViewModelOnTdi<UndeliveryViewModel>(
				this,			
				OpenPageOptions.AsSlave,
				vm =>
				{
					vm.Saved += OnUndeliveryViewModelSaved;
					vm.Initialize(UoW, _routeListItem.Order.Id, isFromRouteListClosing: true);					
				}
				).ViewModel;
		}

		private void OnUndeliveryViewModelSaved(object sender, UndeliveryOnOrderCloseEventArgs e)
		{
			_routeListService.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.RouteList, _routeListItem.Id, _routeListItemStatusToChange, 
				_callTaskWorker, true);
			_routeListItem.SetOrderActualCountsToZeroOnCanceled();
			_routeListItem.BottlesReturned = 0;
			UpdateButtonsState();

			if(e.NeedClose)
			{
				OnCloseTab(false);
			}

			UoW.Save(_routeListItem);
		}

		protected void OnButtonDeliveredClicked(object sender, EventArgs e)
		{
			_routeListService.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.RouteList, _routeListItem.Id, RouteListItemStatus.Completed, 
				_callTaskWorker, true);
			_routeListItem.RestoreOrder();
			_routeListItem.FirstFillClosing(_wageParameterService);
			UpdateListsSentivity();
			UpdateButtonsState();
		}

		void UpdateButtonsState()
		{
			bool isTransfered = _routeListItem.Status == RouteListItemStatus.Transfered;
			buttonDeliveryCanceled.Sensitive = !isTransfered && _routeListItem.Status != RouteListItemStatus.Canceled;
			buttonNotDelivered.Sensitive = !isTransfered && _routeListItem.Status != RouteListItemStatus.Overdue;
			buttonDelivered.Sensitive = !isTransfered && _routeListItem.Status != RouteListItemStatus.Completed
				&& _routeListItem.Status != RouteListItemStatus.EnRoute;
		}

		protected void OnYenumcomboOrderPaymentChangedByUser(object sender, EventArgs e)
		{
			_routeListItem.RecalculateTotalCash();
		}

		private void AcceptOrderChange()
		{
			if(CompletedChange == ChangedType.None)
			{
				Initialize(_routeListItem.Order);
				return;
			}

			if(CompletedChange == ChangedType.DeliveryPoint)
			{
				_routeListItem.Order.UpdateDeliveryPoint(DeliveryPoint, _contractUpdater);
			}

			if(CompletedChange == ChangedType.Both)
			{
				var nomenclatureSettings = ScopeProvider.Scope.Resolve<INomenclatureSettings>();
				//Сначала ставим точку доставки чтобы при установке клиента она была доступна,
				//иначе при записи клиента убирается не его точка доставки и будет ошибка при
				//изменении документов которые должны меняться при смене клиента потомучто точка
				//доставки будет пустая
				_routeListItem.Order.UpdateDeliveryPoint(DeliveryPoint, _contractUpdater);
				_routeListItem.Order.UpdateClient(Client, _contractUpdater, out var updateClientMessage);
				_routeListItem.Order.UpdateBottleMovementOperation(UoW, nomenclatureSettings, _routeListItem.BottlesReturned);
			}
		}

		private void OnClientBeforeChangeByUser(object sender, BeforeChangeEventArgs e)
		{
			if(IsClientSelectedAndOrderCashlessAndPaid)
			{
				ServicesConfig.InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					Errors.Orders.OrderErrors.PaidCashlessOrderClientReplacementError.Message);

				e.CanChange = false;
				return;
			}
			e.CanChange = true;
		}

		protected void OnClientEntryViewModelChangedByUser(object sender, EventArgs e)
		{
			if(!(clientEntry.ViewModel.Entity is Counterparty counterparty))
			{
				return;
			}

			var cts = new CancellationTokenSource();

			try
			{
				_counterpartyService.StopShipmentsIfNeeded(counterparty.Id, _currentEmployee.Id, cts.Token).GetAwaiter().GetResult();
			}
			catch(Exception)
			{
			}

			if(counterparty.RevenueStatus.HasValue && counterparty.RevenueStatus != RevenueStatus.Active)
			{
				if(!CanFormOrderWithLiquidatedCounterparty)
				{
					clientEntry.ViewModel.Entity = null;
					_interactiveService.ShowMessage(ImportanceLevel.Warning, "Нет прав для выбора ликвидированного контрагента!");
					clientEntry.ViewModel.Entity = _lastCounterparty;
					return;
				}

				_interactiveService.ShowMessage(ImportanceLevel.Warning, "Контрагент в статусе ликвидации!");
			}

			_lastCounterparty = clientEntry.ViewModel.Entity as Counterparty;

			ConfigureDeliveryPointRefference(clientEntry.ViewModel.Entity as Counterparty);
			DeliveryPoint = null;
			BaseOrder.ContactPhone = null;

			if(clientEntry.ViewModel.Entity != null)
			{
				entityVMEntryDeliveryPoint.OpenSelectDialog();
			}
		}

		protected void OnClientEntryViewModelChanged(object sender, EventArgs e)
		{
			if(clientEntry.ViewModel.Entity == null)
			{
				return;
			}

			var previousPaymentType = yenumcomboOrderPayment.SelectedItem as PaymentType?;
			Enum[] hideEnums = { PaymentType.Cashless };
			var personType = (clientEntry.ViewModel.Entity as Counterparty).PersonType;

			if(personType == PersonType.natural)
			{
				yenumcomboOrderPayment.AddEnumToHideList(hideEnums);
			}
			else
			{
				yenumcomboOrderPayment.RemoveEnumFromHideList(hideEnums);
			}

			if(previousPaymentType.HasValue)
			{
				if(personType == PersonType.natural && hideEnums.Contains(previousPaymentType.Value))
				{
					yenumcomboOrderPayment.SelectedItem = PaymentType.Cash;
				}
				else
				{
					yenumcomboOrderPayment.SelectedItem = previousPaymentType;
				}
			}

			yenumcomboOrderPayment.Sensitive = !IsClientSelectedAndOrderCashlessAndPaid && _routeListItem.Status != RouteListItemStatus.Transfered;
		}

		protected void OnButtonAddOrderItemClicked(object sender, EventArgs e)
		{
			OpenSelectNomenclatureDlg();
		}

		protected void OnButtonDeleteOrderItemClicked(object sender, EventArgs e)
		{
			if(ytreeToClient.GetSelectedObject() is OrderItemReturnsNode selectedItemNode
				&& selectedItemNode.OrderItem != null)
			{
				_routeListItem.Order.RemoveItemFromClosingOrder(UoW, _contractUpdater, selectedItemNode.OrderItem);
				UpdateItemsList();
			}
		}

		void OrderEquipmentItemsView_OnDeleteEquipment(object sender, OrderEquipment e)
		{
			//Если оборудование добавлено в изменении заказа то базовое количество равно 0,
			//значит такое оборудование можно удалять из изменения заказа
			if(e.OrderItem == null && e.Count == 0)
			{
				_routeListItem.Order.RemoveEquipment(UoW, _contractUpdater, e);
			}
		}

		public bool CanClose()
		{
			var hasReceipts = _orderRepository.OrderHasSentReceipt(UoW, _routeListItem.Order.Id);

			IgnoreReceipt = hasReceipts
				&& _canEditOrderAfterRecieptCreated
				&& _interactiveService.Question("По данному заказу сформирован кассовый чек. Если внесете изменения, то вам нужно будет сообщить в бухгалтерию о чеке и на склад о пересорте. Продолжить?");

			var validationContext = new ValidationContext(_routeListItem.Order, null, new Dictionary<object, object>
			{
				{ "NewStatus", OrderStatus.Closed },
				{ "AddressStatus", _routeListItem.Status },
				{ Order.ValidationKeyIgnoreReceipts, IgnoreReceipt }
			});

			validationContext.ServiceContainer.AddService(_orderSettings);
			validationContext.ServiceContainer.AddService(_deliveryRulesSettings);

			_routeListItem.AddressIsValid = ServicesConfig.ValidationService.Validate(_routeListItem.Order, validationContext);
			_routeListItem.Order.CheckAndSetOrderIsService();
			orderEquipmentItemsView.UnsubscribeOnEquipmentAdd();

			//Не блокируем закрытие вкладки
			return true;
		}

		protected void OnYenumcomboOrderPaymentChanged(object sender, EventArgs e)
		{
			OnlineOrderVisible();
		}

		private void OnlineOrderVisible()
		{
			labelOnlineOrder.Visible = entryOnlineOrder.Visible =
				(_routeListItem.Order.PaymentType == PaymentType.PaidOnline
				 || _routeListItem.Order.PaymentType == PaymentType.Terminal);
		}

		protected void OnYspinbuttonBottlesByStockActualCountChanged(object sender, EventArgs e)
		{
			var orderSettings = ScopeProvider.Scope.Resolve<IOrderSettings>();
			_routeListItem.Order.CalculateBottlesStockDiscounts(orderSettings, true);
		}

		protected void OnEntityVMEntryDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			AcceptOrderChange();
		}

		public override void Dispose()
		{
			if(_undeliveryViewModel != null)
			{
				_undeliveryViewModel.Saved -= OnUndeliveryViewModelSaved;
				UnsubscribeBaseOrder();
			}

			base.Dispose();
		}

		private void UnsubscribeBaseOrder()
		{
			BaseOrder.PropertyChanged -= OnOrderPropertyChanged;
		}
	}
}
