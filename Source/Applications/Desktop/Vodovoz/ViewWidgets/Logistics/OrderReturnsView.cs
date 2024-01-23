using Autofac;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog;
using QS.Dialog.GtkUI;
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
using System.Globalization;
using System.Linq;
using System.Threading;
using Vodovoz.Controllers;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
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
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using Vodovoz.ViewModels.TempAdapters;

namespace Vodovoz
{
	public partial class OrderReturnsView : QS.Dialog.Gtk.TdiTabBase, ITDICloseControlTab, ISingleUoWDialog
	{
		#region Поля и свойства

		private readonly ILifetimeScope _lifetimeScope;
		private readonly ICounterpartyService _counterpartyService;
		private readonly IInteractiveService _interactiveService;
		private readonly IEmployeeService _employeeService;
		private readonly IUserRepository _userRepository;
		private readonly IParametersProvider _parametersProvider;
		private readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider;
		private readonly INavigationManager _navigationManager;
		private readonly IOrderParametersProvider _orderParametersProvider;
		private readonly IOrderRepository _orderRepository;
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IWageParameterService _wageParameterService;
		private readonly INomenclatureOnlineParametersProvider _nomenclatureOnlineParametersProvider;
		private readonly IOrderDiscountsController _discountsController;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly INomenclatureFixedPriceProvider _nomenclatureFixedPriceProvider;

		private List<OrderItemReturnsNode> _itemsToClient;

		public event PropertyChangedEventHandler PropertyChanged;

		private Employee _currentEmployee;

		private Counterparty _lastCounterparty = null;

		private RouteListItem _routeListItem;
		private bool _canEditPrices;
		private bool _canEditOrderAfterRecieptCreated;

		private OrderNode _orderNode;

		public IUnitOfWork UoW { get; }

		public ICallTaskWorker CallTaskWorker { get; }

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
			IParametersProvider parametersProvider,
			IOrderParametersProvider orderParametersProvider,
			INomenclatureOnlineParametersProvider nomenclatureOnlineParametersProvider,
			IDeliveryRulesParametersProvider deliveryRulesParametersProvider,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			if(currentPermissionService is null)
			{
				throw new ArgumentNullException(nameof(currentPermissionService));
			}

			CanFormOrderWithLiquidatedCounterparty = currentPermissionService
				.ValidatePresetPermission(Vodovoz.Permissions.Order.CanFormOrderWithLiquidatedCounterparty);

			_canEditPrices = currentPermissionService
				.ValidatePresetPermission(Vodovoz.Permissions.Order.CanEditPriceDiscountFromRouteList);

			_canEditOrderAfterRecieptCreated = currentPermissionService
				.ValidatePresetPermission(Vodovoz.Permissions.Order.CanChangeOrderAfterRecieptCreated);

			Build();

