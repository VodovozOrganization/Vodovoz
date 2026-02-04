using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Service;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки оборудования в заказе",
		Nominative = "строка оборудования в заказе")]
	[HistoryTrace]
	public class OrderEquipment : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		Direction direction;

		[Display(Name = "Направление")]
		public virtual Direction Direction {
			get => direction;
			set => SetField(ref direction, value, () => Direction);
		}

		DirectionReason directionReason;

		[Display(Name = "Причина забор-доставки")]
		public virtual DirectionReason DirectionReason {
			get => directionReason;
			set => SetField(ref directionReason, value, () => DirectionReason);
		}

		OrderItem orderItem;

		[Display(Name = "Связанная строка")]
		public virtual OrderItem OrderItem {
			get => orderItem;
			set => SetField(ref orderItem, value, () => OrderItem);
		}

		Equipment equipment;

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment {
			get => equipment;
			set => SetField(ref equipment, value, () => Equipment);
		}

		OwnTypes ownType;

		[Display(Name = "Принадлежность")]
		public virtual OwnTypes OwnType {
			get => ownType;
			set => SetField(ref ownType, value, () => OwnType);
		}

		Nomenclature nomenclature;

		[Display(Name = "Номенклатура оборудования")]
		public virtual Nomenclature Nomenclature {
			get => Equipment?.Nomenclature ?? nomenclature;
			set => SetField(ref nomenclature, value, () => Nomenclature);
		}

		Reason reason = Reason.Unknown;

		[Display(Name = "Причина")]
		public virtual Reason Reason {
			get => reason;
			set => SetField(ref reason, value, () => Reason);
		}

		bool confirmed;
		public virtual bool Confirmed {
			get => confirmed;
			set => SetField(ref confirmed, value, () => Confirmed);
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get => counterpartyMovementOperation;
			set => SetField(ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation);
		}

		string confirmedComment;
		[Display(Name = "Комментарий по забору")]
		[StringLength(200)]
		public virtual string ConfirmedComment {
			get => confirmedComment;
			set => SetField(ref confirmedComment, value, () => ConfirmedComment);
		}

		public virtual string NameString {
			get {
				if(Equipment != null)
					return Equipment.Title;
				if(Nomenclature != null)
					return Nomenclature.ShortOrFullName;
				return "Неизвестное оборудование";
			}
		}

		public virtual string FullNameString {
			get {
				if(Equipment != null)
					return Equipment.Title;
				if(Nomenclature != null)
					return Nomenclature.OfficialName;
				return "Неизвестное оборудование";
			}
		}

		private ServiceClaim serviceClaim;
		[Display(Name = "Номер заявки на обслуживание")]
		public virtual ServiceClaim ServiceClaim {
			get => serviceClaim;
			set => SetField(ref serviceClaim, value, () => ServiceClaim);
		}
		
		int count;
		/// <summary>
		/// Количество оборудования, которое изначально должен был привезти/забрать водитель
		/// </summary>
		[Display(Name = "Количество")]
		public virtual int Count {
			get => count;
			set {
				if(SetField(ref count, value)) {
					Order?.UpdateRentsCount();
				}
			}
		}

		int? actualCount;
		/// <summary>
		/// Количество оборудования, которое фактически привез/забрал водитель
		/// </summary>
		public virtual int? ActualCount {
			get => actualCount;
			set => SetField(ref actualCount, value);
		}

		OrderItem orderRentDepositItem;

		[Display(Name = "Связанный залог за аренду")]
		public virtual OrderItem OrderRentDepositItem {
			get => orderRentDepositItem;
			set => SetField(ref orderRentDepositItem, value);
		}
		
		OrderItem orderRentServiceItem;

		[Display(Name = "Связанная услуга аренды")]
		public virtual OrderItem OrderRentServiceItem {
			get => orderRentServiceItem;
			set => SetField(ref orderRentServiceItem, value);
		}

		#region Вычисляемые

		public virtual int CurrentCount => ActualCount ?? Count;

		public virtual int UndeliveredCount => Count - ActualCount ?? 0;
		public virtual bool IsFullyDelivered => UndeliveredCount == 0;

		public virtual string DirectionString => Direction.GetEnumTitle();
		public virtual string DirectionReasonString => DirectionReason.GetEnumTitle();
		public virtual string ReasonString => Reason.GetEnumTitle();

		public virtual string Title => $"{NameString} {DirectionString}";

		#endregion

		#region Функции

		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation()
		{
			if(!ActualCount.HasValue || ActualCount.Value == 0) {
				CounterpartyMovementOperation = null;
				return null;
			}

			if(CounterpartyMovementOperation == null)
				CounterpartyMovementOperation = new CounterpartyMovementOperation();

			CounterpartyMovementOperation.OperationTime = Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59);
			CounterpartyMovementOperation.Amount = ActualCount.Value;
			CounterpartyMovementOperation.Nomenclature = nomenclature;
			CounterpartyMovementOperation.ForRent = Reason != Reason.Sale;
			if(Direction == Direction.Deliver) {
				CounterpartyMovementOperation.IncomingCounterparty = Order.Client;
				CounterpartyMovementOperation.IncomingDeliveryPoint = Order.DeliveryPoint;
				CounterpartyMovementOperation.WriteoffCounterparty = null;
				CounterpartyMovementOperation.WriteoffDeliveryPoint = null;
			} else {
				CounterpartyMovementOperation.WriteoffCounterparty = Order.Client;
				CounterpartyMovementOperation.WriteoffDeliveryPoint = Order.DeliveryPoint;
				CounterpartyMovementOperation.IncomingCounterparty = null;
				CounterpartyMovementOperation.IncomingDeliveryPoint = null;
			}

			return CounterpartyMovementOperation;
		}

		#endregion

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			return null;
		}

		#endregion

		public static OrderEquipment Clone(OrderEquipment orderEquipment)
		{
			return new OrderEquipment
			{
				Order = orderEquipment.Order,
				Direction = orderEquipment.Direction,
				Equipment = orderEquipment.Equipment,
				OrderItem = orderEquipment.OrderItem,
				OrderRentDepositItem = orderEquipment.OrderRentDepositItem,
				OrderRentServiceItem = orderEquipment.OrderRentServiceItem,
				OwnType = orderEquipment.OwnType,
				DirectionReason = orderEquipment.DirectionReason,
				Reason = orderEquipment.Reason,
				Confirmed = orderEquipment.Confirmed,
				Nomenclature = orderEquipment.Nomenclature,
				ActualCount = orderEquipment.ActualCount,
				Count = orderEquipment.Count
			};
		}
	}

	public enum Direction
	{
		[Display(Name = "Доставить")] Deliver,
		[Display(Name = "Забрать")] PickUp
	}

	public enum DirectionReason
	{
		[Display(Name = "")]
		None,
		[Display(Name = "Аренда")]
		Rent,
		[Display(Name = "Ремонт")]
		Repair,
		[Display(Name = "Санобработка")]
		Cleaning,
		[Display(Name = "Ремонт и санобработка")]
		RepairAndCleaning,
		[Display(Name = "Акция \"Трейд-Ин\"")]
		TradeIn,
		[Display(Name = "Подарок клиента")]
		ClientGift,
	}

	public enum Reason
	{
		[Display(Name = "Неизвестна")] Unknown,
		[Display(Name = "Сервис")] Service,
		[Display(Name = "Аренда")] Rent,
		[Display(Name = "Расторжение")] Cancellation,
		[Display(Name = "Продажа")] Sale
	}

	public enum OwnTypes
	{
		[Display(Name = "")] None,
		[Display(Name = "Клиент")] Client,
		[Display(Name = "Дежурный")] Duty,
		[Display(Name = "Аренда")] Rent
	}
}
