using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using NHibernate.Exceptions;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using QS.HistoryLog;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Contacts;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Payments;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Parameters;
using Vodovoz.Repositories.Client;
using Vodovoz.Repository.Client;
using Vodovoz.Services;
using Vodovoz.Tools.CallTasks;
using Vodovoz.Tools.Orders;

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
	public class Order : BusinessObjectBase<Order>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private IEmployeeRepository employeeRepository { get; set; } = EmployeeSingletonRepository.GetInstance();
		private IOrderRepository orderRepository { get; set; } = OrderSingletonRepository.GetInstance();

		public virtual IInteractiveQuestion TaskCreationQuestion { get; set; }

		#region Cвойства

		public virtual int Id { get; set; }

		public virtual IInteractiveService InteractiveService { get; set; }

		DateTime version;
		[Display(Name = "Версия")]
		public virtual DateTime Version {
			get => version;
			set => SetField(ref version, value, () => Version);
		}

		DateTime? createDate;
		[Display(Name = "Дата создания")]
		public virtual DateTime? CreateDate {
			get => createDate;
			set => SetField(ref createDate, value, () => CreateDate);
		}

		bool isFirstOrder;
		[Display(Name = "Первый заказ")]
		public virtual bool IsFirstOrder {
			get => isFirstOrder;
			set => SetField(ref isFirstOrder, value, () => IsFirstOrder);
		}

		OrderStatus orderStatus;

		[Display(Name = "Статус заказа")]
		public virtual OrderStatus OrderStatus {
			get => orderStatus;
			set => SetField(ref orderStatus, value, () => OrderStatus);
		}

		OrderPaymentStatus orderPaymentStatus;
		[Display(Name = "Статус оплаты заказа")]
		public virtual OrderPaymentStatus OrderPaymentStatus {
			get => orderPaymentStatus;
			set => SetField(ref orderPaymentStatus, value);
		}

		Employee author;

		[Display(Name = "Создатель заказа")]
		[IgnoreHistoryTrace]
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


		Counterparty client;
		[Display(Name = "Клиент")]
		public virtual Counterparty Client {
			get => client;
			set {
				if(value == client)
					return;
				if(orderRepository.GetOnClosingOrderStatuses().Contains(OrderStatus)) {
					OnChangeCounterparty(value);
				} else if(client != null && !CanChangeContractor()) {
					OnPropertyChanged(nameof(Client));
					if(InteractiveService == null)
						throw new InvalidOperationException("Нельзя изменить клиента для заполненного заказа.");

					InteractiveService.ShowMessage(ImportanceLevel.Warning,"Нельзя изменить клиента для заполненного заказа.");
					return;
				}

				if(SetField(ref client, value, () => Client)) {
					if(Client == null || (DeliveryPoint != null && NHibernate.NHibernateUtil.IsInitialized(Client.DeliveryPoints) && !Client.DeliveryPoints.Any(d => d.Id == DeliveryPoint.Id))) {
						//FIXME Убрать когда поймем что проблемы с пропаданием точек доставки нет.
						logger.Warn("Очищаем точку доставки, при установке клиента. Возможно это не нужно.");
						DeliveryPoint = null;
					}
				}
			}
		}

		DeliveryPoint deliveryPoint;

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint {
			get => deliveryPoint;
			set {
				if(SetField(ref deliveryPoint, value, () => DeliveryPoint) && value != null) {
					if(DeliverySchedule == null)
						DeliverySchedule = value.DeliverySchedule;

					if(Id == 0)
						AddCertificates = DeliveryPoint.AddCertificatesAlways || Client.FirstOrder == null;
				}
			}
		}

		DateTime? timeDelivered;

		[Display(Name = "Время доставки")]
		public virtual DateTime? TimeDelivered {
			get => timeDelivered;
			set => SetField(ref timeDelivered, value, () => TimeDelivered);
		}


		DateTime? deliveryDate;

		[Display(Name = "Дата доставки")]
		[HistoryDateOnly]
		public virtual DateTime? DeliveryDate {
			get => deliveryDate;
			set => SetField(ref deliveryDate, value, () => DeliveryDate);
		}

		DateTime billDate = DateTime.Now;

		[Display(Name = "Дата счета")]
		[HistoryDateOnly]
		public virtual DateTime BillDate {
			get => billDate;
			set => SetField(ref billDate, value, () => BillDate);
		}

		DeliverySchedule deliverySchedule;

		[Display(Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule {
			get => deliverySchedule;
			set => SetField(ref deliverySchedule, value, () => DeliverySchedule);
		}

		private string deliverySchedule1c;

		[Display(Name = "Время доставки из 1С")]
		public virtual string DeliverySchedule1c {
			get => string.IsNullOrWhiteSpace(deliverySchedule1c)
				  ? "Время доставки из 1С не загружено"
				  : deliverySchedule1c;
			set => SetField(ref deliverySchedule1c, value, () => DeliverySchedule1c);
		}

		bool selfDelivery;

		[Display(Name = "Самовывоз")]
		public virtual bool SelfDelivery {
			get => selfDelivery;
			set {
				if(SetField(ref selfDelivery, value, () => SelfDelivery) && value)
					IsContractCloser = false;
			}
		}

		bool payAfterShipment;

		[Display(Name = "Оплата после отгрузки")]
		public virtual bool PayAfterShipment {
			get => payAfterShipment;
			set => SetField(ref payAfterShipment, value, () => PayAfterShipment);
		}

		Employee loadAllowedBy;

		[Display(Name = "Отгрузку разрешил")]
		public virtual Employee LoadAllowedBy {
			get => loadAllowedBy;
			set => SetField(ref loadAllowedBy, value, () => LoadAllowedBy);
		}

		Order previousOrder;

		[Display(Name = "Предыдущий заказ")]
		public virtual Order PreviousOrder {
			get => previousOrder;
			set => SetField(ref previousOrder, value, () => PreviousOrder);
		}

		private string odzComment;
		[Display(Name = "Комментарий ОДЗ")]
		public virtual string ODZComment {
			get => odzComment;
			set => SetField(ref odzComment, value);
		}
		
		private string opComment;
		[Display(Name = "Комментарий ОП")]
		public virtual string OPComment {
			get => opComment;
			set => SetField(ref opComment, value);
		}

		int? bottlesReturn;

		[Display(Name = "Бутылей на возврат")]
		public virtual int? BottlesReturn {
			get => bottlesReturn;
			set => SetField(ref bottlesReturn, value, () => BottlesReturn);
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value, () => Comment);
		}

		string commentLogist;

		[Display(Name = "Комментарий логиста")]
		public virtual string CommentLogist {
			get => commentLogist;
			set => SetField(ref commentLogist, value, () => CommentLogist);
		}

		string clientPhone;

		[Display(Name = "Номер телефона")]
		public virtual string ClientPhone {
			get => clientPhone;
			set => SetField(ref clientPhone, value, () => ClientPhone);
		}

		OrderSignatureType? signatureType;

		[Display(Name = "Подписание документов")]
		public virtual OrderSignatureType? SignatureType {
			get => signatureType;
			set => SetField(ref signatureType, value, () => SignatureType);
		}

		private decimal extraMoney;

		[Display(Name = "Доплата\\Переплата")]
		[PropertyChangedAlso(nameof(OrderCashSum))]
		public virtual decimal ExtraMoney {
			get => extraMoney;
			set => SetField(ref extraMoney, value, () => ExtraMoney);
		}

		string sumDifferenceReason;

		[Display(Name = "Причина переплаты/недоплаты")]
		public virtual string SumDifferenceReason {
			get => sumDifferenceReason;
			set => SetField(ref sumDifferenceReason, value, () => SumDifferenceReason);
		}

		bool shipped;

		[Display(Name = "Отгружено по платежке")]
		public virtual bool Shipped {
			get => shipped;
			set => SetField(ref shipped, value, () => Shipped);
		}

		PaymentType paymentType;

		[Display(Name = "Форма оплаты")]
		public virtual PaymentType PaymentType {
			get => paymentType;
			set {
				if(value != paymentType && SetField(ref paymentType, value, () => PaymentType)) {
					switch (PaymentType) {
						case PaymentType.cash:
						case PaymentType.barter:
						case PaymentType.cashless:
						case PaymentType.BeveragesWorld:
						case PaymentType.ContractDoc:
							OnlineOrder = null;
							PaymentByCardFrom = null;
							break;
						case PaymentType.ByCard:
						case PaymentType.Terminal:
							break;
					}
					
					//Для изменения уже закрытого или завершенного заказа из закртытия МЛ
					if(Client != null && orderRepository.GetOnClosingOrderStatuses().Contains(OrderStatus))
						OnChangePaymentType();
				}
			}
		}

		CounterpartyContract contract;

		[Display(Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get => contract;
			set => SetField(ref contract, value, () => Contract);
		}

		MoneyMovementOperation moneyMovementOperation;
		[IgnoreHistoryTrace]
		public virtual MoneyMovementOperation MoneyMovementOperation {
			get => moneyMovementOperation;
			set => SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation);
		}

		BottlesMovementOperation bottlesMovementOperation;
		[IgnoreHistoryTrace]
		public virtual BottlesMovementOperation BottlesMovementOperation {
			get => bottlesMovementOperation;
			set => SetField(ref bottlesMovementOperation, value, () => BottlesMovementOperation);
		}

		IList<DepositOperation> depositOperations;

		public virtual IList<DepositOperation> DepositOperations {
			get => depositOperations;
			set => SetField(ref depositOperations, value, () => DepositOperations);
		}

		bool collectBottles;

		public virtual bool CollectBottles {
			get => collectBottles;
			set => SetField(ref collectBottles, value, () => CollectBottles);
		}

		DefaultDocumentType? documentType;

		[Display(Name = "Тип безналичных документов")]
		public virtual DefaultDocumentType? DocumentType {
			get => documentType;
			set => SetField(ref documentType, value, () => DocumentType);
		}

		private string code1c;

		[Display(Name = "Код 1С")]
		public virtual string Code1c {
			get => code1c;
			set => SetField(ref code1c, value, () => Code1c);
		}

		private string address1c;

		[Display(Name = "Адрес 1С")]
		public virtual string Address1c {
			get => address1c;
			set => SetField(ref address1c, value, () => Address1c);
		}

		private string address1cCode;

		[Display(Name = "Код адреса 1С")]
		public virtual string Address1cCode {
			get => address1cCode;
			set => SetField(ref address1cCode, value, () => Address1cCode);
		}

		private string toClientText;

		[Display(Name = "Оборудование к клиенту")]
		public virtual string ToClientText {
			get => toClientText;
			set => SetField(ref toClientText, value, () => ToClientText);
		}

		private string fromClientText;

		[Display(Name = "Оборудование от клиента")]
		public virtual string FromClientText {
			get => fromClientText;
			set => SetField(ref fromClientText, value, () => FromClientText);
		}

		NonReturnReason tareNonReturnReason;
		[Display(Name = "Причина несдачи тары")]
		public virtual NonReturnReason TareNonReturnReason {
			get => tareNonReturnReason;
			set => SetField(ref tareNonReturnReason, value, () => TareNonReturnReason);
		}

		PaymentFrom paymentByCardFrom;
		[Display(Name = "Место, откуда проведена оплата")]
		public virtual PaymentFrom PaymentByCardFrom {
			get => paymentByCardFrom;
			set => SetField(ref paymentByCardFrom, value, () => PaymentByCardFrom);
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

		private int? dailyNumber;

		/// <summary>
		/// Уникапльный номер в передлах одного дня.
		/// ВАЖНО! Номер генерируется и изменяется на стороне БД
		/// </summary>
		[Display(Name = "Ежедневный номер")]
		public virtual int? DailyNumber {
			get => dailyNumber;
			set => SetField(ref dailyNumber, value, () => DailyNumber);
		}

		Employee lastEditor;

		[Display(Name = "Последний редактор")]
		[IgnoreHistoryTrace]
		public virtual Employee LastEditor {
			get => lastEditor;
			set => SetField(ref lastEditor, value, () => LastEditor);
		}

		DateTime lastEditedTime;

		[Display(Name = "Последние изменения")]
		[IgnoreHistoryTrace]
		public virtual DateTime LastEditedTime {
			get => lastEditedTime;
			set => SetField(ref lastEditedTime, value, () => LastEditedTime);
		}

		string commentManager;
		/// <summary>
		/// Комментарий менеджера ответственного за водительский телефон
		/// </summary>
		[Display(Name = "Комментарий менеджера")]
		public virtual string CommentManager {
			get => commentManager;
			set => SetField(ref commentManager, value, () => CommentManager);
		}

		int? returnedTare;

		[Display(Name = "Возвратная тара")]
		public virtual int? ReturnedTare {
			get => returnedTare;
			set => SetField(ref returnedTare, value, () => ReturnedTare);
		}

		string informationOnTara;

		[Display(Name = "Информация о таре")]
		public virtual string InformationOnTara {
			get => informationOnTara;
			set => SetField(ref informationOnTara, value, () => InformationOnTara);
		}

		private bool isBottleStock;
		[Display(Name = "Акция \"Бутыль\" ")]
		public virtual bool IsBottleStock {
			get => isBottleStock;
			set => SetField(ref isBottleStock, value, () => IsBottleStock);
		}

		private int bottlesByStockCount;
		[Display(Name = "Количество бутылей по акции")]
		public virtual int BottlesByStockCount {
			get => bottlesByStockCount;
			set => SetField(ref bottlesByStockCount, value, () => BottlesByStockCount);
		}

		private int bottlesByStockActualCount;
		[Display(Name = "Фактическое количество бутылей по акции")]
		public virtual int BottlesByStockActualCount {
			get => bottlesByStockActualCount;
			set => SetField(ref bottlesByStockActualCount, value, () => BottlesByStockActualCount);
		}

		string onRouteEditReason;

		[Display(Name = "Причина редактирования заказа")]
		[Obsolete("Кусок выпиленного функционала от I-1060. Даша сказала пока не удалять, но скрыть зачем-то.")]
		public virtual string OnRouteEditReason {
			get => onRouteEditReason;
			set => SetField(ref onRouteEditReason, value, () => OnRouteEditReason);
		}

		DriverCallType driverCallType;

		[Display(Name = "Водитель отзвонился")]
		public virtual DriverCallType DriverCallType {
			get => driverCallType;
			set => SetField(ref driverCallType, value, () => DriverCallType);
		}

		int? driverCallId;

		[Display(Name = "Номер звонка водителя")]
		public virtual int? DriverCallId {
			get => driverCallId;
			set => SetField(ref driverCallId, value, () => DriverCallId);
		}

		bool isService;

		[Display(Name = "Сервисное обслуживание")]
		public virtual bool IsService {
			get => isService;
			set => SetField(ref isService, value, () => IsService);
		}

		int? trifle;

		[Display(Name = "Сдача")]
		public virtual int? Trifle {
			get => trifle;
			set => SetField(ref trifle, value, () => Trifle);
		}

		private int? onlineOrder;

		[Display(Name = "Номер онлайн заказа")]
		public virtual int? OnlineOrder {
			get => onlineOrder;
			set => SetField(ref onlineOrder, value, () => OnlineOrder);
		}

		private int? eShopOrder;
		[Display(Name = "Заказ из интернет магазина")]
		public virtual int? EShopOrder {
			get => eShopOrder;
			set => SetField(ref eShopOrder, value);
		}

		private bool isContractCloser;

		[Display(Name = "Заказ - закрывашка по контракту?")]
		public virtual bool IsContractCloser {
			get => isContractCloser;
			set => SetField(ref isContractCloser, value, () => IsContractCloser);
		}

		bool isTareNonReturnReasonChangedByUser;
		[Display(Name = "Причина невозврата тары указана пользователем")]
		[IgnoreHistoryTrace]
		public virtual bool IsTareNonReturnReasonChangedByUser {
			get => isTareNonReturnReasonChangedByUser;
			set => SetField(ref isTareNonReturnReasonChangedByUser, value, () => IsTareNonReturnReasonChangedByUser);
		}

		bool hasCommentForDriver;
		[Display(Name = "Есть комментарий для водителя?")]
		[IgnoreHistoryTrace]
		public virtual bool HasCommentForDriver {
			get => hasCommentForDriver;
			set => SetField(ref hasCommentForDriver, value, () => HasCommentForDriver);
		}

		private OrderSource orderSource = OrderSource.VodovozApp;

		[Display(Name = "Источник заказа")]
		public virtual OrderSource OrderSource {
			get => orderSource;
			set => SetField(ref orderSource, value);
		}

		bool addCertificates;
		[Display(Name = "Добавить сертификаты продукции")]
		public virtual bool AddCertificates {
			get => addCertificates;
			set => SetField(ref addCertificates, value, () => AddCertificates);
		}

		bool contactlessDelivery;
		[Display(Name = "Бесконтактная доставка")]
		public virtual bool ContactlessDelivery {
			get => contactlessDelivery;
			set => SetField(ref contactlessDelivery, value, () => ContactlessDelivery);
		}
		
		bool paymentBySms;
		[Display(Name = "Оплата по SMS")]
		public virtual bool PaymentBySms {
			get => paymentBySms;
			set => SetField(ref paymentBySms, value, () => PaymentBySms);
		}

		ReturnTareReason returnTareReason;
		[Display(Name = "Причина забора тары")]
		public virtual ReturnTareReason ReturnTareReason {
			get => returnTareReason;
			set => SetField(ref returnTareReason, value);
		}

		ReturnTareReasonCategory returnTareReasonCategory;
		[Display(Name = "Категория причины забора тары")]
		public virtual ReturnTareReasonCategory ReturnTareReasonCategory {
			get => returnTareReasonCategory;
			set => SetField(ref returnTareReasonCategory, value);
		}

		#endregion

		public virtual bool CanChangeContractor()
		{
			return (!NHibernate.NHibernateUtil.IsInitialized(OrderDocuments) || !OrderDocuments.Any())
				&& (!NHibernate.NHibernateUtil.IsInitialized(InitialOrderService) || !InitialOrderService.Any())
				&& (!NHibernate.NHibernateUtil.IsInitialized(FinalOrderService) || !FinalOrderService.Any());
		}

		IList<OrderDepositItem> orderDepositItems = new List<OrderDepositItem>();

		[Display(Name = "Залоги заказа")]
		public virtual IList<OrderDepositItem> OrderDepositItems {
			get => orderDepositItems;
			set => SetField(ref orderDepositItems, value, () => OrderDepositItems);
		}

		GenericObservableList<OrderDepositItem> observableOrderDepositItems;
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

		IList<OrderDocument> orderDocuments = new List<OrderDocument>();

		[Display(Name = "Документы заказа")]
		public virtual IList<OrderDocument> OrderDocuments {
			get => orderDocuments;
			set => SetField(ref orderDocuments, value, () => OrderDocuments);
		}

		GenericObservableList<OrderDocument> observableOrderDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDocument> ObservableOrderDocuments {
			get {
				if(observableOrderDocuments == null)
					observableOrderDocuments = new GenericObservableList<OrderDocument>(OrderDocuments);
				return observableOrderDocuments;
			}
		}

		IList<OrderItem> orderItems = new List<OrderItem>();

		[Display(Name = "Строки заказа")]
		public virtual IList<OrderItem> OrderItems {
			get => orderItems;
			set => SetField(ref orderItems, value, () => OrderItems);
		}

		GenericObservableList<OrderItem> observableOrderItems;
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

		IList<OrderEquipment> orderEquipments = new List<OrderEquipment>();

		[Display(Name = "Список оборудования")]
		public virtual IList<OrderEquipment> OrderEquipments {
			get => orderEquipments;
			set => SetField(ref orderEquipments, value, () => OrderEquipments);
		}

		GenericObservableList<OrderEquipment> observableOrderEquipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderEquipment> ObservableOrderEquipments {
			get {
				if(observableOrderEquipments == null)
					observableOrderEquipments = new GenericObservableList<OrderEquipment>(orderEquipments);
				return observableOrderEquipments;
			}
		}

		IList<ServiceClaim> initialOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> InitialOrderService {
			get => initialOrderService;
			set => SetField(ref initialOrderService, value, () => InitialOrderService);
		}

		GenericObservableList<ServiceClaim> observableInitialOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableInitialOrderService {
			get {
				if(observableInitialOrderService == null)
					observableInitialOrderService = new GenericObservableList<ServiceClaim>(InitialOrderService);
				return observableInitialOrderService;
			}
		}

		IList<ServiceClaim> finalOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> FinalOrderService {
			get => finalOrderService;
			set => SetField(ref finalOrderService, value, () => FinalOrderService);
		}

		GenericObservableList<ServiceClaim> observableFinalOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableFinalOrderService {
			get {
				if(observableFinalOrderService == null)
					observableFinalOrderService = new GenericObservableList<ServiceClaim>(FinalOrderService);
				return observableFinalOrderService;
			}
		}

		IList<PromotionalSet> promotionalSets = new List<PromotionalSet>();
		[Display(Name = "Промонаборы заказа")]
		public virtual IList<PromotionalSet> PromotionalSets {
			get => promotionalSets;
			set => SetField(ref promotionalSets, value, () => PromotionalSets);
		}

		GenericObservableList<PromotionalSet> observablePromotionalSets;
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

		public Order()
		{
			Comment = string.Empty;
			OrderStatus = OrderStatus.NewOrder;
			OrderPaymentStatus = OrderPaymentStatus.None;
			SumDifferenceReason = string.Empty;
			ClientPhone = string.Empty;
		}

		public static Order CreateFromServiceClaim(ServiceClaim service, Employee author)
		{
			var order = new Order {
				client = service.Counterparty,
				DeliveryPoint = service.DeliveryPoint,
				DeliveryDate = service.ServiceStartDate,
				PaymentType = service.Payment,
				Author = author
			};
			service.InitialOrder = order;
			order.AddServiceClaimAsInitial(service);
			return order;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(validationContext.Items.ContainsKey("NewStatus")) {
				OrderStatus newStatus = (OrderStatus)validationContext.Items["NewStatus"];
				if((newStatus == OrderStatus.Accepted || newStatus == OrderStatus.WaitForPayment) && Client != null) {

					var key = new OrderStateKey(this, newStatus);
					var messages = new List<string>();
					if(!OrderAcceptProhibitionRulesRepository.CanAcceptOrder(key, ref messages)) {
						foreach(var msg in messages) {
							yield return new ValidationResult(msg);
						}
					}

					if(DeliveryDate == null || DeliveryDate == default(DateTime))
						yield return new ValidationResult("В заказе не указана дата доставки.",
							new[] { this.GetPropertyName(o => o.DeliveryDate) });
					if(!SelfDelivery && DeliverySchedule == null)
						yield return new ValidationResult("В заказе не указано время доставки.",
							new[] { this.GetPropertyName(o => o.DeliverySchedule) });

					if(!IsLoadedFrom1C && PaymentType == PaymentType.cashless && Client.TypeOfOwnership != "ИП" && !SignatureType.HasValue)
						yield return new ValidationResult("В заказе не указано как будут подписаны документы.",
							new[] { this.GetPropertyName(o => o.SignatureType) });

					if(!IsLoadedFrom1C && bottlesReturn == null && this.OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.water && !x.Nomenclature.IsDisposableTare))
						yield return new ValidationResult("В заказе не указана планируемая тара.",
							new[] { this.GetPropertyName(o => o.Contract) });
					if(bottlesReturn.HasValue && bottlesReturn > 0 && GetTotalWater19LCount() == 0 && ReturnTareReason == null)
						yield return new ValidationResult("Необходимо указать причину забора тары.",
							new[] { nameof(ReturnTareReason) });
					if(bottlesReturn.HasValue && bottlesReturn > 0 && GetTotalWater19LCount() == 0 && ReturnTareReasonCategory == null)
						yield return new ValidationResult("Необходимо указать категорию причины забора тары.",
							new[] { nameof(ReturnTareReasonCategory) });
					if(!IsLoadedFrom1C && trifle == null && (PaymentType == PaymentType.cash || PaymentType == PaymentType.BeveragesWorld) && this.TotalSum > 0m)
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

					// Проверка соответствия цен в заказе ценам в номенклатуре
					string priceResult = "В заказе неверно указаны цены на следующие товары:\n";
					List<string> incorrectPriceItems = new List<string>();
					foreach(OrderItem item in ObservableOrderItems) {
						decimal fixedPrice = GetFixedPrice(item);
						decimal nomenclaturePrice = GetNomenclaturePrice(item);
						if(fixedPrice > 0m) {
							if(item.Price < fixedPrice) {
								incorrectPriceItems.Add(string.Format("{0} - цена: {1}, должна быть: {2}\n",
																	  item.NomenclatureString,
																	  item.Price,
																	  fixedPrice));
							}
						} else if(nomenclaturePrice > default(decimal) && item.Price < nomenclaturePrice) {
							incorrectPriceItems.Add(string.Format("{0} - цена: {1}, должна быть: {2}\n",
																  item.NomenclatureString,
																  item.Price,
																  nomenclaturePrice));
						}
					}
					if(incorrectPriceItems.Any()) {
						foreach(string item in incorrectPriceItems) {
							priceResult += item;
						}
						yield return new ValidationResult(priceResult);
					}
					// Конец проверки цен

					//создание нескольких заказов на одну дату и точку доставки
					if(!SelfDelivery && DeliveryPoint != null) {
						var ordersForDeliveryPoints = orderRepository.GetLatestOrdersForDeliveryPoint(UoW, DeliveryPoint)
																	 .Where(
																		 o => o.Id != Id
																		 && o.DeliveryDate == DeliveryDate
																		 && !orderRepository.GetGrantedStatusesToCreateSeveralOrders().Contains(o.OrderStatus)
																		 && !o.IsService
																		);

						bool hasMaster = ObservableOrderItems.Any(i => i.Nomenclature.Category == NomenclatureCategory.master);

						if(!hasMaster
						   && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_create_several_orders_for_date_and_deliv_point")
						   && ordersForDeliveryPoints.Any()
						   && validationContext.Items.ContainsKey("IsCopiedFromUndelivery") && !(bool)validationContext.Items["IsCopiedFromUndelivery"]) {
							yield return new ValidationResult(
								string.Format("Создать заказ нельзя, т.к. для этой даты и точки доставки уже создан заказ №{0}", ordersForDeliveryPoints.First().Id),
								new[] { this.GetPropertyName(o => o.OrderEquipments) });
						}
					}

					if(Client.IsDeliveriesClosed && PaymentType != PaymentType.cash && PaymentType != PaymentType.ByCard)
						yield return new ValidationResult(
							"В заказе неверно указан тип оплаты (для данного клиента закрыты поставки)",
							new[] { this.GetPropertyName(o => o.PaymentType) }
						);

					//FIXME Исправить изменение данных. В валидации нельзя менять объекты.
					if(DeliveryPoint != null && !DeliveryPoint.FindAndAssociateDistrict(UoW))
						yield return new ValidationResult(
							"Район доставки не найден. Укажите правильные координаты или разметьте район доставки.",
							new[] { this.GetPropertyName(o => o.DeliveryPoint) }
					);
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

				if(IsService && PaymentType == PaymentType.cashless
				   && newStatus == OrderStatus.Accepted
				   && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_accept_cashles_service_orders")) {
					yield return new ValidationResult(
						"Недостаточно прав для подтверждения безнального сервисного заказа. Обратитесь к руководителю.",
						new[] { this.GetPropertyName(o => o.OrderStatus) }
					);
				}

				if(IsContractCloser && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_set_contract_closer")) {
					yield return new ValidationResult(
						"Недостаточно прав для подтверждения зыкрывашки по контракту. Обратитесь к руководителю.",
						new[] { this.GetPropertyName(o => o.IsContractCloser) }
					);
				}
			}

			if(ObservableOrderItems.Any(x => x.Discount > 0 && x.DiscountReason == null))
				yield return new ValidationResult("Если в заказе указана скидка на товар, то обязательно должно быть заполнено поле 'Основание'.");

			if(DeliveryDate == null || DeliveryDate == default(DateTime))
				yield return new ValidationResult("В заказе не указана дата доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) });
			if(!SelfDelivery && DeliveryPoint == null)
				yield return new ValidationResult("В заказе необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryPoint) });
			if(DeliveryPoint != null && (!DeliveryPoint.Latitude.HasValue || !DeliveryPoint.Longitude.HasValue)) {
				yield return new ValidationResult("В точке доставки необходимо указать координаты.",
				new[] { this.GetPropertyName(o => o.DeliveryPoint) });
			}
			if(Client == null)
				yield return new ValidationResult("В заказе необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName(o => o.Client) });

			if(PaymentType == PaymentType.ByCard && OnlineOrder == null)
				yield return new ValidationResult("Если в заказе выбран тип оплаты по карте, необходимо заполнить номер онлайн заказа.",
												  new[] { this.GetPropertyName(o => o.OnlineOrder) });

			if(PaymentType == PaymentType.ByCard && PaymentByCardFrom == null)
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

			if(ObservableOrderDepositItems.Any(x => x.Total < 0)) {
				yield return new ValidationResult("В возврате залогов в заказе необходимо вводить положительную сумму.");
			}

			if(!ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_can_create_order_in_advance")
			   && DeliveryDate.HasValue && DeliveryDate.Value < DateTime.Today
			   && OrderStatus <= OrderStatus.Accepted) {
				yield return new ValidationResult(
					"Указана дата заказа более ранняя чем сегодняшняя. Укажите правильную дату доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryDate) }
				);
			}
			if(SelfDelivery && PaymentType == PaymentType.ContractDoc) {
				yield return new ValidationResult(
					"Тип оплаты - контрактная документация невозможен для самовывоза",
					new[] { this.GetPropertyName(o => o.PaymentType) }
				);
			}
		}

		#endregion

		#region Вычисляемые

		public virtual bool IsLoadedFrom1C => !string.IsNullOrEmpty(Code1c);

		public override string ToString() => IsLoadedFrom1C ? string.Format("Заказ №{0}({1})", Id, Code1c) : string.Format("Заказ №{0}", Id);

		public virtual string Title => string.Format("Заказ №{0} от {1:d}", Id, DeliveryDate);

		public virtual int Total19LBottlesToDeliver => 
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water && 
			                           x.Nomenclature.TareVolume == TareVolume.Vol19L).Sum(x => x.Count);

		public virtual int Total6LBottlesToDeliver => 
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water && 
			                           x.Nomenclature.TareVolume == TareVolume.Vol6L).Sum(x => x.Count);

		public virtual int Total600mlBottlesToDeliver => 
			(int)OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water &&
			                           x.Nomenclature.TareVolume == TareVolume.Vol600ml).Sum(x => x.Count);

		public virtual int TotalWeight => 
			(int)OrderItems.Sum(x => x.Count * (decimal) x.Nomenclature.Weight);

		public virtual double TotalVolume => 
			(double)OrderItems.Sum(x => x.Count * (decimal) x.Nomenclature.Volume);

		public virtual string RowColor => PreviousOrder == null ? "black" : "red";

		[Display(Name = "Наличных к получению")]
		public virtual decimal OrderCashSum {
			get => PaymentType == PaymentType.cash || PaymentType == PaymentType.BeveragesWorld ? OrderSumTotal - OrderSumReturnTotal : 0;
			protected set {; }
		}

		[PropertyChangedAlso(nameof(OrderCashSum))]
		public virtual decimal OrderTotalSum => OrderSumTotal - OrderSumReturnTotal;

		[PropertyChangedAlso(nameof(OrderCashSum))]
		public virtual decimal TotalSum => OrderSum - OrderSumReturn;

		public virtual decimal OrderSum {
			get {
				decimal sum = 0;
				foreach(OrderItem item in ObservableOrderItems) {
					sum += item.ActualSum;
				}
				return sum;
			}
		}

		public virtual decimal OrderSumTotal {
			get {
				decimal result = OrderSum;
				if(ExtraMoney > 0) {
					result += ExtraMoney;
				}
				return result;
			}
		}

		public virtual decimal OrderSumReturn {
			get {
				decimal sum = 0;
				foreach(OrderDepositItem dep in ObservableOrderDepositItems) {
					sum += dep.Total;
				}
				return sum;
			}
		}

		public virtual decimal OrderSumReturnTotal {
			get {
				decimal result = OrderSumReturn;
				if(ExtraMoney < 0) {
					result += Math.Abs(ExtraMoney);
				}
				return result;
			}
		}

		public virtual decimal BottleDepositSum => ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Bottles).Sum(x => x.Total);
		public virtual decimal EquipmentDepositSum => ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Equipment).Sum(x => x.Total);

		public virtual decimal ActualTotalSum {
			get {
				decimal sum = 0;
				foreach(OrderItem item in ObservableOrderItems)
					sum += item.ActualSum;

				foreach(OrderDepositItem dep in ObservableOrderDepositItems)
					sum -= dep.Deposit * dep.Count;

				return sum;
			}
		}

		[Obsolete("Должно быть не актуально после ввода новой системы расчёта ЗП (I-2150)")]
		public virtual decimal MoneyForMaster =>
			ObservableOrderItems.Where(i => i.Nomenclature.Category == NomenclatureCategory.master && i.ActualCount.HasValue)
								.Sum(i => (decimal)i.Nomenclature.PercentForMaster / 100 * i.ActualCount.Value * i.Price);

		public virtual decimal? ActualGoodsTotalSum =>
			OrderItems.Sum(item => item.Price * item.ActualCount - item.DiscountMoney);

		public virtual bool CanBeMovedFromClosedToAcepted => 
			new RouteListItemRepository().WasOrderInAnyRouteList(UoW, this)
		        && ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_move_order_from_closed_to_acepted");

		public virtual bool HasItemsNeededToLoad => ObservableOrderItems.Any(orderItem =>
				!Nomenclature.GetCategoriesNotNeededToLoad().Contains(orderItem.Nomenclature.Category) && !orderItem.Nomenclature.NoDelivey)
			|| ObservableOrderEquipments.Any(orderEquipment =>
				!Nomenclature.GetCategoriesNotNeededToLoad().Contains(orderEquipment.Nomenclature.Category) && !orderEquipment.Nomenclature.NoDelivey);

		#endregion

		#region Автосоздание договоров, при изменении подтвержденного заказа

		private void OnChangeCounterparty(Counterparty newClient)
		{
			if(newClient == null || Client == null || newClient.Id == Client.Id) {
				return;
			}

			Contract = FindOrCreateContract(newClient);
			OnChangeContract(true);
		}

		private void OnChangeContract(bool changedClient = false)
		{
			UpdateDocuments();
		}

		private void OnChangePaymentType()
		{
			ChangeContractOnChangePaymentType();
			if(Contract == null) {
				Contract = FindOrCreateContract(Client);
			}
			OnChangeContract();
		}

		#endregion

		#region Функции

		/// <summary>
		/// Рассчитывает скидки в товарах по акции "Бутыль"
		/// </summary>
		public virtual void CalculateBottlesStockDiscounts(IStandartDiscountsService standartDiscountsService, bool byActualCount = false)
		{
			if(standartDiscountsService == null) {
				throw new ArgumentNullException(nameof(standartDiscountsService));
			}
			var reasonId = standartDiscountsService.GetDiscountForStockBottle();
			DiscountReason discountReasonStockBottle = UoW.GetById<DiscountReason>(reasonId);
			if(discountReasonStockBottle == null) {
				throw new InvalidProgramException($"Не возможно найти причину скидки для акции Бутыль (id:{reasonId})");
			}

			var bottlesByStock = byActualCount ? BottlesByStockActualCount : BottlesByStockCount;
			decimal discountForStock = 0m;

			if(bottlesByStock == Total19LBottlesToDeliver) {
				discountForStock = 10m;
			}
			if(bottlesByStock > Total19LBottlesToDeliver) {
				discountForStock = 20m;
			}

			foreach(OrderItem item in ObservableOrderItems
				.Where(x => x.Nomenclature.Category == NomenclatureCategory.water)
				.Where(x => !x.Nomenclature.IsDisposableTare)
				.Where(x => x.Nomenclature.TareVolume == TareVolume.Vol19L)) {
				item.SetDiscountByStock(discountReasonStockBottle, discountForStock);
			}
		}

		public virtual Email GetEmailAddressForBill()
		{
			return Client.Emails.FirstOrDefault(x => (x.EmailType?.EmailPurpose == EmailPurpose.ForBills) || x.EmailType == null);
		}

		public virtual bool NeedSendBill(IEmailRepository emailRepository)
		{
			if((OrderStatus == OrderStatus.Accepted || OrderStatus == OrderStatus.WaitForPayment)
				&& PaymentType == PaymentType.cashless
				&& !emailRepository.HaveSendedEmail(Id, OrderDocumentType.Bill)) {
				//Проверка должен ли формироваться счет для текущего заказа
				return GetRequirementDocTypes().Contains(OrderDocumentType.Bill);
			}
			return false;
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

		public virtual void RecalculateStockBottles(IStandartDiscountsService standartDiscountsService)
		{
			if(!IsBottleStock) {
				BottlesByStockCount = 0;
				BottlesByStockActualCount = 0;
			}
			CalculateBottlesStockDiscounts(standartDiscountsService);
		}

		public virtual void AddContractDocument(CounterpartyContract contract)
		{
			ObservableOrderDocuments.Add(
				new OrderContract {
					Order = this,
					AttachedToOrder = this,
					Contract = contract
				}
			);
		}

		public virtual void ChangeOrderContract()
		{
			if(Client == null || (DeliveryPoint == null && !SelfDelivery) || DeliveryDate == null) {
				return;
			}
			var oldContract = Contract;

			//Нахождение подходящего существующего договора
			var actualContract = Repositories.CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW, Client, Client.PersonType, PaymentType);

			//Нахождение договора созданного в текущем заказе, 
			//который можно изменить под необходимые параметры
			var contractInOrder = OrderDocuments.OfType<OrderContract>()
												.Where(x => x.Order.Id == Id)
												.Select(x => x.Contract)
												.FirstOrDefault();

			//Поиск других заказов, в которых мог использоваться этот договор
			IList<Order> ordersByContract = null;
			bool otherOrdersForContractExist = true;
			if(contractInOrder != null) {
				ordersByContract = UoW.Session.QueryOver<Order>()
									  .Where(o => o.Contract.Id == contractInOrder.Id)
									  .List();
				otherOrdersForContractExist = ordersByContract.Any(o => o.Id != Id);

				Contract = contractInOrder;
			}

			//Изменение существующего договора под необходимые параметры
			//если он не был использован в других заказах
			if(
				actualContract == null
				&& !otherOrdersForContractExist
				&& OrderDocuments.OfType<OrderContract>().Any(
					x => x.Order.Id == Id
					&& x.Contract?.Id == Contract?.Id
				)
			) {
				using(var uowContract = UnitOfWorkFactory.CreateForRoot<CounterpartyContract>(Contract.Id, $"Изменение договора в заказе")) {
					uowContract.Root.ContractType = DocTemplateRepository.GetContractTypeForPaymentType(Client.PersonType, PaymentType);
					uowContract.Root.Organization = Repositories.OrganizationRepository.GetOrganizationByPaymentType(uowContract, Client.PersonType, PaymentType);
					uowContract.Save();
				}
				UoW.Session.Refresh(Contract);
				return;
			}

			if(actualContract == null) {
				actualContract = ClientDocumentsRepository.CreateDefaultContract(UoW, Client, PaymentType, DeliveryDate);
				Contract = actualContract;
			}

			if(actualContract != oldContract) {
				Contract = actualContract;
			}

			UpdateDocuments();
		}

		public virtual bool HasWater()
		{
			var categories = Nomenclature.GetCategoriesRequirementForWaterAgreement();
			return ObservableOrderItems.Any(x => categories.Contains(x.Nomenclature.Category));
		}

		public virtual void CheckAndSetOrderIsService()
		{
			IsService = OrderItems.Any(x => x.Nomenclature.Category == NomenclatureCategory.master);
		}

		public virtual void SetFirstOrder()
		{
			if(Id == 0 && Client.FirstOrder == null) {
				IsFirstOrder = true;
				Client.FirstOrder = this;
			}
		}

		public virtual void CreateDefaultContract()
		{
			Contract = FindOrCreateContract(Client);
			ObservableOrderDocuments.Add(new OrderContract {
				Order = this,
				AttachedToOrder = this,
				Contract = Contract
			});
			OnChangeContract(false);
		}

		public virtual void RecalculateItemsPrice()
		{
			for (int i = 0; i < ObservableOrderItems.Count; i++) {
				if(ObservableOrderItems[i].Nomenclature.Category == NomenclatureCategory.water) {
					ObservableOrderItems[i].RecalculatePrice();
				}
			}
		}

		public virtual int GetTotalWater19LCount(bool doNotCountWaterFromPromoSets = false)
		{
			var water19L = ObservableOrderItems.Where(x => x.Nomenclature.IsWater19L);
			if(doNotCountWaterFromPromoSets)
				water19L = water19L.Where(x => x.PromoSet == null);
			return (int)water19L.Sum(x => x.Count);
		}

		/// <summary>
		/// Находит соответсвующий типу клиента и типу оплаты контракт, если не найден создает новый
		/// </summary>
		private CounterpartyContract FindOrCreateContract(Counterparty client)
		{
			var contractType = DocTemplateRepository.GetContractTypeForPaymentType(client.PersonType, PaymentType);
			var newContract = client.CounterpartyContracts.FirstOrDefault(x => x.ContractType == contractType && !x.IsArchive);
			if(newContract == null) {
				newContract = ClientDocumentsRepository.CreateDefaultContract(UoW, client, PaymentType, DeliveryDate);
			}
			return newContract;
		}

		//Выбирает договор соответствующий форме оплаты если такой найден
		public virtual void ChangeContractOnChangePaymentType()
		{
			var org = Repositories.OrganizationRepository.GetOrganizationByPaymentType(UoW, Client.PersonType, PaymentType);
			if((Contract == null || Contract.Organization.Id != org.Id) && Client != null)
				Contract = Repositories.CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW, Client, Client.PersonType, PaymentType);
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

		public virtual void AddEquipmentNomenclatureFromClient(Nomenclature nomenclature, IUnitOfWork UoW)
		{
			ObservableOrderEquipments.Add(
				new OrderEquipment {
					Order = this,
					Direction = Direction.PickUp,
					Equipment = null,
					OrderItem = null,
					Reason = Reason.Service,
					Confirmed = true,
					Nomenclature = nomenclature
				}
			);
			UpdateDocuments();
		}

		public virtual void AddAnyGoodsNomenclatureForSale(Nomenclature nomenclature, bool isChangeOrder = false, int? cnt = null)
		{
			var acceptableCategories = Nomenclature.GetCategoriesForSale();
			if(!acceptableCategories.Contains(nomenclature.Category)) {
				return;
			}

			var count = (nomenclature.Category == NomenclatureCategory.service
						 || nomenclature.Category == NomenclatureCategory.deposit) && !isChangeOrder ? 1 : 0;

			if(cnt.HasValue)
				count = cnt.Value;

			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice(1)
			});
		}

		public virtual void AddItemWithNomenclatureForSale(OrderItem orderItem)
		{
			var acceptableCategories = Nomenclature.GetCategoriesForSale();
			if(orderItem?.Nomenclature == null || !acceptableCategories.Contains(orderItem.Nomenclature.Category))
				return;

			orderItem.Order = this;
			ObservableOrderItems.Add(orderItem);
		}

		/// <summary>
		/// Добавление в заказ номенклатуры типа "Выезд мастера"
		/// </summary>
		/// <param name="nomenclature">Номенклатура типа "Выезд мастера"</param>
		/// <param name="count">Количество</param>
		/// <param name="quantityOfFollowingNomenclatures">Колличество номенклатуры, указанной в параметрах БД,
		/// которые будут добавлены в заказ вместе с мастером</param>
		public virtual void AddMasterNomenclature(Nomenclature nomenclature, int count, int quantityOfFollowingNomenclatures = 0)
		{
			if(nomenclature.Category != NomenclatureCategory.master) {
				return;
			}

			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice(1)
			});

			Nomenclature followingNomenclature = new NomenclatureRepository(new NomenclatureParametersProvider()).GetNomenclatureToAddWithMaster(UoW);
			if(quantityOfFollowingNomenclatures > 0 && !ObservableOrderItems.Any(i => i.Nomenclature.Id == followingNomenclature.Id))
				AddAnyGoodsNomenclatureForSale(followingNomenclature, false, 1);
		}

		public virtual void AddWaterForSale(Nomenclature nomenclature, decimal count, decimal discount = 0, bool discountInMoney = false, DiscountReason reason = null, PromotionalSet proSet = null)
		{
			if(nomenclature.Category != NomenclatureCategory.water && !nomenclature.IsDisposableTare)
				return;

			//Если номенклатура промо-набора добавляется по фиксе (без скидки), то у нового OrderItem убирается поле discountReason
			if(proSet != null && discount == 0) {
				var fixPricedNomenclaturesId = GetNomenclaturesWithFixPrices.Select(n => n.Id);
				if(fixPricedNomenclaturesId.Contains(nomenclature.Id))
					reason = null;
			}

			if(discount > 0 && reason == null)
				throw new ArgumentException("Требуется указать причину скидки (reason), если она (discount) больше 0!");

			decimal price = GetWaterPrice(nomenclature, proSet, count);
			
			var oi = new OrderItem {
				Order = this,
				Count = count,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = price,
				IsDiscountInMoney = discountInMoney,
				DiscountSetter = discount,
				DiscountReason = reason,
				PromoSet = proSet
			};
			ObservableOrderItems.Add(oi);
		}

		private decimal GetWaterPrice(Nomenclature nomenclature, PromotionalSet promoSet, decimal bottlesCount)
		{
			Nomenclature influentialNomenclature = nomenclature.DependsOnNomenclature;

			IList<NomenclatureFixedPrice> fixedPrices = Contract.Counterparty.NomenclatureFixedPrices;
			if(deliveryPoint != null) {
				fixedPrices = deliveryPoint.NomenclatureFixedPrices;
			}
			
			if(fixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id && influentialNomenclature == null)) {
				return fixedPrices.First(x => x.Nomenclature.Id == nomenclature.Id).Price;
			}
			
			if(influentialNomenclature != null && fixedPrices.Any(x => x.Nomenclature.Id == influentialNomenclature.Id)) {
				return fixedPrices.First(x => x.Nomenclature.Id == influentialNomenclature?.Id).Price;
			}
			
			return nomenclature.GetPrice(promoSet == null ? GetTotalWater19LCount(true) : bottlesCount);
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
			ChangeOrderContract();
		}

		public virtual void SetProxyForOrder()
		{
			if(Client == null)
				return;
			if(!DeliveryDate.HasValue)
				return;
			if(Client.PersonType != PersonType.legal && PaymentType != PaymentType.cashless)
				return;

			bool existProxies = Client.Proxies
									  .Any(
										p => p.IsActiveProxy(DeliveryDate.Value)
										&& (
												p.DeliveryPoints == null || p.DeliveryPoints
																			 .Any(x => DomainHelper.EqualDomainObjects(x, DeliveryPoint))
										   )
									  );

			if(existProxies)
				SignatureType = OrderSignatureType.ByProxy;
		}

		public virtual bool CalculateDeliveryPrice()
		{
			int paidDeliveryNomenclatureId = int.Parse(ParametersProvider.Instance.GetParameterValue("paid_delivery_nomenclature_id"));
			OrderItem deliveryPriceItem = OrderItems.FirstOrDefault(x => x.Nomenclature.Id == paidDeliveryNomenclatureId);

			#region перенести всё это в OrderStateKey
			bool IsDeliveryForFree = SelfDelivery
												  || IsService
												  || DeliveryPoint.AlwaysFreeDelivery
												  || ObservableOrderItems.Any(n => n.Nomenclature.Category == NomenclatureCategory.spare_parts)
												  || !ObservableOrderItems.Any(n => n.Nomenclature.Id != paidDeliveryNomenclatureId) && (BottlesReturn > 0 || ObservableOrderEquipments.Any() || ObservableOrderDepositItems.Any());

			if(IsDeliveryForFree) {
				if(deliveryPriceItem != null)
					ObservableOrderItems.Remove(deliveryPriceItem);
				return false;
			}
			#endregion

			var district = DeliveryPoint?.District;

			OrderStateKey orderKey = new OrderStateKey(this);
			var price = 
				district?.GetDeliveryPrice(orderKey, ObservableOrderItems.Sum(x => x.Nomenclature?.OnlineStoreExternalId != null ? x.ActualSum : 0m )) ?? 0m;

			if(price != 0) {
				if(deliveryPriceItem == null) {
					deliveryPriceItem = new OrderItem {
						Nomenclature = UoW.GetById<Nomenclature>(paidDeliveryNomenclatureId),
						Order = this
					};
					deliveryPriceItem.Price = price;
					deliveryPriceItem.Count = 1;

					var delivery = ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == paidDeliveryNomenclatureId);

					if (delivery == null) {
						ObservableOrderItems.Add(deliveryPriceItem);
						return true;
					}
					else
						return false;
					
				} else if(deliveryPriceItem.Price == price) {
					return false;
				}
				deliveryPriceItem.Price = price;
				deliveryPriceItem.Count = 1;
				return true;
			}

			if(deliveryPriceItem != null)
				ObservableOrderItems.Remove(deliveryPriceItem);
			return false;
		}

		/// <summary>
		/// Добавить оборудование из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		/// <param name="UoW">IUnitOfWork</param>
		public virtual void AddNomenclatureForSaleFromPreviousOrder(OrderItem orderItem, IUnitOfWork UoW)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.additional)
				return;
			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				Count = orderItem.Count,
				Nomenclature = orderItem.Nomenclature,
				Price = orderItem.Price
			});
		}

		/// <summary>
		/// Добавить номенклатуру (не вода и не оборудование из выбранного предыдущего заказа).
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		public virtual void AddAnyGoodsNomenclatureForSaleFromPreviousOrder(OrderItem orderItem)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.additional && orderItem.Nomenclature.Category != NomenclatureCategory.bottle &&
				orderItem.Nomenclature.Category != NomenclatureCategory.service)
				return;
			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				Count = orderItem.Nomenclature.Category == NomenclatureCategory.service ? 1 : 0,
				Equipment = orderItem.Equipment,
				Nomenclature = orderItem.Nomenclature,
				Price = orderItem.Price
			});
		}

		public virtual void AddNomenclature(
			Nomenclature nomenclature, 
			decimal count = 0, 
			decimal discount = 0, 
			bool discountInMoney = false, 
			DiscountReason discountReason = null, 
			PromotionalSet proSet = null)
		{
			OrderItem oi = null;
			switch(nomenclature.Category) {
				case NomenclatureCategory.water:
					AddWaterForSale(nomenclature, count, discount, discountInMoney, discountReason, proSet);
					break;
				case NomenclatureCategory.master:
					contract = CreateServiceContractAddMasterNomenclature(nomenclature);
					break;
				default:
					oi = new OrderItem {
						Count = count,
						Nomenclature = nomenclature,
						Price = nomenclature.GetPrice(1),
						IsDiscountInMoney = discountInMoney,
						DiscountSetter = discount,
						DiscountReason = discountReason,
						PromoSet = proSet
					};
					AddItemWithNomenclatureForSale(oi);
					break;
			}
		}

		/// <summary>
		/// Попытка найти и удалить промо-набор, если нет больше позиций
		/// заказа с промо-набором
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

		void ObservablePromotionalSets_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			if(aObject is PromotionalSet proSet) {
				foreach(OrderItem item in ObservableOrderItems)
					if(item.PromoSet == proSet) {
						item.IsUserPrice = false;
						item.PromoSet = null;
						item.DiscountReason = null;
					}
				RecalculateItemsPrice();
			}
		}

		/// <summary>
		/// Чистка списка промо-наборов заказа, если вручную удалили, изменили
		/// причину скидки или что-то ещё.
		/// </summary>
		void ClearPromotionSets()
		{
			var oigrp = OrderItems.GroupBy(x => x.PromoSet);
			var rem = PromotionalSets.Where(s => !oigrp.Select(g => g.Key).Contains(s)).ToArray();
			foreach(var r in rem) {
				var ps = PromotionalSets.FirstOrDefault(s => s == r);
				PromotionalSets.Remove(ps);
			}
		}

		/// <summary>
		/// Проверка на возможность добавления промо-набора в заказ
		/// </summary>
		/// <returns><c>true</c>, если можно добавить промо-набор,
		/// <c>false</c> если нельзя.</returns>
		/// <param name="proSet">Рекламный набор (промо-набор)</param>
		public virtual bool CanAddPromotionalSet(PromotionalSet proSet)
		{
			if(PromotionalSets.Any(x => x.Id == proSet.Id)) {
				InteractiveService.ShowMessage(ImportanceLevel.Warning, "В заказ нельзя добавить два одинаковых промо-набора");
				return false;
			}
			if((PromotionalSets.Count(x => !x.CanBeAddedWithOtherPromoSets) + (proSet.CanBeAddedWithOtherPromoSets ? 0 : 1)) > 1) {
				InteractiveService.ShowMessage(ImportanceLevel.Warning, "В заказ нельзя добавить больше 1 промо-набора, у которого нет свойства \"Может быть добавлен вместе с другими промонаборами\"");
				return false;
			}

			if(SelfDelivery)
				return true;

			var proSetDict = Repositories.Orders.PromotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(UoW, this);
			if(!proSetDict.Any())
				return true;

			var address = string.Join(", ", DeliveryPoint.City, DeliveryPoint.Street, DeliveryPoint.Building, DeliveryPoint.Room);
			StringBuilder sb = new StringBuilder(string.Format("Для адреса \"{0}\", найдены схожие точки доставки, на которые уже создавались заказы с промо-наборами:\n", address));
			foreach(var d in proSetDict) {
				var proSetTitle = UoW.GetById<PromotionalSet>(d.Key).ShortTitle;
				var orders = string.Join(
					" ,",
					UoW.GetById<Order>(d.Value).Select(o => o.Title)
				);
				sb.AppendLine(string.Format("– {0}: {1}", proSetTitle, orders));
			}
			sb.AppendLine($"Вы уверены, что хотите добавить \"{proSet.Title}\"");
			if(InteractiveService.Question(sb.ToString()))
				return true;
			return false;
		}

		private CounterpartyContract CreateServiceContractAddMasterNomenclature(Nomenclature nomenclature)
		{
			if(Contract == null) {
				Contract = Repositories.CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW, Client, Client.PersonType, PaymentType);
				if(Contract == null) {
					Contract = ClientDocumentsRepository.CreateDefaultContract(UoW, Client, PaymentType, DeliveryDate);
					AddContractDocument(Contract);
				}
			}
			AddMasterNomenclature(nomenclature, 1, 1);
			return Contract;
		}
		
		public virtual void ClearOrderItemsList()
		{
			ObservableOrderItems.Clear();
			UpdateDocuments();
		}

		/// <summary>
		/// Наполнение списка товаров нового заказа элементами списка другого заказа.
		/// </summary>
		/// <param name="order">Заказ, из которого будет производится копирование товаров</param>
		public virtual void CopyItemsFrom(Order order)
		{
			if(Id > 0)
				throw new InvalidOperationException("Копирование списка товаров из другого заказа недопустимо, если этот заказ не новый.");

			foreach(OrderItem orderItem in order.OrderItems) {

				decimal discMoney;
				if(orderItem.DiscountMoney == 0) {
					if(orderItem.OriginalDiscountMoney == null)
						discMoney = 0;
					else
						discMoney = orderItem.OriginalDiscountMoney.Value;
				} else {
					discMoney = orderItem.DiscountMoney;
				}

				decimal disc;
				if(orderItem.Discount == 0) {
					if(orderItem.OriginalDiscount == null)
						disc = 0;
					else
						disc = orderItem.OriginalDiscount.Value;
				} else {
					disc = orderItem.Discount;
				}

				ObservableOrderItems.Add(
					new OrderItem {
						Order = this,
						Nomenclature = orderItem.Nomenclature,
						Equipment = orderItem.Equipment,
						PromoSet = orderItem.PromoSet,
						Price = orderItem.Price,
						IsUserPrice = orderItem.IsUserPrice,
						Count = orderItem.Count,
						IncludeNDS = orderItem.IncludeNDS,
						IsDiscountInMoney = orderItem.IsDiscountInMoney,
						DiscountMoney = discMoney,
						Discount = disc,
						DiscountReason = orderItem.DiscountReason ?? orderItem.OriginalDiscountReason
					}
				);
			}
			RecalculateItemsPrice();
		}

		/// <summary>
		/// Наполнение списка промо-наборов нового заказа элементами списка другого заказа.
		/// </summary>
		/// <param name="order">Заказ, из которого будет производится копирование оборудования</param>
		public virtual void CopyPromotionalSetsFrom(Order order)
		{
			if(Id > 0)
				throw new InvalidOperationException("Копирование списка товаров из другого заказа недопустимо, если этот заказ не новый.");

			foreach(var proSet in order.PromotionalSets)
				ObservablePromotionalSets.Add(proSet);
		}

		/// <summary>
		/// Наполнение списка оборудования нового заказа элементами списка другого заказа.
		/// </summary>
		/// <param name="order">Заказ, из которого будет производится копирование оборудования</param>
		public virtual void CopyEquipmentFrom(Order order)
		{
			if(Id > 0)
				throw new InvalidOperationException("Копирование списка оборудования из другого заказа недопустимо, если этот заказ не новый.");

			foreach(OrderEquipment orderEquipment in order.OrderEquipments) {
				ObservableOrderEquipments.Add(
					new OrderEquipment {
						Order = this,
						Direction = orderEquipment.Direction,
						DirectionReason = orderEquipment.DirectionReason,
						OrderItem = orderEquipment.OrderItem,
						Equipment = orderEquipment.Equipment,
						OwnType = orderEquipment.OwnType,
						Nomenclature = orderEquipment.Nomenclature,
						Reason = orderEquipment.Reason,
						Confirmed = orderEquipment.Confirmed,
						ConfirmedComment = orderEquipment.ConfirmedComment,
						Count = orderEquipment.Count
					}
				);
			}
		}

		/// <summary>
		/// Копирует таблицу залогов в новый заказа из другого заказа
		/// </summary>
		/// <param name="order">Order.</param>
		public virtual void CopyDepositItemsFrom(Order order)
		{
			if(Id > 0)
				throw new InvalidOperationException("Копирование списка залогов из другого заказа недопустимо, если этот заказ не новый.");

			foreach(OrderDepositItem oDepositItem in order.OrderDepositItems) {
				ObservableOrderDepositItems.Add(
					new OrderDepositItem {
						Order = this,
						Count = oDepositItem.Count,
						Deposit = oDepositItem.Deposit,
						DepositType = oDepositItem.DepositType,
						EquipmentNomenclature = oDepositItem.EquipmentNomenclature
					}
				);
			}
		}

		public virtual void CopyDocumentsFrom(Order order)
		{
			if(Id > 0)
				throw new InvalidOperationException("Копирование списка документов из другого заказа недопустимо, если этот заказ не новый.");

			var counterpartyDocTypes = typeof(OrderDocumentType).GetFields()
													   .Where(x => !x.GetCustomAttributes(typeof(DocumentOfOrderAttribute), false).Any())
													   .Where(x => !x.Name.Equals("value__"))
													   .Select(x => (OrderDocumentType)x.GetValue(null))
													   .ToArray();

			var orderDocTypes = typeof(OrderDocumentType).GetFields()
													   .Where(x => x.GetCustomAttributes(typeof(DocumentOfOrderAttribute), false).Any())
													   .Select(x => (OrderDocumentType)x.GetValue(null))
													   .ToArray();

			var counterpartyDocs = order.OrderDocuments.Where(d => counterpartyDocTypes.Contains(d.Type)).ToList();
			var orderDocs = order.OrderDocuments.Where(d => orderDocTypes.Contains(d.Type) && d.AttachedToOrder.Id != d.Order.Id).ToList();
			AddAdditionalDocuments(counterpartyDocs);
			AddAdditionalDocuments(orderDocs);
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
		public virtual void AddAdditionalDocuments(List<OrderDocument> documentsList)
		{
			foreach(var item in documentsList) {
				switch(item.Type) {
					case OrderDocumentType.Contract:
						OrderContract oc = (item as OrderContract);
						if(observableOrderDocuments
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
						OrderM2Proxy m2 = (item as OrderM2Proxy);
						if(observableOrderDocuments
						   .OfType<OrderM2Proxy>()
						   .FirstOrDefault(x => x.M2Proxy == m2.M2Proxy
										   && x.Order == m2.Order)
						   == null) {
							ObservableOrderDocuments.Add(m2);
						}
						break;
					case OrderDocumentType.Bill:
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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
						if(observableOrderDocuments
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

		public virtual void RemoveAloneItem(OrderItem item)
		{
			if(item.Count == 0
			   && !OrderEquipments.Any(x => x.OrderItem == item)) {
				ObservableOrderItems.Remove(item);
			}
		}

		public virtual void RemoveItem(OrderItem item)
		{
			ObservableOrderItems.Remove(item);
			DeleteOrderEquipmentOnOrderItem(item);
			UpdateDocuments();
		}

		public virtual void RemoveEquipment(OrderEquipment item)
		{
			ObservableOrderEquipments.Remove(item);
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
		/// <param name="guilty">Виновный в недовезении заказа</param>
		public virtual void SetUndeliveredStatus(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, CallTaskWorker callTaskWorker, GuiltyTypes? guilty = GuiltyTypes.Client)
		{
			var routeListItem = new RouteListItemRepository().GetRouteListItemForOrder(UoW, this);

			switch(OrderStatus) {
				case OrderStatus.NewOrder:
				case OrderStatus.WaitForPayment:
				case OrderStatus.Accepted:
				case OrderStatus.InTravelList:
				case OrderStatus.OnLoading:
					ChangeStatusAndCreateTasks(OrderStatus.Canceled, callTaskWorker);
					routeListItem?.SetStatusWithoutOrderChange(RouteListItemStatus.Overdue);
					break;
				case OrderStatus.OnTheWay:
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.Shipped:
				case OrderStatus.UnloadingOnStock:
				case OrderStatus.NotDelivered:
				case OrderStatus.Closed:
					if(guilty == GuiltyTypes.Client) {
						ChangeStatusAndCreateTasks(OrderStatus.DeliveryCanceled, callTaskWorker);
						routeListItem?.SetStatusWithoutOrderChange(RouteListItemStatus.Canceled);
					} else {
						ChangeStatusAndCreateTasks(OrderStatus.NotDelivered, callTaskWorker);
						routeListItem?.SetStatusWithoutOrderChange(RouteListItemStatus.Overdue);
					}
					break;
			}
			UpdateBottleMovementOperation(uow, standartNomenclatures, 0);
		}

		public virtual void ChangeStatusAndCreateTasks(OrderStatus newStatus, CallTaskWorker callTaskWorker)
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
				case OrderStatus.InTravelList:
				case OrderStatus.OnTheWay:
				case OrderStatus.Shipped:
				case OrderStatus.UnloadingOnStock:
					break;
				case OrderStatus.Closed:
					OnChangeStatusToClosed();
					break;
				case OrderStatus.DeliveryCanceled:
				case OrderStatus.NotDelivered:
				case OrderStatus.Canceled:
					ChangeOrderPaymentStatus();
					break;
				default:
					break;
			}

			if(Id == 0
			   || newStatus == OrderStatus.Canceled
			   || newStatus == OrderStatus.NotDelivered
			   || initialStatus == newStatus)
				return;

			var undeliveries = Repositories.UndeliveredOrdersRepository.GetListOfUndeliveriesForOrder(UoW, this);
			if(undeliveries.Any()) {
				var text = string.Format(
					"сменил(а) статус заказа\nс \"{0}\" на \"{1}\"",
					initialStatus.GetEnumTitle(),
					newStatus.GetEnumTitle()
				);
				foreach(var u in undeliveries) {
					u.AddCommentToTheField(UoW, CommentedFields.Reason, text);
				}
			}
		}
		

		public virtual void ChangeOrderPaymentStatus()
		{
			var paymentItems = orderRepository.GetPaymentItemsForOrder(UoW, Id);

			if (!paymentItems.Any()) 
				return;
			
			var paymentSum = paymentItems.Select(x => x.CashlessMovementOperation).Sum(x => x.Expense);

			if (paymentSum == 0)
				return;
			
			if (OrderPaymentStatus != OrderPaymentStatus.UnPaid)
			{
				ReturnPaymentToTheClientBalance(paymentSum, paymentItems);
				OrderPaymentStatus = OrderPaymentStatus.UnPaid;
			}
		}

		private void ReturnPaymentToTheClientBalance(decimal paymentSum, IList<PaymentItem> paymentItems)
		{
			var payment = paymentItems.Select(x => x.Payment).FirstOrDefault();

			if (payment == null)
				return;
			
			var newPayment = payment.CreatePaymentForReturnMoneyToClientBalance(paymentSum, this.Id);
			newPayment.CreateIncomeOperation();
			
			UoW.Save(newPayment.CashlessMovementOperation);
			UoW.Save(newPayment);
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
		public virtual void SelfDeliveryToLoading(ICurrentPermissionService permissionService, CallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery) {
				return;
			}
			if(OrderStatus == OrderStatus.Accepted && permissionService.ValidatePresetPermission("allow_load_selfdelivery")) {
				ChangeStatusAndCreateTasks(OrderStatus.OnLoading, callTaskWorker);
				LoadAllowedBy = employeeRepository.GetEmployeeForCurrentUser(UoW);
			}
		}

		public virtual void SetActualCountToSelfDelivery()
		{
			if(!SelfDelivery || OrderStatus != OrderStatus.Closed)
				return;

			foreach(var item in OrderItems)
				item.ActualCount = item.Count;

			foreach(var depositItem in OrderDepositItems)
				depositItem.ActualCount = depositItem.Count;
		}

		/// <summary>
		/// Принятие оплаты самовывоза по безналичному расчету
		/// </summary>
		public virtual void SelfDeliveryAcceptCashlessPaid(CallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery)
				return;
			if(PaymentType != PaymentType.cashless && PaymentType != PaymentType.ByCard)
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
		public virtual void SelfDeliveryAcceptCashPaid(CallTaskWorker callTaskWorker)
		{
			decimal totalCashPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id);
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		public virtual void AcceptSelfDeliveryIncomeCash(decimal incomeCash, CallTaskWorker callTaskWorker, int? incomeExcludedDoc = null)
		{
			decimal totalCashPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id, incomeExcludedDoc) + incomeCash;
			decimal totalCashReturn = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id);
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		public virtual void AcceptSelfDeliveryExpenseCash(decimal expenseCash, CallTaskWorker callTaskWorker, int? expenseExcludedDoc = null)
		{
			decimal totalCashPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id);
			decimal totalCashReturn = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id, expenseExcludedDoc) + expenseCash;
			SelfDeliveryAcceptCashPaid(totalCashPaid, totalCashReturn, callTaskWorker);
		}

		/// <summary>
		/// Принятие оплаты самовывоза по наличному расчету. С указанием дополнительным сумм по приходным и расходным ордерам
		/// Проверяется соответствие суммы заказа с суммой оплаченной в кассе.
		/// Если проверка пройдена заказ закрывается или переводится на погрузку.
		/// <paramref name="expenseCash">Сумма по открытому расходному ордеру, добавляемая к ранее сохранным расходным ордерам</paramref>
		/// <paramref name="incomeCash">Сумма по открытому приходному ордеру, добавляемая к ранее сохранным приходным ордерам</paramref>
		/// </summary>
		private void SelfDeliveryAcceptCashPaid(decimal incomeCash, decimal expenseCash, CallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery)
				return;
			if(PaymentType != PaymentType.cash && PaymentType != PaymentType.BeveragesWorld)
				return;
			if((incomeCash - expenseCash) != OrderCashSum)
				return;

			bool isFullyLoad = IsFullyShippedSelfDeliveryOrder(UoW, new SelfDeliveryRepository());

			if(OrderStatus == OrderStatus.WaitForPayment) {
				if(isFullyLoad) {
					ChangeStatusAndCreateTasks(OrderStatus.Closed, callTaskWorker);
					UpdateBottlesMovementOperationWithoutDelivery(UoW, new BaseParametersProvider(), new RouteListItemRepository(), new CashRepository(), incomeCash, expenseCash);
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
			decimal totalPaid = new CashRepository().GetIncomePaidSumForOrder(UoW, Id);

			return OrderSumTotal == totalPaid;
		}

		/// <summary>
		/// Проверяет полностью ли возвращены деньги по самовывозу
		/// </summary>
		public virtual bool SelfDeliveryIsFullyExpenseReturned()
		{
			decimal totalReturned = new CashRepository().GetExpenseReturnSumForOrder(UoW, Id);

			return OrderSumReturnTotal == totalReturned;
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
		private void AcceptSelfDeliveryOrder(CallTaskWorker callTaskWorker)
		{
			if(!SelfDelivery || OrderStatus != OrderStatus.NewOrder)
				return;

			if(PayAfterShipment || OrderTotalSum == 0)
				ChangeStatusAndCreateTasks(OrderStatus.Accepted, callTaskWorker);
			else
				ChangeStatusAndCreateTasks(OrderStatus.WaitForPayment, callTaskWorker);
		}

		/// <summary>
		/// Устанавливает количество для каждого залога как actualCount, 
		/// если заказ был создан только для залога.
		/// Для отображения этих данных в отчете "Акт по бутылям и залогам"
		/// </summary>
		public virtual void SetDepositsActualCounts() //TODO : проверить актуальность метода
		{
			if(OrderItems.All(x => x.Nomenclature.Id == 157))
				foreach(var oi in orderItems)
					oi.ActualCount = oi.Count > 0 ? oi.Count : (oi.ActualCount ?? 0);
		}

		public virtual void AcceptOrder(Employee currentEmployee, CallTaskWorker callTaskWorker)
		{
			if(SelfDelivery)
				AcceptSelfDeliveryOrder(callTaskWorker);
			else if(CanSetOrderAsAccepted)
				ChangeStatusAndCreateTasks(OrderStatus.Accepted, callTaskWorker);

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

		public virtual bool CanEditOrder => EditableOrderStatuses.Contains(OrderStatus);

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

		public virtual void EditOrder(CallTaskWorker callTaskWorker)
		{
			//Нельзя редактировать заказ с самовывозом
			if(SelfDelivery)
				return;

			if(CanSetOrderAsEditable)
				ChangeStatusAndCreateTasks(OrderStatus.NewOrder, callTaskWorker);
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

				if((int)totalCount != nGrp.Value)
					canCloseOrder = false;
			}

			return canCloseOrder;
		}

		private void UpdateSelfDeliveryActualCounts(SelfDeliveryDocument notSavedDocument = null)
		{
			var loadedDictionary = new SelfDeliveryRepository().OrderNomenclaturesLoaded(UoW, this);
			if(notSavedDocument != null && notSavedDocument.Id <= 0) {//если id > 0, то такой документ был учтён при получении словаря из репозитория
				foreach(var item in notSavedDocument.Items) {
					if(loadedDictionary.ContainsKey(item.Nomenclature.Id))
						loadedDictionary[item.Nomenclature.Id] += item.Amount;
					else
						loadedDictionary.Add(item.Nomenclature.Id, item.Amount);
				}
			}

			foreach(var item in OrderItems) {
				if(loadedDictionary.ContainsKey(item.Nomenclature.Id)) {
					//разбрасываем количества отгруженных по актуальным количествам в позициях заказа.
					int loadedCnt = (int)loadedDictionary[item.Nomenclature.Id];
					item.ActualCount = Math.Min(item.Count, loadedCnt);
					loadedDictionary[item.Nomenclature.Id] -= loadedCnt;
					if(loadedDictionary[item.Nomenclature.Id] <= 0)
						loadedDictionary.Remove(item.Nomenclature.Id);
				}
			}

			foreach(var item in OrderEquipments) {
				if(loadedDictionary.ContainsKey(item.Nomenclature.Id)) {
					//разбрасываем количества отгруженных по актуальным количествам в позициях заказа.
					int loadedCnt = (int)loadedDictionary[item.Nomenclature.Id];
					item.ActualCount = Math.Min(item.Count, loadedCnt);
					loadedDictionary[item.Nomenclature.Id] -= loadedCnt;
					if(loadedDictionary[item.Nomenclature.Id] <= 0)
						loadedDictionary.Remove(item.Nomenclature.Id);
				}
			}
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
			var res = IsWrongWater(out string title, out string message);
			if(res == true)
				return interactiveService.Question(message);
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
		public virtual bool TryCloseSelfDeliveryOrderWithCallTask(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, IRouteListItemRepository routeListItemRepository, ISelfDeliveryRepository selfDeliveryRepository, ICashRepository cashRepository, CallTaskWorker callTaskWorker, SelfDeliveryDocument closingDocument = null)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			bool isNotShipped = !IsFullyShippedSelfDeliveryOrder(uow, selfDeliveryRepository, closingDocument);

			if(!isNotShipped)
				UpdateBottlesMovementOperationWithoutDelivery(UoW, standartNomenclatures, routeListItemRepository, cashRepository);
			else
				return false;

			if(OrderStatus != OrderStatus.OnLoading)
				return false;

			bool isFullyPaid = SelfDeliveryIsFullyPaid(cashRepository);

			switch(PaymentType) {
				case PaymentType.cash:
				case PaymentType.BeveragesWorld:
					ChangeStatusAndCreateTasks(isFullyPaid ? OrderStatus.Closed : OrderStatus.WaitForPayment, callTaskWorker);
					break;
				case PaymentType.cashless:
				case PaymentType.ByCard:
					ChangeStatusAndCreateTasks(PayAfterShipment ? OrderStatus.WaitForPayment : OrderStatus.Closed, callTaskWorker);
					break;
				case PaymentType.barter:
				case PaymentType.ContractDoc:
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
		public virtual bool TryCloseSelfDeliveryOrder(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, IRouteListItemRepository routeListItemRepository, ISelfDeliveryRepository selfDeliveryRepository, ICashRepository cashRepository, SelfDeliveryDocument closingDocument = null)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(selfDeliveryRepository == null)
				throw new ArgumentNullException(nameof(selfDeliveryRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));

			bool isNotShipped = !IsFullyShippedSelfDeliveryOrder(uow, selfDeliveryRepository, closingDocument);

			if(!isNotShipped)
				UpdateBottlesMovementOperationWithoutDelivery(UoW, standartNomenclatures, routeListItemRepository, cashRepository);
			else
				return false;

			if(OrderStatus != OrderStatus.OnLoading)
				return false;

			bool isFullyPaid = SelfDeliveryIsFullyPaid(cashRepository);

			switch(PaymentType) {
				case PaymentType.cash:
				case PaymentType.BeveragesWorld:
					ChangeStatus(isFullyPaid ? OrderStatus.Closed : OrderStatus.WaitForPayment);
					break;
				case PaymentType.cashless:
				case PaymentType.ByCard:
					ChangeStatus(PayAfterShipment ? OrderStatus.WaitForPayment : OrderStatus.Closed);
					break;
				case PaymentType.barter:
				case PaymentType.ContractDoc:
					ChangeStatus(OrderStatus.Closed);
					break;
			}
			//обновление актуальных кол-в из документов самовывоза, включая не сохранённый
			//документ, откуда был вызов метода
			UpdateSelfDeliveryActualCounts(closingDocument);
			return true;
		}
		

		private void DeleteBottlesMovementOperation(IUnitOfWork uow)
		{
			if(BottlesMovementOperation != null) {
				uow.Delete(BottlesMovementOperation);
				BottlesMovementOperation = null;
			}
		}


		public virtual bool UpdateBottleMovementOperation(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, int returnByStock, int? forfeitQuantity = null)
		{
			if(IsContractCloser)
				return false;

			int amountDelivered = (int)OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.water && !item.Nomenclature.IsDisposableTare)
								.Sum(item => item?.ActualCount ?? 0);

			if(forfeitQuantity == null) {
				forfeitQuantity = (int)OrderItems.Where(i => i.Nomenclature.Id == standartNomenclatures.GetForfeitId())
							.Select(i => i?.ActualCount ?? 0)
							.Sum();
			}

			bool isValidCondition = amountDelivered != 0;
			isValidCondition |= returnByStock > 0;
			isValidCondition |= forfeitQuantity > 0;
			isValidCondition &= !orderRepository.GetUndeliveryStatuses().Contains(OrderStatus);

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
		public virtual void UpdateBottlesMovementOperationWithoutDelivery(IUnitOfWork uow, IStandartNomenclatures standartNomenclatures, IRouteListItemRepository routeListItemRepository, ICashRepository cashRepository, decimal incomeCash = 0, decimal expenseCash = 0)
		{
			if(routeListItemRepository == null)
				throw new ArgumentNullException(nameof(routeListItemRepository));
			if(cashRepository == null)
				throw new ArgumentNullException(nameof(cashRepository));
			if(standartNomenclatures == null)
				throw new ArgumentNullException(nameof(standartNomenclatures));

			//По заказам, у которых проставлен крыжик "Закрывашка по контракту", 
			//не должны создаваться операции перемещения тары
			if(IsContractCloser) {
				DeleteBottlesMovementOperation(uow);
				return;
			}

			if(routeListItemRepository.HasRouteListItemsForOrder(uow, this))
				return;

			foreach(OrderItem item in OrderItems)
				if(!item.ActualCount.HasValue)
					item.ActualCount = item.Count;

			int? forfeitQuantity = null;

			if(!SelfDelivery || SelfDeliveryIsFullyPaid(cashRepository, incomeCash, expenseCash))
				forfeitQuantity = (int)OrderItems.Where(i => i.Nomenclature.Id == standartNomenclatures.GetForfeitId())
											.Select(i => i.ActualCount ?? 0)
											.Sum();

			UpdateBottleMovementOperation(uow, standartNomenclatures, ReturnedTare ?? 0, forfeitQuantity ?? 0);
		}

		public virtual void ChangePaymentTypeToByCard (CallTaskWorker callTaskWorker)
		{
			PaymentType = PaymentType.ByCard;
			ChangeStatusAndCreateTasks(!PayAfterShipment ? OrderStatus.Accepted : OrderStatus.Closed, callTaskWorker);
		}

		#region Работа с документами

		public virtual OrderDocumentType[] GetRequirementDocTypes()
		{
			//создаём объект-ключ на основе текущего заказа. Этот ключ содержит набор свойств, 
			//по которым будет происходить подбор правила для создания набора документов
			var key = new OrderStateKey(this);

			//обращение к хранилищу правил для получения массива типов документов по ключу
			return OrderDocumentRulesRepository.GetSetOfDocumets(key);
		}

		public virtual void UpdateDocuments()
		{
			CheckAndCreateDocuments(GetRequirementDocTypes());
		}

		public virtual void UpdateCertificates(out List<Nomenclature> nomenclaturesNeedUpdate)
		{
			nomenclaturesNeedUpdate = new List<Nomenclature>();
			if(AddCertificates && DeliveryDate.HasValue) {
				IList<Certificate> newList = new List<Certificate>();
				foreach(var item in new NomenclatureRepository(new NomenclatureParametersProvider()).GetDictionaryWithCertificatesForNomenclatures(UoW, OrderItems.Select(i => i.Nomenclature).ToArray())) {
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
		}

		private void CheckAndCreateDocuments(params OrderDocumentType[] needed)
		{
			var docsOfOrder = typeof(OrderDocumentType).GetFields()
													   .Where(x => x.GetCustomAttributes(typeof(DocumentOfOrderAttribute), false).Any())
													   .Select(x => (OrderDocumentType)x.GetValue(null))
													   .ToArray();

			if(needed.Any(x => !docsOfOrder.Contains(x)))
				throw new ArgumentException($"В метод можно передавать только типы документов помеченные атрибутом {nameof(DocumentOfOrderAttribute)}", nameof(needed));

			var needCreate = needed.ToList();
			foreach(var doc in OrderDocuments.Where(d => d.Order?.Id == Id && docsOfOrder.Contains(d.Type)).ToList()) {
				if(needed.Contains(doc.Type))
					needCreate.Remove(doc.Type);
				else
					ObservableOrderDocuments.Remove(doc);
				if(OrderDocuments.Any(x => x.Order?.Id == Id && x.Id != doc.Id && x.Type == doc.Type)) {
					ObservableOrderDocuments.Remove(doc);
				}
			}
			//Создаем отсутствующие
			foreach(var type in needCreate) {
				if(ObservableOrderDocuments.Any(x => x.Order?.Id == Id && x.Type == type))
					continue;
				ObservableOrderDocuments.Add(CreateDocumentOfOrder(type));
			}
		}

		private OrderDocument CreateDocumentOfOrder(OrderDocumentType type)
		{
			OrderDocument newDoc;
			switch(type) {
				case OrderDocumentType.Bill:
					newDoc = new BillDocument();
					break;
				case OrderDocumentType.SpecialBill:
					newDoc = new SpecialBillDocument();
					break;
				case OrderDocumentType.UPD:
					newDoc = new UPDDocument();
					break;
				case OrderDocumentType.SpecialUPD:
					newDoc = new SpecialUPDDocument();
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
				default:
					throw new NotSupportedException("Не поддерживаемый тип документа");
			}
			newDoc.Order = newDoc.AttachedToOrder = this;
			return newDoc;
		}

		#endregion

		/// <summary>
		/// Возврат первого попавшегося контакта из цепочки:
		///0. Телефон для чеков точки доставки;
		///1. Телефон для чеков контрагента;
		///2. Эл.почта для чеков контрагентов;
		///3. Мобильный телефон точки доставки;
		///4. Мобильный телефон контрагента;
		///5. Эл.почта для счетов контрагента;
		///6. Иная эл. почта (не для чеков или счетов);
		///7. Городской телефон точки доставки;
		///8. Городской телефон контрагента.
		/// </summary>
		/// <returns>Контакт с минимальным весом.</returns>
		public virtual string GetContact()
		{
			if(Client == null)
				return null;
			//Dictionary<вес контакта, контакт>
			Dictionary<int, string> contacts = new Dictionary<int, string>();
			try {
				if(!SelfDelivery && DeliveryPoint != null && DeliveryPoint.Phones.Any()) {

					var receiptPhone = DeliveryPoint.Phones.FirstOrDefault(p => !String.IsNullOrWhiteSpace(p.DigitsNumber)
						&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts)?.DigitsNumber;
					if(receiptPhone != null)
						contacts[0] = receiptPhone;

					var phone = DeliveryPoint.Phones.FirstOrDefault(p => !String.IsNullOrWhiteSpace(p.DigitsNumber) && p.DigitsNumber.Substring(0, 1) == "9");
					if(phone != null)
						contacts[3] = phone.DigitsNumber;
					else if(DeliveryPoint.Phones.Any(p => !String.IsNullOrWhiteSpace(p.DigitsNumber)))
						contacts[7] = DeliveryPoint.Phones.FirstOrDefault(p => !String.IsNullOrWhiteSpace(p.DigitsNumber)).DigitsNumber;
				}
			} catch(GenericADOException ex) {
				logger.Error(ex.Message);
			}
			try {
				if(Client.Phones.Any()) {

					var receiptPhone = Client.Phones.FirstOrDefault(p => !String.IsNullOrWhiteSpace(p.DigitsNumber) 
						&& p.PhoneType?.PhonePurpose == PhonePurpose.ForReceipts)?.DigitsNumber;
					if(receiptPhone != null)
						contacts[0] = receiptPhone;

					var phone = Client.Phones.FirstOrDefault(p => !String.IsNullOrWhiteSpace(p.DigitsNumber) && p.DigitsNumber.Substring(0, 1) == "9");
					if(phone != null)
						contacts[4] = phone.DigitsNumber;
					else if(Client.Phones.Any(p => !String.IsNullOrWhiteSpace(p.DigitsNumber)))
						contacts[8] = Client.Phones.FirstOrDefault(p => !String.IsNullOrWhiteSpace(p.DigitsNumber)).DigitsNumber;
				}
			} catch(GenericADOException ex) {
				logger.Error(ex.Message);
			}
			try {
				if(Client.Emails.Any()) {
					var receiptEmail = Client.Emails.FirstOrDefault(e => !String.IsNullOrWhiteSpace(e.Address)
						 && e.EmailType?.EmailPurpose == EmailPurpose.ForReceipts)?.Address;
					if(receiptEmail != null)
						contacts[2] = receiptEmail;

					var billsEmail = Client.Emails.FirstOrDefault(e => !String.IsNullOrWhiteSpace(e.Address)
						&& e.EmailType?.EmailPurpose == EmailPurpose.ForBills)?.Address;
					if(billsEmail != null)
						contacts[5] = billsEmail;

					var email = Client.Emails.FirstOrDefault(e => 
						!String.IsNullOrWhiteSpace(e.Address)
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForBills 
						&& e.EmailType?.EmailPurpose != EmailPurpose.ForReceipts)
						?.Address;
					if(email != null)
						contacts[6] = email;
				}
			} catch(GenericADOException ex) {
				logger.Error(ex.Message);
			}
			if(!contacts.Any())
				return null;
			int minWeight = contacts.Min(c => c.Key);
			return contacts[minWeight];
		}

		public virtual void SaveOrderComment()
		{
			if(Id == 0) return;

			using(var uow = UnitOfWorkFactory.CreateForRoot<Order>(Id, "Кнопка сохранить только комментарий к заказу")) {
				uow.Root.Comment = Comment;
				uow.Save();
				uow.Commit();
			}
			UoW.Session.Refresh(this);
		}

		public virtual void SaveEntity(IUnitOfWork uow)
		{
			SetFirstOrder();
			LastEditor = employeeRepository.GetEmployeeForCurrentUser(UoW);
			LastEditedTime = DateTime.Now;
			ParseTareReason();
			ClearPromotionSets();
			uow.Save();
		}
		
		public virtual void RemoveReturnTareReason()
		{
			if (ReturnTareReason != null)
				ReturnTareReason = null;

			if(ReturnTareReasonCategory != null)
				ReturnTareReasonCategory = null;
		}

		#endregion

		#region работа со скидками
		public virtual void SetDiscountUnitsForAll(DiscountUnits unit)
		{
			foreach(OrderItem i in ObservableOrderItems) {
				i.IsDiscountInMoney = unit == DiscountUnits.money;
			}
		}

		/// <summary>
		/// Устанавливает скидку в рублях или процентах.
		/// Если скидка в %, то просто применяется к каждой строке заказа,
		/// а если в рублях - расчитывается % в зависимости от суммы заказа и рублёвой скидки
		/// и применяется этот % аналогично случаю с процентной скидкой.
		/// </summary>
		/// <param name="reason">Причина для скидки.</param>
		/// <param name="discount">Значение скидки.</param>
		/// <param name="unit">рубли или %.</param>
		public virtual void SetDiscount(DiscountReason reason, decimal discount, DiscountUnits unit)
		{
			if(unit == DiscountUnits.money) {
				var sum = ObservableOrderItems.Sum(i => i.CurrentCount * i.Price);
				if(sum == 0)
					return;
				discount = 100 * discount / sum;
			}
			foreach(OrderItem item in ObservableOrderItems) {
				item.DiscountSetter = unit == DiscountUnits.money ? discount * item.Price * item.CurrentCount / 100 : discount;
				item.DiscountReason = reason;
			}
		}

		#endregion

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

		decimal GetFixedPrice(OrderItem item) => item.GetWaterFixedPrice() ?? default(decimal);

		decimal GetNomenclaturePrice(OrderItem item)
		{
			decimal nomenclaturePrice = 0M;
			if(item.Nomenclature.IsWater19L) {
				nomenclaturePrice = item.Nomenclature.GetPrice(GetTotalWater19LCount());
			} else {
				nomenclaturePrice = item.Nomenclature.GetPrice(item.Count);
			}
			return nomenclaturePrice;
		}

		void ObservableOrderDepositItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(TotalSum));
		}

		protected internal virtual void ObservableOrderItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(TotalSum));
			UpdateDocuments();
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

		int CalculateGoDoorCount(int bottles, int atTime) => bottles / atTime + (bottles % atTime > 0 ? 1 : 0);

		/// <summary>
		/// Расчёт веса товаров и оборудования к клиенту для этого заказа
		/// </summary>
		/// <returns>Вес</returns>
		/// <param name="includeGoods">Если <c>true</c>, то в расчёт веса будут включены товары.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт веса будет включено оборудование.</param>
		public virtual double FullWeight(bool includeGoods = true, bool includeEquipment = true)
		{
			double weight = 0;
			if(includeGoods)
				weight += OrderItems.Sum(x => x.Nomenclature.Weight * (double) x.Count);
			if(includeEquipment)
				weight += OrderEquipments.Where(x => x.Direction == Direction.Deliver)
										 .Sum(x => x.Nomenclature.Weight * x.Count);
			return weight;
		}

		/// <summary>
		/// Расчёт объёма товаров и оборудования к клиенту для этого заказа
		/// </summary>
		/// <returns>Объём</returns>
		/// <param name="includeGoods">Если <c>true</c>, то в расчёт веса будут включены товары.</param>
		/// <param name="includeEquipment">Если <c>true</c>, то в расчёт веса будет включено оборудование.</param>
		public virtual double FullVolume(bool includeGoods = true, bool includeEquipment = true)
		{
			double volume = 0;
			if(includeGoods)
				volume += OrderItems.Sum(x => x.Nomenclature.Volume * (double) x.Count);
			if(includeEquipment)
				volume += OrderEquipments.Where(x => x.Direction == Direction.Deliver)
										 .Sum(x => x.Nomenclature.Volume * x.Count);
			return volume;
		}

		#endregion

		#region Статические

		public static OrderStatus[] StatusesToExport1c => new[] {
			OrderStatus.Accepted,
			OrderStatus.Closed,
			OrderStatus.InTravelList,
			OrderStatus.OnLoading,
			OrderStatus.OnTheWay,
			OrderStatus.Shipped,
			OrderStatus.UnloadingOnStock
		};

		#endregion

		#region Операции

		public virtual List<DepositOperation> UpdateDepositOperations(IUnitOfWork uow)
		{
			var bottleRefundDeposit = ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Bottles).Sum(x => x.Total);
			var equipmentRefundDeposit = ObservableOrderDepositItems.Where(x => x.DepositType == DepositType.Equipment).Sum(x => x.Total);
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
	}
}