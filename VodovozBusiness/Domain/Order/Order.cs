﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Service;
using Vodovoz.Repository;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Documents;
using QSProjectsLib;

namespace Vodovoz.Domain.Orders
{

	[OrmSubject(Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "заказы",
		Nominative = "заказ",
		Prepositional = "заказе",
		PrepositionalPlural = "заказах"
	)]
	public class Order : BusinessObjectBase<Order>, IDomainObject, IValidatableObject
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		#region Cвойства

		public virtual int Id { get; set; }

		OrderStatus orderStatus;

		[Display(Name = "Статус заказа")]
		public virtual OrderStatus OrderStatus {
			get { return orderStatus; }
			set { SetField(ref orderStatus, value, () => OrderStatus); }
		}

		Employee author;

		[Display(Name = "Создатель заказа")]

		public virtual Employee Author {
			get { return author; }
			set { SetField(ref author, value, () => Author); }
		}

		Counterparty client;

		[Display(Name = "Клиент")]
		public virtual Counterparty Client {
			get { return client; }
			set {
				if (value == client)
					return;
				if (client != null && !CanChangeContractor())
					throw new InvalidOperationException("Нельзя изменить клиента для заполненного заказа.");
				SetField(ref client, value, () => Client);
				if (DeliveryPoint != null && NHibernate.NHibernateUtil.IsInitialized(Client.DeliveryPoints) && !Client.DeliveryPoints.Any(d => d.Id == DeliveryPoint.Id)) {
					//FIXME Убрать когда поймем что проблемы с пропаданием точек доставки нет.
					logger.Warn("Очишаем точку доставки, при установке клиента. Возможно это не нужно.");
					DeliveryPoint = null;
				}
			}
		}

		DeliveryPoint deliveryPoint;

		[Display(Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set {
				SetField(ref deliveryPoint, value, () => DeliveryPoint);
				if (value != null && DeliverySchedule == null) {
					DeliverySchedule = value.DeliverySchedule;
				}
			}
		}

		DateTime? deliveryDate;

		[Display(Name = "Дата доставки")]
		public virtual DateTime? DeliveryDate {
			get { return deliveryDate; }
			set {
				SetField(ref deliveryDate, value, () => DeliveryDate);
				if (NHibernate.NHibernateUtil.IsInitialized(OrderDocuments) && value.HasValue) {
					foreach (OrderDocument document in OrderDocuments) {
						if (document.Type == OrderDocumentType.AdditionalAgreement) {
							(document as OrderAgreement).AdditionalAgreement.IssueDate = value.Value;
							(document as OrderAgreement).AdditionalAgreement.StartDate = value.Value;
						}
						//TODO FIXME Когда сделаю добавление документов для печати - фильтровать их здесь и не менять им дату.
					}
				}
			}
		}

		DeliverySchedule deliverySchedule;

		[Display(Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule {
			get { return deliverySchedule; }
			set { SetField(ref deliverySchedule, value, () => DeliverySchedule); }
		}

		private string deliverySchedule1c;

		[Display(Name = "Время доставки из 1С")]
		public virtual string DeliverySchedule1c {
			get {
				return string.IsNullOrWhiteSpace(deliverySchedule1c)
				  ? "Время доставки из 1С не загружено"
				  : deliverySchedule1c;
			}
			set { SetField(ref deliverySchedule1c, value, () => DeliverySchedule1c); }
		}

		bool selfDelivery;

		[Display(Name = "Самовывоз")]
		public virtual bool SelfDelivery {
			get { return selfDelivery; }
			set { SetField(ref selfDelivery, value, () => SelfDelivery); }
		}

		Order previousOrder;

		[Display(Name = "Предыдущий заказ")]
		public virtual Order PreviousOrder {
			get { return previousOrder; }
			set { SetField(ref previousOrder, value, () => PreviousOrder); }
		}

		int bottlesReturn;

		[Display(Name = "Бутылей на возврат")]
		public virtual int BottlesReturn {
			get { return bottlesReturn; }
			set { SetField(ref bottlesReturn, value, () => BottlesReturn); }
		}

		string comment;

		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField(ref comment, value, () => Comment); }
		}

		string commentLogist;

		[Display(Name = "Комментарий логиста")]
		public virtual string CommentLogist {
			get { return commentLogist; }
			set { SetField(ref commentLogist, value, () => CommentLogist); }
		}

		string clientPhone;

		[Display(Name = "Номер телефона")]
		public virtual string ClientPhone {
			get { return clientPhone; }
			set { SetField(ref clientPhone, value, () => ClientPhone); }
		}

		OrderSignatureType? signatureType;

		[Display(Name = "Подписание документов")]
		public virtual OrderSignatureType? SignatureType {
			get { return signatureType; }
			set { SetField(ref signatureType, value, () => SignatureType); }
		}

		private Decimal extraMoney;

		[Display(Name = "Доплата\\Переплата")]
		[PropertyChangedAlso(nameof(SumToReceive))]
		public virtual Decimal ExtraMoney {
			get { return extraMoney; }
			set { SetField(ref extraMoney, value, () => ExtraMoney); }
		}

		[Display(Name = "Наличных к получению")]
		public virtual Decimal SumToReceive {
			get {
				return PaymentType == PaymentType.cash ? TotalSum + ExtraMoney : 0;
			}
			protected set {; }
		}

		string sumDifferenceReason;

		[Display(Name = "Причина переплаты/недоплаты")]
		public virtual string SumDifferenceReason {
			get { return sumDifferenceReason; }
			set { SetField(ref sumDifferenceReason, value, () => SumDifferenceReason); }
		}

		bool shipped;

		[Display(Name = "Отгружено по платежке")]
		public virtual bool Shipped {
			get { return shipped; }
			set { SetField(ref shipped, value, () => Shipped); }
		}

		PaymentType paymentType;

		[Display(Name = "Форма оплаты")]
		public virtual PaymentType PaymentType {
			get { return paymentType; }
			set {
				if (value == paymentType)
					return;
				SetField(ref paymentType, value, () => PaymentType);
			}
		}

		CounterpartyContract contract;

		[Display(Name = "Договор")]
		public virtual CounterpartyContract Contract {
			get { return contract; }
			set { SetField(ref contract, value, () => Contract); }
		}

		MoneyMovementOperation moneyMovementOperation;

		public virtual MoneyMovementOperation MoneyMovementOperation {
			get { return moneyMovementOperation; }
			set {
				SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation);
			}
		}

