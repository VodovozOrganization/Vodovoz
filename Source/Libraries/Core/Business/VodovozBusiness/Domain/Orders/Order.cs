using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Autofac;
using Core.Infrastructure;
using fyiReporting.RDL;
using Gamma.Utilities;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Exceptions;
using NLog;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Services;
using QS.Validation;
using Vodovoz.Controllers;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Contacts;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Permissions;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Extensions;
using Vodovoz.Services;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Common;
using Vodovoz.Settings.Delivery;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Services;
using VodovozBusiness.Services.Orders;
using Nomenclature = Vodovoz.Domain.Goods.Nomenclature;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "заказы",
		Nominative = "заказ",
		Prepositional = "заказе",
		PrepositionalPlural = "заказах"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class Order : OrderEntity, IValidatableObject
	{
		public const string DontArriveBeforeIntervalString = "Не приезжать раньше интервала!";
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private IOrderRepository _orderRepository => ScopeProvider.Scope
			.Resolve<IOrderRepository>();
		private IUndeliveredOrdersRepository _undeliveredOrdersRepository => ScopeProvider.Scope
			.Resolve<IUndeliveredOrdersRepository>();
		private IPaymentFromBankClientController _paymentFromBankClientController => ScopeProvider.Scope
			.Resolve<IPaymentFromBankClientController>();
		private INomenclatureRepository _nomenclatureRepository => ScopeProvider.Scope
			.Resolve<INomenclatureRepository>();
		private IDocumentOrganizationCounterRepository _documentOrganizationCounterRepository => ScopeProvider.Scope
			.Resolve<IDocumentOrganizationCounterRepository>();
		private INomenclatureSettings _nomenclatureSettings => ScopeProvider.Scope
			.Resolve<INomenclatureSettings>();
		private IEmailRepository _emailRepository => ScopeProvider.Scope
			.Resolve<IEmailRepository>();
		private IEmailService _emailService => ScopeProvider.Scope
			.Resolve<IEmailService>();
		private INomenclatureService _nomenclatureService => ScopeProvider.Scope
			.Resolve<INomenclatureService>();
		private IDeliveryRepository _deliveryRepository => ScopeProvider.Scope
			.Resolve<IDeliveryRepository>();
		private IGeneralSettings _generalSettingsParameters => ScopeProvider.Scope
			.Resolve<IGeneralSettings>();
		private IOrderSettings _orderSettings => ScopeProvider.Scope
			.Resolve<IOrderSettings>();
		private IDeliveryScheduleSettings _deliveryScheduleSettings => ScopeProvider.Scope
			.Resolve<IDeliveryScheduleSettings>();
		private OrderItemComparerForCopyingFromUndelivery _itemComparerForCopyingFromUndelivery => ScopeProvider.Scope
			.Resolve<OrderItemComparerForCopyingFromUndelivery>();
		public virtual IInteractiveService InteractiveService { get; set; }

		private ICounterpartyContractRepository _counterpartyContractRepository => ScopeProvider.Scope.Resolve<ICounterpartyContractRepository>();

		private IRouteListItemRepository _routeListItemRepository => ScopeProvider.Scope.Resolve<IRouteListItemRepository>();

		private ICashRepository _cashRepository => ScopeProvider.Scope.Resolve<ICashRepository>();

		private ISelfDeliveryRepository _selfDeliveryRepository => ScopeProvider.Scope.Resolve<ISelfDeliveryRepository>();
		private IOrderService _orderService => ScopeProvider.Scope.Resolve<IOrderService>();

		private readonly double _futureDeliveryDaysLimit = 30;

		#region Платная доставка

		private int paidDeliveryNomenclatureId;
		private int PaidDeliveryNomenclatureId
		{
			get
			{
				if(paidDeliveryNomenclatureId == default(int))
				{
					paidDeliveryNomenclatureId = _nomenclatureSettings.PaidDeliveryNomenclatureId;
				}

				return paidDeliveryNomenclatureId;
			}
		}

		#endregion

		private Phone _contactPhone;
		private DateTime? _commentOPManagerUpdatedAt;
		private Employee _commentOPManagerChangedBy;
		private bool? _canCreateOrderInAdvance;
		private GeoGroup _selfDeliveryGeoGroup;
		private OnlineOrder _onlineOrder;

		public Order()
		{
			Comment = string.Empty;
			OrderStatus = OrderStatus.NewOrder;
			OrderPaymentStatus = OrderPaymentStatus.None;
			SumDifferenceReason = string.Empty;
			ClientPhone = string.Empty;
		}

		#region Cвойства

		private Employee author;

		[Display(Name = "Создатель заказа")]
		public virtual Employee Author {
			get => author;
			set => SetField(ref author, value, () => Author);
		}

		private Employee acceptedOrderEmployee;
		[Display(Name = "Заказ подтвердил")]
		public virtual Employee AcceptedOrderEmployee {
			get => acceptedOrderEmployee;
			set => SetField(ref acceptedOrderEmployee, value);
		}

		private Counterparty _client;
		[Display(Name = "Клиент")]
		[OrderTracker1c]
		public virtual new Counterparty Client {
			get => _client;
			protected set => SetField(ref _client, value);
		}

		private DeliveryPoint _deliveryPoint;

		[Display(Name = "Точка доставки")]
		public virtual new DeliveryPoint DeliveryPoint {
			get => _deliveryPoint;
			protected set => SetField(ref _deliveryPoint, value);
		}

		private DeliverySchedule _deliverySchedule;

		[Display(Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule
		{
			get => _deliverySchedule;
			set 
			{
				SetField(ref _deliverySchedule, value);

				if(_deliverySchedule != null)
				{
					IsSecondOrderSetter();
				}
			}
		}

		private Employee loadAllowedBy;

		[Display(Name = "Отгрузку разрешил")]
		public virtual Employee LoadAllowedBy {
			get => loadAllowedBy;
			set => SetField(ref loadAllowedBy, value, () => LoadAllowedBy);
		}

		private Order previousOrder;

		[Display(Name = "Предыдущий заказ")]
		public virtual Order PreviousOrder {
			get => previousOrder;
			set => SetField(ref previousOrder, value, () => PreviousOrder);
		}

		#region OPComment

		[Display(Name = "Последний редактировал комментарий менеджера")]
		public virtual Employee CommentOPManagerChangedBy
		{
			get => _commentOPManagerChangedBy;
			set => SetField(ref _commentOPManagerChangedBy, value);
		}
		
		public virtual string LastOPCommentUpdate
		{
			get => CommentOPManagerChangedBy != null && CommentOPManagerUpdatedAt != null
				? $"Последнее изменение вносил(а) {CommentOPManagerChangedBy.FullName} в {CommentOPManagerUpdatedAt:HH:mm dd.MM.yy}"
				: string.Empty;
		}

		#endregion

		private decimal _extraMoney;

		[Display(Name = "Доплата\\Переплата")]
		[PropertyChangedAlso(nameof(OrderCashSum))]
		public virtual decimal ExtraMoney
		{
			get => _extraMoney;
			set => SetField(ref _extraMoney, value, () => ExtraMoney);
		}

		private PaymentType _paymentType;

		[Display(Name = "Форма оплаты")]
		public virtual new PaymentType PaymentType {
			get => _paymentType;
			protected set => SetField(ref _paymentType, value);
		}

		private CounterpartyContract contract;

		[Display(Name = "Договор")]
		[OrderTracker1c]
		public virtual new CounterpartyContract Contract {
			get => contract;
			set => SetField(ref contract, value, () => Contract);
		}

		[Display(Name = "Номер для связи")]
		public virtual Phone ContactPhone
		{
			get => _contactPhone;
			set => SetField(ref _contactPhone, value);
		}

		private MoneyMovementOperation moneyMovementOperation;
		[IgnoreHistoryTrace]
		public virtual MoneyMovementOperation MoneyMovementOperation {
			get => moneyMovementOperation;
			set => SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation);
		}

		private BottlesMovementOperation bottlesMovementOperation;
		[IgnoreHistoryTrace]
		public virtual BottlesMovementOperation BottlesMovementOperation {
			get => bottlesMovementOperation;
			set => SetField(ref bottlesMovementOperation, value, () => BottlesMovementOperation);
		}

		private IList<DepositOperation> depositOperations;

		public virtual IList<DepositOperation> DepositOperations {
			get => depositOperations;
			set => SetField(ref depositOperations, value, () => DepositOperations);
		}

		private NonReturnReason tareNonReturnReason;
		[Display(Name = "Причина несдачи тары")]
		public virtual NonReturnReason TareNonReturnReason {
			get => tareNonReturnReason;
			set => SetField(ref tareNonReturnReason, value, () => TareNonReturnReason);
		}

		private PaymentFrom _paymentByCardFrom;

		[Display(Name = "Место, откуда проведена оплата")]
		public virtual new PaymentFrom PaymentByCardFrom
		{
			get => _paymentByCardFrom;
			protected set => SetField(ref _paymentByCardFrom, value);
		}

		[Display(Name = "Колонка МЛ от клиента")]
		public virtual string EquipmentsFromClient {
			get {
				string result = "";
				List<OrderEquipment> equipments = OrderEquipments.Where(x => x.Direction == Direction.PickUp).ToList();
				foreach(var equip in equipments) {
					result += equip.NameString;
					result += " " + equip.Count.ToString() + "шт.";
					if(equip != equipments.Last()) {
						result += ", ";
					}
				}
				return result;
			}
		}

		[Display(Name = "Колонка МЛ к клиенту")]
		public virtual string EquipmentsToClient {
			get {
				string result = "";
				List<OrderEquipment> equipments = OrderEquipments.Where(x => x.Direction == Direction.Deliver).ToList();
				foreach(var equip in equipments) {
					result += equip.NameString;
					result += " " + equip.Count.ToString() + "шт.";
					if(equip != equipments.Last()) {
						result += ", ";
					}
				}
				return result;
			}
		}

		private Employee lastEditor;

		[Display(Name = "Последний редактор")]
		[IgnoreHistoryTrace]
		public virtual Employee LastEditor {
			get => lastEditor;
			set => SetField(ref lastEditor, value, () => LastEditor);
		}


		private ReturnTareReason returnTareReason;
		[Display(Name = "Причина забора тары")]
		public virtual ReturnTareReason ReturnTareReason {
			get => returnTareReason;
			set => SetField(ref returnTareReason, value);
		}

		private ReturnTareReasonCategory returnTareReasonCategory;
		[Display(Name = "Категория причины забора тары")]
		public virtual ReturnTareReasonCategory ReturnTareReasonCategory {
			get => returnTareReasonCategory;
			set => SetField(ref returnTareReasonCategory, value);
		}

		private LogisticsRequirements _logisticsRequirements;
		[Display(Name = "Требования к логистике")]
		public virtual LogisticsRequirements LogisticsRequirements
		{
			get => _logisticsRequirements;
			set => SetField(ref _logisticsRequirements, value);
		}

		private Organization _ourOrganization;
		[Display(Name = "Наша организация")]
		public virtual Organization OurOrganization
		{
			get => _ourOrganization;
			set => SetField(ref _ourOrganization, value);
		}

		[Display(Name = "Район города склада самовывоза")]
		public virtual new GeoGroup SelfDeliveryGeoGroup
		{
			get => _selfDeliveryGeoGroup;
			set => SetField(ref _selfDeliveryGeoGroup, value);
		}
		
		[Display(Name = "Онлайн заказ")]
		public virtual OnlineOrder OnlineOrder
		{
			get => _onlineOrder;
			set => SetField(ref _onlineOrder, value);
		}

		#endregion

		#region SecondOrderDiscount
		private void IsSecondOrderSetter()
		{
			if(!_generalSettingsParameters.GetIsClientsSecondOrderDiscountActive)
			{
				return;
			}

			var closingDocumentDeliveryScheduleId = _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId;

			if(IsFirstOrder || DeliverySchedule?.Id == closingDocumentDeliveryScheduleId)
			{
				IsSecondOrder = false;
				return;
			}

			if(Id > 0 || IsSecondOrder)
			{
				return;
			}

			var firstOrderAvailableStatuses = new OrderStatus[] { OrderStatus.Shipped, OrderStatus.UnloadingOnStock, OrderStatus.Closed };

			var firstOrder = UoW.GetAll<Order>()
				.Where(o =>
					o.Client == Client
					&& o.IsFirstOrder
					&& firstOrderAvailableStatuses.Contains(o.OrderStatus))
				.ToList();

			bool hasFirstOrder = firstOrder.Count() > 0;

			if(!hasFirstOrder)
			{
				return;
			}

			bool isFirstOrderIsClosingDocuments = false;

			if(firstOrder[0].DeliverySchedule?.Id == closingDocumentDeliveryScheduleId)
			{
				isFirstOrderIsClosingDocuments = true;
			}

			var nextOrdersAvailableStatuses = new OrderStatus[] { OrderStatus.Canceled, OrderStatus.NotDelivered };

			var nextOrders = UoW.GetAll<Order>()
				.Where(o =>
					o.Client == Client
					&& !o.IsFirstOrder
					&& o.Id != Id
					&& o.DeliverySchedule.Id != closingDocumentDeliveryScheduleId
					&& !nextOrdersAvailableStatuses.Contains(o.OrderStatus))
				.ToList();

			IsSecondOrder =
				isFirstOrderIsClosingDocuments
				? nextOrders.Count() == 1
				: nextOrders.Count() == 0;
		}

		public virtual void UpdateClientSecondOrderDiscount(IOrderDiscountsController discountsController)
		{
			if(!_generalSettingsParameters.GetIsClientsSecondOrderDiscountActive)
			{
				return;
			}

			int discountReasonId = _orderSettings.GetClientsSecondOrderDiscountReasonId;

			if(IsSecondOrder)
			{
				SetClientSecondOrderDiscount(discountsController, discountReasonId);
				return;
			}

			ResetClientSecondOrderDiscount(discountsController, discountReasonId);
		}

		private void SetClientSecondOrderDiscount(IOrderDiscountsController discountsController, int discountReasonId)
		{
			if(IsSecondOrder)
			{
				foreach(var item in ObservableOrderItems)
				{
					if(item.DiscountReason?.Id != discountReasonId)
					{
						SetClientSecondOrderDiscount(discountsController, item, discountReasonId);
					}
				}
			}
		}

		private void ResetClientSecondOrderDiscount(IOrderDiscountsController discountsController, int discountReasonId)
		{
			if(!IsSecondOrder)
			{
				var orderItemsHavingClientsSecondOrderDiscount = new List<OrderItem>();

				foreach(var item in ObservableOrderItems)
				{
					if(item.DiscountReason?.Id == discountReasonId)
					{
						orderItemsHavingClientsSecondOrderDiscount.Add(item);
					}
				}
				discountsController.RemoveDiscountFromOrder(orderItemsHavingClientsSecondOrderDiscount);
			}
		}

		private void SetClientSecondOrderDiscount(IOrderDiscountsController discountsController, OrderItem orderItem, int discountReasonId)
		{
			if(!IsSecondOrder)
			{
				return;
			}

			if(orderItem.DiscountReason != null
				|| orderItem.PromoSet != null)
			{
				return;
			}

			var discountReason = UoW.GetById<DiscountReason>(discountReasonId);

			if(discountReason != null)
			{
				discountsController.SetDiscountFromDiscountReasonForOrderItem(discountReason, orderItem, true, out string message);

				if(message != null)
				{
					ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning,
						$"Не удалось применить скидку для второго заказа клиента!");
				}
			}
		}
		#endregion

		public virtual bool CanChangeContractor()
		{
			return (!NHibernateUtil.IsInitialized(OrderDocuments) || !OrderDocuments.Any())
				&& (!NHibernateUtil.IsInitialized(InitialOrderService) || !InitialOrderService.Any())
				&& (!NHibernateUtil.IsInitialized(FinalOrderService) || !FinalOrderService.Any());
		}

		private IList<OrderDepositItem> orderDepositItems = new List<OrderDepositItem>();

		[Display(Name = "Залоги заказа")]
		public virtual new IList<OrderDepositItem> OrderDepositItems {
			get => orderDepositItems;
			set => SetField(ref orderDepositItems, value, () => OrderDepositItems);
		}

		private GenericObservableList<OrderDepositItem> observableOrderDepositItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDepositItem> ObservableOrderDepositItems {
			get {
				if(observableOrderDepositItems == null) {
					observableOrderDepositItems = new GenericObservableList<OrderDepositItem>(OrderDepositItems);
					observableOrderDepositItems.ListContentChanged += ObservableOrderDepositItems_ListContentChanged;
				}
				return observableOrderDepositItems;
			}
		}

		private IList<OrderDocument> _orderDocuments = new List<OrderDocument>();
		[Display(Name = "Документы заказа")]
		public virtual new IList<OrderDocument> OrderDocuments
		{
			get => _orderDocuments;
			set => SetField(ref _orderDocuments, value, () => OrderDocuments);
		}

		private GenericObservableList<OrderDocument> _observableOrderDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDocument> ObservableOrderDocuments => 
			_observableOrderDocuments?.ReconnectToObject(OrderDocuments) ?? (_observableOrderDocuments = new GenericObservableList<OrderDocument>(OrderDocuments));

		private IList<OrderItem> orderItems = new List<OrderItem>();

		[Display(Name = "Строки заказа")]
		[OrderTracker1c]
		public virtual new IList<OrderItem> OrderItems {
			get => orderItems;
			set => SetField(ref orderItems, value, () => OrderItems);
		}

		private GenericObservableList<OrderItem> observableOrderItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderItem> ObservableOrderItems {
			get {
				if(observableOrderItems == null) {
					observableOrderItems = new GenericObservableList<OrderItem>(orderItems);
					observableOrderItems.ListContentChanged += ObservableOrderItems_ListContentChanged;
				}

				return observableOrderItems;
			}
		}

		private IList<OrderEquipment> orderEquipments = new List<OrderEquipment>();

		[Display(Name = "Список оборудования")]
		public virtual new IList<OrderEquipment> OrderEquipments {
			get => orderEquipments;
			set => SetField(ref orderEquipments, value, () => OrderEquipments);
		}

		private GenericObservableList<OrderEquipment> observableOrderEquipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderEquipment> ObservableOrderEquipments {
			get {
				if(observableOrderEquipments == null)
					observableOrderEquipments = new GenericObservableList<OrderEquipment>(orderEquipments);
				return observableOrderEquipments;
			}
		}

		private IList<ServiceClaim> initialOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> InitialOrderService {
			get => initialOrderService;
			set => SetField(ref initialOrderService, value, () => InitialOrderService);
		}

		private GenericObservableList<ServiceClaim> observableInitialOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableInitialOrderService {
			get {
				if(observableInitialOrderService == null)
					observableInitialOrderService = new GenericObservableList<ServiceClaim>(InitialOrderService);
				return observableInitialOrderService;
			}
		}

		private IList<ServiceClaim> finalOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> FinalOrderService {
			get => finalOrderService;
			set => SetField(ref finalOrderService, value, () => FinalOrderService);
		}

		private GenericObservableList<ServiceClaim> observableFinalOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableFinalOrderService {
			get {
				if(observableFinalOrderService == null)
					observableFinalOrderService = new GenericObservableList<ServiceClaim>(FinalOrderService);
				return observableFinalOrderService;
			}
		}

		private IList<PromotionalSet> promotionalSets = new List<PromotionalSet>();
		[Display(Name = "Промонаборы заказа")]
		public virtual IList<PromotionalSet> PromotionalSets {
			get => promotionalSets;
			set => SetField(ref promotionalSets, value, () => PromotionalSets);
		}

		private GenericObservableList<PromotionalSet> observablePromotionalSets;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<PromotionalSet> ObservablePromotionalSets {
			get {
				if(observablePromotionalSets == null) {
					observablePromotionalSets = new GenericObservableList<PromotionalSet>(PromotionalSets);
					observablePromotionalSets.ElementRemoved += ObservablePromotionalSets_ElementRemoved;
				}
				return observablePromotionalSets;
			}
		}

		public virtual bool IsOrderCashlessAndPaid =>
			PaymentType == PaymentType.Cashless
			&& (OrderPaymentStatus == OrderPaymentStatus.Paid || OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid);
		
		/// <summary>
		/// Полная сумма заказа
		/// </summary>
		public override decimal OrderSum => OrderPositiveSum - OrderNegativeSum;

		/// <summary>
		/// Вся положительная сумма заказа
		/// </summary>
		public override decimal OrderPositiveSum => OrderItems.Sum(item => item.ActualSum);

		/// <summary>
		/// Вся положительная изначальная сумма заказа
		/// </summary>
		public override decimal OrderPositiveOriginalSum => OrderItems.Sum(item => item.Sum);

		/// <summary>
		/// Вся отрицательная сумма заказа
		/// </summary>
		public override decimal OrderNegativeSum => OrderDepositItems.Sum(dep => dep.ActualSum);

		public static Order CreateFromServiceClaim(ServiceClaim service, Employee author)
		{
			var order = new Order {
				_client = service.Counterparty,
				DeliveryPoint = service.DeliveryPoint,
				DeliveryDate = service.ServiceStartDate,
				PaymentType = service.Payment,
				Author = author
			};
			service.InitialOrder = order;
			order.AddServiceClaimAsInitial(service);
			return order;
		}
		
		public virtual IEnumerable<PartOrderWithGoods> OrganizationsByOrderItems { get; protected set; }

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			var uowFactory = validationContext.GetRequiredService<IUnitOfWorkFactory>();
			var deliveryRepository = validationContext.GetRequiredService<IDeliveryRepository>();
			var orderStateKey = validationContext.GetRequiredService<OrderStateKey>();
			var clientDeliveryPointsChecker = validationContext.GetRequiredService<IClientDeliveryPointsChecker>();

			if(DeliveryDate == null || DeliveryDate == default(DateTime))
				yield return new ValidationResult("В заказе не указана дата доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) });

			OrderStatus? newStatus = null;

			if(validationContext.Items.ContainsKey("NewStatus")) {
				newStatus = (OrderStatus)validationContext.Items["NewStatus"];
				if((newStatus == OrderStatus.Accepted || newStatus == OrderStatus.WaitForPayment) && Client != null)
				{
					orderStateKey.InitializeFields(this, newStatus.Value);

					var messages = new List<string>();
					if(!OrderAcceptProhibitionRulesRepository.CanAcceptOrder(orderStateKey, ref messages)) {
						foreach(var msg in messages) {
							yield return new ValidationResult(msg);
						}
					}

					if(!SelfDelivery && DeliverySchedule == null)
						yield return new ValidationResult("В заказе не указано время доставки.",
							new[] { this.GetPropertyName(o => o.DeliverySchedule) });

					if(!IsLoadedFrom1C && PaymentType == PaymentType.Cashless && Client.TypeOfOwnership != "ИП" && !SignatureType.HasValue)
						yield return new ValidationResult("В заказе не указано как будут подписаны документы.",
							new[] { this.GetPropertyName(o => o.SignatureType) });

					if(!IsLoadedFrom1C && BottlesReturn == null && this.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.water && !x.Nomenclature.IsDisposableTare)
					   && OrderAddressType != OrderAddressType.Service)
						yield return new ValidationResult("В заказе не указана планируемая тара.",
							new[] { this.GetPropertyName(o => o.Contract) });
					if(BottlesReturn.HasValue && BottlesReturn > 0 && GetTotalWater19LCount() == 0 && ReturnTareReason == null)
						yield return new ValidationResult("Необходимо указать причину забора тары.",
							new[] { nameof(ReturnTareReason) });
					if(BottlesReturn.HasValue && BottlesReturn > 0 && GetTotalWater19LCount() == 0 && ReturnTareReasonCategory == null)
						yield return new ValidationResult("Необходимо указать категорию причины забора тары.",
							new[] { nameof(ReturnTareReasonCategory) });

					if(!IsLoadedFrom1C && Trifle == null && (PaymentType == PaymentType.Cash) && this.OrderSum > 0m)
						yield return new ValidationResult("В заказе не указана сдача.",
							new[] { this.GetPropertyName(o => o.Trifle) });
					if(ObservableOrderItems.Any(x => x.Count <= 0) || ObservableOrderEquipments.Any(x => x.Count <= 0))
						yield return new ValidationResult("В заказе должно быть указано количество во всех позициях товара и оборудования");
					//если ни у точки доставки, ни у контрагента нет ни одного номера телефона
					if(!IsLoadedFrom1C && !((DeliveryPoint != null && DeliveryPoint.Phones.Any()) || Client.Phones.Any()))
						yield return new ValidationResult("Ни для контрагента, ни для точки доставки заказа не указано ни одного номера телефона.");

					if(!IsLoadedFrom1C && DeliveryPoint != null) {
						if(string.IsNullOrWhiteSpace(DeliveryPoint.Entrance)) {
							yield return new ValidationResult("Не заполнена парадная в точке доставки");
						}
						if(string.IsNullOrWhiteSpace(DeliveryPoint.Floor)) {
							yield return new ValidationResult("Не заполнен этаж в точке доставки");
						}
						if(string.IsNullOrWhiteSpace(DeliveryPoint.Room)) {
							yield return new ValidationResult("Не заполнен номер помещения в точке доставки");
						}
					}

					List<string> incorrectPriceItems = new List<string>();
					string priceResult = "В заказе неверно указаны цены на следующие товары:\n";
					
					if(!IsCopiedFromUndelivery)
					{
						OrderItemsPriceValidation(ObservableOrderItems, incorrectPriceItems);
					}
					else //если копия из недовоза сверяем цены с переносимым заказом
					{
						var currentCopiedItems = ObservableOrderItems.Where(oi => oi.CopiedFromUndelivery != null).ToArray();
					
						//сначала проверяем все позиции у которых можно менять цену из старого заказа
						CopiedOrderItemsPriceValidation(currentCopiedItems, incorrectPriceItems);

						//затем смотрим у новых добавленных, если таковые имеются
						var newAddedItems = ObservableOrderItems.Where(oi => oi.CopiedFromUndelivery == null).ToArray();

						if(newAddedItems.Any())
						{
							OrderItemsPriceValidation(newAddedItems, incorrectPriceItems);
						}
					}

					if(incorrectPriceItems.Any())
					{
						foreach(string item in incorrectPriceItems)
						{
							priceResult += item;
						}

						yield return new ValidationResult(priceResult);
					}
					// Конец проверки цен

					//создание нескольких заказов на одну дату и точку доставки

					var canCreateSeveralOrdersValidationResult = ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(validationContext);

					if(canCreateSeveralOrdersValidationResult != ValidationResult.Success)
					{
						yield return canCreateSeveralOrdersValidationResult;
					}

					if(Client.IsDeliveriesClosed
						&& PaymentType != PaymentType.Cash
						&& PaymentType != PaymentType.PaidOnline
						&& PaymentType != PaymentType.Terminal
						&& PaymentType != PaymentType.DriverApplicationQR
						&& PaymentType != PaymentType.SmsQR)
						yield return new ValidationResult(
							"В заказе неверно указан тип оплаты (для данного клиента закрыты поставки)",
							new[] { nameof(PaymentType) }
						);

					//FIXME Исправить изменение данных. В валидации нельзя менять объекты.
					if(DeliveryPoint != null && !DeliveryPoint.FindAndAssociateDistrict(UoW, deliveryRepository))
						yield return new ValidationResult(
							"Район доставки не найден. Укажите правильные координаты или разметьте район доставки.",
							new[] { this.GetPropertyName(o => o.DeliveryPoint) }
					);

					if(Client.DoNotMixMarkedAndUnmarkedGoodsInOrder && HasMarkedAndUnmarkedOrderItems())
					{
						var doNotMixMarkedAndUnmarkedGoodsInOrderName =
							Client.GetPropertyInfo(c => c.DoNotMixMarkedAndUnmarkedGoodsInOrder)
							.GetCustomAttribute<DisplayAttribute>(true).Name;

						yield return new ValidationResult(
							$"У клиента стоит признак \"{doNotMixMarkedAndUnmarkedGoodsInOrderName}\"",
							new[] { nameof(OrderItems) });
					}
					
					if(OrderItems.Where(x => x.Nomenclature.IsArchive) is IEnumerable<OrderItem> archivedNomenclatures && archivedNomenclatures.Any())
					{
						yield return new ValidationResult($"В заказе присутствуют архивные номенклатуры: " +
														$"{string.Join(", ", archivedNomenclatures.Select(x => $"№{x.Nomenclature.Id} { x.Nomenclature.Name}"))}.",
							new[] { nameof(Nomenclature) });
					}

					if(Client != null)
					{
						foreach(var email in Client.Emails)
						{
							if(!email.IsValidEmail)
							{
								yield return new ValidationResult($"Адрес электронной почты клиента {email.Address} имеет неправильный формат.");
							}
						}

						foreach(var phone in Client.Phones)
						{
							if(!phone.IsValidPhoneNumber)
							{
								yield return new ValidationResult($"Номер телефона клиента {phone.Number} имеет неправильный формат.");
							}
						}
					}

					if(DeliveryPoint != null)
					{
						foreach(var phone in DeliveryPoint.Phones)
						{
							if(!phone.IsValidPhoneNumber)
							{
								yield return new ValidationResult($"Номер телефона точки доставки {phone.Number} имеет неправильный формат.");
							}
						}
					}
				}

				if(newStatus == OrderStatus.Closed) {
					foreach(var equipment in OrderEquipments.Where(x => x.Direction == Direction.PickUp)) {
						if(!equipment.Confirmed && string.IsNullOrWhiteSpace(equipment.ConfirmedComment))
							yield return new ValidationResult(
								string.Format("Забор оборудования {0} по заказу {1} не произведен, а в комментарии не указана причина.",
									equipment.NameString, Id),
								new[] { this.GetPropertyName(o => o.OrderEquipments) });
					}
				}

				if(OrderAddressType == OrderAddressType.Service && PaymentType == PaymentType.Cashless
				   && newStatus == OrderStatus.Accepted
				   && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_accept_cashles_service_orders"))
				   {
					yield return new ValidationResult(
						"Недостаточно прав для подтверждения безнального сервисного заказа. Обратитесь к руководителю.",
						new[] { this.GetPropertyName(o => o.OrderStatus) }
					);
				}

				if(IsContractCloser && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer"))
				{
					yield return new ValidationResult(
						"Недостаточно прав для подтверждения зыкрывашки по контракту. Обратитесь к руководителю.",
						new[] { this.GetPropertyName(o => o.IsContractCloser) }
					);
				}
			}

			var isCashOrderClose = validationContext.Items.ContainsKey("cash_order_close") && (bool)validationContext.Items["cash_order_close"];
			var isTransferedAddress = validationContext.Items.ContainsKey("AddressStatus") && (RouteListItemStatus)validationContext.Items["AddressStatus"] == RouteListItemStatus.Transfered;
			var isCancellingOrder = newStatus.HasValue && newStatus.Value.IsIn(_orderRepository.GetUndeliveryStatuses());

			if(isCashOrderClose
				&& !isTransferedAddress
				&& PaymentTypesNeededOnlineOrder.Contains(PaymentType)
				&& OnlinePaymentNumber == null
				&& !_orderRepository.GetUndeliveryStatuses().Contains(OrderStatus)				)
			{
				yield return new ValidationResult($"В заказе №{Id} с оплатой по \"{PaymentType.GetEnumDisplayName(true)}\"  отсутствует номер оплаты.");
			}

			if (ObservableOrderItems.Any(x => x.Discount > 0 && x.DiscountReason == null && x.PromoSet == null))
				yield return new ValidationResult("Если в заказе указана скидка на товар, то обязательно должно быть заполнено поле 'Основание'.");

			if(!SelfDelivery && DeliveryPoint == null)
				yield return new ValidationResult("В заказе необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryPoint) });
			if(DeliveryPoint != null && (!DeliveryPoint.Latitude.HasValue || !DeliveryPoint.Longitude.HasValue))
			{
				yield return new ValidationResult($"В точке доставки №{DeliveryPoint.Id} {DeliveryPoint.ShortAddress} необходимо указать координаты.",
				new[] { nameof(DeliveryPoint) });
			}

			if(DriverCallId != null && string.IsNullOrWhiteSpace(CommentManager)){
				yield return new ValidationResult("Необходимо заполнить комментарий менеджера водительского телефона.",
					new[] { this.GetPropertyName(o => o.CommentManager) });
			}

			if (Client == null)
				yield return new ValidationResult("В заказе необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName(o => o.Client) });

			if(PaymentType == PaymentType.PaidOnline && OnlinePaymentNumber == null)
				yield return new ValidationResult("Если в заказе выбран тип оплаты по карте, необходимо заполнить номер онлайн заказа.",
												  new[] { this.GetPropertyName(o => o.OnlinePaymentNumber) });

			if(PaymentType == PaymentType.PaidOnline && PaymentByCardFrom == null)
				yield return new ValidationResult(
					"Выбран тип оплаты по карте. Необходимо указать откуда произведена оплата.",
					new[] { this.GetPropertyName(o => o.PaymentByCardFrom) }
				);

			if(
				ObservableOrderEquipments
			   .Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
			   .Any(x => x.OwnType == OwnTypes.None)
			  )
				yield return new ValidationResult("У оборудования в заказе должна быть выбрана принадлежность.");

			if(
				ObservableOrderEquipments
			   .Where(x => x.Nomenclature.Category == NomenclatureCategory.equipment)
			   .Where(x => x.DirectionReason == DirectionReason.None && x.OwnType != OwnTypes.Duty)
			   .Any(x => x.Nomenclature?.SaleCategory != SaleCategory.forSale)
			  )
				yield return new ValidationResult("У оборудования в заказе должна быть указана причина забор-доставки.");

			if(ObservableOrderDepositItems.Any(x => x.ActualSum < 0)) {
				yield return new ValidationResult("В возврате залогов в заказе необходимо вводить положительную сумму.");
			}

			if(!_canCreateOrderInAdvance.HasValue)
			{
				_canCreateOrderInAdvance =
					ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_can_create_order_in_advance");
			}
			if(!_canCreateOrderInAdvance.Value
			   && DeliveryDate.HasValue && DeliveryDate.Value < DateTime.Today
			   && OrderStatus <= OrderStatus.Accepted) {
				yield return new ValidationResult(
					"Указана дата заказа более ранняя чем сегодняшняя. Укажите правильную дату доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) }
				);
			}

			if(DeliveryDate > DateTime.Now.AddDays(_futureDeliveryDaysLimit))
			{
				yield return new ValidationResult(
					$"Дата доставки заказа должна быть не более {_futureDeliveryDaysLimit} дней вперед. Измените, пожалуйста, дату доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) }
				);
			}

			if(SelfDelivery && PaymentType == PaymentType.ContractDocumentation) {
				yield return new ValidationResult(
					"Тип оплаты - контрактная документация невозможен для самовывоза",
					new[] { this.GetPropertyName(o => o.PaymentType) }
				);
			}

			if(SelfDelivery && PaymentType == PaymentType.DriverApplicationQR)
			{
				yield return new ValidationResult(
					"Тип оплаты - Qr-код МП водителя невозможен для самовывоза",
					new[] { this.GetPropertyName(o => o.PaymentType) }
				);
			}

			if(SelfDelivery && SelfDeliveryGeoGroup == null)
			{
				yield return new ValidationResult(
					"Для заказов с самовывозом обязательно указание района города",
					new[] { this.GetPropertyName(o => o.SelfDeliveryGeoGroup) }
				);
			}

			if(!PaymentTypesFastDeliveryAvailableFor.Contains(PaymentType) && IsFastDelivery)
			{
				yield return new ValidationResult(
					$"Доставку за час можно выбрать только для заказа с формой оплаты из списка: {string.Join(", ", PaymentTypesFastDeliveryAvailableFor)}",
					new[] { nameof(PaymentType) });
			}

			var deliveryParameters = validationContext.GetService<IDeliveryRulesSettings>();
			var isFastDeliverySchedule = DeliverySchedule?.Id == deliveryParameters.FastDeliveryScheduleId;
			if(IsFastDelivery != isFastDeliverySchedule)
			{
				yield return new ValidationResult(
					"Свойство заказа 'Доставка за час' должно совпадать с графиком доставки заказа 'Доставка за час'",
					new[] { nameof(PaymentType) });
			}
			if(IsFastDelivery && (DeliveryPoint == null || SelfDelivery))
			{
				yield return new ValidationResult("Нельзя выбрать доставку за час для заказа-самовывоза", new[] { nameof(DeliveryPoint) });
			}

			var orderSettings = validationContext.GetService(typeof(IOrderSettings)) as IOrderSettings;

			if(SelfDelivery && PaymentType == PaymentType.PaidOnline && PaymentByCardFrom != null && OnlinePaymentNumber == null)
			{
				if(orderSettings == null)
				{
					throw new ArgumentNullException(nameof(IOrderSettings));
				}
				if(PaymentByCardFrom.Id == orderSettings.PaymentFromTerminalId)
				{
					yield return new ValidationResult($"В заказe №{Id} с формой оплаты По карте и источником оплаты Терминал отсутствует номер оплаты.");
				}
			}

			if((new[] { PaymentType.Cash, PaymentType.Terminal }.Contains(PaymentType)
				   || (PaymentType == PaymentType.PaidOnline
					   && PaymentByCardFrom != null
					   && !(orderSettings ?? throw new ArgumentNullException(nameof(IOrderSettings)))
						   .PaymentsByCardFromNotToSendSalesReceipts.Contains(PaymentByCardFrom.Id)))
			   && Contract?.Organization != null && Contract.Organization.CashBoxId == null)
			{
				yield return new ValidationResult(
					"Невозможно сохранить заказ.\n" +
					$"К нашей организации '{Contract.Organization.Name}' не привязан кассовый аппарат.\n" +
					"Измените в заказе либо форму оплаты, либо нашу организацию",
					new[] { nameof(Contract.Organization) });
			}

			if (OrderItems.Any(oi => !string.IsNullOrWhiteSpace(oi.Nomenclature.OnlineStoreExternalId))
				&& EShopOrder == null)
			{
				yield return new ValidationResult(
					"В заказе есть товары ИМ, но не указан номер заказа ИМ",
					new[] { this.GetPropertyName(o => o.EShopOrder) }
				);
			}

			if (DeliveryPoint != null)
			{
				if (DeliveryPoint.MinimalOrderSumLimit != 0 && OrderSum < DeliveryPoint.MinimalOrderSumLimit)
				{
					yield return new ValidationResult(
						"Сумма заказа меньше минимальной погоровой установленной для точки доставки",
						new[] { this.GetPropertyName(o => o.OrderSum) }
					);
				}

				if (DeliveryPoint.MaximalOrderSumLimit != 0 && OrderSum > DeliveryPoint.MaximalOrderSumLimit)
				{
					yield return new ValidationResult(
						"Сумма заказа больше максимальной погоровой установленной для точки доставки",
						new[] { this.GetPropertyName(o => o.OrderSum) }
					);
				}
			}

			if(ContactPhone != null && ContactPhone.Counterparty?.Id != Client?.Id && ContactPhone.DeliveryPoint?.Id != DeliveryPoint?.Id)
			{
				yield return new ValidationResult($"Номер для связи с Id {ContactPhone.Id} : {ContactPhone.Number} не найден в списке телефонных номеров ни контрагента, ни точки доставки.",
					new[] { nameof(ContactPhone) });
			}

			if(OPComment?.Length > 255)
			{
				yield return new ValidationResult($"Значение поля \"Комментарий ОП/ОСК\" не должно превышать 255 символов",
					new[] { nameof(OPComment) });
			}

			if(ODZComment?.Length > 255)
			{
				yield return new ValidationResult($"Значение поля \"Комментарий ОДЗ\" не должно превышать 255 символов",
					new[] { nameof(OPComment) });
			}

			if(!SelfDelivery && !IsFastDelivery && CallBeforeArrivalMinutes == null && (IsDoNotMakeCallBeforeArrival is null || IsDoNotMakeCallBeforeArrival == false))
			{
				yield return new ValidationResult($"В заказе не заполнено поле \"Отзвон за\"",
					new[] { nameof(CallBeforeArrivalMinutes) });
			}

			#region Валидация, если уже есть чек

			var hasReceipts = _orderRepository.OrderHasSentReceipt(UoW, Id);

			validationContext.Items.TryGetValue(ValidationKeyIgnoreReceipts, out var ignoreReceipts);

			if(!((bool?)ignoreReceipts ?? false)
				&& hasReceipts)
			{
				var incorrectReceiptItems = new List<string>();

				using(var uow = uowFactory.CreateWithoutRoot("Валидация заказа, если уже есть чек"))
				{
					if(uow.GetById<Order>(Id) is Order oldOrder)
					{
						incorrectReceiptItems = ValidateChangesInOrderWithReceipt(uow, oldOrder);
					}
				}

				if(incorrectReceiptItems.Any())
				{
					yield return new ValidationResult(string.Join("\n", incorrectReceiptItems));
				}
			}

			#endregion

			#region Проверка соответствия точки доставки выбранному контрагенту

			if(Client != null && DeliveryPoint != null)
			{
				if(!clientDeliveryPointsChecker.ClientDeliveryPointExists(Client.Id, DeliveryPoint.Id))
				{
					yield return new ValidationResult("Среди точек доставок выбранного контрагента указанная точка доставки не найдена",
						new[] { nameof(DeliveryPoint) });
				}
			}

			#endregion

			#region Проверка кол-ва бутылей по акции Приведи друга

			// Отменять заказ с акцией можно
			if((newStatus == null || !isCancellingOrder)
				&& OrderItems.Where(oi => oi.DiscountReason?.Id == _orderSettings.ReferFriendDiscountReasonId).Sum(oi => oi.CurrentCount) is decimal referPromoBottlesInOrderCount
				&& referPromoBottlesInOrderCount > 0)
			{
				var referredCounterparties = _orderRepository.GetReferredCounterpartiesCountByReferPromotion(UoW, Client.Id);
				var alreadyReceivedBottles = _orderRepository.GetAlreadyReceivedBottlesCountByReferPromotion(UoW, this, _orderSettings.ReferFriendDiscountReasonId);
				var maxReferPromoBottles = referredCounterparties - alreadyReceivedBottles;

				if(referPromoBottlesInOrderCount > maxReferPromoBottles)
				{
					yield return new ValidationResult($"Для данного КА по акции приведи друга заработано {referredCounterparties} бесплатных бутылей\n" +
						$"Ранее отвезено данному КА {alreadyReceivedBottles} бесплатных бутылей\n" +
						$"В заказе можно указать не более {maxReferPromoBottles} бесплатных бутылей",
						new[] { nameof(OrderItem) });
				}
			}

			#endregion

			if(DeliverySchedule?.Id == _deliveryScheduleSettings.ClosingDocumentDeliveryScheduleId)
			{
				var orderItemsTrueMarkCodesMustBeAdded = OrderItems.Where(x => x.IsTrueMarkCodesMustBeAdded).ToList();

				if(orderItemsTrueMarkCodesMustBeAdded.Any())
				{
					yield return new ValidationResult($"В заказе с \"Закр док\" не должно быть товаров с маркировкой ЧЗ. " +
						$"В заказ были добавлены следующие маркированные товары: {string.Join(", ", orderItemsTrueMarkCodesMustBeAdded.Select(x => x.Nomenclature.Name))}",
						new[] { nameof(OrderItems) });
				}
			}

			#region Отмена заказа с кодами маркировки

			if(isCancellingOrder && hasReceipts)
			{
				yield return new ValidationResult($"По данному заказу уже оформлен и отправлен чек клиенту и отменить его нельзя");
			}

			#endregion Отмена заказа с кодами маркировки
		}

		private void CopiedOrderItemsPriceValidation(OrderItem[] currentCopiedItems, List<string> incorrectPriceItems)
		{
			for(var i = 0; i < currentCopiedItems.Length; i++)
			{
				var currentItem = currentCopiedItems[i];
				var copiedItem = UoW.GetById<OrderItem>(currentItem.CopiedFromUndelivery.Id);
				
				if(currentItem.Price < copiedItem.Price)
				{
					incorrectPriceItems.Add(
						$"{currentItem.NomenclatureString} - цена: {currentItem.Price}, должна быть: {copiedItem.Price}\n");
				}
			}
		}

		private void OrderItemsPriceValidation(IEnumerable<OrderItem> validatedOrderItems, IList<string> incorrectPriceItems)
		{
			// Проверка соответствия цен в заказе ценам в номенклатуре
			foreach(var item in validatedOrderItems)
			{
				decimal fixedPrice = GetFixedPrice(item);
				decimal nomenclaturePrice = GetNomenclaturePrice(item, false);
				var alternativeNomenclaturePrice = GetNomenclaturePrice(item, true);
				var isMasterCallNomenclature = item.Nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId;

				var conditionForNomenclaturePrice = nomenclaturePrice > default(decimal)
						  && item.Price < nomenclaturePrice
						  && (alternativeNomenclaturePrice == default(decimal)
							  || item.Price < alternativeNomenclaturePrice)
						  && !isMasterCallNomenclature;

				var conditionForAlternativeNomenclaturePrice = alternativeNomenclaturePrice > default(decimal)
				          && item.Price < alternativeNomenclaturePrice
						  && (nomenclaturePrice == default(decimal)
						      || item.Price < nomenclaturePrice)
						  && !isMasterCallNomenclature;

				if(fixedPrice > 0m)
				{
					if(item.Price < fixedPrice)
					{
						incorrectPriceItems.Add(string.Format("{0} - цена: {1}, должна быть: {2}\n",
							item.NomenclatureString,
							item.Price,
							fixedPrice));
					}
				}
				else if(conditionForNomenclaturePrice || conditionForAlternativeNomenclaturePrice)
				{
					incorrectPriceItems.Add(
							$"{item.NomenclatureString} - цена: {item.Price}, должна быть: {nomenclaturePrice}, либо {alternativeNomenclaturePrice}");
				}
			}
		}

		private List<string> ValidateChangesInOrderWithReceipt(IUnitOfWork uow, Order oldOrder)
		{
			List<string> incorrectReceiptItems = new List<string>();

			var oldOrderItems = _orderRepository.GetIsAccountableInTrueMarkOrderItems(uow, Id).GroupBy(x => x.Nomenclature.Id).ToArray();

			if(oldOrder.Client.Id != Client.Id)
			{
				incorrectReceiptItems.Add($"Нельзя менять клиента у заказа, по которому сформирован чек.");
			}

			var newOrderItems = OrderItems
				.Where(x => x.Nomenclature.IsAccountableInTrueMark)
				.GroupBy(x => x.Nomenclature.Id)
				.ToArray();

			var missingInNewOrderIds = oldOrderItems
				.Select(x => x.Key)
				.Except(newOrderItems.Select(x => x.Key))
				.ToArray();

			if(missingInNewOrderIds.Any())
			{
				var missingNames = oldOrderItems
					.Where(x => missingInNewOrderIds.Contains(x.Key))
					.Select(x => x.First().Nomenclature.Name);

				incorrectReceiptItems.Add($"Нельзя удалять номенклатуры, по которым сформирован чек: {string.Join(", ", missingNames)}.");
			}

			var missingInOldOrderIds = newOrderItems
				.Select(x => x.Key)
				.Except(oldOrderItems.Select(x => x.Key))
				.ToArray();

			if(missingInOldOrderIds.Any())
			{
				var newNames = newOrderItems
					.Where(x => missingInOldOrderIds.Contains(x.Key))
					.Select(x => x.First().Nomenclature.Name);

				incorrectReceiptItems.Add($"Нельзя добавлять новые номенклатуры в заказ, по которому сформирован чек: {string.Join(", ", newNames)}.");
			}

			foreach(var oldItem in oldOrderItems)
			{
				var newItem = newOrderItems
					.Where(x => x.Key == oldItem.Key)
					.ToArray();

				if(!newItem.Any())
				{
					continue;
				}

				var oldCount = oldItem.Sum(x => x.Count);
				var newCount = newItem.First().Sum(x => x.Count);

				if(oldCount != newCount)
				{
					incorrectReceiptItems.Add($"Нельзя менять кол-во номенклатуры {newItem.First().First().Nomenclature.Name}, " +
					                          $"по которой сформирован чек (было {oldCount:F0} стало {newCount:F0}).");
				}
			}

			return incorrectReceiptItems;
		}

		public static string ValidationKeyIgnoreReceipts => nameof(ValidationKeyIgnoreReceipts);

		#endregion IValidatableObject implementation

		#region Вычисляемые

		public virtual string Title => string.Format("Заказ №{0} от {1:d}", Id, DeliveryDate);

		public virtual int Total19LBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol19L).Sum(x => x.Count);

		public virtual int Total6LBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol6L).Sum(x => x.Count);

		public virtual int Total1500mlBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol1500ml).Sum(x => x.Count);

		public virtual int Total600mlBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol600ml).Sum(x => x.Count);

		public virtual int Total500mlBottlesToDeliver =>
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
									   x.Nomenclature.TareVolume == TareVolume.Vol500ml).Sum(x => x.Count);

		public virtual int TotalWeight =>
			(int)OrderItems.Sum(x => x.Count * (decimal) x.Nomenclature.Weight);

		public virtual decimal TotalVolume =>
			OrderItems.Sum(x => x.Count * (decimal) x.Nomenclature.Volume);

		public virtual decimal DepositsSum => OrderDepositItems.Sum(x => x.ActualSum);


		[Display(Name = "Наличных к получению")]
		public virtual decimal OrderCashSum
		{
			get => PaymentType == PaymentType.Cash ? OrderSum : 0;
			protected set {; }
		}

		public virtual decimal BottleDepositSum => ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Bottles).Sum(x => x.ActualSum);
		public virtual decimal EquipmentDepositSum => ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Equipment).Sum(x => x.ActualSum);


		public virtual decimal? ActualGoodsTotalSum =>
			OrderItems.Sum(item => Decimal.Round(item.Price * item.ActualCount - item.DiscountMoney ?? 0, 2));

		public virtual bool CanBeMovedFromClosedToAcepted =>
			_routeListItemRepository.WasOrderInAnyRouteList(UoW, this)
				&& ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_move_order_from_closed_to_acepted");

		public virtual bool HasItemsNeededToLoad => ObservableOrderItems.Any(orderItem =>
				!Nomenclature.GetCategoriesNotNeededToLoad().Contains(orderItem.Nomenclature.Category) && !orderItem.Nomenclature.NoDelivery)
			|| ObservableOrderEquipments.Any(orderEquipment =>
				!Nomenclature.GetCategoriesNotNeededToLoad().Contains(orderEquipment.Nomenclature.Category) && !orderEquipment.Nomenclature.NoDelivery);

		public virtual bool IsCashlessPaymentTypeAndOrganizationWithoutVAT => PaymentType == PaymentType.Cashless
			&& Contract?.Organization?.GetActualVatRateVersion(DeliveryDate)?.VatRate.VatRateValue == 0;

		public virtual void RefreshContactPhone()
		{
			if(ContactPhone?.Counterparty?.Id != Client?.Id && ContactPhone?.DeliveryPoint?.Id != DeliveryPoint?.Id)
			{
				ContactPhone = null;
			}
		}

		public virtual bool HasPermissionsForAlternativePrice => Author?.Subdivision?.Id != null && _generalSettingsParameters.SubdivisionsForAlternativePrices.Contains(Author.Subdivision.Id);

		public virtual bool IsSmallBottlesAddedToOrder =>
			Total500mlBottlesToDeliver > 10
			|| Total1500mlBottlesToDeliver > 4
			|| Total6LBottlesToDeliver > 2;

		public virtual bool IsCoolerAddedToOrder => 
			OrderItems.Where(x => x.Nomenclature.Kind != null).Select(x => x.Nomenclature)
			.Concat(OrderEquipments.Where(x => x.Nomenclature.Kind != null).Select(x => x.Nomenclature))
			.Where(x => _nomenclatureSettings.EquipmentKindsHavingGlassHolder.Any(n => n == x.Kind.Id))
			.Count() > 0;

		public virtual bool IsOrderContainsIsAccountableInTrueMarkItems =>
			ObservableOrderItems.Any(x =>
			x.Nomenclature.IsAccountableInTrueMark && x.Nomenclature.Gtins.Any() && x.Count > 0);

		/// <summary>
		/// Проверка, является ли целью покупки заказа - для перепродажи
		/// </summary>
		public virtual bool IsOrderForResale =>
			Client?.ReasonForLeaving == ReasonForLeaving.Resale;
		
		/// <summary>
		/// Проверка, является ли целью покупки заказа - госзакупки
		/// </summary>
		public virtual bool IsOrderForTender =>
			Client?.ReasonForLeaving == ReasonForLeaving.Tender;
		
		/// <summary>
		/// Проверка на госзаказ
		/// и нужно ли собирать данный заказ отдельно при отгрузке со склада
		/// (сканировать марки на складе для отправки документов в статусе заказа "В Пути")
		/// </summary>
		public virtual bool IsNeedIndividualSetOnLoadForTender =>
			IsOrderForTender
			&& Client?.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute
			&& PaymentType == PaymentType.Cashless;

		public virtual string OrderDocumentStringNumber(DocumentContainerType documentContainerType)
		{
			if(DeliveryDate.Value.Year < 2026)
			{
				return Id.ToString();
			}

			var documentTypes = documentContainerType == DocumentContainerType.Upd
				? new[] { OrderDocumentType.UPD, OrderDocumentType.SpecialUPD }
				: new[] { OrderDocumentType.Bill, OrderDocumentType.SpecialBill };

			var document = OrderDocuments
				.FirstOrDefault(x => documentTypes.Contains(x.Type) && x.Order.Id == Id);

			return document.DocumentOrganizationCounter?.DocumentNumber ?? Id.ToString();
		}

		#endregion

		#region Автосоздание договоров, при изменении подтвержденного заказа

		private void OnChangeCounterparty(IUnitOfWork uow, IOrderContractUpdater contractUpdater, Counterparty newClient)
		{
			if(newClient == null || Client == null || newClient.Id == Client.Id) {
				return;
			}
			
			contractUpdater.UpdateContract(uow, this);
		}

		private void UpdateContractOnPaymentTypeChanged(IUnitOfWork uow, IOrderContractUpdater contractUpdater)
		{
			contractUpdater.UpdateContract(uow, this, true);
		}

		public virtual void UpdateContractDocument()
		{
			var contractDocuments = OrderDocuments.Where(x =>
				x.Type == OrderDocumentType.Contract && x.Order == this && x.AttachedToOrder == this);
			
			if(!contractDocuments.Any()) {
				return;
			}

			foreach(var contractDocument in contractDocuments.ToList())
			{
				if(contractDocument is OrderContract orderContract)
				{
					if(orderContract.Contract == Contract)
					{
						continue;
					}
				}

				ObservableOrderDocuments.Remove(contractDocument);
			}

			AddContractDocument(Contract);
		}

		#endregion

		#region Добавление/удаление товаров
		
		public virtual void UpdateDeliveryItem(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature,
			decimal price)
		{
			//Т.к. запускается пересчет различных параметров, который может привести к добавлению платной доставки
			//создание строки с платной доставкой лучше запускать до ее поиска в коллекции
			var newDeliveryItem = OrderItem.CreateDeliveryOrderItem(this, nomenclature, price);
			var currentDeliveryItem = ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == PaidDeliveryNomenclatureId);

			if(price > 0)
			{
				AddOrUpdateDeliveryItem(uow, contractUpdater, currentDeliveryItem, newDeliveryItem, price);
				return;
			}
			
			if(currentDeliveryItem != null)
			{
				RemoveOrderItem(uow, contractUpdater, currentDeliveryItem);
			}
		}

		public virtual void AddOrderItem(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			OrderItem orderItem,
			bool forceUseAlternativePrice = false)
		{
			if(ObservableOrderItems.Contains(orderItem)) {
				return;
			}

			var curCount = orderItem.Nomenclature.IsWater19L
				? GetTotalWater19LCount(true, true)
				: orderItem.Count;
			
			var isAlternativePriceCopiedFromUndelivery = orderItem.CopiedFromUndelivery != null && orderItem.IsAlternativePrice;
			var canApplyAlternativePrice =
				isAlternativePriceCopiedFromUndelivery
					|| (HasPermissionsForAlternativePrice
						&& orderItem.Nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= curCount)
						&& orderItem.GetWaterFixedPrice() == null);

			orderItem.IsAlternativePrice = canApplyAlternativePrice;

			ObservableOrderItems.Add(orderItem);
			Recalculate();
			contractUpdater.UpdateContract(uow, this);

			if(orderItems.Any(x => x.Nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId))
			{
				_nomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, this);
			}
		}

		public virtual void RemoveOrderItem(IUnitOfWork uow, IOrderContractUpdater contractUpdater, OrderItem orderItem)
		{
			if(!ObservableOrderItems.Contains(orderItem)) {
				return;
			}

			if (orderItem.PromoSet != null)
			{
				var itemsToRemove = ObservableOrderItems.Where(oi => oi.PromoSet == orderItem.PromoSet).ToList();
				foreach (var item in itemsToRemove)
				{
					ObservableOrderItems.Remove(item);
				}
			}
			else
			{
				ObservableOrderItems.Remove(orderItem);
			}

			//Если была удалена последняя номенклатура "мастер" - переходит в стандартный тип адреса
			if(OrderItems.All(x => !(x.IsMasterNomenclature && x.Nomenclature.Id != _nomenclatureSettings.MasterCallNomenclatureId))
				&& orderItem.IsMasterNomenclature
				&& orderItem.Nomenclature.Id != _nomenclatureSettings.MasterCallNomenclatureId)
			{
				OrderAddressType = OrderAddressType.Delivery;
			}

			contractUpdater.UpdateContract(uow, this);
		}

		public virtual void SetOrderItemCount(OrderItem orderItem, decimal newCount)
		{
			orderItem?.SetCount(newCount);
		}
		
		private void AddOrUpdateDeliveryItem(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			OrderItem currentDeliveryItem,
			OrderItem newDeliveryItem,
			decimal price)
		{
			if(currentDeliveryItem is null)
			{
				AddOrderItem(uow, contractUpdater, newDeliveryItem);
				return;
			}

			if(currentDeliveryItem.Price == price)
			{
				return;
			}

			currentDeliveryItem.SetPrice(price);
		}

		#endregion

		#region Функции
		
		public virtual void UpdatePaymentByCardFrom(
			PaymentFrom paymentByCardFrom,
			IOrderContractUpdater orderContractUpdater,
			bool needUpdateContract = true)
		{
			if(_paymentByCardFrom == paymentByCardFrom)
			{
				return;
			}

			PaymentByCardFrom = paymentByCardFrom;

			if(needUpdateContract)
			{
				orderContractUpdater.UpdateContract(UoW, this);
			}
		}

		public virtual void UpdatePaymentType(
			PaymentType paymentType,
			IOrderContractUpdater orderContractUpdater,
			bool needUpdateContract = true)
		{
			if(paymentType == _paymentType)
			{
				return;
			}

			PaymentType = paymentType;
				
			if(PaymentType != PaymentType.PaidOnline)
			{
				PaymentByCardFrom = null;
			}

			if(PaymentType != PaymentType.Terminal)
			{
				PaymentByTerminalSource = null;
			}

			if(PaymentType == PaymentType.Terminal && PaymentByTerminalSource == null)
			{
				PaymentByTerminalSource = Domain.Client.PaymentByTerminalSource.ByCard;
			}

			if(needUpdateContract)
			{
				UpdateContractOnPaymentTypeChanged(UoW, orderContractUpdater);
			}
		}

		public virtual void UpdateClient(Counterparty counterparty, IOrderContractUpdater orderContractUpdater, out string message)
		{
			message = string.Empty;
			
			if(counterparty == _client)
			{
				return;
			}

			if(_orderRepository.GetOnClosingOrderStatuses().Contains(OrderStatus))
			{
				OnChangeCounterparty(UoW, orderContractUpdater, counterparty);
			}
			else if(_client != null && !CanChangeContractor())
			{
				OnPropertyChanged(nameof(Client));
				message = "Нельзя изменить клиента для заполненного заказа.";
				return;
			}
			
			var oldClient = _client;

			if(oldClient != counterparty)
			{
				Client = counterparty;
				
				if(Client == null
					|| (DeliveryPoint != null && NHibernateUtil.IsInitialized(Client.DeliveryPoints)
						&& !Client.DeliveryPoints.Any(d => d.Id == DeliveryPoint.Id)))
				{
					//FIXME Убрать когда поймем что проблемы с пропаданием точек доставки нет.
					logger.Warn("Очищаем точку доставки, при установке клиента. Возможно это не нужно.");
					DeliveryPoint = null;
				}
				
				if(oldClient != null)
				{
					orderContractUpdater.UpdateContract(UoW, this);
				}

				RefreshContactPhone();
				IsSecondOrderSetter();
			}
		}
		
		public virtual void UpdateDeliveryPoint(DeliveryPoint deliveryPoint, IOrderContractUpdater orderContractUpdater)
		{
			int? oldDeliveryPointId = _deliveryPoint?.Id;
			
			DeliveryPoint = deliveryPoint;

			if(deliveryPoint == null || deliveryPoint.Id == oldDeliveryPointId)
			{
				return;
			}

			if(DeliverySchedule is null)
			{
				DeliverySchedule = DeliveryPoint.DeliverySchedule;
			}

			if(Id == 0)
			{
				AddCertificates = DeliveryPoint.Category?.Id == EducationalInstitutionDeliveryPointCategoryId
					&& (DeliveryPoint.AddCertificatesAlways || Client.FirstOrder == null);
			}

			if(oldDeliveryPointId.HasValue)
			{
				orderContractUpdater.UpdateContract(UoW, this);
				RefreshContactPhone();
			}

			if(orderItems.Any(x => x.Nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId))
			{
				_nomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, this);
			}
		}
		
		public virtual void UpdateDeliveryDate(DateTime? deliveryDate, IOrderContractUpdater orderContractUpdater, out string message)
		{
			var lastDate = _deliveryDate;
			message = string.Empty;
			
			DeliveryDate = deliveryDate;
			
			if(lastDate != _deliveryDate && Contract != null && Contract.Id == 0)
			{
				orderContractUpdater.UpdateContract(UoW, this);
			}
			
			if(Contract != null 
				&& Contract.Id != 0 
				&& DeliveryDate.HasValue
				&& lastDate == Contract.IssueDate
				&& Contract.IssueDate != DeliveryDate.Value
				&& _orderRepository.CanChangeContractDate(UoW, Client, DeliveryDate.Value, Id)
				&& OrderStatus != OrderStatus.Closed)
			{
				Contract.IssueDate = DeliveryDate.Value.Date;
				message = "Дата договора будет изменена при сохранении текущего заказа!";
			}

			if(orderItems.Any(x => x.Nomenclature.Id == _nomenclatureSettings.MasterCallNomenclatureId))
			{
				_nomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, this);
			}
		}
		
		/// <summary>
		/// Проверка, является ли клиент по заказу сетевым покупателем
		/// и нужно ли собирать данный заказ отдельно при отгрузке со склада
		/// </summary>
		public virtual bool IsNeedIndividualSetOnLoad(ICounterpartyEdoAccountController edoAccountController)
		{
			if(Client is null)
			{
				return false;
			}
			
			var edoAccount = edoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(Client, Contract?.Organization?.Id);
			
			return PaymentType == PaymentType.Cashless
				&& Client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute
				&& edoAccount.ConsentForEdoStatus == ConsentForEdoStatus.Agree;
		}

		/// <summary>
		/// Документооборот по ЭДО с клиентом по заказу осуществляется по новой схеме
		/// </summary>
		public virtual bool IsClientWorksWithNewEdoProcessing =>
			Client?.IsNewEdoProcessing ?? false;

		public virtual void AddDeliveryPointCommentToOrder()
		{
			if(DeliveryPoint == null)
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(Comment))
			{
				Comment = DeliveryPoint.Comment;
			}
			else
			{
				Comment += $"\n{DeliveryPoint.Comment}";
			}
		}

		public virtual void UpdateAddressType()
		{
			if(Client != null
				&& Client.IsChainStore
				&& !OrderItems.Any(x => x.IsMasterNomenclature && x.Nomenclature.Id != _nomenclatureSettings.MasterCallNomenclatureId))
			{
				OrderAddressType = OrderAddressType.ChainStore;
			}
			if(Client != null
				&& !Client.IsChainStore
				&& !OrderItems.Any(x => x.IsMasterNomenclature && x.Nomenclature.Id != _nomenclatureSettings.MasterCallNomenclatureId)
				&& OrderAddressType != OrderAddressType.StorageLogistics)
			{
				OrderAddressType = OrderAddressType.Delivery;
			}
		}

		private DiscountReason GetDiscountReasonStockBottle(
			IOrderSettings orderSettings, decimal discount)
		{
			var reasonId = discount == 10m
				? orderSettings.GetDiscountReasonStockBottle10PercentsId
				: orderSettings.GetDiscountReasonStockBottle20PercentsId;

			var discountReasonStockBottle = UoW.GetById<DiscountReason>(reasonId)
				?? throw new InvalidProgramException($"Не возможно найти причину скидки для акции Бутыль (id:{reasonId})");

			return discountReasonStockBottle;
		}

		/// <summary>
		/// Рассчитывает скидки в товарах по акции "Бутыль"
		/// </summary>
		public virtual void CalculateBottlesStockDiscounts(IOrderSettings orderSettings, bool byActualCount = false)
		{
			if(orderSettings == null) {
				throw new ArgumentNullException(nameof(orderSettings));
			}

			var bottlesByStock = byActualCount ? BottlesByStockActualCount : BottlesByStockCount;
			decimal stockBottleDiscountPercent = 0m;
			DiscountReason stockBottleDiscountReason = null;

			if(bottlesByStock == Total19LBottlesToDeliver)
			{
				stockBottleDiscountPercent = 10m;
				stockBottleDiscountReason = GetDiscountReasonStockBottle(orderSettings, stockBottleDiscountPercent);
			}
			if(bottlesByStock > Total19LBottlesToDeliver)
			{
				stockBottleDiscountPercent = 20m;
				stockBottleDiscountReason = GetDiscountReasonStockBottle(orderSettings, stockBottleDiscountPercent);
			}

			foreach(OrderItem item in ObservableOrderItems
				.Where(x => x.Nomenclature.Category == NomenclatureCategory.water)
				.Where(x => !x.Nomenclature.IsDisposableTare)
				.Where(x => x.Nomenclature.TareVolume == TareVolume.Vol19L)) {
				item.SetDiscountByStock(stockBottleDiscountReason, stockBottleDiscountPercent);
			}
		}

		public virtual void ParseTareReason()
		{
			if(!IsTareNonReturnReasonChangedByUser) {
				var reasons = UoW.Session.QueryOver<NonReturnReason>().OrderBy(x => x.Name).Asc.List();
				TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "ПРИЧИНА НЕИЗВЕСТНА");
				if(!string.IsNullOrWhiteSpace(Comment)) {
					if(Comment.ToUpper().Contains("НОВЫЙ АДРЕС") && reasons.Any(x => x.Name.ToUpper() == "НОВЫЙ АДРЕС"))
						TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "НОВЫЙ АДРЕС");
					if(Comment.ToUpper().Contains("УВЕЛИЧЕНИЕ ЗАКАЗА") && reasons.Any(x => x.Name.ToUpper() == "УВЕЛИЧЕНИЕ ЗАКАЗА"))
						TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "УВЕЛИЧЕНИЕ ЗАКАЗА");
					if(Comment.ToUpper().Contains("ПЕРВЫЙ ЗАКАЗ") && reasons.Any(x => x.Name.ToUpper() == "ПЕРВЫЙ ЗАКАЗ"))
						TareNonReturnReason = reasons.FirstOrDefault(x => x.Name.ToUpper() == "ПЕРВЫЙ ЗАКАЗ");
				}
			}
		}

		public virtual void RecalculateStockBottles(IOrderSettings orderSettings)
		{
			if(!IsBottleStock) {
				BottlesByStockCount = 0;
				BottlesByStockActualCount = 0;
			}
			CalculateBottlesStockDiscounts(orderSettings);
		}

		public virtual void AddContractDocument(CounterpartyContract contract)
		{
			if(ObservableOrderDocuments.OfType<OrderContract>().Any(x => x.Contract == contract))
			{
				return;
			}

			ObservableOrderDocuments.Add(
				new OrderContract
				{
					Order = this,
					AttachedToOrder = this,
					Contract = contract
				});
		}

		public virtual bool HasWater()
		{
			var categories = Nomenclature.GetCategoriesRequirementForWaterAgreement();
			return ObservableOrderItems.Any(x => categories.Contains(x.Nomenclature.Category));
		}

		public virtual void CheckAndSetOrderIsService()
		{
			if(OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master))
			{
				OrderAddressType = OrderAddressType.Service;
			}
		}

		public virtual void SetFirstOrder()
		{
			if(Id == 0 && Client.FirstOrder == null) {
				IsFirstOrder = true;
				Client.FirstOrder = this;
			}
		}

		public virtual void RecalculateItemsPrice()
		{
			for(var i = 0; i < OrderItems.Count; i++)
			{
				if(OrderItems[i].Nomenclature.Category == NomenclatureCategory.water)
				{
					OrderItems[i].RecalculatePrice();
				}
			}
		}

		public virtual int GetTotalWater19LCount(bool doNotCountWaterFromPromoSets = false, bool doNotCountPresentsDiscount = false)
		{
			var water19L = ObservableOrderItems.Where(x => x.Nomenclature.IsWater19L);

			if(doNotCountWaterFromPromoSets)
			{
				water19L = water19L.Where(x => x.PromoSet == null);
			}

			if(doNotCountPresentsDiscount)
			{
				water19L = water19L.Where(x => x.DiscountReason?.IsPresent != true);
			}
			return (int)water19L.Sum(x => x.Count);
		}

		public virtual void AddEquipmentNomenclatureToClient(Nomenclature nomenclature, IUnitOfWork UoW)
		{
			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = Direction.Deliver,
					Equipment = null,
					OrderItem = null,
					Reason = Reason.Service,
					Confirmed = true,
					Nomenclature = nomenclature
				}
			);

			UpdateDocuments();
		}
		
		public virtual void AddEquipmentFromPartOrder(OrderEquipment equipment)
		{
			var newEquipment = OrderEquipment.Clone(equipment);
			
			ObservableOrderEquipments.Add(newEquipment);
			
			UpdateDocuments();
		}

		public virtual void AddEquipmentNomenclatureFromClient(
			Nomenclature nomenclature,
			IUnitOfWork UoW,
			int count = 0,
			Direction direction = Direction.PickUp,
			DirectionReason directionReason = DirectionReason.None,
			OwnTypes ownType = OwnTypes.None,
			Reason reason = Reason.Service)
		{
			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = direction,
					Equipment = null,
					OrderItem = null,
					OwnType = ownType,
					DirectionReason = directionReason,
					Reason = reason,
					Confirmed = true,
					Nomenclature = nomenclature,
					Count = count
				}
			);
			UpdateDocuments();
		}

		public virtual void AddAnyGoodsNomenclatureForSale(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature,
			bool isChangeOrder = false,
			int? cnt = null)
		{
			var acceptableCategories = Nomenclature.GetCategoriesForSale();
			if(!acceptableCategories.Contains(nomenclature.Category))
			{
				return;
			}

			var count = (nomenclature.Category == NomenclatureCategory.service
				|| nomenclature.Category == NomenclatureCategory.deposit) && !isChangeOrder ? 1 : 0;

			if(cnt.HasValue)
			{
				count = cnt.Value;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddOrderItem(
				uow,
				contractUpdater,
				OrderItem.CreateForSale(this, nomenclature, count, nomenclature.GetPrice(1, canApplyAlternativePrice)));
		}

		/// <summary>
		/// Добавление в заказ номенклатуры типа "Сервисное обслуживание"
		/// </summary>
		/// <param name="uow">unit of work"</param>
		/// <param name="contractUpdater">Сервис обновления договора заказа</param>
		/// <param name="nomenclature">Номенклатура типа "Сервисное обслуживание"</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature,
			int count,
			int quantityOfFollowingNomenclatures = 0)
		{
			if(nomenclature.Category != NomenclatureCategory.master) {
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
			    && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			AddOrderItem(
				uow,
				contractUpdater,
				OrderItem.CreateForSale(this, nomenclature, count, nomenclature.GetPrice(1, canApplyAlternativePrice)));

			if(quantityOfFollowingNomenclatures > 0)
			{
				Nomenclature followingNomenclature = _nomenclatureRepository.GetNomenclatureToAddWithMaster(UoW);
				if(!ObservableOrderItems.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				{
					AddAnyGoodsNomenclatureForSale(
						uow,
						contractUpdater,
						followingNomenclature,
						false,
						1);
				}
			}
		}

		public virtual void AddWaterForSale(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature,
			decimal count,
			decimal discount = 0,
			bool isDiscountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason reason = null,
			PromotionalSet proSet = null)
		{
			if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
			{
				return;
			}

			//Если номенклатура промонабора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0) {
				var fixPricedNomenclaturesId = GetNomenclaturesWithFixPrices.Select(n => n.Id);
				if(fixPricedNomenclaturesId.Contains(nomenclature.Id))
				{
					reason = null;
				}
			}

			if(discount > 0 && reason == null && proSet == null)
			{
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");
			}

			var price = GetWaterPrice(nomenclature, proSet, count, needGetFixedPrice);
			AddOrderItem(
				uow,
				contractUpdater,
				OrderItem.CreateForSaleWithDiscount(this, nomenclature, count, price, isDiscountInMoney, discount, reason, proSet));
		}

		public virtual void AddFlyerNomenclature(Nomenclature flyerNomenclature)
		{
			if (ObservableOrderEquipments.Any(x => x.Nomenclature.Id == flyerNomenclature.Id)) {
				return;
			}

			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = Direction.Deliver,
					Count = 1,
					Equipment = null,
					OrderItem = null,
					Reason = Reason.Sale,
					Confirmed = true,
					Nomenclature = flyerNomenclature
				}
			);
			UpdateDocuments();
		}

		private decimal GetWaterPrice(
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			decimal bottlesCount,
			bool needGetFixedPrice)
		{
			//Т.к. в онлайн заказах можно применять скидку(промокод), если скидка больше чем фикса, но на прайс
			//то могут быть ситуации, когда у клиента есть фикса, но на позицию применена скидка,
			//для этого передаем флаг нужно ли подбирать фиксу
			if(needGetFixedPrice)
			{
				var fixedPrice = GetFixedPriceOrNull(nomenclature, GetTotalWater19LCount(doNotCountPresentsDiscount: true) + bottlesCount);
				if (fixedPrice != null && promoSet == null)
				{
					return fixedPrice.Price;
				}
			}

			var count = promoSet == null ? GetTotalWater19LCount(true, true) : bottlesCount;

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

			return nomenclature.GetPrice(count, canApplyAlternativePrice);
		}

		public virtual NomenclatureFixedPrice GetFixedPriceOrNull(Nomenclature nomenclature, decimal bottlesCount)
		{
			IList<NomenclatureFixedPrice> fixedPrices;

			if(_deliveryPoint == null)
			{
				if (Contract == null)
					return null;

				fixedPrices = Contract.Counterparty.NomenclatureFixedPrices;
			}
			else
			{
				fixedPrices = _deliveryPoint.NomenclatureFixedPrices;
			}

			Nomenclature influentialNomenclature = nomenclature.DependsOnNomenclature;

			if(fixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount && influentialNomenclature == null)) 
			{
				return fixedPrices.OrderBy(x=>x.MinCount).Last(x => x.Nomenclature.Id == nomenclature.Id && bottlesCount >= x.MinCount);
			}

			if(influentialNomenclature != null && fixedPrices.Any(x => x.Nomenclature.Id == influentialNomenclature.Id && bottlesCount >= x.MinCount)) 
			{
				return fixedPrices.OrderBy(x => x.MinCount).Last(x => x.Nomenclature.Id == influentialNomenclature?.Id && bottlesCount >= x.MinCount);
			}

			return null;
		}

		public virtual IEnumerable<Nomenclature> GetNomenclaturesWithFixPrices{
			get {
				List<NomenclatureFixedPrice> fixedPrices = new List<NomenclatureFixedPrice>();
				fixedPrices.AddRange(Client.NomenclatureFixedPrices);
				fixedPrices.AddRange(Client.DeliveryPoints.SelectMany(x => x.NomenclatureFixedPrices));
				return fixedPrices.Select(x => x.Nomenclature).Distinct();
			}
		}

		public virtual void UpdateClientDefaultParam()
		{
			if(Client == null)
				return;
			if(OrderStatus != OrderStatus.NewOrder)
				return;

			DeliveryDate = null;
			DeliveryPoint = null;
			DeliverySchedule = null;
			Contract = null;
			DocumentType = Client.DefaultDocumentType ?? DefaultDocumentType.upd;

			if(!SelfDelivery && Client.DeliveryPoints?.Count == 1)
				DeliveryPoint = Client.DeliveryPoints.FirstOrDefault();

			PaymentType = Client.PaymentMethod;
		}

		public virtual void SetProxyForOrder()
		{
			if(Client == null)
			{
				return;
			}

			if(!DeliveryDate.HasValue)
			{
				return;
			}

			if(Client.PersonType != PersonType.legal && PaymentType != PaymentType.Cashless)
			{
				return;
			}

			bool existProxies = Client.Proxies
				.Any(p => p.IsActiveProxy(DeliveryDate.Value)
						&& (p.DeliveryPoints == null
							|| p.DeliveryPoints.Any(x => DomainHelper.EqualDomainObjects(x, DeliveryPoint))));

			if(existProxies)
			{
				SignatureType = OrderSignatureType.ByProxy;
			}
		}

		/// <summary>
		/// Добавить оборудование из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="contractUpdater">Сервис обновления договора заказа</param>
		/// <param name="orderItem">Элемент заказа.</param>
		public virtual void AddNomenclatureForSaleFromPreviousOrder(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			OrderItem orderItem)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.additional)
			{
				return;
			}

			AddOrderItem(
				uow,
				contractUpdater,
				OrderItem.CreateForSale(this, orderItem.Nomenclature, orderItem.Count, orderItem.Price));
		}

		public virtual void AddNomenclature(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			bool discountInMoney = false,
			bool needGetFixedPrice = true,
			DiscountReason discountReason = null,
			PromotionalSet proSet = null)
		{
			switch(nomenclature.Category) {
				case NomenclatureCategory.water:
					AddWaterForSale(
						uow,
						contractUpdater,
						nomenclature,
						count,
						discount,
						discountInMoney,
						needGetFixedPrice,
						discountReason,
						proSet);
					break;
				case NomenclatureCategory.master:
					contract = CreateServiceContractAddMasterNomenclature(uow, contractUpdater, nomenclature);
					break;
				default:
					var canApplyAlternativePrice = HasPermissionsForAlternativePrice && nomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= count);

					var orderItem = OrderItem.CreateForSaleWithDiscount(this, nomenclature, count, nomenclature.GetPrice(1, canApplyAlternativePrice), discountInMoney, discount, discountReason, proSet);

					var acceptableCategories = Nomenclature.GetCategoriesForSale();
					if(orderItem?.Nomenclature == null
						|| !acceptableCategories.Contains(orderItem.Nomenclature.Category))
					{
						return;
					}
					AddOrderItem(uow, contractUpdater, orderItem);

					break;
			}
		}

		/// <summary>
		/// Попытка найти и удалить промонабор, если нет больше позиций
		/// заказа с промонабором
		/// </summary>
		/// <param name="orderItem">Позиция заказа</param>
		public virtual void TryToRemovePromotionalSet(OrderItem orderItem)
		{
			var proSetFromOrderItem = orderItem.PromoSet;
			if(proSetFromOrderItem != null) {
				var proSetToRemove = ObservablePromotionalSets.FirstOrDefault(s => s == proSetFromOrderItem);
				if(proSetToRemove != null && !OrderItems.Any(i => i.PromoSet == proSetToRemove)) {
					foreach(PromotionalSetActionBase action in proSetToRemove.ObservablePromotionalSetActions) {
						action.Deactivate(this);
					}
					ObservablePromotionalSets.Remove(proSetToRemove);
				}
			}
		}

		private void ObservablePromotionalSets_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject is PromotionalSet proSet)
			{
				foreach(OrderItem item in ObservableOrderItems)
				{
					if(item.PromoSet == proSet)
					{
						item.IsUserPrice = false;
						item.PromoSet = null;
						item.DiscountReason = null;
					}
				}

				RecalculateItemsPrice();
			}
		}

		/// <summary>
		/// Чистка списка промонаборов заказа, если вручную удалили, изменили
		/// причину скидки или что-то ещё.
		/// </summary>
		private void ClearPromotionSets()
		{
			var oigrp = OrderItems.GroupBy(x => x.PromoSet);
			var rem = PromotionalSets.Where(s => !oigrp.Select(g => g.Key).Contains(s)).ToArray();
			foreach(var r in rem) {
				var ps = PromotionalSets.FirstOrDefault(s => s == r);
				PromotionalSets.Remove(ps);
			}
		}

		/// <summary>
		/// Проверка, есть ли в заказе товары типа "Залог"
		/// </summary>
		/// <returns></returns>
		public virtual bool HasDepositItems() =>
			OrderItems.Any(x =>
				x.Nomenclature.Category == NomenclatureCategory.deposit);

		/// <summary>
		/// Проверка, есть ли в заказе товары с бесплатной доставкой
		/// </summary>
		/// <returns></returns>
		public virtual bool HasNonPaidDeliveryItems() =>
			OrderItems.Any(x =>
				_nomenclatureSettings.PaidDeliveryNomenclatureId != x.Nomenclature.Id);


		/// <summary>
		/// Проверка на возможность добавления промонабора в заказ
		/// </summary>
		/// <returns><c>true</c>, если можно добавить промонабор,
		/// <c>false</c> если нельзя.</returns>
		/// <param name="proSet">Промонабор (промонабор)</param>
		public virtual bool CanAddPromotionalSet(
			PromotionalSet proSet,
			IFreeLoaderChecker freeLoaderChecker,
			IPromotionalSetRepository promotionalSetRepository)
		{
			if(PromotionalSets.Any(x => x.PromotionalSetForNewClients && proSet.PromotionalSetForNewClients))
			{
				InteractiveService.ShowMessage(
					ImportanceLevel.Warning,
					"В заказ нельзя добавить два промо-набора для новых клиентов");
				return false;
			}

			if(SelfDelivery)
			{
				return true;
			}

			if(proSet.PromotionalSetForNewClients
				&& freeLoaderChecker.CheckFreeLoaderOrderByNaturalClientToOfficeOrStore(UoW, SelfDelivery, Client, DeliveryPoint))
			{
				var message = "По этому адресу уже была ранее отгрузка промонабора на другое физ.лицо.";
				InteractiveService.ShowMessage(ImportanceLevel.Warning, message);
				return false;
			}

			var proSetDict = promotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(UoW, this);

			if(!proSet.PromotionalSetForNewClients | !proSetDict.Any())
			{
				return true;
			}

			var address = string.Join(", ", DeliveryPoint.City, DeliveryPoint.Street, DeliveryPoint.Building, DeliveryPoint.Room);
			var sb = new StringBuilder(
				$"Для адреса \"{address}\", найдены схожие точки доставки, на которые уже создавались заказы с промо-наборами:\n");
			foreach(var d in proSetDict) {
				var proSetTitle = UoW.GetById<PromotionalSet>(d.Key).ShortTitle;
				var orders = string.Join(
					" ,",
					UoW.GetById<Order>(d.Value).Select(o => o.Title)
				);
				sb.AppendLine($"– {proSetTitle}: {orders}");
			}
			sb.AppendLine($"Вы уверены, что хотите добавить \"{proSet.Title}\"");
			return InteractiveService.Question(sb.ToString());
		}

		private CounterpartyContract CreateServiceContractAddMasterNomenclature(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature nomenclature)
		{
			//TODO: проверить целесообразность этой установки, т.к. при добавлении номенклатуры обновляется и сам договор
			if(Contract == null)
			{
				contractUpdater.ForceUpdateContract(uow, this);
			}
			AddMasterNomenclature(uow, contractUpdater, nomenclature, 1);
			return Contract;
		}

		public virtual void ClearOrderItemsList()
		{
			ObservableOrderItems.Clear();
			UpdateDocuments();
		}

		/// <summary>
		/// Удаляет дополнительные документы выделенные пользователем, которые не относятся к текущему заказу.
		/// </summary>
		/// <returns>Документы текущего заказа, которые не были удалены.</returns>
		/// <param name="documents">Список документов для удаления.</param>
		public virtual List<OrderDocument> RemoveAdditionalDocuments(OrderDocument[] documents)
		{
			if(documents == null || !documents.Any())
				return null;

			List<OrderDocument> thisOrderDocuments = new List<OrderDocument>();
			foreach(OrderDocument doc in documents) {
				if(doc.Order != this)
					ObservableOrderDocuments.Remove(doc);
				else
					thisOrderDocuments.Add(doc);
			}

			return thisOrderDocuments;
		}

		/// <summary>
		/// Добавляет дополнительные документы выбранные пользователем в диалоге,
		/// с проверкой их наличия в текущем заказе
		/// </summary>
		public virtual void AddAdditionalDocuments(IEnumerable<OrderDocument> documents)
		{
			foreach(var item in documents) {
				switch(item.Type) {
					case OrderDocumentType.Contract:
						OrderContract oc = (item as OrderContract);
						if(ObservableOrderDocuments
						   .OfType<OrderContract>()
						   .FirstOrDefault(x => x.Contract == oc.Contract
										   && x.Order == oc.Order)
						   == null) {
							ObservableOrderDocuments.Add(new OrderContract {
								Order = item.Order,
								AttachedToOrder = this,
								Contract = oc.Contract
							});
						}
						break;
					case OrderDocumentType.M2Proxy:
						OrderM2Proxy m2 = item as OrderM2Proxy;
						var hasDocument = ObservableOrderDocuments
						   .OfType<OrderM2Proxy>()
						   .Any(x => x.M2Proxy == m2.M2Proxy && x.Order == m2.Order);

						if(!hasDocument)
						{
							var newM2 = new OrderM2Proxy();
							newM2.AttachedToOrder = this;
							newM2.Order = m2.Order;
							newM2.M2Proxy = m2.M2Proxy;

							ObservableOrderDocuments.Add(newM2);
						}
						break;
					case OrderDocumentType.Bill:
						if(ObservableOrderDocuments
						   .OfType<BillDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new BillDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.DoneWorkReport:
						if(ObservableOrderDocuments
						   .OfType<DoneWorkDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new DoneWorkDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.EquipmentTransfer:
						if(ObservableOrderDocuments
						   .OfType<EquipmentTransferDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new EquipmentTransferDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.Invoice:
						if(ObservableOrderDocuments
						   .OfType<InvoiceDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new InvoiceDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.InvoiceBarter:
						if(ObservableOrderDocuments
						   .OfType<InvoiceBarterDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new InvoiceBarterDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.InvoiceContractDoc:
						if(ObservableOrderDocuments
						   .OfType<InvoiceContractDoc>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new InvoiceContractDoc {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.UPD:
						if(ObservableOrderDocuments
						   .OfType<UPDDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new UPDDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.DriverTicket:
						if(ObservableOrderDocuments
						   .OfType<DriverTicketDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new DriverTicketDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.Torg12:
						if(ObservableOrderDocuments
						   .OfType<Torg12Document>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new Torg12Document {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.ShetFactura:
						if(ObservableOrderDocuments
						   .OfType<ShetFacturaDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new ShetFacturaDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					case OrderDocumentType.ProductCertificate:
						if(item is NomenclatureCertificateDocument cert &&
							!ObservableOrderDocuments.OfType<NomenclatureCertificateDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new NomenclatureCertificateDocument {
									Order = item.Order,
									AttachedToOrder = this,
									Certificate = cert.Certificate
								}
							);
						}
						break;
					case OrderDocumentType.SpecialBill:
						if(item is SpecialBillDocument &&
							!ObservableOrderDocuments.OfType<SpecialBillDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new SpecialBillDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.EquipmentReturn:
						if(item is EquipmentReturnDocument &&
							!ObservableOrderDocuments.OfType<EquipmentReturnDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new EquipmentReturnDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.SpecialUPD:
						if(item is SpecialUPDDocument &&
							!ObservableOrderDocuments.OfType<SpecialUPDDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new SpecialUPDDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.TransportInvoice:
						if(item is TransportInvoiceDocument &&
							!ObservableOrderDocuments.OfType<TransportInvoiceDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new TransportInvoiceDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.Torg2:
						if(item is Torg2Document &&
							!ObservableOrderDocuments.OfType<Torg2Document>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new Torg2Document {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.AssemblyList:
						if(item is AssemblyListDocument &&
							!ObservableOrderDocuments.OfType<AssemblyListDocument>().Any(x => x.Order == item.Order)) {
							ObservableOrderDocuments.Add(
								new AssemblyListDocument {
									Order = item.Order,
									AttachedToOrder = this
								}
							);
						}
						break;
					case OrderDocumentType.LetterOfDebt:
						if(ObservableOrderDocuments
						   .OfType<LetterOfDebtDocument>()
						   .FirstOrDefault(x => x.Order == item.Order)
						   == null) {
							ObservableOrderDocuments.Add(new LetterOfDebtDocument {
								Order = item.Order,
								AttachedToOrder = this
							});
						}
						break;
					default:
						break;
				}
			}
		}

		/// <summary>
		/// Ожидаемое количество залогов за бутыли
		/// </summary>
		public virtual int GetExpectedBottlesDepositsCount()
		{
			if(Client == null || Client.PersonType == PersonType.legal)
				return 0;

			var waterItemsCount = (int)ObservableOrderItems.Select(item => item)
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && !item.Nomenclature.IsDisposableTare)
				.Sum(item => item.Count);

			return waterItemsCount - BottlesReturn ?? 0;
		}

		public virtual void RemoveItemFromClosingOrder(IUnitOfWork uow, IOrderContractUpdater contractUpdater, OrderItem item)
		{
			if((item.Count != 0 && item.Price != 0) || OrderEquipments.Any(x => x.OrderItem == item))
			{
				return;
			}

			RemoveOrderItem(uow, contractUpdater, item);
		}

		public virtual void RemoveItem(IUnitOfWork uow, IOrderContractUpdater contractUpdater, OrderItem item)
		{
			RemoveOrderItem(uow, contractUpdater, item);
			DeleteOrderEquipmentOnOrderItem(item);
			UpdateDocuments();
			_nomenclatureService.CalculateMasterCallNomenclaturePriceIfNeeded(UoW, this);
		}

		public virtual void RemoveEquipment(IUnitOfWork uow, IOrderContractUpdater contractUpdater, OrderEquipment item)
		{
			var rentDepositOrderItem = item.OrderRentDepositItem;
			var rentServiceOrderItem = item.OrderRentServiceItem;
			var totalEquipmentCountForDeposit = 0;
			var totalEquipmentCountForService = 0;

			if(rentDepositOrderItem != null)
			{
				totalEquipmentCountForDeposit = GetRentEquipmentTotalCountForDepositItem(rentDepositOrderItem);
			}
			if(rentServiceOrderItem != null)
			{
				totalEquipmentCountForService = GetRentEquipmentTotalCountForServiceItem(rentServiceOrderItem);
			}

			if(totalEquipmentCountForDeposit == item.Count || totalEquipmentCountForService == item.Count)
			{
				ObservableOrderEquipments.Remove(item);
				RemoveOrderItem(uow, contractUpdater, rentDepositOrderItem);
				RemoveOrderItem(uow, contractUpdater, rentServiceOrderItem);
			}
			else
			{
				ObservableOrderEquipments.Remove(item);
				UpdateRentsCount();
			}

			UpdateDocuments();
		}

		/// <summary>
		/// Удаляет оборудование в заказе связанное с товаром в заказе
		/// </summary>
		/// <param name="orderItem">Товар в заказе по которому будет удалятся оборудование</param>
		private void DeleteOrderEquipmentOnOrderItem(OrderItem orderItem)
		{
			var orderEquipments = ObservableOrderEquipments
				.Where(x => x.OrderItem == orderItem)
				.ToList();
			foreach(var orderEquipment in orderEquipments) {
				ObservableOrderEquipments.Remove(orderEquipment);
			}
		}

		public virtual void RemoveDepositItem(OrderDepositItem item)
		{
			ObservableOrderDepositItems.Remove(item);
			UpdateDocuments();
		}

		public virtual void AddServiceClaimAsInitial(ServiceClaim service)
		{
			if(service.InitialOrder != null && service.InitialOrder.Id == Id) {
				if(service.Equipment == null || ObservableOrderEquipments.FirstOrDefault(eq => eq.Equipment.Id == service.Equipment.Id) == null) {
					ObservableOrderEquipments.Add(new OrderEquipment {
						Order = this,
						Direction = Direction.PickUp,
						Equipment = service.Equipment,
						Nomenclature = service.Equipment == null ? service.Nomenclature : null,
						OrderItem = null,
						Reason = Reason.Service,
						ServiceClaim = service
					});
				}
				if(service.ReplacementEquipment != null) {
					ObservableOrderEquipments.Add(new OrderEquipment {
						Order = this,
						Direction = Direction.Deliver,
						Equipment = service.ReplacementEquipment,
						Nomenclature = null,
						OrderItem = null,
						Reason = Reason.Service
					});
				}
				if(ObservableInitialOrderService.FirstOrDefault(sc => sc.Id == service.Id) == null)
					ObservableInitialOrderService.Add(service);
				if(ObservableOrderDocuments.Where(doc => doc.Type == OrderDocumentType.EquipmentTransfer).Cast<EquipmentTransferDocument>()
					.FirstOrDefault() == null) {
					ObservableOrderDocuments.Add(new EquipmentTransferDocument {
						Order = this,
						AttachedToOrder = this
					});
				}
			}
		}

		public virtual void FillNewEquipment(Equipment registeredEquipment)
		{
			var newEquipment = ObservableOrderEquipments
				.Where(orderEq => orderEq.Nomenclature != null)
				.FirstOrDefault(orderEq => orderEq.Nomenclature.Id == registeredEquipment.Nomenclature.Id);
			if(newEquipment != null) {
				newEquipment.Equipment = registeredEquipment;
				newEquipment.Nomenclature = null;
			}
		}

		/// <summary>
		/// Присвоение текущему заказу статуса недовоза
		/// </summary>
		/// <param name="guilty">Ответственный в недовезении заказа</param>
		public virtual void SetUndeliveredStatus(IUnitOfWork uow, IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings, ICallTaskWorker callTaskWorker,
			GuiltyTypes? guilty = GuiltyTypes.Client, bool needCreateDeliveryFreeBalanceOperation = false)
		{
			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(UoW, this);
			var routeList = routeListItem?.RouteList;
			switch(OrderStatus)
			{
				case OrderStatus.NewOrder:
				case OrderStatus.WaitForPayment:
				case OrderStatus.Accepted:
				case OrderStatus.InTravelList:
				case OrderStatus.OnLoading:
					ChangeStatusAndCreateTasks(OrderStatus.Canceled, callTaskWorker);
					routeListService.SetAddressStatusWithoutOrderChange(uow, routeList, routeListItem, RouteListItemStatus.Overdue, needCreateDeliveryFreeBalanceOperation);
					break;
				case OrderStatus.OnTheWay:
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.Shipped:
				case OrderStatus.UnloadingOnStock:
				case OrderStatus.NotDelivered:
				case OrderStatus.Closed:
					if(guilty == GuiltyTypes.Client)
					{
						ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
						routeListService.SetAddressStatusWithoutOrderChange(uow, routeList, routeListItem, RouteListItemStatus.Canceled, needCreateDeliveryFreeBalanceOperation);
					}
					else
					{
						ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
						routeListService.SetAddressStatusWithoutOrderChange(uow, routeList, routeListItem, RouteListItemStatus.Overdue, needCreateDeliveryFreeBalanceOperation);
					}
					break;
			}
			UpdateBottleMovementOperation(uow, nomenclatureSettings, 0);

			_orderService.RejectOrderTrueMarkCodes(uow, this.Id);
		}

		public virtual void CancelDelivery(IUnitOfWork uow, ICallTaskWorker callTaskWorker)
		{
			ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
			_orderService.RejectOrderTrueMarkCodes(uow, this.Id);
		}

		public virtual void OverdueDelivery(IUnitOfWork uow, ICallTaskWorker callTaskWorker)
		{
			ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
			_orderService.RejectOrderTrueMarkCodes(uow, this.Id);
		}

		public virtual void ChangeStatusAndCreateTasks(OrderStatus newStatus, ICallTaskWorker callTaskWorker)
		{
			ChangeStatus(newStatus);
			callTaskWorker.CreateTasks(this);
		}

		public virtual void ChangeStatus(OrderStatus newStatus)
		{
			var initialStatus = OrderStatus;
			OrderStatus = newStatus;
			switch(newStatus) {
				case OrderStatus.NewOrder:
					break;
				case OrderStatus.WaitForPayment:
					OnChangeStatusToWaitingForPayment();
					break;
				case OrderStatus.Accepted:
					//Удаляем операции перемещения тары, если возвращаем
					//из "закрыт" без доставки в "принят"
					if(initialStatus == OrderStatus.Closed)
						DeleteBottlesMovementOperation(UoW);
					OnChangeStatusToAccepted();
					break;
				case OrderStatus.OnLoading:
					OnChangeStatusToOnLoading();
					break;
				case OrderStatus.OnTheWay:
					break;
				case OrderStatus.Shipped:
				case OrderStatus.UnloadingOnStock:
					OnChangeStatusToShipped();
					break;
				case OrderStatus.Closed:
					OnChangeStatusToClosed();
					break;
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.NotDelivered:
				case OrderStatus.Canceled:
					if(PaymentType == PaymentType.Cashless)
					{
						_paymentFromBankClientController.ReturnAllocatedSumToClientBalance(UoW, this);
					}
					break;
				default:
					break;
			}

			if(Id != 0 && initialStatus != newStatus)
			{
				var undeliveries = _undeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, this);
				if(undeliveries.Any())
				{
					var text = $"сменил(а) статус заказа\nс \"{initialStatus.GetEnumTitle()}\" на \"{newStatus.GetEnumTitle()}\"";
					foreach(var u in undeliveries)
					{
						u.AddAutoCommentToOkkDiscussion(UoW, text);
					}
				}
			}

			if(Id == 0
			   || newStatus == OrderStatus.Canceled
			   || newStatus == OrderStatus.NotDelivered
			   || initialStatus == newStatus)
				return;

			_paymentFromBankClientController.CancelRefundedPaymentIfOrderRevertFromUndelivery(UoW, this, initialStatus);
		}

		private void OnChangeStatusToShipped() => SendUpdToEmailOnFinishIfNeeded();

		private void SendUpdToEmailOnFinishIfNeeded()
		{
			var emailSendUpdResult =
				_emailService.SendUpdToEmailOnFinishIfNeeded(UoW, this);

			if(emailSendUpdResult.IsSuccess)
			{
				return;
			}

			var errorStrings = emailSendUpdResult.Errors.Select(x => x.Message);
			ServicesConfig.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Не удалось отправить УПД по email для заказа № {Id}:\n" +
				$"{string.Join("\n", errorStrings)}");
		}

		private void SendBillForClosingDocumentOnFinishIfNeeded()
		{
			var emailSendBillResult = _emailService.SendBillForClosingDocumentOrderToEmailOnFinishIfNeeded(UoW, this);
			
			if(emailSendBillResult.IsSuccess)
			{
				return;
			}

			var errorStrings = emailSendBillResult.Errors.Select(x => x.Message);
			ServicesConfig.InteractiveService.ShowMessage (ImportanceLevel.Warning, $"Не удалось отправить счёт по email для заказа № {Id}:\n" +
				$"{string.Join("\n", errorStrings)}");
		}

		/// <summary>
		/// Действия при закрытии заказа
		/// </summary>
		private void OnChangeStatusToOnLoading() => UpdateDocuments();

		/// <summary>
		/// Действия при закрытии заказа
		/// </summary>
		private void OnChangeStatusToClosed()
		{
			SetDepositsActualCounts();
			if(SelfDelivery) {
				UpdateDepositOperations(UoW);
				SetActualCountToSelfDelivery();
			}

			SendUpdToEmailOnFinishIfNeeded();
			SendBillForClosingDocumentOnFinishIfNeeded();
		}

		/// <summary>
		/// Действия при переводе заказа в ожидание оплаты
		/// </summary>
		private void OnChangeStatusToWaitingForPayment() => UpdateDocuments();

		/// <summary>
		/// Действия при подтверждении заказа
		/// </summary>
		private void OnChangeStatusToAccepted() => UpdateDocuments();

		/// <summary>
		/// Отправка самовывоза на погрузку
		/// </summary>
		public virtual void SelfDeliveryToLoading(
			Employee employee,
			ICurrentPermissionService permissionService,
			ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery) {
				return;
			}
			if(OrderStatus == OrderStatus.Accepted
				&& permissionService.ValidatePresetPermission(StorePermissions.Documents.CanLoadSelfDeliveryDocument))
			{
				ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);
				LoadAllowedBy = employee;
			}
		}

		public virtual void SetActualCountToSelfDelivery()
		{
			if(!SelfDelivery || OrderStatus != OrderStatus.Closed)
			{
				return;
			}

			ResetOrderItemsActualCounts();

			ResetDepositItemsActualCounts();
		}

		/// <summary>
		/// Принятие оплаты самовывоза по безналичному расчету
		/// </summary>
		public virtual void SelfDeliveryAcceptCashlessPaid(ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery)
				return;
			if(PaymentType != PaymentType.Cashless && PaymentType != PaymentType.PaidOnline)
				return;
			if(OrderStatus != OrderStatus.WaitForPayment)
				return;
			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("accept_cashless_paid_selfdelivery"))
				return;

			ChangeStatusAndCreateTasks(PayAfterShipment ? OrderStatus.Closed : OrderStatus.Accepted, callTaskWorker);
		}

		/// <summary>
		/// Принятие оплаты самовывоза по наличному расчету.
		/// Проверяется соответствие суммы заказа с суммой оплаченной в кассе.
		/// Если проверка пройдена заказ закрывается или переводится на погрузку.
		/// </summary>
		public virtual void SelfDeliveryAcceptCashPaid(ICallTaskWorker callTaskWorker)
		{
			decimal totalCashPaid = _cashRepository.GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = _cashRepository.GetExpenseReturnSumForOrder(UoW, Id);
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		public virtual void AcceptSelfDeliveryIncomeCash(decimal incomeCash, ICallTaskWorker callTaskWorker, int? incomeExcludedDoc = null)
		{
			decimal totalCashPaid = _cashRepository.GetIncomePaidSumForOrder(UoW, Id, incomeExcludedDoc) + incomeCash;
			decimal totalCashReturn = _cashRepository.GetExpenseReturnSumForOrder(UoW, Id);
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		public virtual void AcceptSelfDeliveryExpenseCash(decimal expenseCash, ICallTaskWorker callTaskWorker, int? expenseExcludedDoc = null)
		{
			decimal totalCashPaid = _cashRepository.GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = _cashRepository.GetExpenseReturnSumForOrder(UoW, Id, expenseExcludedDoc) + expenseCash;
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		/// <summary>
		/// Принятие оплаты самовывоза по наличному расчету. С указанием дополнительным сумм по приходным и расходным ордерам
		/// Проверяется соответствие суммы заказа с суммой оплаченной в кассе.
		/// Если проверка пройдена заказ закрывается или переводится на погрузку.
		/// <paramref name="expenseCash">Сумма по открытому расходному ордеру, добавляемая к ранее сохранным расходным ордерам</paramref>
		/// <paramref name="incomeCash">Сумма по открытому приходному ордеру, добавляемая к ранее сохранным приходным ордерам</paramref>
		/// </summary>
		private void SelfDeliveryAcceptCashPaid(decimal incomeCash, decimal expenseCash, ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery)
				return;
			if(PaymentType != PaymentType.Cash)
				return;
			if((incomeCash - expenseCash) != OrderCashSum)
				return;

			IsSelfDeliveryPaid = true;

			bool isFullyLoad = IsFullyShippedSelfDeliveryOrder(UoW, _selfDeliveryRepository);

			if(OrderStatus == OrderStatus.WaitForPayment) {
				if(isFullyLoad) {
					ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
					var nomenclatureSettings = ScopeProvider.Scope.Resolve<INomenclatureSettings>();
					UpdateBottlesMovementOperationWithoutDelivery(
						UoW, nomenclatureSettings, _routeListItemRepository, _cashRepository, incomeCash, expenseCash);
				} else
					ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);

				return;
			}

			if(OrderStatus == OrderStatus.OnLoading && isFullyLoad)
				ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
		}

		/// <summary>
		/// Проверяет полностью ли оплачен самовывоз и возвращены все деньги
		/// </summary>
		public virtual bool SelfDeliveryIsFullyPaid(ICashRepository cashRepository, decimal incomeCash = 0, decimal expenseCash = 0)
		{
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			decimal totalCash = GetSelfDeliveryTotalPayedCash(cashRepository) + incomeCash - expenseCash;

			return OrderCashSum == totalCash;
		}

		/// <summary>
		/// Проверяет полностью ли получены деньги по самовывозу
		/// </summary>
		public virtual bool SelfDeliveryIsFullyIncomePaid()
		{
			decimal totalPaid = _cashRepository.GetIncomePaidSumForOrder(UoW, Id);

			return OrderPositiveSum == totalPaid;
		}

		/// <summary>
		/// Проверяет полностью ли возвращены деньги по самовывозу
		/// </summary>
		public virtual bool SelfDeliveryIsFullyExpenseReturned()
		{
			decimal totalReturned = _cashRepository.GetExpenseReturnSumForOrder(UoW, Id);

			return OrderNegativeSum == totalReturned;
		}

		private decimal GetSelfDeliveryTotalPayedCash(ICashRepository cashRepository)
		{
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			decimal totalCashPaid = cashRepository.GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = cashRepository.GetExpenseReturnSumForOrder(UoW, Id);

			return totalCashPaid - totalCashReturn;
		}

		/// <summary>
		/// Принятие заказа с самовывозом
		/// </summary>
		private void AcceptSelfDeliveryOrder(ICallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery || OrderStatus != OrderStatus.NewOrder)
			{
				return;
			}

			if(PayAfterShipment || OrderSum == 0)
			{
				ChangeStatusAndCreateTasks(OrderStatus.Accepted, callTaskWorker);
			}
			else
			{
				ChangeStatusAndCreateTasks(OrderStatus.WaitForPayment, callTaskWorker);
			}
		}

		/// <summary>
		/// Устанавливает количество для каждого залога как actualCount,
		/// если заказ был создан только для залога.
		/// Для отображения этих данных в отчете "Акт по бутылям и залогам"
		/// </summary>
		public virtual void SetDepositsActualCounts() //TODO: проверить актуальность метода
		{
			if(OrderItems.All(x => x.Nomenclature.Id == 157))
			{
				foreach(var oi in orderItems)
				{
					oi.SetActualCount(oi.Count > 0 ? oi.Count : (oi.ActualCount ?? 0));
				}
			}
		}

		public virtual void AcceptOrder(Employee currentEmployee, ICallTaskWorker callTaskWorker)
		{
			if(SelfDelivery)
			{
				AcceptSelfDeliveryOrder(callTaskWorker);
			}
			else if(CanSetOrderAsAccepted)
			{
				ChangeStatusAndCreateTasks(OrderStatus.Accepted, callTaskWorker);
			}

			AcceptedOrderEmployee = currentEmployee;
		}

		/// <summary>
		/// Статусы в которых возможно редактировать заказ
		/// </summary>
		private OrderStatus[] EditableOrderStatuses {
			get {
				if(SelfDelivery) {
					return new OrderStatus[] {
						OrderStatus.NewOrder
					};
				} else {
					return new OrderStatus[] {
						OrderStatus.NewOrder,
						OrderStatus.WaitForPayment
					};
				}
			}
		}

		public virtual bool CanEditByStatus => EditableOrderStatuses.Contains(OrderStatus);

		/// <summary>
		/// Статусы из которых возможен переход заказа в подтвержденное состояние
		/// </summary>
		public virtual OrderStatus[] SetOrderAsAcceptedStatuses {
			get {
				if(SelfDelivery) {
					return new OrderStatus[] {
						OrderStatus.NewOrder
					};
				} else {
					return new OrderStatus[] {
						OrderStatus.NewOrder,
						OrderStatus.WaitForPayment
					};
				}
			}
		}

		public virtual bool CanSetOrderAsAccepted => EditableOrderStatuses.Contains(OrderStatus);

		public virtual void EditOrder(ICallTaskWorker callTaskWorker)
		{
			//Нельзя редактировать заказ с самовывозом
			if(SelfDelivery)
			{
				return;
			}

			if(CanSetOrderAsEditable)
			{
				if(OrderStatus == OrderStatus.Canceled)
				{
					RestoreOrder();
				}

				ChangeStatusAndCreateTasks(OrderStatus.NewOrder, callTaskWorker);
			}
		}

		/// <summary>
		/// Статусы из которых возможен переход заказа в редактируемое состояние
		/// </summary>
		public virtual OrderStatus[] SetOrderAsEditableStatuses {
			get {
				if(SelfDelivery) {
					return new OrderStatus[0];
				} else {
					return new OrderStatus[] {
						OrderStatus.Accepted,
						OrderStatus.Canceled
					};
				}
			}
		}

		public virtual bool CanSetOrderAsEditable => SetOrderAsEditableStatuses.Contains(OrderStatus);

		public virtual bool IsFullyShippedSelfDeliveryOrder(IUnitOfWork uow, ISelfDeliveryRepository selfDeliveryRepository, SelfDeliveryDocument closingDocument = null)
		{
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));

			if(!SelfDelivery)
				return false;

			var categoriesForShipping = Nomenclature.GetCategoriesForShipment();
			var oItemsGrps = OrderItems.Where(x => categoriesForShipping.Contains(x.Nomenclature.Category))
									   .GroupBy(i => i.Nomenclature.Id, i => i.Count);
			var oEquipmentGrps = OrderEquipments.Where(x => categoriesForShipping.Contains(x.Nomenclature.Category))
												.Where(x => x.Direction == Direction.Deliver)
												.GroupBy(i => i.Nomenclature.Id, i => i.Count);
			var nomGrp = oItemsGrps.ToDictionary(g => g.Key, g => g.Sum());

			foreach(var g in oEquipmentGrps) {
				if(nomGrp.ContainsKey(g.Key))
					nomGrp[g.Key] += g.Sum();
				else
					nomGrp.Add(g.Key, g.Sum());
			}

			// Разрешает закрыть заказ и создать операцию движения бутылей если все товары в заказе отгружены
			bool canCloseOrder = true;
			var unloadedNoms = selfDeliveryRepository.OrderNomenclaturesUnloaded(uow, this, closingDocument);
			foreach(var nGrp in nomGrp) {
				decimal totalCount = default(decimal);
				if(unloadedNoms.ContainsKey(nGrp.Key))
					totalCount += unloadedNoms[nGrp.Key];

				if(totalCount != nGrp.Value)
					canCloseOrder = false;
			}

			return canCloseOrder;
		}

		private void UpdateSelfDeliveryActualCounts(SelfDeliveryDocument notSavedDocument = null)
		{
			var loadedDictionary = _selfDeliveryRepository.OrderNomenclaturesLoaded(UoW, this);
			if(notSavedDocument != null && notSavedDocument.Id <= 0)
			{ //если id > 0, то такой документ был учтён при получении словаря из репозитория
				foreach(var item in notSavedDocument.Items)
				{
					if(loadedDictionary.ContainsKey(item.Nomenclature.Id))
					{
						loadedDictionary[item.Nomenclature.Id] += item.Amount;
					}
					else
					{
						loadedDictionary.Add(item.Nomenclature.Id, item.Amount);
					}
				}
			}

			foreach(var item in OrderItems)
			{
				if(loadedDictionary.ContainsKey(item.Nomenclature.Id))
				{ //разбрасываем количества отгруженных по актуальным количествам в позициях заказа.
					int loadedCnt = (int)loadedDictionary[item.Nomenclature.Id];
					item.SetActualCount(Math.Min(item.Count, loadedCnt));
					loadedDictionary[item.Nomenclature.Id] -= loadedCnt;

					if(loadedDictionary[item.Nomenclature.Id] <= 0)
					{
						loadedDictionary.Remove(item.Nomenclature.Id);
					}
				}
			}

			foreach(var item in OrderEquipments)
			{
				if(loadedDictionary.ContainsKey(item.Nomenclature.Id))
				{ //разбрасываем количества отгруженных по актуальным количествам в позициях заказа.
					int loadedCnt = (int)loadedDictionary[item.Nomenclature.Id];
					item.ActualCount = Math.Min(item.Count, loadedCnt);
					loadedDictionary[item.Nomenclature.Id] -= loadedCnt;

					if(loadedDictionary[item.Nomenclature.Id] <= 0)
					{
						loadedDictionary.Remove(item.Nomenclature.Id);
					}
				}
			}
		}

		/// <summary>
		/// Проверка возможности создания нескольких заказов на одну дату и точку доставки
		/// </summary>
		public virtual ValidationResult ValidateCanCreateSeveralOrderForDateAndDeliveryPoint(ValidationContext validationContext)
		{
			if(!SelfDelivery && DeliveryPoint != null
			                 && DeliveryDate.HasValue
			                 && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_several_orders_for_date_and_deliv_point")
			                 && validationContext.Items.ContainsKey("uowFactory"))
			{
				bool hasMaster = ObservableOrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.master);

				var orderCheckedOutsideSession = _orderRepository
					.GetSameOrderForDateAndDeliveryPoint((IUnitOfWorkFactory)validationContext.Items["uowFactory"],
						DeliveryDate.Value, DeliveryPoint)
					.Where(o => o.Id != Id
					            && !_orderRepository.GetGrantedStatusesToCreateSeveralOrders().Contains(o.OrderStatus)
					            && o.OrderAddressType != OrderAddressType.Service).ToList();

				if(!hasMaster
				   && orderCheckedOutsideSession.Any())
				{
					return new ValidationResult(
						string.Format("Создать заказ нельзя, т.к. для этой даты и точки доставки уже был создан заказ {0}", orderCheckedOutsideSession.FirstOrDefault().Id),
						new[] { this.GetPropertyName(o => o.OrderEquipments) });

				}
			}

			return ValidationResult.Success;
		}

		/// <summary>
		/// Проверка на наличие воды по умолчанию в заказе для выбранной точки доставки и выдача сообщения о возможном штрафе
		/// </summary>
		/// <returns><c>true</c>, если пользователь подтвердил замену воды по умолчанию
		/// или если для точки доставки не указана вода по умолчанию
		/// или если среди товаров в заказе имеется вода по умолчанию,
		/// <c>false</c> если в заказе среди воды нет воды по умолчанию и
		/// пользователь не хочет её добавлять в заказ,
		/// <c>null</c> если данных для проверки не достаточно</returns>
		public virtual bool? DefaultWaterCheck(IInteractiveService interactiveService)
		{
			var res = IsWrongWater(out _, out string message);
			if(res == true)
			{
				return interactiveService.Question(message);
			}

			return !res;
		}

		/// <summary>
		/// Проверка на наличие воды по умолчанию в заказе для выбранной
		/// точки доставки и формирование сообщения о возможном штрафе
		/// </summary>
		/// <returns><c>false</c> если для точки доставки не указана вода по
		/// умолчанию, или если в заказе есть какая-либо 19л вода, среди которой
		/// имеется вода по умолчанию <c>true</c> если в точке доставки указана
		/// вода по умолчанию и в заказе есть какая-либо 19л вода, среди которой
		/// этой умолчальной нет <c>null</c> если проверка не может быть
		/// выполнена ввиду отсутствия каких-либо данных</returns>
		public virtual bool? IsWrongWater(out string title, out string msg)
		{
			title = msg = string.Empty;
			if(DeliveryPoint == null)
				return null;
			Nomenclature defaultWater = DeliveryPoint.DefaultWaterNomenclature;
			var orderWaters = ObservableOrderItems.Where(w => w.Nomenclature.Category == NomenclatureCategory.water && !w.Nomenclature.IsDisposableTare);

			//Если имеется для точки доставки номенклатура по умолчанию,
			//если имеется вода в заказе и ни одна 19 литровая вода в заказе
			//не совпадает с номенклатурой по умолчанию, то сообщение о штрафе!
			if(defaultWater != null
			   && orderWaters.Any()
			   && !ObservableOrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.water && !i.Nomenclature.IsDisposableTare
												   && i.Nomenclature == defaultWater)) {

				//список вод в заказе за исключением дефолтной для сообщения о штрафе
				string waterInOrder = string.Empty;
				foreach(var item in orderWaters) {
					if(item.Nomenclature != defaultWater)
						waterInOrder += string.Format(",\n\t'{0}'", item.Nomenclature.ShortOrFullName);
				}
				waterInOrder = waterInOrder.TrimStart(',');
				title = "Внимание!";
				string header = "Есть риск получить <span foreground=\"Red\" size=\"x-large\">ШТРАФ</span>!\n";
				string text = string.Format("Клиент '{0}' для адреса '{1}' заказывает фиксировано воду \n'{2}'.\nВ заказе же вы указали: {3}. \nДля подтверждения что это не ошибка, нажмите 'Да'.",
											Client.Name,
											DeliveryPoint.ShortAddress,
											defaultWater.ShortOrFullName,
											waterInOrder);
				msg = header + text;
				return true;
			}
			return false;
		}


		/// <summary>
		/// Закрывает заказ с самовывозом если по всем документам самовывоза со
		/// склада все отгружено, и произведена оплата
		/// </summary>
		public virtual bool TryCloseSelfDeliveryOrderWithCallTask(IUnitOfWork uow, INomenclatureSettings nomenclatureSettings, IRouteListItemRepository routeListItemRepository, ISelfDeliveryRepository selfDeliveryRepository, ICashRepository cashRepository, ICallTaskWorker callTaskWorker, SelfDeliveryDocument closingDocument = null)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			bool isNotShipped = !IsFullyShippedSelfDeliveryOrder(uow, selfDeliveryRepository, closingDocument);

			if(!isNotShipped)
				UpdateBottlesMovementOperationWithoutDelivery(UoW, nomenclatureSettings, routeListItemRepository, cashRepository);
			else
				return false;

			if(OrderStatus != OrderStatus.OnLoading)
				return false;

			bool isFullyPaid = SelfDeliveryIsFullyPaid(cashRepository);

			switch(PaymentType)
			{
				case PaymentType.Cash:
					ChangeStatusAndCreateTasks(isFullyPaid ? OrderStatus.Closed : OrderStatus.WaitForPayment, callTaskWorker);
					break;
				case PaymentType.Cashless:
				case PaymentType.PaidOnline:
				case PaymentType.Terminal:
					ChangeStatusAndCreateTasks(PayAfterShipment ? OrderStatus.WaitForPayment : OrderStatus.Closed, callTaskWorker);
					break;
				case PaymentType.SmsQR:
					ChangeStatusAndCreateTasks(
						PayAfterShipment 
						? OnlinePaymentNumber != null 
							? OrderStatus.Closed 
							: OrderStatus.WaitForPayment
						: OrderStatus.Closed, 
						callTaskWorker);
					break;
				case PaymentType.Barter:
				case PaymentType.ContractDocumentation:
					ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
					break;
			}
			//обновление актуальных кол-в из документов самовывоза, включая не сохранённый
			//документ, откуда был вызов метода
			UpdateSelfDeliveryActualCounts(closingDocument);
			return true;
		}

		/// <summary>
		/// Закрывает заказ с самовывозом если по всем документам самовывоза со
		/// склада все отгружено, и произведена оплата
		/// </summary>
		public virtual bool TryCloseSelfDeliveryPayAfterShipmentOrder(
			IUnitOfWork uow,
			INomenclatureSettings nomenclatureSettings,
			IRouteListItemRepository routeListItemRepository,
			ISelfDeliveryRepository selfDeliveryRepository,
			ICashRepository cashRepository,
			SelfDeliveryDocument closingDocument = null)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			bool isNotShipped = !IsFullyShippedSelfDeliveryOrder(uow, selfDeliveryRepository, closingDocument);

			if(!isNotShipped)
				UpdateBottlesMovementOperationWithoutDelivery(UoW, nomenclatureSettings, routeListItemRepository, cashRepository);
			else
				return false;

			if(OrderStatus == OrderStatus.WaitForPayment && PayAfterShipment)
			{
				bool isFullyPaid = SelfDeliveryIsFullyPaid(cashRepository);

				switch(PaymentType)
				{
					case PaymentType.Cash:
						ChangeStatus(isFullyPaid ? OrderStatus.Closed : OrderStatus.WaitForPayment);
						break;
					case PaymentType.Cashless:
					case PaymentType.PaidOnline:
						ChangeStatus(OrderStatus.Closed);
						break;
					case PaymentType.SmsQR:
						ChangeStatus(OrderStatus.Closed);
						break;
					case PaymentType.Barter:
					case PaymentType.ContractDocumentation:
						ChangeStatus(OrderStatus.Closed);
						break;
				}
				//обновление актуальных кол-в из документов самовывоза, включая не сохранённый
				//документ, откуда был вызов метода
				UpdateSelfDeliveryActualCounts(closingDocument);
				return true;

			}

			return false;
		}


		private void DeleteBottlesMovementOperation(IUnitOfWork uow)
		{
			if(BottlesMovementOperation != null) {
				uow.Delete(BottlesMovementOperation);
				BottlesMovementOperation = null;
			}
		}


		public virtual bool UpdateBottleMovementOperation(IUnitOfWork uow, INomenclatureSettings nomenclatureSettings, int returnByStock, int? forfeitQuantity = null)
		{
			if(IsContractCloser)
				return false;

			int amountDelivered = (int)OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water
					&& !item.Nomenclature.IsDisposableTare
					&& item.Nomenclature.TareVolume == TareVolume.Vol19L)
				.Sum(item => item.ActualCount ?? 0);

			int amountDeliveredInDisposableTare = (int)OrderItems
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water
					&& item.Nomenclature.IsDisposableTare
					&& item.Nomenclature.TareVolume == TareVolume.Vol19L)
				.Sum(item => item.ActualCount ?? 0);

			if(forfeitQuantity == null) {
				forfeitQuantity = (int)OrderItems.Where(i => i.Nomenclature.Id == nomenclatureSettings.ForfeitId)
							.Select(i => i?.ActualCount ?? 0)
							.Sum();
			}

			bool isValidCondition = amountDelivered != 0 || amountDeliveredInDisposableTare != 0;
			isValidCondition |= returnByStock > 0;
			isValidCondition |= forfeitQuantity > 0;
			isValidCondition &= !_orderRepository.GetUndeliveryStatuses().Contains(OrderStatus);

			if(isValidCondition) {
				if(BottlesMovementOperation == null) {
					BottlesMovementOperation = new BottlesMovementOperation {
						Order = this,
					};
				}
				BottlesMovementOperation.Counterparty = Client;
				BottlesMovementOperation.DeliveryPoint = DeliveryPoint;
				BottlesMovementOperation.OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59);
				BottlesMovementOperation.Delivered = amountDelivered;
				BottlesMovementOperation.DeliveredInDisposableTare = amountDeliveredInDisposableTare;
				BottlesMovementOperation.Returned = returnByStock + forfeitQuantity.Value;
				uow.Save(BottlesMovementOperation);
			} else {
				DeleteBottlesMovementOperation(uow);
			}

			return true;
		}

		/// <summary>
		/// Создание операций перемещения бутылей для заказов без доставки
		/// </summary>
		public virtual void UpdateBottlesMovementOperationWithoutDelivery(IUnitOfWork uow, INomenclatureSettings nomenclatureSettings, IRouteListItemRepository routeListItemRepository, ICashRepository cashRepository, decimal incomeCash = 0, decimal expenseCash = 0)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));
			if(nomenclatureSettings == null)
				throw new ArgumentNullException(nameof(nomenclatureSettings));

			//По заказам, у которых проставлен крыжик "Закрывашка по контракту",
			//не должны создаваться операции перемещения тары
			if(IsContractCloser) {
				DeleteBottlesMovementOperation(uow);
				return;
			}

			if(routeListItemRepository.HasRouteListItemsForOrder(uow, this))
				return;

			foreach(OrderItem item in OrderItems)
			{
				item.PreserveActualCount();
			}

			int? forfeitQuantity = null;

			if(!SelfDelivery || SelfDeliveryIsFullyPaid(cashRepository, incomeCash, expenseCash))
				forfeitQuantity = (int)OrderItems.Where(i => i.Nomenclature.Id == nomenclatureSettings.ForfeitId)
											.Select(i => i.ActualCount ?? 0)
											.Sum();

			UpdateBottleMovementOperation(uow, nomenclatureSettings, ReturnedTare ?? 0, forfeitQuantity ?? 0);
		}

		public virtual void ChangePaymentTypeToByCardTerminal (ICallTaskWorker callTaskWorker)
		{
			PaymentType = PaymentType.Terminal;
			ChangeStatusAndCreateTasks(!PayAfterShipment ? OrderStatus.Accepted : OrderStatus.Closed, callTaskWorker);
		}

		public virtual void ChangePaymentTypeToOnline(ICallTaskWorker callTaskWorker)
		{
			PaymentType = PaymentType.PaidOnline;
			ChangeStatusAndCreateTasks(!PayAfterShipment ? OrderStatus.Accepted : OrderStatus.Closed, callTaskWorker);
		}

		#region Работа с документами

		public virtual void UpdateDocuments()
		{
			if(Client is null)
			{
				return;
			}
			
			CheckAndCreateDocuments(_emailService.GetRequiredDocumentTypes(this));
		}

		public virtual void UpdateCertificates(out List<Nomenclature> nomenclaturesNeedUpdate)
		{
			nomenclaturesNeedUpdate = new List<Nomenclature>();
			if(AddCertificates && DeliveryDate.HasValue) {
				IList<Certificate> newList = new List<Certificate>();
				foreach(var item in _nomenclatureRepository.GetDictionaryWithCertificatesForNomenclatures(UoW, OrderItems.Select(i => i.Nomenclature).ToArray())) {
					if(item.Value.All(c => c.IsArchive || c.ExpirationDate.HasValue && c.ExpirationDate.Value < DeliveryDate))
						nomenclaturesNeedUpdate.Add(item.Key);
					else
						newList.Add(item.Value.FirstOrDefault(c => c.ExpirationDate == item.Value.Max(cert => cert.ExpirationDate)));
				}

				newList = newList.Distinct().ToList();
				var oldList = new List<Certificate>();

				foreach(var cer in OrderDocuments.Where(d => d.Type == OrderDocumentType.ProductCertificate)
												 .Cast<NomenclatureCertificateDocument>()
												 .Select(c => c.Certificate))
					oldList.Add(cer);

				foreach(var cer in oldList) {
					if(!newList.Any(c => c == cer)) {
						var removingDoc = OrderDocuments.Where(d => d.Type == OrderDocumentType.ProductCertificate)
														.Cast<NomenclatureCertificateDocument>()
														.FirstOrDefault(d => d.Certificate == cer);
						ObservableOrderDocuments.Remove(removingDoc);
					}
				}

				foreach(var cer in newList) {
					if(!oldList.Any(c => c == cer))
						ObservableOrderDocuments.Add(
							new NomenclatureCertificateDocument {
								Order = this,
								AttachedToOrder = this,
								Certificate = cer
							}
						);
				}
			}

			if(!AddCertificates)
			{
				ObservableOrderDocuments.Where(od => od.Type == OrderDocumentType.ProductCertificate).ToList()
					.ForEach(od => ObservableOrderDocuments.Remove(od));
			}
		}

		public virtual void CheckDocumentExportPermissions()
		{
			var updDoc = OrderDocuments.OfType<UPDDocument>().FirstOrDefault();
			if(updDoc != null && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel")) {
				updDoc.RestrictedOutputPresentationTypes = new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };
			}

			var specialUpdDoc = OrderDocuments.OfType<SpecialUPDDocument>().FirstOrDefault();
			if(specialUpdDoc != null && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel")) {
				specialUpdDoc.RestrictedOutputPresentationTypes = new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };
			}
		}

		private void CheckAndCreateDocuments(params OrderDocumentType[] needed)
		{
			var docsOfOrder = typeof(OrderDocumentType).GetFields()
													   .Where(x => x.GetCustomAttributes(typeof(DocumentOfOrderAttribute), false).Any())
													   .Select(x => (OrderDocumentType)x.GetValue(null))
													   .ToArray();

			if(needed.Any(x => !docsOfOrder.Contains(x)))
				throw new ArgumentException($@"В метод можно передавать только типы документов помеченные атрибутом {nameof(DocumentOfOrderAttribute)}", nameof(needed));

			var needCreate = needed.ToList();
			foreach(var doc in OrderDocuments.Where(d => d.Order?.Id == Id && docsOfOrder.Contains(d.Type)).ToList()) {
				var needUpdateUpdNumber =
					(doc.Type == OrderDocumentType.UPD || doc.Type == OrderDocumentType.SpecialUPD)
					&& doc.DocumentOrganizationCounter?.Organization?.Id != Contract?.Organization?.Id;
				if(needed.Contains(doc.Type) && !needUpdateUpdNumber)
					needCreate.Remove(doc.Type);
				else
					ObservableOrderDocuments.Remove(doc);
				if(OrderDocuments.Any(x => x.Order?.Id == Id && x.Id != doc.Id && x.Type == doc.Type)) {
					ObservableOrderDocuments.Remove(doc);
				}
			}
			//Создаем отсутствующие
			foreach(var type in needCreate) 
			{
				if(ObservableOrderDocuments.Any(x => x.Order?.Id == Id && x.Type == type))
				{
					continue;
				}

				ObservableOrderDocuments.Add(CreateDocumentOfOrder(type));
			}
			CheckDocumentCount(this);
			UpdateIfUpdUsing(this);
		}

		private void CheckDocumentCount(Order order)
		{
			var torg12document = order.ObservableOrderDocuments.FirstOrDefault(x => x is Torg12Document && x.Type == OrderDocumentType.Torg12);
			if(torg12document != null && IsCashlessPaymentTypeAndOrganizationWithoutVAT) {
				((Torg12Document)torg12document).CopiesToPrint = 2;
			}
		}

		private void UpdateIfUpdUsing(Order order)
		{
			if(!order.OrderDocuments.Any(d => d.Order.Id == order.Id && (d.Type == OrderDocumentType.UPD || d.Type == OrderDocumentType.SpecialUPD)))
			{
				return;
			}
			
			var targetTypesForUpdReference = new List<OrderDocumentType>()
			{
				OrderDocumentType.Bill, 
				OrderDocumentType.SpecialBill, 
				OrderDocumentType.DoneWorkReport, 
				OrderDocumentType.EquipmentTransfer,
				OrderDocumentType.DriverTicket
			};
			
			var upd = order.OrderDocuments.First(d => d.Order.Id == order.Id && (d.Type == OrderDocumentType.UPD || d.Type == OrderDocumentType.SpecialUPD));
				
			foreach(var orderDocument in order.OrderDocuments)
			{
				if(targetTypesForUpdReference.Any(t => t == orderDocument.Type))
				{
					orderDocument.DocumentOrganizationCounter = upd.DocumentOrganizationCounter;
				}
			}
		}

		private OrderDocument CreateDocumentOfOrder(OrderDocumentType type)
		{
			var contractOrganizationId = Contract?.Organization?.Id ?? 0;

			OrderDocument newDoc;

			switch(type)
			{
				case OrderDocumentType.Bill:
					newDoc = new BillDocument();
					break;
				case OrderDocumentType.SpecialBill:
					newDoc = new SpecialBillDocument();
					break;
				case OrderDocumentType.UPD:
				{
					var updOrderCounter = _documentOrganizationCounterRepository.GetDocumentOrganizationCounterByOrder(UoW, this, contractOrganizationId);
					
					var updCounter = _documentOrganizationCounterRepository
						.GetMaxDocumentOrganizationCounterOnYear(UoW, DeliveryDate.Value, Contract?.Organization);
					
					var updCounterValue = updOrderCounter?.Counter ?? (updCounter == null
						? 1
						: updCounter.Counter + 1);

					var documentOrganizationCounter = updOrderCounter ?? new DocumentOrganizationCounter()
					{
						Organization = Contract?.Organization,
						CounterDateYear = DeliveryDate?.Year,
						Counter = updCounterValue,
						DocumentNumber = DocumentNumberBuilder.BuildDocumentNumber(Contract?.Organization, DeliveryDate.Value, updCounterValue),
						Order = this
					};

					UoW.Save(documentOrganizationCounter);

					var updDocument = new UPDDocument
					{
						DocumentOrganizationCounter = documentOrganizationCounter
					};

					if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel"))
						updDocument.RestrictedOutputPresentationTypes =
							new[] { OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };

					newDoc = updDocument;
			}

			break;
				case OrderDocumentType.SpecialUPD:
				{
					var updOrderCounter = _documentOrganizationCounterRepository.GetDocumentOrganizationCounterByOrder(UoW, this, contractOrganizationId);
					
					var updCounter = _documentOrganizationCounterRepository
						.GetMaxDocumentOrganizationCounterOnYear(UoW, DeliveryDate.Value, Contract?.Organization);
					
					var specialUpdCounterValue = updOrderCounter?.Counter ?? (updCounter == null
						? 1
						: updCounter.Counter + 1);

					var documentOrganizationCounter = updOrderCounter ?? new DocumentOrganizationCounter()
					{
						Organization = Contract?.Organization,
						CounterDateYear = DeliveryDate?.Year,
						Counter = specialUpdCounterValue,
						DocumentNumber = DocumentNumberBuilder.BuildDocumentNumber(Contract?.Organization, DeliveryDate.Value, specialUpdCounterValue),
						Order = this
					};

					UoW.Save(documentOrganizationCounter);

					var specialUpdDocument = new SpecialUPDDocument
					{
						DocumentOrganizationCounter = documentOrganizationCounter
					};

					if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_export_UPD_to_excel"))
						specialUpdDocument.RestrictedOutputPresentationTypes = new[]
							{ OutputPresentationType.ExcelTableOnly, OutputPresentationType.Excel2007 };

					newDoc = specialUpdDocument;
				}
					break;
				case OrderDocumentType.Invoice:
					newDoc = new InvoiceDocument();
					break;
				case OrderDocumentType.InvoiceBarter:
					newDoc = new InvoiceBarterDocument();
					break;
				case OrderDocumentType.InvoiceContractDoc:
					newDoc = new InvoiceContractDoc();
					break;
				case OrderDocumentType.Torg12:
					newDoc = new Torg12Document();
					break;
				case OrderDocumentType.ShetFactura:
					newDoc = new ShetFacturaDocument();
					break;
				case OrderDocumentType.DriverTicket:
					newDoc = new DriverTicketDocument();
					break;
				case OrderDocumentType.DoneWorkReport:
					newDoc = new DoneWorkDocument();
					break;
				case OrderDocumentType.EquipmentTransfer:
					newDoc = new EquipmentTransferDocument();
					break;
				case OrderDocumentType.EquipmentReturn:
					newDoc = new EquipmentReturnDocument();
					break;
				case OrderDocumentType.TransportInvoice:
					newDoc = new TransportInvoiceDocument();
					break;
				case OrderDocumentType.Torg2:
					newDoc = new Torg2Document();
					break;
				case OrderDocumentType.AssemblyList:
					newDoc = new AssemblyListDocument();
					break;
				case OrderDocumentType.LetterOfDebt:
					newDoc = new LetterOfDebtDocument();
					break;
				default:
					throw new NotSupportedException("Не поддерживаемый тип документа");
			}
			newDoc.Order = newDoc.AttachedToOrder = this;
			return newDoc;
		}

		#endregion

		/// <summary>
		/// Возврат первого попавшегося контакта из цепочки:<br/>
		/// 0. Почта для чеков в контрагенте<br/>
		/// 1. Почта для счетов в контрагенте<br/>
		/// 2. Телефон для чеков в точке доставки<br/>
		/// 3. Телефон для чеков в контрагенте<br/>
		/// 4. Телефон личный в ТД<br/>
		/// 5. Телефон личный в контрагенте<br/>
		/// 6. Иная почта в контрагенте<br/>
		/// 7. Городской телефон в ТД<br/>
		/// 8. Городской телефон в контрагенте<br/>
		/// </summary>
		/// <returns>Контакт с минимальным весом.<br/>Телефоны возвращает в формате +7</returns>
		public virtual string GetContact()
		{
			if(Client == null)
			{
				return null;
			}

			//Dictionary<вес контакта, контакт>
			Dictionary<int, string> contacts = new Dictionary<int, string>();

			try
			{
				if(!SelfDelivery && DeliveryPoint != null && DeliveryPoint.Phones.Any())
				{
					var deliveryPointReceiptPhone = DeliveryPoint.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts
							&& !p.IsArchive);
					
					if(deliveryPointReceiptPhone != null)
					{
						contacts[2] = "+7" + deliveryPointReceiptPhone.DigitsNumber;
					}

					var phone = DeliveryPoint.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.DigitsNumber.Substring(0, 1) == "9"
							&& !p.IsArchive);
					
					if(phone != null)
					{
						contacts[4] = "+7" + phone.DigitsNumber;
					}
					else if(DeliveryPoint.Phones.Any(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& !p.IsArchive))
					{
						contacts[7] = "+7" + DeliveryPoint.Phones.FirstOrDefault(
							p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
								&& !p.IsArchive).DigitsNumber;
					}
				}
			}
			catch(GenericADOException ex)
			{
				logger.Error(ex.Message);
			}

			try
			{
				if(Client.Phones.Any())
				{
					var clientReceiptPhone = Client.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts
							&& !p.IsArchive);
					
					if(clientReceiptPhone != null)
					{
						contacts[3] = "+7" + clientReceiptPhone.DigitsNumber;
					}

					var phone = Client.Phones.FirstOrDefault(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& p.DigitsNumber.Substring(0, 1) == "9"
							&& !p.IsArchive);
					
					if(phone != null)
					{
						contacts[5] = "+7" + phone.DigitsNumber;
					}
					else if(Client.Phones.Any(
						p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
							&& !p.IsArchive))
					{
						contacts[8] = "+7" + Client.Phones.FirstOrDefault(
							p => !string.IsNullOrWhiteSpace(p.DigitsNumber)
								&& !p.IsArchive).DigitsNumber;
					}
				}
			}
			catch(GenericADOException ex)
			{
				logger.Error(ex.Message);
			}
			try
			{
				if(Client.Emails.Any())
				{
					var receiptEmail = Client.Emails.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Address)
						 && e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)?.Address;

					if(receiptEmail != null)
					{
						contacts[0] = receiptEmail;
					}

					var billsEmail = Client.Emails.FirstOrDefault(
						e => !string.IsNullOrWhiteSpace(e.Address)
							&& e.EmailType?.EmailPurpose == EmailPurpose.ForBills)?.Address;
					
					if(billsEmail != null)
					{
						contacts[1] = billsEmail;
					}

					var email = Client.Emails.FirstOrDefault(e =>
						!string.IsNullOrWhiteSpace(e.Address)
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForBills
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForReceipts)
						?.Address;
					
					if(email != null)
					{
						contacts[6] = email;
					}
				}
			}
			catch(GenericADOException ex)
			{
				logger.Error(ex.Message);
			}

			if(!contacts.Any())
			{
				return null;
			}

			var onlyWithValidPhones = contacts.Where(x =>
				(x.Value.StartsWith("+7")
					&& x.Value.Length == 12)
				|| !x.Value.StartsWith("+7"));

			if(!onlyWithValidPhones.Any())
			{
				throw new InvalidOperationException($"Не удалось подобрать контакт для заказа {Id}");
			}

			int minWeight = onlyWithValidPhones.Min(c => c.Key);
			var contact = contacts[minWeight];

			if(string.IsNullOrWhiteSpace(contact))
			{
				throw new InvalidOperationException($"Не удалось подобрать контакт для заказа {Id}");
			}

			return contact;
		}

		public virtual void SaveOrderComment()
		{
			if(Id == 0) return;

			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateForRoot<Order>(Id, "Кнопка сохранить только комментарий к заказу")) {
				uow.Root.Comment = Comment;
				uow.Save();
				uow.Commit();
			}
			UoW.Session.Refresh(this);
		}

		public virtual void SaveEntity(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Employee currentEmployee,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			bool needUpdateContract = true
		)
		{
			SetFirstOrder();

			if(FirstDeliveryDate is null)
			{
				FirstDeliveryDate = DeliveryDate;
			}

			if(!IsLoadedFrom1C && needUpdateContract)
			{
				contractUpdater.UpdateContract(uow, this);
			}

			LastEditor = currentEmployee;
			LastEditedTime = DateTime.Now;
			
			if(TareNonReturnReason is null)
			{
				ParseTareReason();
			}
			
			ClearPromotionSets();
			orderDailyNumberController.UpdateDailyNumber(this);
			paymentFromBankClientController.UpdateAllocatedSum(UoW, this);
			paymentFromBankClientController.ReturnAllocatedSumToClientBalanceIfChangedPaymentTypeFromCashless(UoW, this);

			var hasPromotionalSetForNewClient = PromotionalSets.Any(x => x.PromotionalSetForNewClients);

			if(hasPromotionalSetForNewClient
			   && ContactlessDelivery
			   && !SelfDelivery
			   && !Comment.Contains(_generalSettingsParameters.OrderAutoComment))
			{
				Comment = $"{_generalSettingsParameters.OrderAutoComment}{Environment.NewLine}{Comment}";
			}

			if(uow is IUnitOfWorkGeneric<Order>)
			{
				uow.Save();
			}
			else
			{
				uow.Save(this);
			}
		}

		public virtual void RemoveReturnTareReason()
		{
			if (ReturnTareReason != null)
				ReturnTareReason = null;

			if(ReturnTareReasonCategory != null)
				ReturnTareReasonCategory = null;
		}

		public virtual void SetActualCountsToZeroOnCanceled()
		{
			foreach(var item in OrderItems)
			{
				if(!item.OriginalDiscountMoney.HasValue || !item.OriginalDiscount.HasValue)
				{
					item.OriginalDiscountMoney = item.DiscountMoney > 0 ? (decimal?)item.DiscountMoney : null;
					item.OriginalDiscount = item.Discount > 0 ? (decimal?)item.Discount : null;
					item.OriginalDiscountReason = (item.DiscountMoney > 0 || item.Discount > 0) ? item.DiscountReason : null;
				}

				item.SetActualCountZero();
			}

			foreach(var equip in OrderEquipments)
			{
				equip.ActualCount = 0;
			}

			foreach(var deposit in OrderDepositItems)
			{
				deposit.ActualCount = 0;
			}
		}

		public virtual void RestoreOrder()
		{
			foreach(var item in OrderItems)
			{
				item.RestoreOriginalDiscountFromRestoreOrder();
			}

			foreach(var equip in OrderEquipments)
			{
				equip.ActualCount = null;
			}

			foreach(var deposit in OrderDepositItems)
			{
				deposit.ActualCount = null;
			}
		}

		/// <summary>
		/// Возвращает список со всеми товарами, которые нужно доставить клиенту
		/// </summary>
		/// <returns></returns>
		public virtual IList<NomenclatureAmountNode> GetAllGoodsToDeliver(bool isActualCount = false)
		{
			var result = new List<NomenclatureAmountNode>();

			foreach(var orderItem in OrderItems.Where(x => Nomenclature.GetCategoriesForShipment().Contains(x.Nomenclature.Category)))
			{
				var amount = isActualCount && orderItem.ActualCount.HasValue ? orderItem.ActualCount.Value : orderItem.Count;

				var found = result.FirstOrDefault(x => x.NomenclatureId == orderItem.Nomenclature.Id);
				if(found != null)
				{
					found.Amount += amount;
				}
				else
				{
					result.Add(new NomenclatureAmountNode
					{
						NomenclatureId = orderItem.Nomenclature.Id,
						Nomenclature = orderItem.Nomenclature,
						Amount = amount
				});
				}
			}

			foreach(var equipment in OrderEquipments.Where(x => x.Direction == Direction.Deliver
						&& Nomenclature.GetCategoriesForShipment().Contains(x.Nomenclature.Category)))
			{
				var amount = isActualCount && equipment.ActualCount.HasValue ? equipment.ActualCount.Value : equipment.Count;

				var found = result.FirstOrDefault(x => x.NomenclatureId == equipment.Nomenclature.Id);
				if(found != null)
				{
					found.Amount += amount;
				}
				else
				{
					result.Add(new NomenclatureAmountNode
					{
						NomenclatureId = equipment.Nomenclature.Id,
						Nomenclature = equipment.Nomenclature,
						Amount = amount
					});
				}
			}
			return result;
		}

		public virtual void SetNeedToRecendEdoUpd(IUnitOfWorkFactory uowFactory)
		{
			var userCanResendUpd = ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_resend_edo_documents");
			if(!userCanResendUpd)
			{
				InteractiveService.ShowMessage(ImportanceLevel.Warning, "Текущий пользователь не имеет права повторной отправки УПД");
				return;
			}

			if(OrderPaymentStatus == OrderPaymentStatus.Paid)
			{
				if(!ServicesConfig.InteractiveService.Question(
					$"Счет по заказу №{Id} оплачен.\r\nПроверьте, пожалуйста, статус УПД в ЭДО перед повторной отправкой на предмет аннулирован/не аннулирован, подписан/не подписан.\r\n\r\n" +
					$"Вы уверены, что хотите отправить повторно?"))
				{
					return;
				}
			}

			using(var uow = uowFactory.CreateWithoutRoot())
			{
				var edoDocumentsActions = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(x => x.Order.Id == Id)
					.FirstOrDefault();

				if(edoDocumentsActions == null)
				{
					edoDocumentsActions = new OrderEdoTrueMarkDocumentsActions();
					edoDocumentsActions.Order = this;
				}

				edoDocumentsActions.IsNeedToResendEdoUpd = true;
				edoDocumentsActions.Created = DateTime.Now;

				var orderLastTrueMarkDocument = uow.GetAll<TrueMarkDocument>()
					.Where(x => x.Order.Id == Id)
					.OrderByDescending(x => x.CreationDate)
					.FirstOrDefault();

				if(orderLastTrueMarkDocument != null 
					&& orderLastTrueMarkDocument.Type != TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation)
				{
					edoDocumentsActions.IsNeedToCancelTrueMarkDocument = true;
				}

				uow.Save(edoDocumentsActions);
				uow.Commit();
			}				
		}

		#endregion

		#region Аренда

        #region NonFreeRent

        public virtual void AddNonFreeRent(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature)
		{
			OrderItem orderRentDepositItem = GetExistingNonFreeRentDepositItem(paidRentPackage);
			if(orderRentDepositItem == null) {
				orderRentDepositItem = OrderItem.CreateNewNonFreeRentDepositItem(this, paidRentPackage);
				AddOrderItem(uow, contractUpdater, orderRentDepositItem);
			}

			OrderItem orderRentServiceItem = GetExistingNonFreeRentServiceItem(paidRentPackage);
			if(orderRentServiceItem == null) {
				orderRentServiceItem = OrderItem.CreateNewNonFreeRentServiceItem(this, paidRentPackage);
				AddOrderItem(uow, contractUpdater, orderRentServiceItem);
			}

			OrderEquipment orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
			if (orderRentEquipment == null) {
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
				ObservableOrderEquipments.Add(orderRentEquipment);
			} else {
				orderRentEquipment.Count++;
			}

			UpdateRentsCount();

			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		private OrderItem GetExistingNonFreeRentDepositItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentDepositItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.NonFreeRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem)
				.FirstOrDefault();
			return orderRentDepositItem;
		}

		private OrderItem GetExistingNonFreeRentServiceItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentServiceItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.NonFreeRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentServiceItem)
				.FirstOrDefault();
			return orderRentServiceItem;
		}

		#endregion NonFreeRent

		#region DailyRent

		public virtual void AddDailyRent(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			PaidRentPackage paidRentPackage,
			Nomenclature equipmentNomenclature)
		{
			var orderRentDepositItem = GetExistingDailyRentDepositItem(paidRentPackage);
			if(orderRentDepositItem == null) {
				orderRentDepositItem = OrderItem.CreateNewDailyRentDepositItem(this, paidRentPackage);
				AddOrderItem(uow, contractUpdater, orderRentDepositItem);
			}

			var orderRentServiceItem = GetExistingDailyRentServiceItem(paidRentPackage);
			if(orderRentServiceItem == null) {
				orderRentServiceItem = OrderItem.CreateNewDailyRentServiceItem(this, paidRentPackage);
				AddOrderItem(uow, contractUpdater, orderRentServiceItem);
			}

			OrderEquipment orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
			if (orderRentEquipment == null) {
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem, orderRentServiceItem);
				ObservableOrderEquipments.Add(orderRentEquipment);
			} else {
				orderRentEquipment.Count++;
			}

			UpdateRentsCount();

			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		private OrderItem GetExistingDailyRentDepositItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentDepositItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.DailyRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem)
				.FirstOrDefault();
			return orderRentDepositItem;
		}

		private OrderItem GetExistingDailyRentServiceItem(PaidRentPackage paidRentPackage)
		{
			OrderItem orderRentServiceItem = OrderItems
				.Where(x => x.PaidRentPackage != null && x.PaidRentPackage.Id == paidRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.DailyRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentServiceItem)
				.FirstOrDefault();
			return orderRentServiceItem;
		}

		#endregion DailyRent

		#region FreeRent

		public virtual void AddFreeRent(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			FreeRentPackage freeRentPackage,
			Nomenclature equipmentNomenclature)
		{
			var orderRentDepositItem = GetExistingFreeRentDepositItem(freeRentPackage);
			if(orderRentDepositItem == null) {
				orderRentDepositItem = OrderItem.CreateNewFreeRentDepositItem(this, freeRentPackage);
				AddOrderItem(uow, contractUpdater, orderRentDepositItem);
			}

			var orderRentEquipment = GetExistingRentEquipmentItem(equipmentNomenclature, orderRentDepositItem);
			if (orderRentEquipment == null) {
				orderRentEquipment = CreateNewRentEquipmentItem(equipmentNomenclature, orderRentDepositItem);
				ObservableOrderEquipments.Add(orderRentEquipment);
			} else {
				orderRentEquipment.Count++;
			}

			UpdateRentsCount();

			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		private OrderItem GetExistingFreeRentDepositItem(FreeRentPackage freeRentPackage)
		{
			OrderItem orderRentDepositItem = OrderItems
				.Where(x => x.FreeRentPackage != null && x.FreeRentPackage.Id == freeRentPackage.Id)
				.Where(x => x.RentType == OrderRentType.FreeRent)
				.Where(x => x.OrderItemRentSubType == OrderItemRentSubType.RentDepositItem)
				.FirstOrDefault();
			return orderRentDepositItem;
		}

		#endregion FreeRent

		private OrderEquipment GetExistingRentEquipmentItem(Nomenclature nomenclature, OrderItem rentDepositItem, OrderItem rentServiceItem = null)
		{
			OrderEquipment rentEquipment = OrderEquipments
				.Where(x => x.Reason == Reason.Rent)
				.Where(x => x.Nomenclature == nomenclature)
				.Where(x => x.OrderRentDepositItem == rentDepositItem)
				.Where(x => x.OrderRentServiceItem == rentServiceItem)
				.FirstOrDefault();
			return rentEquipment;
		}

		private OrderEquipment CreateNewRentEquipmentItem(Nomenclature nomenclature, OrderItem rentDepositItem, OrderItem rentServiceItem = null)
		{
			OrderEquipment rentEquipment = new OrderEquipment {
				Order = this,
				Count = 1,
				Direction = Direction.Deliver,
				Nomenclature = nomenclature,
				Reason = Reason.Rent,
				DirectionReason = DirectionReason.Rent,
				OwnType = OwnTypes.Rent,
				OrderRentDepositItem = rentDepositItem,
				OrderRentServiceItem = rentServiceItem
			};
			return rentEquipment;
		}

		public virtual void UpdateRentsCount()
		{
			var orderRentalItems = OrderItems.Where(x => x.OrderItemRentSubType != OrderItemRentSubType.None).ToList();

			foreach(var orderItem in orderRentalItems)
			{
				if(!OrderItems.Contains(orderItem))
				{
					continue;
				}

				switch(orderItem.OrderItemRentSubType)
				{
					case OrderItemRentSubType.RentServiceItem:
						var totalEquipmentCountForService = GetRentEquipmentTotalCountForServiceItem(orderItem);
						orderItem.SetRentEquipmentCount(totalEquipmentCountForService);
						break;
					case OrderItemRentSubType.RentDepositItem:
						var totalEquipmentCountForDeposit = GetRentEquipmentTotalCountForDepositItem(orderItem);
						orderItem.SetRentEquipmentCount(totalEquipmentCountForDeposit);
						break;
				}
			}
		}

		private int GetRentEquipmentTotalCountForDepositItem(OrderItem orderRentDepositItem)
		{
			var totalCount = orderEquipments
				.Where(x => x.OrderRentDepositItem == orderRentDepositItem)
				.Sum(x => x.Count);

			return totalCount;
		}

		private int GetRentEquipmentTotalCountForServiceItem(OrderItem orderRentServiceItem)
		{
			var totalCount = orderEquipments
				.Where(x => x.OrderRentServiceItem == orderRentServiceItem)
				.Sum(x => x.Count);

			return totalCount;
		}

		#endregion Аренда

		#region Акции

		/// <summary>
		/// Можем применить акцию "Бутыль"?
		/// </summary>
		public virtual bool CanAddStockBottle(IOrderRepository orderRepository)
		{
			bool result = Client != null && orderRepository.GetFirstRealOrderForClientForActionBottle(UoW, this,Client) == null;
			if(result) {
				BottlesReturn = 0;
			}
			return result;
		}

		#endregion

		#region	Внутренние функции

		private bool HasMarkedAndUnmarkedOrderItems()
		{
			var hasMarkedOrderItem = false;
			var hasUnmarkedOrderItem = false;

			foreach(var orderItem in ObservableOrderItems)
			{
				if(!hasMarkedOrderItem)
				{
					hasMarkedOrderItem = orderItem.Nomenclature.Gtins.Any();
				}

				if(!hasUnmarkedOrderItem)
				{
					hasUnmarkedOrderItem = !orderItem.Nomenclature.Gtins.Any();
				}
			}

			return hasMarkedOrderItem && hasUnmarkedOrderItem;
		}

		private decimal GetFixedPrice(OrderItem item) => item.GetWaterFixedPrice() ?? default(decimal);

		private decimal GetNomenclaturePrice(OrderItem item, bool useAlternativePrice)
		{
			decimal nomenclaturePrice = 0M;
			if(item.Nomenclature.IsWater19L) {
				nomenclaturePrice = item.Nomenclature.GetPrice(GetTotalWater19LCount(doNotCountPresentsDiscount: true), useAlternativePrice);
			} else {
				nomenclaturePrice = item.Nomenclature.GetPrice(item.Count, useAlternativePrice);
			}
			return nomenclaturePrice;
		}

		private void ObservableOrderDepositItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
		}

		protected internal virtual void ObservableOrderItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(OrderSum));
			OnPropertyChanged(nameof(OrderCashSum));
			UpdateDocuments();
		}

		public virtual void UpdateCommentManagerInfo(Employee editor)
		{
			CommentOPManagerUpdatedAt = DateTime.Now;
			CommentOPManagerChangedBy = editor;
		}
		
		private void Recalculate()
		{
			RecalculateItemsPrice();
			UpdateRentsCount();
		}

		#endregion

		#region Для расчетов в логистике

		/// <summary>
		/// Время разгрузки в секундах.
		/// </summary>
		public virtual int CalculateTimeOnPoint(bool hasForwarder)
		{
			int byFormula = 3 * 60; //На подпись документво 3 мин.
			int bottels = Total19LBottlesToDeliver;
			if(!hasForwarder)
				byFormula += CalculateGoDoorCount(bottels, 2) * 100; //100 секун(5/3 минуты) на 2 бутыли без экспедитора.
			else
				byFormula += CalculateGoDoorCount(bottels, 4) * 1 * 60; //1 минута на 4 бутыли c экспедитором.

			return byFormula < 5 * 60 ? 5 * 60 : byFormula;
		}

		private int CalculateGoDoorCount(int bottles, int atTime) => bottles / atTime + (bottles % atTime > 0 ? 1 : 0);

		/// <summary>
		/// Расчёт веса товаров и оборудования к клиенту для этого заказа
		/// </summary>
		/// <returns>Вес</returns>
		/// <param name="includeGoods">Если <c>true</c>, то в расчёт веса будут включены товары.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт веса будет включено оборудование.</param>
		public virtual decimal FullWeight(bool includeGoods = true, bool includeEquipment = true)
		{
			decimal weight = 0;
			if(includeGoods)
				weight += OrderItems.Sum(x => x.Nomenclature.Weight * x.Count);
			if(includeEquipment)
				weight += OrderEquipments.Where(x => x.Direction == Direction.Deliver)
										 .Sum(x => x.Nomenclature.Weight * x.Count);
			return weight;
		}

		public virtual decimal GetSalesItemsWeight(bool includeGoods = true, bool includeEquipment = true)
		{
			decimal weight = OrderItems.Sum(x => x.Nomenclature.Weight * (x.ActualCount ?? x.Count));
			return weight;
		}

		/// <summary>
		/// Расчёт объёма товаров и оборудования к клиенту для этого заказа
		/// </summary>
		/// <returns>Объём</returns>
		/// <param name="includeGoods">Если <c>true</c>, то в расчёт веса будут включены товары.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт веса будет включено оборудование.</param>
		public virtual decimal FullVolume(bool includeGoods = true, bool includeEquipment = true)
		{
			decimal volume = 0;
			if(includeGoods)
				volume += OrderItems.Sum(x => x.Nomenclature.Volume * x.Count);
			if(includeEquipment)
				volume += OrderEquipments.Where(x => x.Direction == Direction.Deliver)
										 .Sum(x => x.Nomenclature.Volume * x.Count);
			return volume;
		}

		/// <summary>
		/// Расчёт объёма ВОЗВРАЩАЕМОГО оборудования (из тары включается только 19-литровая), имеющие наравление от клиента для этого заказа
		/// </summary>
		/// <param name="includeBottlesReturn">Если <c>true</c>, то в расчёт объема будут включены возвращаемые бутыли, количество которых указано в свойстве BottlesReturn.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт объема будет включено оборудование, имеющее направление "От клиента"</param>
		/// <param name="one19LitersBottleVolume">Расчетное значение объема одного 19-литрового бутыля</param>
		/// <returns>Объём</returns>
		public virtual decimal FullReverseVolume(bool includeBottlesReturn = true, bool includeEquipment = true, decimal one19LitersBottleVolume = 0.03645m)
		{
			decimal volume = 0;
			if (includeBottlesReturn)
			{
				volume += (BottlesReturn ?? 0) * one19LitersBottleVolume;
			}
			if (includeEquipment)
			{
				volume += OrderEquipments
					.Where(
						x => x.Direction == Direction.PickUp
						&& (x.Nomenclature.Category == NomenclatureCategory.equipment
							|| (x.Nomenclature.Category == NomenclatureCategory.bottle && x.Nomenclature.TareVolume == TareVolume.Vol19L)))
					.Sum(x => x.Nomenclature.Volume * x.Count);
			}
			return volume;
		}

		#endregion

		#region Статические

		public static OrderStatus[] StatusesToExport1c => new[] {
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock,			
			OrderStatus.Closed,					
		};

		public static PaymentType[] PaymentTypesFastDeliveryAvailableFor => new[]
		{
			PaymentType.Cash,
			PaymentType.PaidOnline,
			PaymentType.Terminal,
			PaymentType.DriverApplicationQR,
			PaymentType.SmsQR,
			PaymentType.Cashless
		};

		public static PaymentType[] PaymentTypesNeededOnlineOrder => new[]
		{
			PaymentType.Terminal,
			PaymentType.PaidOnline,
			PaymentType.SmsQR,
			PaymentType.DriverApplicationQR
		};

		private static readonly PaymentType[] _editablePaymentTypes = new[]
		{
			PaymentType.Cash,
			PaymentType.Terminal,
			PaymentType.DriverApplicationQR
		};

		public static PaymentType[] EditablePaymentTypes => _editablePaymentTypes;

		#endregion

		#region Операции

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork uow)
		{
			var bottleRefundDeposit = ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Bottles).Sum(x => x.ActualSum);
			var equipmentRefundDeposit = ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Equipment).Sum(x => x.ActualSum);
			var operations = UpdateDepositOperations(uow, equipmentRefundDeposit, bottleRefundDeposit);
			return operations;
		}

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork uow, decimal equipmentRefundDeposit, decimal bottleRefundDeposit)
		{
			if(IsContractCloser == true) {
				DepositOperations?.Clear();
				return null;
			}

			var result = new List<DepositOperation>();
			DepositOperation bottlesOperation = null;
			DepositOperation equipmentOperation = null;
			bottlesOperation = DepositOperations?.FirstOrDefault(x => x.DepositType == DepositType.Bottles);
			equipmentOperation = DepositOperations?.FirstOrDefault(x => x.DepositType == DepositType.Equipment);

			//Залоги
			var bottleReceivedDeposit = OrderItems.Where(x => x.Nomenclature.TypeOfDepositCategory == TypeOfDepositCategory.BottleDeposit)
												  .Sum(x => x.ActualSum);
			var equipmentReceivedDeposit = OrderItems.Where(x => x.Nomenclature.TypeOfDepositCategory == TypeOfDepositCategory.EquipmentDeposit)
													 .Sum(x => x.ActualSum);

			//ЗАЛОГИ ЗА БУТЫЛИ
			if(bottleReceivedDeposit != 0m || bottleRefundDeposit != 0m) {
				if(bottlesOperation == null) {
					bottlesOperation = new DepositOperation {
						Order = this,
						OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						DepositType = DepositType.Bottles,
						Counterparty = Client
					};
				}
				bottlesOperation.DeliveryPoint = DeliveryPoint;
				bottlesOperation.ReceivedDeposit = bottleReceivedDeposit;
				bottlesOperation.RefundDeposit = bottleRefundDeposit;
				result.Add(bottlesOperation);
			} else {
				if(bottlesOperation != null) {
					DepositOperations?.Remove(bottlesOperation);
					UoW.Delete(bottlesOperation);
				}
			}

			//ЗАЛОГИ ЗА ОБОРУДОВАНИЕ
			if(equipmentReceivedDeposit != 0m || equipmentRefundDeposit != 0m) {
				if(equipmentOperation == null) {
					equipmentOperation = new DepositOperation {
						Order = this,
						OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						DepositType = DepositType.Equipment,
						Counterparty = Client
					};
				}
				equipmentOperation.DeliveryPoint = DeliveryPoint;
				equipmentOperation.ReceivedDeposit = equipmentReceivedDeposit;
				equipmentOperation.RefundDeposit = equipmentRefundDeposit;
				result.Add(equipmentOperation);
			} else {
				if(equipmentOperation != null) {
					DepositOperations.Remove(equipmentOperation);
					UoW.Delete(equipmentOperation);
				}
			}

			result.ForEach(x => uow.Save(x));

			return result;
		}

		#endregion

		#region Доставка за час

		public virtual bool CanChangeFastDelivery => OrderStatus == OrderStatus.NewOrder;

		private Nomenclature _fastDeliveryNomenclature;
		private Nomenclature FastDeliveryNomenclature
		{
			get
			{
				if(_fastDeliveryNomenclature == null)
				{
					_fastDeliveryNomenclature = _nomenclatureRepository.GetFastDeliveryNomenclature(UoW);
				}

				return _fastDeliveryNomenclature;
			}
		}

		public virtual void AddFastDeliveryNomenclatureIfNeeded(IUnitOfWork uow, IOrderContractUpdater contractUpdater)
		{
			if(IsFastDelivery && orderItems.All(x => x.Nomenclature.Id != FastDeliveryNomenclature.Id))
			{
				var canApplyAlternativePrice = HasPermissionsForAlternativePrice
					&& FastDeliveryNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);

				AddOrderItem(
					uow,
					contractUpdater,
					OrderItem.CreateForSale(
						this, FastDeliveryNomenclature, 1, FastDeliveryNomenclature.GetPrice(1, canApplyAlternativePrice)));
			}
		}

		public virtual void RemoveFastDeliveryNomenclature(IUnitOfWork uow, IOrderContractUpdater contractUpdater)
		{
			var fastDeliveryItemToRemove =
					ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == FastDeliveryNomenclature.Id);

			RemoveOrderItem(uow, contractUpdater, fastDeliveryItemToRemove);
		}

		public virtual void ResetOrderItemsActualCounts()
		{
			foreach(var orderItem in ObservableOrderItems)
			{
				orderItem.PreserveActualCount(true);
			}
		}

		protected void ResetDepositItemsActualCounts()
		{
			foreach(var depositItem in OrderDepositItems)
			{
				depositItem.ActualCount = depositItem.Count;
			}
		}

		#endregion

		#region Точка доставки

		private int _educationalInstitutionDeliveryPointCategoryId;

		private int EducationalInstitutionDeliveryPointCategoryId
		{
			get
			{
				if(_educationalInstitutionDeliveryPointCategoryId == default(int))
				{
					var deliveryPointSettings = ScopeProvider.Scope.Resolve<IDeliveryPointSettings>();
					_educationalInstitutionDeliveryPointCategoryId = deliveryPointSettings.EducationalInstitutionDeliveryPointCategoryId;
				}

				return _educationalInstitutionDeliveryPointCategoryId;
			}
		}

		#endregion

		#region Правила сервисной доставки
		public virtual IList<int> GetAvailableDeliveryScheduleIds(bool isForMasterCall = false)
		{
			return isForMasterCall 
				? GetAvailableServiceDeliveryScheduleIds() 
				: GetAvailableDeliveryScheduleIds();
		}

		public virtual IList<int> GetAvailableDeliveryScheduleIds()
		{
			var availableDeliverySchedules = new List<int>();

			if(DeliveryPoint?.District != null)
			{
				availableDeliverySchedules = DeliveryPoint
					.District
					.GetAvailableDeliveryScheduleRestrictionsByDeliveryDate(DeliveryDate)
					.OrderBy(s => s.DeliverySchedule.DeliveryTime)
					.Select(r => r.DeliverySchedule.Id)
					.ToList();
			}

			return availableDeliverySchedules;
		}

		public virtual IList<int> GetAvailableServiceDeliveryScheduleIds()
		{
			var latitude = DeliveryPoint.Latitude;
			var longitude = DeliveryPoint.Longitude;

			var district = _deliveryRepository.GetServiceDistrictByCoordinates(UoW, latitude.Value, longitude.Value);

			var availableDeliverySchedules = new List<int>();

			if(district != null)
			{
				availableDeliverySchedules = district
					.GetAvailableServiceDeliveryScheduleRestrictionsByDeliveryDate(DeliveryDate)
					.OrderBy(s => s.DeliverySchedule.DeliveryTime)
					.Select(r => r.DeliverySchedule.Id)
					.ToList();
			}

			return availableDeliverySchedules;
		}
		#endregion Правила сервисной доставка

		public virtual bool IsOldServiceOrder => OrderAddressType == OrderAddressType.Service && CreateDate < new DateTime(2024, 10, 24);

		/// <summary>
		/// Добавление/удаление номенклатуры для вызова мастера в зависимости от типа адреса
		/// </summary>
		public virtual void UpdateMasterCallNomenclatureIfNeeded(IUnitOfWork unitOfWork, IOrderContractUpdater contractUpdater)
		{
			var masterCallNomenclature = _nomenclatureRepository.GetMasterCallNomenclature(unitOfWork);

			if(OrderAddressType == OrderAddressType.Service
				&& !SelfDelivery)
			{
				AddMasterCallNomenclatureIfNeeded(unitOfWork, contractUpdater, masterCallNomenclature);
			}
			else
			{
				RemoveMasterCallNomenclature(unitOfWork, contractUpdater, masterCallNomenclature);
			}
		}

		private void AddMasterCallNomenclatureIfNeeded(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature masterCallNomenclature)
		{
			if(OrderItems.Any(x => x.Nomenclature.Id == masterCallNomenclature.Id))
			{
				return;
			}

			var canApplyAlternativePrice = HasPermissionsForAlternativePrice
				&& masterCallNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);

			AddOrderItem(
				uow,
				contractUpdater,
				OrderItem.CreateForSale(this, masterCallNomenclature, 1, 0));
		}

		private void RemoveMasterCallNomenclature(
			IUnitOfWork uow,
			IOrderContractUpdater contractUpdater,
			Nomenclature masterCallNomenclature)
		{
			var fastDeliveryItemToRemove =
					ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == masterCallNomenclature.Id);

			RemoveOrderItem(uow, contractUpdater, fastDeliveryItemToRemove);
		}

		#region Obsolete

		[Obsolete("Должно быть не актуально после ввода новой системы расчёта ЗП (I-2150)")]
		public virtual decimal MoneyForMaster => ObservableOrderItems
			.Where(i => i.Nomenclature.Category == NomenclatureCategory.master && i.ActualCount.HasValue)
			.Sum(i => (decimal)i.Nomenclature.PercentForMaster / 100 * i.ActualCount.Value * i.Price);

		private string onRouteEditReason;

		[Display(Name = "Причина редактирования заказа")]
		[Obsolete("Кусок выпиленного функционала от I-1060. Даша сказала пока не удалять, но скрыть зачем-то.")]
		public virtual string OnRouteEditReason
		{
			get => onRouteEditReason;
			set => SetField(ref onRouteEditReason, value, () => OnRouteEditReason);
		}

		#endregion Obsolete
	}
}
