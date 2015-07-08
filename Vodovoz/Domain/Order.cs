using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Repository;
using Gtk;
using QSTDI;
using System.Linq;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Заказы", ObjectName = "заказ")]
	public class Order: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		OrderStatus orderStatus;

		[Display (Name = "Статус заказа")]
		public virtual OrderStatus OrderStatus {
			get { return orderStatus; }
			set { SetField (ref orderStatus, value, () => OrderStatus); }
		}

		Counterparty client;

		[Display (Name = "Клиент")]
		public virtual Counterparty Client {
			get { return client; }
			set {
				SetField (ref client, value, () => Client); 
				if (DeliveryPoint != null && Client.DeliveryPoints.Any (d => d.Id == DeliveryPoint.Id)) {
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

		Payment paymentType;

		[Display (Name = "Форма оплаты")]
		public virtual Payment PaymentType {
			get { return paymentType; }
			set { SetField (ref paymentType, value, () => PaymentType); }
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

		RouteList routeList;

		[Display (Name = "Маршрутный лист")]
		public virtual RouteList RouteList {
			get { return routeList; }
			set { SetField (ref routeList, value, () => RouteList); }
		}

		//TODO: Договор. Какой договор имеется в виду?

		//TODO: Печатаемые документы

		//TODO: Сервисное обслуживание.

		#region IValidatableObject implementation

		public IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (DeliveryPoint == null)
				yield return new ValidationResult ("Необходимо заполнить точку доставки.",
					new[] { this.GetPropertyName (o => o.DeliveryPoint) });
			if (Client == null)
				yield return new ValidationResult ("Необходимо заполнить поле \"клиент\".",
					new[] { this.GetPropertyName (o => o.Client) });
		}

		#endregion

		public virtual string StatusString { get { return OrderStatus.GetEnumTitle (); } }

		public virtual string ClientString { get { return Client.Name; } }

		public virtual decimal TotalSum {
			get {
				Decimal sum = 0;
				foreach (OrderItem item in ObservableOrderItems) {
					sum += item.Price;
				}
				return sum;}
		}

		public Order ()
		{
			Comment = String.Empty;
			OrderStatus = OrderStatus.NewOrder;
			DeliveryDate = DateTime.Now;
			SumDifferenceReason = String.Empty;
			DeliveryDate = DateTime.Now.AddDays (1);
		}

		public void AddEquipmentNomenclatureForSale (Nomenclature nomenclature, IUnitOfWork UoW)
		{
			if (nomenclature.Category != NomenclatureCategory.equipment)
				return;
			Equipment eq = EquipmentRepository.GetEquipmentForSaleByNomenclature (UoW, nomenclature);
			int ItemId;
			ItemId = ObservableOrderItems.AddWithReturn (new OrderItem {
				AdditionalAgreement = null,
				Count = 1,
				Equipment = eq,
				Nomenclature = nomenclature,
				Price = nomenclature.NomenclaturePrice [0].Price //FIXME
			});
			ObservableOrderEquipments.Add (new OrderEquipment {
				Direction = Vodovoz.Domain.Direction.Deliver,
				Equipment = eq,
				OrderItem = ObservableOrderItems [ItemId],
				Reason = Reason.Rent	//TODO FIXME Добавить причину - продажа.
			});
		}

		public void AddAdditionalNomenclatureForSale (Nomenclature nomenclature)
		{
			if (nomenclature.Category != NomenclatureCategory.additional)
				return;
			ObservableOrderItems.Add (new OrderItem {
				AdditionalAgreement = null,
				Count = 1,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = nomenclature.NomenclaturePrice [0].Price //FIXME
			});
		}

		public void AddWaterForSale (Nomenclature nomenclature, WaterSalesAgreement wsa)
		{
			if (nomenclature.Category != NomenclatureCategory.water)
				return;
			ObservableOrderItems.Add (new OrderItem {
				AdditionalAgreement = wsa,
				Count = 1,
				Equipment = null,
				Nomenclature = nomenclature,
				Price = wsa.IsFixedPrice ? wsa.FixedPrice : nomenclature.NomenclaturePrice [0].Price //FIXME
			});
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
					ObservableOrderItems.Add (
						new OrderItem {
							AdditionalAgreement = a,
							Count = 1,
							Equipment = null,
							Nomenclature = equipment.PaidRentPackage.DepositService,
							Price = equipment.Deposit
						}
					);
					//Добавляем услугу аренды
					ItemId = ObservableOrderItems.AddWithReturn (
						new OrderItem {
							AdditionalAgreement = a,
							Count = 1,
							Equipment = null,
							Nomenclature = IsDaily ? equipment.PaidRentPackage.RentServiceDaily : equipment.PaidRentPackage.RentServiceMonthly,
							Price = equipment.Price * (IsDaily ? (a as DailyRentAgreement).RentDays : 1)
						}
					);
					//Добавляем оборудование
					ObservableOrderEquipments.Add (
						new OrderEquipment { 
							Direction = Vodovoz.Domain.Direction.Deliver,
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
							Direction = Vodovoz.Domain.Direction.Deliver,
							Equipment = equipment.Equipment,
							Reason = Reason.Rent,
							OrderItem = ObservableOrderItems [ItemId]
						}
					);
				}
			}
		}
	}

	public enum OrderStatus
	{
		[ItemTitleAttribute ("Новый")] NewOrder,
		[ItemTitleAttribute ("Принят")] Accepted,
		[ItemTitleAttribute ("В маршрутном листе")]InTravelList,
		[ItemTitleAttribute ("На погрузке")]OnLoading,
		[ItemTitleAttribute ("В пути")]OnTheWay,
		[ItemTitleAttribute ("Доставлен")]Shipped,
		[ItemTitleAttribute ("Выгрузка на складе")]UnloadingOnStock,
		[ItemTitleAttribute ("Отчет не закрыт")]ReportNotClosed,
		[ItemTitleAttribute ("Закрыт")]Closed,
		[ItemTitleAttribute ("Отменен")]Canceled,
		[ItemTitleAttribute ("Недовоз")]NotDelivered
	}

	public class OrderStatusStringType : NHibernate.Type.EnumStringType
	{
		public OrderStatusStringType () : base (typeof(OrderStatus))
		{
		}
	}

	public enum OrderSignatureType
	{
		[ItemTitleAttribute ("По печати")]BySeal,
		[ItemTitleAttribute ("По доверенности")]ByProxy
	}

	public class OrderSignatureTypeStringType : NHibernate.Type.EnumStringType
	{
		public OrderSignatureTypeStringType () : base (typeof(OrderSignatureType))
		{
		}
	}
}

