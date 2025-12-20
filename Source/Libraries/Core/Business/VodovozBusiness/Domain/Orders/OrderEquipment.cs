using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Service;

namespace Vodovoz.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки оборудования в заказе",
		Nominative = "строка оборудования в заказе")]
	[HistoryTrace]
	public class OrderEquipment : OrderEquipmentEntity, IValidatableObject
	{
		private Order _order;
		private OrderItem _orderItem;
		private Equipment _equipment;
		private Nomenclature _nomenclature;

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual new Order Order {
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Связанная строка
		/// </summary>
		[Display(Name = "Связанная строка")]
		public virtual new OrderItem OrderItem {
			get => _orderItem;
			set => SetField(ref _orderItem, value);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display(Name = "Оборудование")]
		public virtual new Equipment Equipment {
			get => _equipment;
			set => SetField(ref _equipment, value);
		}

		/// <summary>
		/// Номенклатура оборудования
		/// </summary>
		[Display(Name = "Номенклатура оборудования")]
		public virtual new Nomenclature Nomenclature {
			get => Equipment?.Nomenclature ?? _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get => counterpartyMovementOperation;
			set => SetField(ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation);
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
		public virtual new int Count {
			get => count;
			set {
				if(SetField(ref count, value)) {
					Order?.UpdateRentsCount();
				}
			}
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
			CounterpartyMovementOperation.Nomenclature = _nomenclature;
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
}
