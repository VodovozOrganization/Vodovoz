using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Tdi;
using QSProjectsLib;
using Vodovoz.Core.DataService;
using Vodovoz.Dialogs;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalFilters;
using Vodovoz.Services;
using QS.Dialog;
using QS.Project.Journal;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using Vodovoz.EntityRepositories.WageCalculation;
using QS.Project.Services;
using QS.Utilities.Extensions;
using Vodovoz.Controllers;
using Vodovoz.Domain;
using Vodovoz.Domain.EntityFactories;
using Vodovoz.EntityRepositories;
using Vodovoz.Tools.CallTasks;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Tools;
using Vodovoz.Infrastructure.Converters;
using Vodovoz.Filters.ViewModels;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.Parameters;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz
{
	public partial class OrderReturnsView : QS.Dialog.Gtk.TdiTabBase, ITDICloseControlTab, ISingleUoWDialog
	{
		private class OrderNode : PropertyChangedBase
		{
			public enum ChangedType
			{
				None,
				DeliveryPoint,
				Both
			}

			Counterparty client;

			public Counterparty Client
			{
				get => client;
				set => SetField(ref client, value, () => Client);
			}

			DeliveryPoint deliveryPoint;

			public DeliveryPoint DeliveryPoint
			{
				get => deliveryPoint;
				set => SetField(ref deliveryPoint, value, () => DeliveryPoint);
			}

			private Order BaseOrder { get; set; }

			public OrderNode(Order order)
			{
				DeliveryPoint = order.DeliveryPoint;
				Client = order.Client;
				BaseOrder = order;
			}

			public ChangedType CompletedChange
			{
				get
				{
					if(Client == null || DeliveryPoint == null)
					{
						return ChangedType.None;
					}

					if(Client.Id == BaseOrder.Client.Id && DeliveryPoint.Id != BaseOrder.DeliveryPoint.Id)
					{
						return ChangedType.DeliveryPoint;
					}

					if(Client.Id != BaseOrder.Client.Id)
					{
						return ChangedType.Both;
					}

					return ChangedType.None;
				}
			}
		}

		#region Поля и свойства

		private static readonly IParametersProvider _parametersProvider = new ParametersProvider();
		private static readonly IDeliveryRulesParametersProvider _deliveryRulesParametersProvider =
			new DeliveryRulesParametersProvider(_parametersProvider);
		private static readonly IOrderParametersProvider _orderParametersProvider = new OrderParametersProvider(_parametersProvider);
		private readonly IOrderRepository _orderRepository = new OrderRepository();
		private readonly IDiscountReasonRepository _discountReasonRepository = new DiscountReasonRepository();
		private readonly WageParameterService _wageParameterService =
			new WageParameterService(new WageCalculationRepository(), new BaseParametersProvider(_parametersProvider));
		private readonly RouteListItem _routeListItem;
		private readonly IOrderDiscountsController _discountsController;

		private IUnitOfWork _uow;
		private bool _canEditPrices;
		private OrderNode _orderNode;
		private CallTaskWorker _callTaskWorker;
		private List<OrderItemReturnsNode> _itemsToClient;
		private INomenclatureFixedPriceProvider _nomenclatureFixedPriceProvider;
		private INomenclatureRepository _nomenclatureRepository;
		public event PropertyChangedEventHandler PropertyChanged;

		public IUnitOfWork UoW
		{
			get => _uow;
			set
			{
				_uow = value;
				depositrefunditemsview1.Configure(_uow, _routeListItem.Order, true);
			}
		}

		public virtual CallTaskWorker CallTaskWorker
		{
			get =>
				_callTaskWorker ?? (_callTaskWorker = new CallTaskWorker(
					CallTaskSingletonFactory.GetInstance(),
					new CallTaskRepository(),
					_orderRepository,
					new EmployeeRepository(),
					new BaseParametersProvider(_parametersProvider),
					ServicesConfig.CommonServices.UserService,
					ErrorReporter.Instance));
			set => _callTaskWorker = value;
		}
		
		#endregion

		public OrderReturnsView(RouteListItem routeListItem, IUnitOfWork uow)
		{
			Build();
			_routeListItem = routeListItem;
			TabName = "Изменение заказа №" + routeListItem.Order.Id;

			UoW = uow;

			UpdateListsSentivity();
			entryTotal.Sensitive = yenumcomboOrderPayment.Sensitive = routeListItem.Status != RouteListItemStatus.Transfered;
			
			orderEquipmentItemsView.OnDeleteEquipment += OrderEquipmentItemsView_OnDeleteEquipment;
			Configure();
			UpdateItemsList();
			_discountsController = new OrderDiscountsController(_nomenclatureFixedPriceProvider);
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
			var nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.AvailableCategories = Nomenclature.GetCategoriesForSaleToOrder(),
				x => x.SelectCategory = NomenclatureCategory.deposit,
				x => x.SelectSaleCategory = SaleCategory.forSale,
				x => x.RestrictArchive = false
			);

			NomenclaturesJournalViewModel journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				new NomenclatureJournalFactory(),
				new CounterpartyJournalFactory(),
				_nomenclatureRepository,
				new UserRepository()
			) {
				SelectionMode = JournalSelectionMode.Single
			};
			journalViewModel.TabName = "Номенклатура на продажу";
			journalViewModel.CalculateQuantityOnStock = true;
			journalViewModel.OnEntitySelectedResult += OnNomenclatureSelected;
			TabParent.AddSlaveTab(this, journalViewModel);
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
			if(_routeListItem.Order.IsLoadedFrom1C || nomenclature == null || contract == null)
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
			_nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			_nomenclatureFixedPriceProvider =
				new NomenclatureFixedPriceController(new NomenclatureFixedPriceFactory());
			_canEditPrices =
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_edit_price_discount_from_route_list");
			_orderNode = new OrderNode(_routeListItem.Order);
			var counterpartyFilter = new CounterpartyFilter(UoW);
			counterpartyFilter.SetAndRefilterAtOnce(x => x.RestrictIncludeArhive = false);
			referenceClient.RepresentationModel = new ViewModel.CounterpartyVM(counterpartyFilter);
			referenceClient.Binding.AddBinding(_orderNode, s => s.Client, w => w.Subject).InitializeFromSource();
			referenceClient.CanEditReference = false;
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
						.AddSetter((cell, node) => cell.Editable = node.Nomenclature.Category != NomenclatureCategory.deposit)
						.Adjustment(new Adjustment(0, 0, 9999, 1, 1, 0))
						.AddSetter((cell, node) => cell.Adjustment = new Adjustment(0, 0, GetMaxCount(node), 1, 1, 0))
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
						.AddSetter((cell, node) => cell.Editable =  _canEditPrices)
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
						.AddSetter((c, n) => c.Editable =  _canEditPrices)
						.AddSetter(
							(c, n) =>
								c.BackgroundGdk = n.Discount > 0 && n.DiscountReason == null && n.OrderItem?.PromoSet == null
									? new Gdk.Color(0xff, 0x66, 0x66)
									: new Gdk.Color(0xff, 0xff, 0xff)
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

			if (_routeListItem.Order.PaymentType == PaymentType.ByCard)
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
			ySpecPaymentFrom.Binding.AddFuncBinding(_routeListItem.Order, e => e.PaymentType == PaymentType.ByCard, w => w.Visible)
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
			
			Application.Invoke((sender, eventArgs) =>
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
						ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
							$"На позицию:\n№{index + 1} {message}нельзя применить скидку," +
							" т.к. она из промо-набора или на нее есть фикса.\nОбратитесь к руководителю");
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
					dep.ActualCount = 0;
			}
		}

		void ActualCountsOfOrderEqupmentFromNullToZero()
		{
			foreach(var equip in _routeListItem.Order.OrderEquipments)
			{
				if(equip.ActualCount == null)
					equip.ActualCount = 0;
			}
		}

		void ActualCountsOfOrderItemsFromNullToZero()
		{
			foreach(var item in _routeListItem.Order.OrderItems)
			{
				if(item.ActualCount == null)
					item.ActualCount = 0;
			}
		}

		int GetMaxCount(OrderItemReturnsNode node)
		{
			var count = node.Nomenclature.Category == NomenclatureCategory.deposit
				? 1
				: 9999;
			return count;
		}

		private void ConfigureDeliveryPointRefference(Counterparty client = null)
		{
			var deliveryPointFilter = new DeliveryPointJournalFilterViewModel
			{
				Counterparty = client
			};
			entityVMEntryDeliveryPoint.SetEntityAutocompleteSelectorFactory(new DeliveryPointJournalFactory(deliveryPointFilter)
				.CreateDeliveryPointByClientAutocompleteSelectorFactory());
			entityVMEntryDeliveryPoint.Binding.AddBinding(_orderNode, s => s.DeliveryPoint, w => w.Subject).InitializeFromSource();
		}

		protected void OnButtonNotDeliveredClicked(object sender, EventArgs e)
		{
			UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(_routeListItem.Order, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (s, ea) =>
			{
				_routeListItem.RouteList.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.Id, RouteListItemStatus.Overdue, CallTaskWorker, true);
				_routeListItem.SetOrderActualCountsToZeroOnCanceled();
				_routeListItem.BottlesReturned = 0;
				UpdateButtonsState();
				OnCloseTab(false);
			};
		}

		protected void OnButtonDeliveryCanceledClicked(object sender, EventArgs e)
		{
			UndeliveryOnOrderCloseDlg dlg = new UndeliveryOnOrderCloseDlg(_routeListItem.Order, UoW);
			TabParent.AddSlaveTab(this, dlg);
			dlg.DlgSaved += (s, ea) =>
			{
				_routeListItem.RouteList.ChangeAddressStatusAndCreateTask(UoW, _routeListItem.Id, RouteListItemStatus.Canceled, CallTaskWorker, true);
				_routeListItem.SetOrderActualCountsToZeroOnCanceled();
				_routeListItem.BottlesReturned = 0;
				UpdateButtonsState();
				OnCloseTab(false);
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
			buttonDelivered.Sensitive = !isTransfered && _routeListItem.Status != RouteListItemStatus.Completed &&
										_routeListItem.Status != RouteListItemStatus.EnRoute;
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

		protected void OnReferenceClientChangedByUser(object sender, EventArgs e)
		{
			ConfigureDeliveryPointRefference(_orderNode.Client);
			entityVMEntryDeliveryPoint.OpenSelectDialog();
		}

		protected void OnReferenceClientChanged(object sender, EventArgs e)
		{
			if(referenceClient.Subject == null)
			{
				return;
			}
			
			PaymentType? previousPaymentType = yenumcomboOrderPayment.SelectedItem as PaymentType?;
			Enum[] hideEnums = {PaymentType.cashless};
			PersonType personType = (referenceClient.Subject as Counterparty).PersonType;
			if(personType == PersonType.natural)
				yenumcomboOrderPayment.AddEnumToHideList(hideEnums);
			else
				yenumcomboOrderPayment.RemoveEnumFromHideList(hideEnums);

			if(previousPaymentType.HasValue)
			{
				if(personType == PersonType.natural && hideEnums.Contains(previousPaymentType.Value))
					yenumcomboOrderPayment.SelectedItem = PaymentType.cash;
				else
					yenumcomboOrderPayment.SelectedItem = previousPaymentType;
			}
		}

		protected void OnButtonAddOrderItemClicked(object sender, EventArgs e)
		{
			OpenSelectNomenclatureDlg();
		}

		protected void OnButtonDeleteOrderItemClicked(object sender, EventArgs e)
		{
			if(ytreeToClient.GetSelectedObject() is OrderItemReturnsNode selectedItemNode && selectedItemNode.OrderItem != null)
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
			ValidationContext validationContext = new ValidationContext(_routeListItem.Order, null, new Dictionary<object, object>
			{
				{"NewStatus", OrderStatus.Closed},
				{"AddressStatus", _routeListItem.Status}
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
				(_routeListItem.Order.PaymentType == PaymentType.ByCard
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

	public class OrderItemReturnsNode
	{
		OrderItem orderItem;
		OrderEquipment orderEquipment;

		public OrderItem OrderItem => orderItem;

		public OrderItemReturnsNode(OrderItem item)
		{
			orderItem = item;
		}

		public OrderItemReturnsNode(OrderEquipment equipment)
		{
			orderEquipment = equipment;
		}

		public bool IsEquipment => orderEquipment != null;

		public bool IsSerialEquipment
		{
			get
			{
				return
					IsEquipment
					&& orderEquipment.Equipment != null
					&& orderEquipment.Equipment.Nomenclature.IsSerial;
			}
		}

		public bool IsDelivered
		{
			get => ActualCount > 0;
			set
			{
				if(IsEquipment && IsSerialEquipment)
					ActualCount = value ? 1 : 0;
			}
		}

		public decimal ActualCount
		{
			get
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
						return orderEquipment.Confirmed ? 1 : 0;
					return orderEquipment.ActualCount ?? 0;
				}

				return orderItem.ActualCount ?? 0;
			}
			set
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
						orderEquipment.ActualCount = value > 0 ? 1 : 0;
					orderEquipment.ActualCount = (int?) value;
				}
				else
				{
					orderItem.ActualCount = value;
				}
			}
		}

		public Nomenclature Nomenclature
		{
			get
			{
				if(IsEquipment)
				{
					if(IsSerialEquipment)
					{
						return orderEquipment.Equipment.Nomenclature;
					}

					return orderEquipment.Nomenclature;
				}

				return orderItem.Nomenclature;
			}
		}

		public decimal Count => IsEquipment ? 1 : orderItem.Count;

		public string Name => IsEquipment ? orderEquipment.NameString : orderItem.NomenclatureString;

		public bool HasPrice => !IsEquipment || orderEquipment.OrderItem != null;

		public string ConfirmedComments
		{
			get => IsEquipment ? orderEquipment.ConfirmedComment : null;
			set
			{
				if(IsEquipment)
					orderEquipment.ConfirmedComment = value;
			}
		}

		public decimal Price
		{
			get
			{
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.Price : 0;
				return orderItem.Price;
			}
			set
			{
				if(IsEquipment)
				{
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.Price = value;
				}
				else
					orderItem.Price = value;
			}
		}

		public bool IsDiscountInMoney
		{
			get
			{
				if(IsEquipment)
					return orderEquipment.OrderItem != null && orderEquipment.OrderItem.IsDiscountInMoney;
				return orderItem.IsDiscountInMoney;
			}

			set
			{
				if(IsEquipment)
					orderEquipment.OrderItem.IsDiscountInMoney = orderEquipment.OrderItem != null && value;
				else
					orderItem.IsDiscountInMoney = value;
			}
		}

		public decimal ManualChangingDiscount
		{
			get
			{
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.ManualChangingDiscount : 0;
				return orderItem.ManualChangingDiscount;
			}

			set
			{
				if(IsEquipment)
				{
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.ManualChangingDiscount = value;
				}
				else
					orderItem.ManualChangingDiscount = value;
			}
		}

		public decimal DiscountSetter
		{
			get
			{
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.DiscountSetter : 0;
				return orderItem.DiscountSetter;
			}

			set
			{
				if(IsEquipment)
				{
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.DiscountSetter = value;
				}
				else
					orderItem.DiscountSetter = value;
			}
		}

		public decimal Discount
		{
			get
			{
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.Discount : 0m;
				return orderItem.Discount;
			}
			set
			{
				if(IsEquipment)
				{
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.Discount = value;
				}
				else
					orderItem.Discount = value;
			}
		}

		public decimal DiscountMoney
		{
			get
			{
				if(IsEquipment)
					return orderEquipment.OrderItem != null ? orderEquipment.OrderItem.DiscountMoney : 0m;
				return orderItem.DiscountMoney;
			}
		}

		public DiscountReason DiscountReason
		{
			get => IsEquipment ? orderEquipment.OrderItem?.DiscountReason : orderItem.DiscountReason;
			set
			{
				if(IsEquipment)
				{
					if(orderEquipment.OrderItem != null)
						orderEquipment.OrderItem.DiscountReason = value;
				}
				else
					orderItem.DiscountReason = value;
			}
		}

		public decimal Sum => Price * ActualCount - DiscountMoney;
	}
}
