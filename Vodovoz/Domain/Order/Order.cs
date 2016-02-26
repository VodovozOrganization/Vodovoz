using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Repository;
using System.Linq;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Service;
using Vodovoz.Domain.Orders.Documents;
using Gamma.Utilities;

namespace Vodovoz.Domain.Orders
{

	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Masculine,
		NominativePlural = "заказы",
		Nominative = "заказ",
		Prepositional = "заказе",
		PrepositionalPlural = "заказах"
	)]
	public class Order: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		public virtual string Title { 
			get { return String.Format ("Заказ №{0}", Id); }
		}

		OrderStatus orderStatus;

		[Display (Name = "Статус заказа")]
		public virtual OrderStatus OrderStatus {
			get { return orderStatus; }
			set { SetField (ref orderStatus, value, () => OrderStatus); }
		}

		Employee author;

		[Display (Name = "Создатель заказа")]

		public virtual Employee Author {
			get { return author; }
			set { SetField (ref author, value, () => Author); }
		}

		Counterparty client;

		[Display (Name = "Клиент")]
		public virtual Counterparty Client {
			get { return client; }
			set {
				if (value == client)
					return;
				if (client != null && !CanChangeContractor ())
					throw new InvalidOperationException ("Нельзя изменить клиента для заполненного заказа.");
				if (value != null)
					PaymentType = value.PaymentMethod;
				SetField (ref client, value, () => Client); 
				if (DeliveryPoint != null && !Client.DeliveryPoints.Any (d => d.Id == DeliveryPoint.Id)) {
					DeliveryPoint = null;
				}
			}
		}

		DeliveryPoint deliveryPoint;

		[Display (Name = "Точка доставки")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set {
				SetField (ref deliveryPoint, value, () => DeliveryPoint); 
				if (value != null && DeliverySchedule == null) {
					DeliverySchedule = value.DeliverySchedule;
				}
			}
		}

		DateTime deliveryDate;

		[Display (Name = "Дата доставки")]
		public virtual DateTime DeliveryDate {
			get { return deliveryDate; }
			set { 
				SetField (ref deliveryDate, value, () => DeliveryDate); 
				foreach (OrderDocument document in OrderDocuments) {
					if (document.Type == OrderDocumentType.AdditionalAgreement) {
						(document as OrderAgreement).AdditionalAgreement.IssueDate = value;
						(document as OrderAgreement).AdditionalAgreement.StartDate = value;
					}
					//TODO FIXME Когда сделаю добавление документов для печати - фильтровать их сдесь и не менять им дату.
				}
			}
		}

		DeliverySchedule deliverySchedule;

		[Display (Name = "Время доставки")]
		public virtual DeliverySchedule DeliverySchedule {
			get { return deliverySchedule; }
			set { SetField (ref deliverySchedule, value, () => DeliverySchedule); }
		}

		bool selfDelivery;

		[Display (Name = "Самовывоз")]
		public virtual bool SelfDelivery {
			get { return selfDelivery; }
			set { SetField (ref selfDelivery, value, () => SelfDelivery); }
		}

		Order previousOrder;

		[Display (Name = "Предыдущий заказ")]
		public virtual Order PreviousOrder {
			get { return previousOrder; }
			set { SetField (ref previousOrder, value, () => PreviousOrder); }
		}

		int bottlesReturn;

		[Display (Name = "Бутылей на возврат")]
		public virtual int BottlesReturn {
			get { return bottlesReturn; }
			set { SetField (ref bottlesReturn, value, () => BottlesReturn); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

		OrderSignatureType signatureType;

		[Display (Name = "Подписание документов")]
		public virtual OrderSignatureType SignatureType {
			get { return signatureType; }
			set { SetField (ref signatureType, value, () => SignatureType); }
		}

		Decimal sumToReceive;

		[Display (Name = "Сумма к получению")]
		public virtual Decimal SumToReceive {
			get { return sumToReceive; }
			set { SetField (ref sumToReceive, value, () => SumToReceive); }
		}

		string sumDifferenceReason;

		[Display (Name = "Причина переплаты/недоплаты")]
		public virtual string SumDifferenceReason {
			get { return sumDifferenceReason; }
			set { SetField (ref sumDifferenceReason, value, () => SumDifferenceReason); }
		}

		bool shipped;

		[Display (Name = "Отгружено по платежке")]
		public virtual bool Shipped {
			get { return shipped; }
			set { SetField (ref shipped, value, () => Shipped); }
		}

		PaymentType paymentType;

		[Display (Name = "Форма оплаты")]
		public virtual PaymentType PaymentType {
			get { return paymentType; }
			set { 
				if (value == paymentType)
					return;
				if (!CanChangePaymentType ())
					throw new InvalidOperationException ("Нельзя изменить тип оплаты для заполненного заказа.");
				SetField (ref paymentType, value, () => PaymentType);
			}
		}

		MoneyMovementOperation moneyMovementOperation;

		public virtual MoneyMovementOperation MoneyMovementOperation
		{
			get{ return moneyMovementOperation; }
			set
			{
				SetField(ref moneyMovementOperation, value, () => MoneyMovementOperation);
			}
		}

		BottlesMovementOperation bottlesMovementOperation;

		public virtual BottlesMovementOperation BottlesMovementOperation{
			get{
				return bottlesMovementOperation;
			}
			set{
				SetField(ref bottlesMovementOperation, value, () => BottlesMovementOperation);
			}
		}

		bool collectBottles;

		public virtual bool CollectBottles{
			get{
				return collectBottles;
			}
			set{
				SetField(ref collectBottles, value, () => CollectBottles);
			}
		}

		public bool CanChangePaymentType ()
		{
			if ((NHibernate.NHibernateUtil.IsInitialized (OrderItems) && OrderItems.Count > 0) ||
			    (NHibernate.NHibernateUtil.IsInitialized (OrderDepositItems) && OrderDepositItems.Count > 0) ||
			    (NHibernate.NHibernateUtil.IsInitialized (FinalOrderService) && FinalOrderService.Count > 0))
				return false;
			return true;
		}

		public bool CanChangeContractor ()
		{
			if ((NHibernate.NHibernateUtil.IsInitialized (OrderDocuments) && OrderDocuments.Count > 0) ||
			    (NHibernate.NHibernateUtil.IsInitialized (InitialOrderService) && InitialOrderService.Count > 0) ||
			    (NHibernate.NHibernateUtil.IsInitialized (FinalOrderService) && FinalOrderService.Count > 0))
				return false;
			return true;
		}

		IList<OrderDepositItem> orderDepositItems = new List<OrderDepositItem> ();

		[Display (Name = "Залоги заказа")]
		public virtual IList<OrderDepositItem> OrderDepositItems {
			get { return orderDepositItems; }
			set { SetField (ref orderDepositItems, value, () => OrderDepositItems); }
		}

		GenericObservableList<OrderDepositItem> observableOrderDepositItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<OrderDepositItem> ObservableOrderDepositItems {
			get {
				if (observableOrderDepositItems == null)
					observableOrderDepositItems = new GenericObservableList<OrderDepositItem> (OrderDepositItems);
				return observableOrderDepositItems;
			}
		}

		IList<OrderDocument> orderDocuments = new List<OrderDocument> ();

		[Display (Name = "Документы заказа")]
		public virtual IList<OrderDocument> OrderDocuments {
			get { return orderDocuments; }
			set { SetField (ref orderDocuments, value, () => OrderDocuments); }
		}

		GenericObservableList<OrderDocument> observableOrderDocuments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<OrderDocument> ObservableOrderDocuments {
			get {
				if (observableOrderDocuments == null)
					observableOrderDocuments = new GenericObservableList<OrderDocument> (OrderDocuments);
				return observableOrderDocuments;
			}
		}

		IList<OrderItem> orderItems = new List<OrderItem> ();

		[Display (Name = "Строки заказа")]
		public virtual IList<OrderItem> OrderItems {
			get { return orderItems; }
			set { SetField (ref orderItems, value, () => OrderItems); }
		}

		GenericObservableList<OrderItem> observableOrderItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<OrderItem> ObservableOrderItems {
			get {
				if (observableOrderItems == null)
					observableOrderItems = new GenericObservableList<OrderItem> (orderItems);
				return observableOrderItems;
			}
		}

		IList<OrderEquipment> orderEquipments = new List<OrderEquipment> ();

		[Display (Name = "Список оборудования")]
		public virtual IList<OrderEquipment> OrderEquipments {
			get { return orderEquipments; }
			set { SetField (ref orderEquipments, value, () => OrderEquipments); }
		}

		GenericObservableList<OrderEquipment> observableOrderEquipments;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<OrderEquipment> ObservableOrderEquipments {
			get {
				if (observableOrderEquipments == null)
					observableOrderEquipments = new GenericObservableList<OrderEquipment> (orderEquipments);
				return observableOrderEquipments;
			}
		}

		IList<ServiceClaim> initialOrderService = new List<ServiceClaim> ();

		[Display (Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> InitialOrderService {
			get { return initialOrderService; }
			set { SetField (ref initialOrderService, value, () => InitialOrderService); }
		}

		GenericObservableList<ServiceClaim> observableInitialOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<ServiceClaim> ObservableInitialOrderService {
			get {
				if (observableInitialOrderService == null)
					observableInitialOrderService = new GenericObservableList<ServiceClaim> (InitialOrderService);
				return observableInitialOrderService;
			}
		}

		IList<ServiceClaim> finalOrderService = new List<ServiceClaim> ();

		[Display (Name = "Список заявок на сервис")]
		public virtual IList<ServiceClaim> FinalOrderService {
			get { return finalOrderService; }
			set { SetField (ref finalOrderService, value, () => FinalOrderService); }
		}

		GenericObservableList<ServiceClaim> observableFinalOrderService;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public GenericObservableList<ServiceClaim> ObservableFinalOrderService {
			get {
				if (observableFinalOrderService == null)
					observableFinalOrderService = new GenericObservableList<ServiceClaim> (FinalOrderService);
				return observableFinalOrderService;
			}
		}
			
		public Order ()
		{
			Comment = String.Empty;
			OrderStatus = OrderStatus.NewOrder;
			DeliveryDate = DateTime.Today;
			SumDifferenceReason = String.Empty;
			DeliveryDate = DateTime.Now.AddDays (1);
		}

		public static Order CreateFromServiceClaim(ServiceClaim service, Employee author){
			var order = new Order();
			order.client = service.Counterparty;
			order.DeliveryPoint = service.DeliveryPoint;
			order.DeliveryDate = service.ServiceStartDate;
			order.PaymentType = service.Payment;
			order.Author = author;
			order.AddServiceClaimAsInitial(service);
			service.InitialOrder = order;
			return order;
		}

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (validationContext.Items.ContainsKey ("NewStatus")) {
				OrderStatus newStatus = (OrderStatus)validationContext.Items ["NewStatus"];
				if (newStatus == OrderStatus.Accepted) {
					if (DeliveryDate == default(DateTime))
						yield return new ValidationResult ("Не указана дата доставки.",
							new[] { this.GetPropertyName (o => o.DeliveryDate) });
					if (!SelfDelivery && DeliverySchedule == null)
						yield return new ValidationResult ("Не указано время доставки.",
							new[] { this.GetPropertyName (o => o.DeliverySchedule) });
				}
			}

			if (!SelfDelivery && DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName (o => o.DeliveryPoint) });
			if (Client == null)
				yield return new ValidationResult ("Необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName (o => o.Client) });
			if (ObservableOrderItems.Any (item => item.Count < 1))
				yield return new ValidationResult ("В заказе присутствуют позиции с нулевым количеством.", 
					new[] { this.GetPropertyName (o => o.OrderItems) });
		}

		#endregion

		public virtual string RowColor { get { return PreviousOrder == null ? "black" : "red"; } }

		public virtual decimal TotalSum {
			get {
				Decimal sum = 0;
				foreach (OrderItem item in ObservableOrderItems) {
					sum += item.Price * item.Count;
				}
				foreach (OrderDepositItem dep in ObservableOrderDepositItems) {
					if (dep.PaymentDirection == PaymentDirection.ToClient)
						sum -= dep.Deposit * dep.Count;
				}
				return sum;
			}
		}

		public virtual decimal ActualTotalSum{
			get {
				Decimal sum = 0;
				foreach (OrderItem item in ObservableOrderItems) {
					sum += item.Price * item.ActualCount;
				}
				foreach (OrderDepositItem dep in ObservableOrderDepositItems) {
					if (dep.PaymentDirection == PaymentDirection.ToClient)
						sum -= dep.Deposit * dep.Count;
				}
				return sum;
			}
		}

		public virtual decimal ActualGoodsTotalSum
		{
			get{
				return OrderItems.Sum(item => item.Price * item.ActualCount);
			}
		}

		public void AddEquipmentNomenclatureForSale (Nomenclature nomenclature, IUnitOfWork UoW)
		{
			if (nomenclature.Category != NomenclatureCategory.equipment)
				return;
			if (!nomenclature.Serial) {
				ObservableOrderItems.Add (new OrderItem {
					Order = this,
					AdditionalAgreement = null,
					Count = 0,
					Equipment = null,
					Nomenclature = nomenclature,
					Price = nomenclature.GetPrice (1)
				});
			} else {
				Equipment eq = EquipmentRepository.GetEquipmentForSaleByNomenclature (UoW, nomenclature);
				int ItemId;
				ItemId = ObservableOrderItems.AddWithReturn (new OrderItem {
					Order = this,
					AdditionalAgreement = null,
					Count = 1,
					Equipment = eq,
					Nomenclature = nomenclature,
					Price = nomenclature.GetPrice (1)
				});
			}
			UpdateDocuments ();
		}

		public void AddAdditionalNomenclatureForSale (Nomenclature nomenclature)
		{
			if (nomenclature.Category != NomenclatureCategory.additional)
				return;
			ObservableOrderItems.Add (new OrderItem {
				Order = this,
				AdditionalAgreement = null,
				Count = 0,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice (1)
			});
			UpdateDocuments ();
		}

		public void AddDisposableBottleWater(Nomenclature nomenclature){
			if (nomenclature.Category != NomenclatureCategory.disposableBottleWater)
				return;
			ObservableOrderItems.Add (new OrderItem {
				Order = this,
				AdditionalAgreement = null,
				Count = 0,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.GetPrice (1)
			});
			UpdateDocuments ();
		}

		public void AddWaterForSale (Nomenclature nomenclature, WaterSalesAgreement wsa)
		{
			if (nomenclature.Category != NomenclatureCategory.water)
				return;
			if (ObservableOrderItems.Any (item => item.Nomenclature.Id == nomenclature.Id &&
			    item.AdditionalAgreement.Id == wsa.Id))
				return;
			ObservableOrderItems.Add (new OrderItem {
				Order = this,
				AdditionalAgreement = wsa,
				Count = 0,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = wsa.IsFixedPrice ? wsa.FixedPrice : nomenclature.GetPrice (1)
			});
			UpdateDocuments ();
		}

		public void RecalcBottlesDeposits (IUnitOfWork uow)
		{
			if (Client.PersonType == PersonType.legal)
				return;
			var waterItemsCount = ObservableOrderItems.Select (item => item)
				.Where (item => item.Nomenclature.Category == NomenclatureCategory.water)
				.Sum (item => item.Count);
			
			var depositPaymentItem = ObservableOrderItems.FirstOrDefault (item => item.Nomenclature.Id == NomenclatureRepository.GetBottleDeposit (uow).Id);
			var depositRefundItem = ObservableOrderDepositItems.FirstOrDefault (item => item.DepositType == DepositType.Bottles);

			//Надо создать услугу залога
			if (BottlesReturn < waterItemsCount) {
				if (depositRefundItem != null) {
					depositRefundItem.Count = waterItemsCount - BottlesReturn;
					depositRefundItem.PaymentDirection = PaymentDirection.FromClient;
				}
				if (depositPaymentItem != null)
					depositPaymentItem.Count = waterItemsCount - BottlesReturn;
				else {
					ObservableOrderItems.Add (new OrderItem {
						Order = this,
						AdditionalAgreement = null,
						Count = waterItemsCount - BottlesReturn,
						Equipment = null,
						Nomenclature = NomenclatureRepository.GetBottleDeposit (uow),
						Price = NomenclatureRepository.GetBottleDeposit (uow).GetPrice (waterItemsCount - BottlesReturn)
					});
					ObservableOrderDepositItems.Add (new OrderDepositItem {
						Order = this,
						Count = waterItemsCount - BottlesReturn,
						Deposit = NomenclatureRepository.GetBottleDeposit (uow).GetPrice (waterItemsCount - BottlesReturn),
						DepositOperation = null,
						DepositType = DepositType.Bottles,
						FreeRentItem = null,
						PaidRentItem = null,
						PaymentDirection = PaymentDirection.FromClient
					});
				}
				return;
			}
			if (BottlesReturn == waterItemsCount) {
				if (depositRefundItem != null)
					ObservableOrderDepositItems.Remove (depositRefundItem);
				if (depositPaymentItem != null)
					ObservableOrderItems.Remove (depositPaymentItem);
				return;
			}
			if (BottlesReturn > waterItemsCount) {
				if (depositPaymentItem != null)
					ObservableOrderItems.Remove (depositPaymentItem);
				if (depositRefundItem != null) {
					depositRefundItem.Deposit = NomenclatureRepository.GetBottleDeposit (uow).GetPrice (BottlesReturn - waterItemsCount);
					depositRefundItem.Count = BottlesReturn - waterItemsCount;
				} else
					ObservableOrderDepositItems.Add (new OrderDepositItem {
						Order = this,
						DepositOperation = null,
						DepositType = DepositType.Bottles,
						Deposit = NomenclatureRepository.GetBottleDeposit (uow).GetPrice (BottlesReturn - waterItemsCount),
						PaidRentItem = null,
						FreeRentItem = null,
						PaymentDirection = PaymentDirection.ToClient,
						Count = BottlesReturn - waterItemsCount
					});
				return;
			}
		}

		public void FillItemsFromAgreement (AdditionalAgreement a)
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
					if ((orderItem = ObservableOrderItems.FirstOrDefault<OrderItem> (
						    item => item.AdditionalAgreement.Id == a.Id &&
						    item.Nomenclature.Id == equipment.PaidRentPackage.DepositService.Id &&
						    item.Price == equipment.Deposit)) != null) {
						orderItem.Count++;
						orderItem.Price = equipment.Deposit;
					} else {
						ObservableOrderItems.Add (
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
					if ((orderItem = ObservableOrderItems.FirstOrDefault<OrderItem> (
						    item => item.AdditionalAgreement.Id == a.Id &&
						    item.Nomenclature.Id == (IsDaily ? equipment.PaidRentPackage.RentServiceDaily.Id : equipment.PaidRentPackage.RentServiceMonthly.Id) &&
						    item.Price == equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1))) != null) {
						orderItem.Count++;
						orderItem.Price = orderItem.Nomenclature.GetPrice (orderItem.Count);
						ItemId = ObservableOrderItems.IndexOf (orderItem);
					} else {
						ItemId = ObservableOrderItems.AddWithReturn (
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
					ObservableOrderEquipments.Add (
						new OrderEquipment { 
							Order = this,
							Direction = Vodovoz.Domain.Orders.Direction.Deliver,
							Equipment = equipment.Equipment,
							Reason = Reason.Rent,
							OrderItem = ObservableOrderItems [ItemId]
						}
					);
					SumToReceive += equipment.Deposit + equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1);
				}
			} else if (a.Type == AgreementType.FreeRent) {
				FreeRentAgreement agreement = a as FreeRentAgreement;
				foreach (FreeRentEquipment equipment in agreement.Equipment) {
					int ItemId;
					//Добавляем номенклатуру залога.
					ItemId = ObservableOrderItems.AddWithReturn (
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
					ObservableOrderEquipments.Add (
						new OrderEquipment { 
							Order = this,
							Direction = Direction.Deliver,
							Equipment = equipment.Equipment,
							Reason = Reason.Rent,
							OrderItem = ObservableOrderItems [ItemId]
						}
					);
				}
			}
			UpdateDocuments ();
		}

		public void RemoveItem (OrderItem item)
		{
			ObservableOrderItems.Remove (item);
			foreach (var equip in ObservableOrderEquipments.Where (e => e.OrderItem == item).ToList ()) {
				ObservableOrderEquipments.Remove (equip);
			}
			UpdateDocuments ();
		}

		public void AddServiceClaimAsInitial (ServiceClaim service)
		{
			if (service.InitialOrder != null && service.InitialOrder.Id == Id) {
				if (service.Equipment==null || ObservableOrderEquipments.FirstOrDefault (eq => eq.Equipment.Id == service.Equipment.Id) == null) {
					ObservableOrderEquipments.Add (new OrderEquipment { 
						Order = this,
						Direction = Direction.PickUp,
						Equipment = service.Equipment,
						NewEquipmentNomenclature = service.Equipment == null ? service.Nomenclature : null,
						OrderItem = null,
						Reason = Reason.Service
					});
				}
				if (service.ReplacementEquipment != null) {
					observableOrderEquipments.Add (new OrderEquipment {
						Order=this,
						Direction = Direction.Deliver,
						Equipment=service.ReplacementEquipment,
						NewEquipmentNomenclature = null,
						OrderItem=null,
						Reason = Reason.Service
					});				
				}
				if (ObservableInitialOrderService.FirstOrDefault (sc => sc.Id == service.Id) == null)
					ObservableInitialOrderService.Add (service);
				if (ObservableOrderDocuments.Where (doc => doc.Type == OrderDocumentType.EquipmentTransfer).Cast<EquipmentTransferDocument> ()
					.FirstOrDefault (doc => doc.ServiceClaim.Id == service.Id) == null) {
					ObservableOrderDocuments.Add (new EquipmentTransferDocument {
						Order = this,
						ServiceClaim = service
					});
				}
			}
		}

		public void AddServiceClaimAsFinal (ServiceClaim service)
		{
			if (service.FinalOrder != null && service.FinalOrder.Id == Id) {
				if (ObservableOrderEquipments.FirstOrDefault (eq => eq.Equipment.Id == service.Equipment.Id) == null) {
					ObservableOrderEquipments.Add (new OrderEquipment { 
						Order = this,
						Direction = Direction.Deliver,
						Equipment = service.Equipment,
						OrderItem = null,
						Reason = Reason.Service
					});
				}
				if (ObservableOrderDocuments.Where (doc => doc.Type == OrderDocumentType.DoneWorkReport).Cast<DoneWorkDocument> ()
					.FirstOrDefault (doc => doc.ServiceClaim.Id == service.Id) == null) {
					ObservableOrderDocuments.Add (new DoneWorkDocument {
						Order = this,
						ServiceClaim = service
					});
				}
			}
			//TODO FIXME Добавить строку сервиса OrderItems
			//И вообще много чего тут сделать.
		}

		public void FillNewEquipment(Equipment registeredEquipment)
		{
			var newEquipment = ObservableOrderEquipments
				.Where(orderEq=>orderEq.NewEquipmentNomenclature!=null)
				.FirstOrDefault(orderEq => orderEq.NewEquipmentNomenclature.Id == registeredEquipment.Nomenclature.Id);
			if (newEquipment != null)
			{
				newEquipment.Equipment = registeredEquipment;
				newEquipment.NewEquipmentNomenclature = null;
			}
		}

		public void ChangeStatus(OrderStatus newStatus)
		{
			OrderStatus = newStatus;
		}
			
		public void UpdateDocuments()
		{
			if (ObservableOrderItems.Count > 0 && PaymentType == PaymentType.cashless)
			{
				AddDocumentIfNotExist(new BillDocument
					{
						Order = this
					});
			}
			else
				RemoveDocumentByType(OrderDocumentType.Bill);
			
			if (ObservableOrderItems.Count > 0 && OrderStatus==OrderStatus.Accepted)
			{
				if (paymentType == PaymentType.cashless)
				{
		
					AddDocumentIfNotExist(new UPDDocument
						{
							Order = this
						});					
				}
				if (paymentType == PaymentType.cash)
				{
					AddDocumentIfNotExist(new InvoiceDocument
						{
							Order = this
						});					
				}
				if (paymentType == PaymentType.barter)
				{
					AddDocumentIfNotExist(new InvoiceBarterDocument
						{
							Order = this
						});
				}
			}
			else
			{
				RemoveDocumentByType(OrderDocumentType.Invoice);
				RemoveDocumentByType(OrderDocumentType.InvoiceBarter);
				RemoveDocumentByType(OrderDocumentType.UPD);
			}

			var equipmentforSaleWithCoolerWarranty = ObservableOrderEquipments
				.Where(orderEquipment => 
					orderEquipment.Reason == Reason.Sale)
				.Where(orderEquipment => 
					orderEquipment.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.CoolerWarranty);
			if (equipmentforSaleWithCoolerWarranty.Count()>0 && OrderStatus==OrderStatus.Accepted)
			{				
				AddDocumentIfNotExist(new CoolerWarrantyDocument
					{
						Order = this
					});				
			}
			else
				RemoveDocumentByType(OrderDocumentType.CoolerWarranty);
				
			var equipmentforSaleWithPumpWarranty = ObservableOrderEquipments
				.Where(orderEquipment => 
					orderEquipment.Reason == Reason.Sale)
				.Where(orderEquipment => 
					orderEquipment.Equipment.Nomenclature.Type.WarrantyCardType == WarrantyCardType.PumpWarranty);
			if (equipmentforSaleWithPumpWarranty.Count() > 0 && OrderStatus==OrderStatus.Accepted)
			{
				AddDocumentIfNotExist(new PumpWarrantyDocument
					{
						Order = this
					});
			}
			else
				RemoveDocumentByType(OrderDocumentType.PumpWarranty);
		}

		protected void AddDocumentIfNotExist(OrderDocument document)
		{
			var currentOrderDocuments = ObservableOrderDocuments.Where(doc => doc.Order.Id == Id);
			if (!currentOrderDocuments.Any(doc => doc.Type == document.Type))
				ObservableOrderDocuments.Add(document);
		}

		protected void RemoveDocumentByType(OrderDocumentType type)
		{
			var currentOrderDocuments = ObservableOrderDocuments.Where(doc => doc.Order.Id == Id);
			ObservableOrderDocuments.Remove(
				currentOrderDocuments.FirstOrDefault(doc => doc.Type == type)
			);
		}

		public void Close()
		{
			//FIXME Правильно закрывать заказ
			OrderStatus = OrderStatus.Closed;
		}
	}
}