		BottlesMovementOperation bottlesMovementOperation;

		public virtual BottlesMovementOperation BottlesMovementOperation {
			get {
				return bottlesMovementOperation;
			}
			set {
				SetField(ref bottlesMovementOperation, value, () => BottlesMovementOperation);
			}
		}

		IList<DepositOperation> depositOperations;

		public virtual IList<DepositOperation> DepositOperations {
			get { return depositOperations; }
			set { SetField(ref depositOperations, value, () => DepositOperations);}
		}

		bool collectBottles;

		public virtual bool CollectBottles {
			get {
				return collectBottles;
			}
			set {
				SetField(ref collectBottles, value, () => CollectBottles);
			}
		}

		DefaultDocumentType? documentType;

		[Display(Name = "Тип безналичных документов")]
		public virtual DefaultDocumentType? DocumentType {
			get { return documentType; }
			set { SetField(ref documentType, value, () => DocumentType); }
		}

		private string code1c;

		[Display(Name = "Код 1С")]
		public virtual string Code1c {
			get { return code1c; }
			set { SetField(ref code1c, value, () => Code1c); }
		}

		private string address1c;

		[Display(Name = "Адрес 1С")]
		public virtual string Address1c {
			get { return address1c; }
			set { SetField(ref address1c, value, () => Address1c); }
		}

		private string address1cCode;

		[Display(Name = "Код адреса 1С")]
		public virtual string Address1cCode {
			get { return address1cCode; }
			set { SetField(ref address1cCode, value, () => Address1cCode); }
		}

		private string fromClientText;

		[Display(Name = "Колонка МЛ от клиента")]
		public virtual string FromClientText {
			get { return fromClientText; }
			set { SetField(ref fromClientText, value, () => FromClientText); }
		}

		private string toClientText;

		[Display(Name = "Колонка МЛ к клиенту")]
		public virtual string ToClientText {
			get { return toClientText; }
			set { SetField(ref toClientText, value, () => ToClientText); }
		}

		private int? dailyNumber1c;

		[Display(Name = "Ежедневный номер из 1с")]
		public virtual int? DailyNumber1c {
			get { return dailyNumber1c; }
			set { SetField(ref dailyNumber1c, value, () => DailyNumber1c); }
		}

		Employee lastEditor;

		[Display(Name = "Последний редактор")]

		public virtual Employee LastEditor {
			get { return lastEditor; }
			set { SetField(ref lastEditor, value, () => LastEditor); }
		}

		DateTime lastEditedTime;

		[Display(Name = "Последние изменения")]
		public virtual DateTime LastEditedTime {
			get { return lastEditedTime; }
			set { SetField(ref lastEditedTime, value, () => LastEditedTime); }
		}

		string commentManager;

		[Display(Name = "Комментарий менеджера")]
		public virtual string CommentManager {
			get { return commentManager; }
			set { SetField(ref commentManager, value, () => CommentManager); }
		}

		int? returnedTare;

		[Display(Name = "Возвратная тара")]
		public virtual int? ReturnedTare {
			get { return returnedTare; }
			set { SetField(ref returnedTare, value, () => ReturnedTare); }
		}

		string informationOnTara;

		[Display(Name = "Информация о таре")]
		public virtual string InformationOnTara {
			get { return informationOnTara; }
			set { SetField(ref informationOnTara, value, () => InformationOnTara); }
		}

		ReasonType resonType;

		[Display(Name = "Тип причины")]
		public virtual ReasonType ReasonType {
			get { return resonType; }
			set { SetField(ref resonType, value, () => ReasonType); }
		}

		DriverCallType driverCallType;

		[Display(Name = "Водитель отзвонился")]
		public virtual DriverCallType DriverCallType {
			get { return driverCallType; }
			set { SetField(ref driverCallType, value, () => DriverCallType); }
		}

		int? driverCallId;

		[Display(Name = "Номер звонка водителя")]
		public virtual int? DriverCallId {
			get { return driverCallId; }
			set { SetField(ref driverCallId, value, () => DriverCallId); }
		}

		bool isService;

		[Display(Name = "Сервисное обслуживание")]
		public virtual bool IsService
		{
			get { return isService; }
			set { SetField(ref isService, value, () => IsService); }
		}

		#endregion

		public virtual bool CanChangeContractor()
		{
			if ((NHibernate.NHibernateUtil.IsInitialized(OrderDocuments) && OrderDocuments.Count > 0) ||
				(NHibernate.NHibernateUtil.IsInitialized(InitialOrderService) && InitialOrderService.Count > 0) ||
				(NHibernate.NHibernateUtil.IsInitialized(FinalOrderService) && FinalOrderService.Count > 0))
				return false;
			return true;
		}

		IList<OrderDepositItem> orderDepositItems = new List<OrderDepositItem>();

		[Display(Name = "Залоги заказа")]
		public virtual IList<OrderDepositItem> OrderDepositItems {
			get { return orderDepositItems; }
			set { SetField(ref orderDepositItems, value, () => OrderDepositItems); }
		}

