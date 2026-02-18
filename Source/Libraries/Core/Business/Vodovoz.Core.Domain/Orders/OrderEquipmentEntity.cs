using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки оборудования в заказе",
		Nominative = "строка оборудования в заказе")]
	[HistoryTrace]
	public class OrderEquipmentEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private Direction _direction;
		private DirectionReason _directionReason;
		private OwnTypes _ownType;
		private Reason _reason = Reason.Unknown;
		private bool _confirmed;
		private string _confirmedComment;
		private int _count;
		private int? _actualCount;
		private OrderEntity _order;
		private OrderItemEntity _orderItem;
		private EquipmentEntity _equipment;
		private NomenclatureEntity _nomenclature;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Направление
		/// </summary>
		[Display(Name = "Направление")]
		public virtual Direction Direction
		{
			get => _direction;
			set => SetField(ref _direction, value);
		}

		/// <summary>
		/// Причина забор-доставки
		/// </summary>
		[Display(Name = "Причина забор-доставки")]
		public virtual DirectionReason DirectionReason
		{
			get => _directionReason;
			set => SetField(ref _directionReason, value);
		}

		/// <summary>
		/// Принадлежность
		/// </summary>
		[Display(Name = "Принадлежность")]
		public virtual OwnTypes OwnType
		{
			get => _ownType;
			set => SetField(ref _ownType, value);
		}

		/// <summary>
		/// Причина
		/// </summary>
		[Display(Name = "Причина")]
		public virtual Reason Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}

		/// <summary>
		/// Подтвержден
		/// </summary>
		[Display(Name = "Подтвержден")]
		public virtual bool Confirmed
		{
			get => _confirmed;
			set => SetField(ref _confirmed, value);
		}

		/// <summary>
		/// Комментарий по забору
		/// </summary>
		[Display(Name = "Комментарий по забору")]
		[StringLength(200)]
		public virtual string ConfirmedComment
		{
			get => _confirmedComment;
			set => SetField(ref _confirmedComment, value);
		}

		/// <summary>
		/// Количество оборудования, которое изначально должен был привезти/забрать водитель
		/// </summary>
		[Display(Name = "Количество")]
		public virtual int Count
		{
			get => _count;
			//Нельзя устанавливать, см. логику в OrderEquipment.cs
			protected set => SetField(ref _count, value);
		}

		/// <summary>
		/// Количество оборудования, которое фактически привез/забрал водитель
		/// </summary>
		public virtual int? ActualCount
		{
			get => _actualCount;
			set => SetField(ref _actualCount, value);
		}

		/// <summary>
		/// Заказ
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual OrderEntity Order
		{
			get => _order;
			set => SetField(ref _order, value, () => Order);
		}

		/// <summary>
		/// Связанная строка
		/// </summary>
		[Display(Name = "Связанная строка")]
		public virtual OrderItemEntity OrderItem
		{
			get => _orderItem;
			set => SetField(ref _orderItem, value, () => OrderItem);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display(Name = "Оборудование")]
		public virtual EquipmentEntity Equipment
		{
			get => _equipment;
			set => SetField(ref _equipment, value, () => Equipment);
		}

		/// <summary>
		/// Номенклатура оборудования
		/// </summary>
		[Display(Name = "Номенклатура оборудования")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => Equipment?.Nomenclature ?? _nomenclature;
			set => SetField(ref _nomenclature, value, () => Nomenclature);
		}

		#region Вычисляемые

		/// <summary>
		/// Направление (в виде строки)
		/// </summary>
		public virtual string DirectionString => Direction.GetEnumTitle();
		/// <summary>
		/// Причина забор-доставки (в виде строки)
		/// </summary>
		public virtual string DirectionReasonString => DirectionReason.GetEnumTitle();
		/// <summary>
		/// Причина (в виде строки)
		/// </summary>
		public virtual string ReasonString => Reason.GetEnumTitle();

		#endregion
	}
}