			UoW = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_discountsController = orderDiscountsController ?? throw new ArgumentNullException(nameof(orderDiscountsController));
			CallTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_counterpartyService = counterpartyService ?? throw new ArgumentNullException(nameof(counterpartyService));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
			_employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_wageParameterService = wageParameterService ?? throw new ArgumentNullException(nameof(wageParameterService));
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
			_orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
			_nomenclatureOnlineParametersProvider = nomenclatureOnlineParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureOnlineParametersProvider));
			_deliveryRulesParametersProvider = deliveryRulesParametersProvider ?? throw new ArgumentNullException(nameof(deliveryRulesParametersProvider));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public bool CanFormOrderWithLiquidatedCounterparty { get; }

		public bool IgnoreReceipt { get; private set; } = false;

		public int? OrderId => _routeListItem?.Order?.Id;

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
			var journalViewModel = (_navigationManager as ITdiCompatibilityNavigation)
				.OpenViewModelOnTdi<NomenclaturesJournalViewModel, Action<NomenclatureFilterViewModel>>(this, filter =>
			{
				filter.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder();
				filter.SelectCategory = NomenclatureCategory.deposit;
				filter.SelectSaleCategory = SaleCategory.forSale;
				filter.RestrictArchive = false;
			},
			OpenPageOptions.AsSlave,
			viewModel =>
			{
				viewModel.SelectionMode = JournalSelectionMode.Single;
				viewModel.TabName = "Номенклатура на продажу";
				viewModel.CalculateQuantityOnStock = true;
				viewModel.OnEntitySelectedResult += OnNomenclatureSelected;
			});
		}

		private void OnNomenclatureSelected(object sender, JournalSelectedNodesEventArgs e)
		{
			var selectedNodes = e.SelectedNodes;

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
					_routeListItem.Order.AddWaterForSale(nomenclature, 0, 0);
					break;
				case NomenclatureCategory.master:
					_routeListItem.Order.AddMasterNomenclature(nomenclature, 0);
					break;
				default:
					_routeListItem.Order.AddAnyGoodsNomenclatureForSale(nomenclature, true);
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

			_orderNode = new OrderNode(_routeListItem.Order);

			var builder = new LegacyEEVMBuilderFactory<OrderNode>(this, _orderNode, UoW, _navigationManager, _lifetimeScope);

			clientEntry.ViewModel = builder.ForProperty(x => x.Client)
				.UseTdiEntityDialog()
				.UseViewModelJournalAndAutocompleter<CounterpartyJournalViewModel>()
				.Finish();
			clientEntry.ViewModel.Changed += OnClientEntryViewModelChanged;
			clientEntry.ViewModel.ChangedByUser += OnClientEntryViewModelChangedByUser;

			orderEquipmentItemsView.Configure(UoW, _routeListItem.Order, new FlyerRepository());
			ConfigureDeliveryPointRefference(_orderNode.Client);

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
						.EditedEvent(OnSpinActualCountEdited)
						.AddSetter((cell, node) => cell.Editable = node.Nomenclature.Category != NomenclatureCategory.deposit)
						.Adjustment(new Adjustment(0, 0, 9999, 1, 1, 0))
						.AddSetter((cell, node) => cell.Editable = !node.IsEquipment)
					.AddTextRenderer(node => node.Nomenclature.Unit == null ? string.Empty : node.Nomenclature.Unit.Name, false)
				.AddColumn("Цена")
					.AddNumericRenderer(node => node.Price).Digits(2).WidthChars(10)
						.EditedEvent(OnSpinPriceEdited)
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
				.AddColumn("")
				.Finish();

			yenumcomboOrderPayment.ItemsEnum = typeof(PaymentType);
			yenumcomboOrderPayment.Binding.AddBinding(_routeListItem.Order, o => o.PaymentType, w => w.SelectedItem).InitializeFromSource();

			if(_routeListItem.Order.PaymentType == PaymentType.PaidOnline)
			{
				ySpecPaymentFrom.ItemsList = UoW.Session.QueryOver<PaymentFrom>()
					.Where(
						p => !p.IsArchive
						|| _routeListItem.Order.PaymentByCardFrom.Id == p.Id
				).List();
			}
			else
			{
				ySpecPaymentFrom.ItemsList = UoW.Session.QueryOver<PaymentFrom>().Where(p => !p.IsArchive).List();
			}

			ySpecPaymentFrom.Binding.AddBinding(_routeListItem.Order, e => e.PaymentByCardFrom, w => w.SelectedItem).InitializeFromSource();
			ySpecPaymentFrom.Binding.AddFuncBinding(_routeListItem.Order, e => e.PaymentType == PaymentType.PaidOnline, w => w.Visible)
				.InitializeFromSource();

			yenumcomboboxTerminalSubtype.ItemsEnum = typeof(PaymentByTerminalSource);
			yenumcomboboxTerminalSubtype.Binding
				.AddSource(_routeListItem.Order)
				.AddBinding(s => s.PaymentByTerminalSource, w => w.SelectedItemOrNull)
				.AddFuncBinding(s => s.PaymentType == PaymentType.Terminal, w => w.Visible)
				.InitializeFromSource();

			entryOnlineOrder.ValidationMode = QSWidgetLib.ValidationType.numeric;
			entryOnlineOrder.Binding.AddBinding(_routeListItem.Order, e => e.OnlineOrder, w => w.Text, new NullableIntToStringConverter())
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

		private void OnSpinActualCountEdited(object o, EditedArgs args)
		{
			decimal.TryParse(args.NewText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newActualCount);
			var node = ytreeToClient.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			orderItem.SetActualCount(newActualCount);
		}

		private void OnSpinPriceEdited(object o, EditedArgs args)
		{
			decimal.TryParse(args.NewText, NumberStyles.Any, CultureInfo.InvariantCulture, out var newPrice);
			var node = ytreeToClient.YTreeModel.NodeAtPath(new TreePath(args.Path));
			if(!(node is OrderItem orderItem))
			{
				return;
			}

			orderItem.SetPrice(newPrice);
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
			entityVMEntryDeliveryPoint.SetEntityAutocompleteSelectorFactory(deliveryPointJournalFactory.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			entityVMEntryDeliveryPoint.Binding.AddBinding(_orderNode, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
		}

		protected void OnButtonNotDeliveredClicked(object sender, EventArgs e)
		{
			var dlg = new UndeliveryOnOrderCloseDlg(_routeListItem.Order, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (s, ea) =>
			{
				_routeListItem.RouteList.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.Id, RouteListItemStatus.Overdue, CallTaskWorker, true);
				_routeListItem.SetOrderActualCountsToZeroOnCanceled();
				_routeListItem.BottlesReturned = 0;
				UpdateButtonsState();

				if(ea.NeedClose)
				{
					OnCloseTab(false);
				}

				UoW.Save(_routeListItem);
				UoW.Commit();
			};
		}

		protected void OnButtonDeliveryCanceledClicked(object sender, EventArgs e)
		{
			var dlg = new UndeliveryOnOrderCloseDlg(_routeListItem.Order, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (s, ea) =>
			{
				_routeListItem.RouteList.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.Id, RouteListItemStatus.Canceled, CallTaskWorker, true);
				_routeListItem.SetOrderActualCountsToZeroOnCanceled();
				_routeListItem.BottlesReturned = 0;
				UpdateButtonsState();

				if(ea.NeedClose)
				{
					OnCloseTab(false);
				}

				UoW.Save(_routeListItem);
				UoW.Commit();
			};
		}

		protected void OnButtonDeliveredClicked(object sender, EventArgs e)
		{
			_routeListItem.RouteList.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.Id, RouteListItemStatus.Completed, CallTaskWorker, true);
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
			if(_orderNode.CompletedChange == OrderNode.ChangedType.None)
			{
				_orderNode = new OrderNode(_routeListItem.Order);
				return;
			}

			if(_orderNode.CompletedChange == OrderNode.ChangedType.DeliveryPoint)
			{
				_routeListItem.Order.DeliveryPoint = _orderNode.DeliveryPoint;
			}

			if(_orderNode.CompletedChange == OrderNode.ChangedType.Both)
			{
				//Сначала ставим точку доставки чтобы при установке клиента она была доступна,
				//иначе при записи клиента убирается не его точка доставки и будет ошибка при
				//изменении документов которые должны меняться при смене клиента потомучто точка
				//доставки будет пустая
				_routeListItem.Order.DeliveryPoint = _orderNode.DeliveryPoint;
				_routeListItem.Order.Client = _orderNode.Client;
				_routeListItem.Order.UpdateBottleMovementOperation(
					UoW, new BaseParametersProvider(_parametersProvider), _routeListItem.BottlesReturned);
			}
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

			if(counterparty.IsLiquidating)
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
			_orderNode.DeliveryPoint = null;

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

			PaymentType? previousPaymentType = yenumcomboOrderPayment.SelectedItem as PaymentType?;
			Enum[] hideEnums = { PaymentType.Cashless };
			PersonType personType = (clientEntry.ViewModel.Entity as Counterparty).PersonType;

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
				_routeListItem.Order.RemoveAloneItem(selectedItemNode.OrderItem);
				UpdateItemsList();
			}
		}

		void OrderEquipmentItemsView_OnDeleteEquipment(object sender, OrderEquipment e)
		{
			//Если оборудование добавлено в изменении заказа то базовое количество равно 0,
			//значит такое оборудование можно удалять из изменения заказа
			if(e.OrderItem == null && e.Count == 0)
			{
				_routeListItem.Order.RemoveEquipment(e);
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

			validationContext.ServiceContainer.AddService(_orderParametersProvider);
			validationContext.ServiceContainer.AddService(_deliveryRulesParametersProvider);

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
			var orderParametersProvider = new OrderParametersProvider(_parametersProvider);
			_routeListItem.Order.CalculateBottlesStockDiscounts(orderParametersProvider, true);
		}

		protected void OnEntityVMEntryDeliveryPointChangedByUser(object sender, EventArgs e)
		{
			AcceptOrderChange();
		}
	}
}