		GenericObservableList<OrderDepositItem> observableOrderDepositItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDepositItem> ObservableOrderDepositItems {
			get {
				if (observableOrderDepositItems == null) {
					observableOrderDepositItems = new GenericObservableList<OrderDepositItem>(OrderDepositItems);
					observableOrderDepositItems.ListContentChanged += ObservableOrderDepositItems_ListContentChanged;
				}
				return observableOrderDepositItems;
			}
		}

		IList<OrderDocument> orderDocuments = new List<OrderDocument>();

		[Display(Name = "Документы заказа")]
		public virtual IList<OrderDocument> OrderDocuments {
			get { return orderDocuments; }
			set { SetField(ref orderDocuments, value, () => OrderDocuments); }
		}

		GenericObservableList<OrderDocument> observableOrderDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderDocument> ObservableOrderDocuments {
			get {
				if (observableOrderDocuments == null)
					observableOrderDocuments = new GenericObservableList<OrderDocument>(OrderDocuments);
				return observableOrderDocuments;
			}
		}

		IList<OrderItem> orderItems = new List<OrderItem>();

		[Display(Name = "Строки заказа")]
		public virtual IList<OrderItem> OrderItems {
			get { return orderItems; }
			set { SetField(ref orderItems, value, () => OrderItems); }
		}

		GenericObservableList<OrderItem> observableOrderItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderItem> ObservableOrderItems {
			get {
				if (observableOrderItems == null) {
					observableOrderItems = new GenericObservableList<OrderItem>(orderItems);
					observableOrderItems.ListContentChanged += ObservableOrderItems_ListContentChanged;
				}

				return observableOrderItems;
			}
		}

		IList<OrderEquipment> orderEquipments = new List<OrderEquipment>();

		[Display(Name = "Список оборудования")]
		public virtual IList<OrderEquipment> OrderEquipments {
			get { return orderEquipments; }
			set { SetField(ref orderEquipments, value, () => OrderEquipments); }
		}

		GenericObservableList<OrderEquipment> observableOrderEquipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<OrderEquipment> ObservableOrderEquipments {
			get {
				if (observableOrderEquipments == null)
					observableOrderEquipments = new GenericObservableList<OrderEquipment>(orderEquipments);
				return observableOrderEquipments;
			}
		}

		IList<ServiceClaim> initialOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> InitialOrderService {
			get { return initialOrderService; }
			set { SetField(ref initialOrderService, value, () => InitialOrderService); }
		}

		GenericObservableList<ServiceClaim> observableInitialOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableInitialOrderService {
			get {
				if (observableInitialOrderService == null)
					observableInitialOrderService = new GenericObservableList<ServiceClaim>(InitialOrderService);
				return observableInitialOrderService;
			}
		}

		IList<ServiceClaim> finalOrderService = new List<ServiceClaim>();

		[Display(Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> FinalOrderService {
			get { return finalOrderService; }
			set { SetField(ref finalOrderService, value, () => FinalOrderService); }
		}

		GenericObservableList<ServiceClaim> observableFinalOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ServiceClaim> ObservableFinalOrderService {
			get {
				if (observableFinalOrderService == null)
					observableFinalOrderService = new GenericObservableList<ServiceClaim>(FinalOrderService);
				return observableFinalOrderService;
			}
		}

		public Order()
		{
			Comment = String.Empty;
			OrderStatus = OrderStatus.NewOrder;
			SumDifferenceReason = String.Empty;
			ClientPhone = String.Empty;
		}

		public static Order CreateFromServiceClaim(ServiceClaim service, Employee author)
		{
			var order = new Order();
			order.client = service.Counterparty;
			order.DeliveryPoint = service.DeliveryPoint;
			order.DeliveryDate = service.ServiceStartDate;
			order.PaymentType = service.Payment;
			order.Author = author;
			service.InitialOrder = order;
			order.AddServiceClaimAsInitial(service);
			return order;
		}

		#region IValidatableObject implementation

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (validationContext.Items.ContainsKey("NewStatus")) {
				OrderStatus newStatus = (OrderStatus)validationContext.Items["NewStatus"];
				if (newStatus == OrderStatus.Accepted) {
					if (DeliveryDate == null || DeliveryDate == default(DateTime))
						yield return new ValidationResult("Не указана дата доставки.",
							new[] { this.GetPropertyName(o => o.DeliveryDate) });
					if (!SelfDelivery && DeliverySchedule == null)
						yield return new ValidationResult("Не указано время доставки.",
							new[] { this.GetPropertyName(o => o.DeliverySchedule) });

#if !SHORT
					if (PaymentType == PaymentType.cashless && !SignatureType.HasValue)
						yield return new ValidationResult ("Не указано как будут подписаны документы.",
							new[] { this.GetPropertyName (o => o.SignatureType) });

					if (Contract == null)
						yield return new ValidationResult ("Не указан договор.",
							new[] { this.GetPropertyName (o => o.Contract) });

					//Проверка товаров
					var itemsWithBlankWarehouse = OrderItems
						.Where(orderItem => Nomenclature.GetCategoriesForShipment().Contains(orderItem.Nomenclature.Category))
						.Where(orderItem => orderItem.Nomenclature.Warehouse==null);
					foreach(var itemWithBlankWarehouse in itemsWithBlankWarehouse)
					{
						yield return new ValidationResult (
							String.Format("Невозможно подтвердить заказ т.к. у товара \"{0}\" не указан склад отгрузки.",
							itemWithBlankWarehouse.NomenclatureString),
							new[] { this.GetPropertyName (o => o.OrderItems) });						
					}

					var orderItemsForShipment = OrderItems
						.Where(orderItem => Nomenclature.GetCategoriesForShipment().Contains(orderItem.Nomenclature.Category))
						.Where(orderItem => orderItem.Nomenclature.Warehouse!=null)
						.Where(orderItem => orderItem.Nomenclature.DoNotReserve == false);
					foreach (var item in orderItemsForShipment)
					{
						var inStock = Repository.StockRepository.NomenclatureInStock(UoW, item.Nomenclature);
						var reserved = Repository.StockRepository.NomenclatureReserved(UoW, item.Nomenclature);
						if (inStock-reserved < item.Count)
						{
							if (item.Nomenclature.Unit == null)
							{
								yield return new ValidationResult(
									String.Format("У номенклатуры \"{0}\" с кодом {1} не стоит единица измерения).",
										item.NomenclatureString,
										item.Nomenclature.Id
									),
									new[] { this.GetPropertyName(o => o.OrderItems) });
							} else {
								yield return new ValidationResult(
									String.Format("Товара \"{0}\" нет на складе \"{1}\" в достаточном количестве(на складе: {2}, в резерве: {3}).",
										item.NomenclatureString,
										item.Nomenclature.Warehouse.Name,
										item.Nomenclature.Unit.MakeAmountShortStr(inStock),
										item.Nomenclature.Unit.MakeAmountShortStr(reserved)
									),
									new[] { this.GetPropertyName(o => o.OrderItems) });
							}
						}
					}

#endif
				}

				if (newStatus == OrderStatus.Closed) {
					foreach (var equipment in OrderEquipments) {
						if (!equipment.Confirmed && String.IsNullOrWhiteSpace(equipment.ConfirmedComment))
							yield return new ValidationResult(
								String.Format("Забор оборудования {0} по заказу {1} не произведен, а в комментарии не указана причина.",
									equipment.NameString, Id),
								new[] { this.GetPropertyName(o => o.OrderEquipments) });
					}
				}
			}

			if (!SelfDelivery && DeliveryPoint == null)
				yield return new ValidationResult("Необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName(o => o.DeliveryPoint) });
			if (Client == null)
				yield return new ValidationResult("Необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName(o => o.Client) });
#if !SHORT
			if (ObservableOrderItems.Any (item => item.Count < 1))
				yield return new ValidationResult ("В заказе присутствуют позиции с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.OrderItems) });
#endif
		}

		#endregion

		#region Вычисляемые

		public override string ToString()
		{
			return String.Format("Заказ №{0}({1})", Id, Code1c);
		}

		public virtual string Title {
			get { return String.Format("Заказ №{0} от {1:d}", Id, DeliveryDate); }
		}

		public virtual int TotalDeliveredBottles {
			get {
				return OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water).Sum(x => x.Count);
			}
		}

		public virtual int TotalDeliveredBottlesSix {
			get {
				return OrderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.disposableBottleWater).Sum(x => x.Count);
			}
		}

		public virtual int TotalWeight {
			get {
				return (int)OrderItems.Sum(x => x.Count * x.Nomenclature.Weight);
			}
		}

		public virtual string RowColor { get { return PreviousOrder == null ? "black" : "red"; } }

		[PropertyChangedAlso(nameof(SumToReceive))]
		public virtual decimal TotalSum {
			get {
				Decimal sum = 0;
				foreach (OrderItem item in ObservableOrderItems) {
					sum += item.Price * item.Count * (1 - (decimal)item.Discount / 100);
				}
				foreach (OrderDepositItem dep in ObservableOrderDepositItems) {
					if (dep.PaymentDirection == PaymentDirection.ToClient)
						sum -= dep.Deposit * dep.Count;
				}
				return sum;
			}
		}

		public virtual decimal ActualTotalSum {
			get {
				Decimal sum = 0;
				foreach (OrderItem item in ObservableOrderItems) {
					sum += item.Price * item.ActualCount * (1 - (decimal)item.Discount / 100);
				}
				foreach (OrderDepositItem dep in ObservableOrderDepositItems) {
					if (dep.PaymentDirection == PaymentDirection.ToClient)
						sum -= dep.Deposit * dep.Count;
				}
				return sum;
			}
		}

		public virtual decimal ActualGoodsTotalSum {
			get {
				return OrderItems.Sum(item => item.Price * item.ActualCount);
			}
		}

		/// <summary>
		/// Количество 19л бутылей в заказе
		/// </summary>
		public virtual int TotalWaterBottles
		{
			get {
				return orderItems.Where(x => x.Nomenclature.Category == NomenclatureCategory.water).Sum(x => x.Count);
			}
		}

		#endregion

		#region Функции

		public virtual void AddEquipmentNomenclatureForSale(Nomenclature nomenclature, IUnitOfWork UoW)
		{
			if (nomenclature.Category != NomenclatureCategory.equipment)
				return;
			if(!nomenclature.IsSerial) {
				ObservableOrderItems.Add(new OrderItem {
					Order = this,
					AdditionalAgreement = null,
					Count = 0,
					Equipment = null,
					Nomenclature = nomenclature,
					Price = nomenclature.GetPrice(1)
				});
			} 
			else {
				Equipment eq = EquipmentRepository.GetEquipmentForSaleByNomenclature(UoW, nomenclature);
				ObservableOrderItems.AddWithReturn(new OrderItem {
					Order = this,
					AdditionalAgreement = null,
					Count = 1,
					Equipment = eq,
					Nomenclature = nomenclature,
					Price = nomenclature.GetPrice(1)
				});
			}
			UpdateDocuments();
		}

		public virtual void AddEquipmentNomenclatureForRepair(Nomenclature nomenclature, IUnitOfWork UoW)
		{
			if(nomenclature.Category != NomenclatureCategory.equipment)
				return;
			if(!nomenclature.IsSerial) {
				ObservableOrderEquipments.Add(new OrderEquipment {
					
					Order = this,
					Direction = Direction.Deliver,
				    Equipment = null,
					OrderItem = null,
					Reason = Reason.Service,
					Confirmed = true,
					Nomenclature = nomenclature
				});
			 } 
			UpdateDocuments();
		}

		public virtual void AddEquipmentNomenclatureForRepairFromClient(Nomenclature nomenclature, IUnitOfWork UoW)
		{
			if(nomenclature.Category != NomenclatureCategory.equipment)
				return;
			if(!nomenclature.IsSerial) {
				ObservableOrderEquipments.Add(new OrderEquipment {

					Order = this,
					Direction = Direction.PickUp,
					Equipment = null,
					OrderItem = null,
					Reason = Reason.Service,
					Confirmed = true,
					Nomenclature = nomenclature
				});
			}
			UpdateDocuments();
		}

		public virtual void DeleteEquipment( OrderEquipment item)
		{
			ObservableOrderEquipments.Remove(item);
			UpdateDocuments();
		}

		public virtual void AddAnyGoodsNomenclatureForSale(Nomenclature nomenclature)
		{
			if (nomenclature.Category != NomenclatureCategory.additional && nomenclature.Category != NomenclatureCategory.bottle &&
				nomenclature.Category != NomenclatureCategory.service && nomenclature.Category != NomenclatureCategory.disposableBottleWater)
				return;
			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				AdditionalAgreement = null,
				Count = nomenclature.Category == NomenclatureCategory.service ? 1 : 0,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice(1)
			});
			UpdateDocuments();
		}

		public virtual void AddWaterForSale(Nomenclature nomenclature, WaterSalesAgreement wsa)
		{
			if (nomenclature.Category != NomenclatureCategory.water)
				return;
			/*	if (ObservableOrderItems.Any (item => item.Nomenclature.Id == nomenclature.Id &&
					item.AdditionalAgreement.Id == wsa.Id))
					return; */ // (I-441) @Дима
			decimal price;
			if (wsa.IsFixedPrice && wsa.FixedPrices.Any(x => x.Nomenclature.Id == nomenclature.Id))
				price = wsa.FixedPrices.First(x => x.Nomenclature.Id == nomenclature.Id).Price;
			else
				price = nomenclature.GetPrice(1);

			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				AdditionalAgreement = wsa,
				Count = 0,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = price
			});
			UpdateDocuments();
		}

		#region test_methods_for_sidebar

		/// <summary>
		/// Добавить оборудование из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		/// <param name="UoW">IUnitOfWork</param>
		public virtual void AddEquipmentNomenclatureForSaleFromPreviousOrder(OrderItem orderItem, IUnitOfWork UoW)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.equipment)
				return;
			if(!orderItem.Nomenclature.IsSerial) {
				ObservableOrderItems.Add(new OrderItem {
					Order = this,
					AdditionalAgreement = orderItem.AdditionalAgreement,
					Count = orderItem.Count,
					Equipment = orderItem.Equipment,
					Nomenclature = orderItem.Nomenclature,
					Price = orderItem.Price
				});
			} else {
				ObservableOrderItems.AddWithReturn(new OrderItem {
					Order = this,
					AdditionalAgreement = orderItem.AdditionalAgreement,
					Count = orderItem.Count,
					Equipment = orderItem.Equipment,
					Nomenclature = orderItem.Nomenclature,
					Price = orderItem.Price
				});
			}
			UpdateDocuments();
		}

		/// <summary>
		/// Добавить номенклатуру (не вода и не оборудование из выбранного предыдущего заказа).
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		public virtual void AddAnyGoodsNomenclatureForSaleFromPreviousOrder(OrderItem orderItem)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.additional && orderItem.Nomenclature.Category != NomenclatureCategory.bottle &&
				orderItem.Nomenclature.Category != NomenclatureCategory.service && orderItem.Nomenclature.Category != NomenclatureCategory.disposableBottleWater)
				return;
			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				AdditionalAgreement = orderItem.AdditionalAgreement,
				Count = orderItem.Nomenclature.Category == NomenclatureCategory.service ? 1 : 0,
				Equipment = orderItem.Equipment,
				Nomenclature = orderItem.Nomenclature,
				Price = orderItem.Price
			});
			UpdateDocuments();
		}

		/// <summary>
		/// Добавить воду из выбранного прерыдущего заказа.
		/// </summary>
		/// <param name="orderItem">Элемент заказа.</param>
		/// <param name="wsa">Договор о продаже воды.</param>
		public virtual void AddWaterForSaleFromPreviousOrder(OrderItem orderItem, WaterSalesAgreement wsa)
		{
			if(orderItem.Nomenclature.Category != NomenclatureCategory.water)
				return;
			decimal price;
			if(wsa.IsFixedPrice && wsa.FixedPrices.Any(x => x.Nomenclature.Id == orderItem.Nomenclature.Id))
				price = wsa.FixedPrices.First(x => x.Nomenclature.Id == orderItem.Nomenclature.Id).Price;
			else
				price = orderItem.Price;

			ObservableOrderItems.Add(new OrderItem {
				Order = this,
				AdditionalAgreement = wsa,
				Count = orderItem.Count,
				Equipment = null,
				Nomenclature = orderItem.Nomenclature,
				Price = price
			});
			UpdateDocuments();
		}

		public virtual void ClearOrderItemsList()
		{
			ObservableOrderItems.Clear();
			UpdateDocuments();
		}

		#endregion

		public virtual void RecalcBottlesDeposits(IUnitOfWork uow)
		{
			var expectedBottleDepositsCount = GetExpectedBottlesDepositsCount();
			var bottleDeposit = NomenclatureRepository.GetBottleDeposit(uow);
			if (bottleDeposit == null)
				throw new InvalidProgramException("В параметрах базы не настроена номенклатура залога за бутыли.");
			var depositPaymentItem = ObservableOrderItems.FirstOrDefault(item => item.Nomenclature.Id == bottleDeposit.Id);
			var depositRefundItem = ObservableOrderDepositItems.FirstOrDefault(item => item.DepositType == DepositType.Bottles);

			//Надо создать услугу залога
			if (expectedBottleDepositsCount > 0) {
				if (depositRefundItem != null) {
					depositRefundItem.Count = expectedBottleDepositsCount;
					depositRefundItem.PaymentDirection = PaymentDirection.FromClient;
				}
				if (depositPaymentItem != null)
					depositPaymentItem.Count = expectedBottleDepositsCount;
				else {
					/* Временно отключил взятие с клиента залогов за бутыли. Удалить если залоги так и не вернутся.
					 * 					ObservableOrderItems.Add (new OrderItem {
											Order = this,
											AdditionalAgreement = null,
											Count = expectedBottleDepositsCount,
											Equipment = null,
											Nomenclature = NomenclatureRepository.GetBottleDeposit (uow),
											Price = NomenclatureRepository.GetBottleDeposit (uow).GetPrice (expectedBottleDepositsCount)
										});
										ObservableOrderDepositItems.Add (new OrderDepositItem {
											Order = this,
											Count = expectedBottleDepositsCount,
											Deposit = NomenclatureRepository.GetBottleDeposit (uow).GetPrice (expectedBottleDepositsCount),
											DepositOperation = null,
											DepositType = DepositType.Bottles,
											FreeRentItem = null,
											PaidRentItem = null,
											PaymentDirection = PaymentDirection.FromClient
										}); */
				}
				return;
			}
			if (expectedBottleDepositsCount == 0) {
				if (depositRefundItem != null)
					ObservableOrderDepositItems.Remove(depositRefundItem);
				if (depositPaymentItem != null)
					ObservableOrderItems.Remove(depositPaymentItem);
				return;
			}
			if (expectedBottleDepositsCount < 0) {
				if (depositPaymentItem != null)
					ObservableOrderItems.Remove(depositPaymentItem);
				//Проверяем, сколько надо отдать клиенту залог за бутыли
				decimal clientDeposit = default(decimal);
				decimal deposit = NomenclatureRepository.GetBottleDeposit(uow).GetPrice(-expectedBottleDepositsCount);
				int count = -expectedBottleDepositsCount;
				if (Client != null)
					clientDeposit = Repository.Operations.DepositRepository.GetDepositsAtCounterparty(UoW, Client, DepositType.Bottles);
				if (clientDeposit - deposit * count >= 0)
					if (depositRefundItem != null) {
						depositRefundItem.Deposit = deposit;
						depositRefundItem.Count = count;
					} else
						ObservableOrderDepositItems.Add(new OrderDepositItem {
							Order = this,
							DepositOperation = null,
							DepositType = DepositType.Bottles,
							Deposit = deposit,
							PaidRentItem = null,
							FreeRentItem = null,
							PaymentDirection = PaymentDirection.ToClient,
							Count = count
						});
				return;
			}
		}

		public virtual int GetExpectedBottlesDepositsCount()
		{
			if (Client == null || Client.PersonType == PersonType.legal)
				return 0;

			var waterItemsCount = ObservableOrderItems.Select(item => item)
				.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum(item => item.Count);

			return waterItemsCount - BottlesReturn;
		}

		public virtual void FillItemsFromAgreement(AdditionalAgreement a)
		{
			if (a.Type == AgreementType.DailyRent || a.Type == AgreementType.NonfreeRent) {
				IList<PaidRentEquipment> EquipmentList;
				bool IsDaily = false;

				if (a.Type == AgreementType.DailyRent) {
					EquipmentList = (a as DailyRentAgreement).Equipment;
					IsDaily = true;
				} else
					EquipmentList = (a as NonfreeRentAgreement).Equipment;

				foreach (PaidRentEquipment equipment in EquipmentList) {
					int ItemId;
					//Добавляем номенклатуру залога
					OrderItem orderItem = null;
					if ((orderItem = ObservableOrderItems.FirstOrDefault<OrderItem>(
							item => item.AdditionalAgreement.Id == a.Id &&
							item.Nomenclature.Id == equipment.PaidRentPackage.DepositService.Id &&
							item.Price == equipment.Deposit)) != null) {
						orderItem.Count++;
						orderItem.Price = equipment.Deposit;
					} else {
						ObservableOrderItems.Add(
							new OrderItem {
								Order = this,
								AdditionalAgreement = a,
								Count = 1,
								Equipment = null,
								Nomenclature = equipment.PaidRentPackage.DepositService,
								Price = equipment.Deposit
							}
						);
					}
					//Добавляем услугу аренды
					orderItem = null;
					if ((orderItem = ObservableOrderItems.FirstOrDefault<OrderItem>(
							item => item.AdditionalAgreement.Id == a.Id &&
							item.Nomenclature.Id == (IsDaily ? equipment.PaidRentPackage.RentServiceDaily.Id : equipment.PaidRentPackage.RentServiceMonthly.Id) &&
							item.Price == equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1))) != null) {
						orderItem.Count++;
						orderItem.Price = orderItem.Nomenclature.GetPrice(orderItem.Count);
						ItemId = ObservableOrderItems.IndexOf(orderItem);
					} else {
						ItemId = ObservableOrderItems.AddWithReturn(
							new OrderItem {
								Order = this,
								AdditionalAgreement = a,
								Count = 1,
								Equipment = null,
								Nomenclature = IsDaily ? equipment.PaidRentPackage.RentServiceDaily : equipment.PaidRentPackage.RentServiceMonthly,
								Price = equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1)
							}
						);
					}
					//Добавляем оборудование
					ObservableOrderEquipments.Add(
						new OrderEquipment {
							Order = this,
							Direction = Direction.Deliver,
							Equipment = equipment.Equipment,
							Nomenclature = equipment.Equipment.Nomenclature,
							Reason = Reason.Rent,
							OrderItem = ObservableOrderItems[ItemId]
						}
					);
					OnPropertyChanged(nameof(TotalSum));
					OnPropertyChanged(nameof(SumToReceive));
				}
			} else if (a.Type == AgreementType.FreeRent) {
				FreeRentAgreement agreement = a as FreeRentAgreement;
				foreach (FreeRentEquipment equipment in agreement.Equipment) {
					int ItemId;
					//Добавляем номенклатуру залога.
					ItemId = ObservableOrderItems.AddWithReturn(
						new OrderItem {
							Order = this,
							AdditionalAgreement = agreement,
							Count = 1,
							Equipment = null,
							Nomenclature = equipment.FreeRentPackage.DepositService,
							Price = equipment.Deposit
						}
					);
					//Добавляем оборудование.
					ObservableOrderEquipments.Add(
						new OrderEquipment {
							Order = this,
							Direction = Direction.Deliver,
							Equipment = equipment.Equipment,
							Reason = Reason.Rent,
							OrderItem = ObservableOrderItems[ItemId]
						}
					);
				}
			}
			UpdateDocuments();
		}

		public virtual void RemoveItem(OrderItem item)
		{
			ObservableOrderItems.Remove(item);
			foreach (var equip in ObservableOrderEquipments.Where(e => e.OrderItem == item).ToList()) {
				ObservableOrderEquipments.Remove(equip);
			}
			UpdateDocuments();
		}

		public virtual void AddServiceClaimAsInitial(ServiceClaim service)
		{
			if (service.InitialOrder != null && service.InitialOrder.Id == Id) {
				if (service.Equipment == null || ObservableOrderEquipments.FirstOrDefault(eq => eq.Equipment.Id == service.Equipment.Id) == null) {
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
				if (service.ReplacementEquipment != null) {
					observableOrderEquipments.Add(new OrderEquipment {
						Order = this,
						Direction = Direction.Deliver,
						Equipment = service.ReplacementEquipment,
						Nomenclature = null,
						OrderItem = null,
						Reason = Reason.Service
					});
				}
				if (ObservableInitialOrderService.FirstOrDefault(sc => sc.Id == service.Id) == null)
					ObservableInitialOrderService.Add(service);
				if (ObservableOrderDocuments.Where(doc => doc.Type == OrderDocumentType.EquipmentTransfer).Cast<EquipmentTransferDocument>()
					.FirstOrDefault(doc => doc.ServiceClaim.Id == service.Id) == null) {
					ObservableOrderDocuments.Add(new EquipmentTransferDocument {
						Order = this,
						ServiceClaim = service
					});
				}
			}
		}

		public virtual void AddServiceClaimAsFinal(ServiceClaim service)
		{
			if (service.FinalOrder != null && service.FinalOrder.Id == Id) {
				if (ObservableOrderEquipments.FirstOrDefault(eq => eq.Equipment.Id == service.Equipment.Id) == null) {
					ObservableOrderEquipments.Add(new OrderEquipment {
						Order = this,
						Direction = Direction.Deliver,
						Equipment = service.Equipment,
						OrderItem = null,
						Reason = Reason.Service
					});
				}
				if (ObservableOrderDocuments.Where(doc => doc.Type == OrderDocumentType.DoneWorkReport).Cast<DoneWorkDocument>()
					.FirstOrDefault(doc => doc.ServiceClaim.Id == service.Id) == null) {
					ObservableOrderDocuments.Add(new DoneWorkDocument {
						Order = this,
						ServiceClaim = service
					});
				}
			}
			//TODO FIXME Добавить строку сервиса OrderItems
			//И вообще много чего тут сделать.
		}

		public virtual void FillNewEquipment(Equipment registeredEquipment)
		{
			var newEquipment = ObservableOrderEquipments
				.Where(orderEq => orderEq.Nomenclature != null)
				.FirstOrDefault(orderEq => orderEq.Nomenclature.Id == registeredEquipment.Nomenclature.Id);
			if (newEquipment != null) {
				newEquipment.Equipment = registeredEquipment;
				newEquipment.Nomenclature = null;
			}
		}

		public virtual void ChangeStatus(OrderStatus newStatus)
		{
			OrderStatus = newStatus;
			if(newStatus == OrderStatus.Closed) {
				OnClosedOrder();
			}
		}

		/// <summary>
		/// Действия при закрытии заказа
		/// </summary>
		public virtual void OnClosedOrder()
		{
			SetDepositsActualCounts();
		}

		/// <summary>
		/// Устанавливает количество для каждого залога как actualCount, 
		/// если заказ был создан только для залога.
		/// Для отображения этих данных в отчете "Акт по бутылям и залогам"
		/// </summary>
		public virtual void SetDepositsActualCounts()
		{
			if(OrderItems.All(x => x.Nomenclature.Id == 157)) {
				foreach(var oi in orderItems) {
					oi.ActualCount = oi.Count;
				}
			}
		}

		public virtual void UpdateDocuments()
		{
			if (ObservableOrderItems.Count > 0 && PaymentType == PaymentType.cashless) {
				AddDocumentIfNotExist(new BillDocument {
					Order = this
				});
			} else
				RemoveDocumentByType(OrderDocumentType.Bill);

			if (ObservableOrderItems.Count > 0 && OrderStatus == OrderStatus.Accepted) {
				if (paymentType == PaymentType.cashless) {
					if (this.DocumentType == DefaultDocumentType.upd) {
						RemoveDocumentByType(OrderDocumentType.Torg12);
						RemoveDocumentByType(OrderDocumentType.ShetFactura);
						AddDocumentIfNotExist(new UPDDocument {
							Order = this
						});
					} else if (this.DocumentType == DefaultDocumentType.torg12) {
						RemoveDocumentByType(OrderDocumentType.UPD);
						AddDocumentIfNotExist(new Torg12Document {
							Order = this
						});
						AddDocumentIfNotExist(new ShetFacturaDocument {
							Order = this
						});
					}
					AddDocumentIfNotExist(new DriverTicketDocument {
						Order = this
					});
				}
				if (paymentType == PaymentType.cash) {
					AddDocumentIfNotExist(new InvoiceDocument {
						Order = this
					});
				}
				if (paymentType == PaymentType.barter) {
					AddDocumentIfNotExist(new InvoiceBarterDocument {
						Order = this
					});
				}
			} else {
				RemoveDocumentByType(OrderDocumentType.Invoice);
				RemoveDocumentByType(OrderDocumentType.InvoiceBarter);
				RemoveDocumentByType(OrderDocumentType.UPD);
				RemoveDocumentByType(OrderDocumentType.Torg12);
				RemoveDocumentByType(OrderDocumentType.ShetFactura);
			}

			var equipmentforSaleWithCoolerWarranty = ObservableOrderEquipments
				.Where(orderEquipment =>
					orderEquipment.Reason == Reason.Sale)
				.Where(orderEquipment =>
					orderEquipment.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			if (equipmentforSaleWithCoolerWarranty.Count() > 0 && OrderStatus == OrderStatus.Accepted) {
				AddDocumentIfNotExist(new CoolerWarrantyDocument {
					Order = this
				});
			} else
				RemoveDocumentByType(OrderDocumentType.CoolerWarranty);

			var equipmentforSaleWithPumpWarranty = ObservableOrderEquipments
				.Where(orderEquipment =>
					orderEquipment.Reason == Reason.Sale)
				.Where(orderEquipment =>
					orderEquipment.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			if (equipmentforSaleWithPumpWarranty.Count() > 0 && OrderStatus == OrderStatus.Accepted) {
				AddDocumentIfNotExist(new PumpWarrantyDocument {
					Order = this
				});
			} else
				RemoveDocumentByType(OrderDocumentType.PumpWarranty);
		}

		protected virtual void AddDocumentIfNotExist(OrderDocument document)
		{
			var currentOrderDocuments = ObservableOrderDocuments.Where(doc => doc.Order.Id == Id);
			if (!currentOrderDocuments.Any(doc => doc.Type == document.Type))
				ObservableOrderDocuments.Add(document);
		}

		protected virtual void RemoveDocumentByType(OrderDocumentType type)
		{
			var currentOrderDocuments = ObservableOrderDocuments.Where(doc => doc.Order.Id == Id);
			ObservableOrderDocuments.Remove(
				currentOrderDocuments.FirstOrDefault(doc => doc.Type == type)
			);
		}

		/// <summary>
		/// Закрывает заказ с самовывозом если по всем документам самовывоза со склада все отгружено
		/// </summary>
		public virtual bool TryCloseSelfDeliveryOrder(IUnitOfWork uow, SelfDeliveryDocument closingDocument)
		{
			// Закрывает заказ и создает операцию движения бутылей если все товары в заказе отгружены
			var unloadedItems = Repository.Store.SelfDeliveryRepository.OrderItemUnloaded(uow, this, closingDocument);
			bool canCloseOrder = true;
			foreach(var item in OrderItems) {
				decimal totalCount = default(decimal);
				var deliveryItem = closingDocument.Items.FirstOrDefault(x => x.OrderItem.Id == item.Id);
				if(deliveryItem != null) {
					totalCount += deliveryItem.Amount;
				}
				if(unloadedItems.ContainsKey(item.Id)) {
					totalCount += unloadedItems[item.Id];
				}
				if((int)totalCount != item.Count) {
					canCloseOrder = false;
				}
			}
			if(canCloseOrder) {
				CreateBottlesMovementOperation(uow);
				ChangeStatus(OrderStatus.Closed);
			}
			return canCloseOrder;
		}

		public virtual void CreateBottlesMovementOperation(IUnitOfWork uow)
		{
			foreach(OrderItem item in OrderItems) {
				item.ActualCount = item.Count;
			}

			int amountDelivered = OrderItems
					.Where(item => item.Nomenclature.Category == NomenclatureCategory.water)
					.Sum(item => item.ActualCount);
			
			if(amountDelivered != 0 || (ReturnedTare != 0 && ReturnedTare != null)) {
				if(BottlesMovementOperation == null) {
					var bottlesOperation = new BottlesMovementOperation {
						OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59),
						Order = this,
						Delivered = amountDelivered,
						Returned = ReturnedTare.GetValueOrDefault(),
						Counterparty = Client,
						DeliveryPoint = DeliveryPoint
					};
					uow.Save(bottlesOperation);
					BottlesMovementOperation = bottlesOperation;
				} else {
					BottlesMovementOperation.OperationTime = DeliveryDate.Value.Date.AddHours(23).AddMinutes(59);
					BottlesMovementOperation.Delivered = amountDelivered;
					BottlesMovementOperation.Returned = ReturnedTare.GetValueOrDefault();
					uow.Save(BottlesMovementOperation);
				}
			}
		}

		#endregion

		#region	Внутренние функции

		void ObservableOrderDepositItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(TotalSum));
		}

		void ObservableOrderItems_ListContentChanged(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(TotalSum));
		}

		#endregion

		#region Для расчетов в логистике

		/// <summary>
		/// Время разгрузки в секундах.
		/// </summary>
		public virtual int CalculateTimeOnPoint(bool hasForwarder)
		{
			int byFormula = 3 * 60; //На подпись документво 3 мин.
			int bottels = TotalDeliveredBottles;
			if(!hasForwarder)
				byFormula += CalculateGoDoorCount(bottels, 2) * 100; //100 секун(5/3 минуты) на 2 бутыли без экспедитора.
			else
				byFormula += CalculateGoDoorCount(bottels, 4) * 1 * 60; //1 минута на 4 бутыли c экспедитором.

			if(byFormula < 5 * 60) // минимальное время нахождения на адресе.
				return 5 * 60;
			else
				return byFormula;
		}

		private int CalculateGoDoorCount(int bottles, int atTime)
		{
			return bottles / atTime + (bottles % atTime > 0 ? 1 : 0);
		}

		#endregion
	}
}

