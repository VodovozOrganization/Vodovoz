using QS.DomainModel.Entity;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Строки документа самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentReturnedEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private SelfDeliveryDocumentEntity _document;
		private NomenclatureEntity _nomenclature;
		private EquipmentEntity _equipment;
		private decimal _amount;
		private int? _actualCount;
		private CounterpartyMovementOperation _counterpartyMovementOperation;
		private Direction? _direction;
		private DirectionReason _directionReason;
		private OwnTypes _ownType;

		decimal _amountUnloaded;

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
		/// Документ самовывоза
		/// </summary>
		[Display(Name = "Документ самовывоза")]
		public virtual SelfDeliveryDocumentEntity Document
		{
			get => _document;
			protected set => SetField(ref _document, value);
		}

		/// <summary>
		/// Номенклатура
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			protected set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display(Name = "Оборудование")]
		public virtual EquipmentEntity Equipment
		{
			get => _equipment;
			set
			{
				SetField(ref _equipment, value);

				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Equipment != _equipment)
				{
					CounterpartyMovementOperation.Equipment = _equipment;
				}
			}
		}

		/// <summary>
		/// Количество
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		/// <summary>
		/// Количество оборудования, которое фактически привез/забрал клиент
		/// </summary>
		[Display(Name = "Фактическое количество")]
		public virtual int? ActualCount
		{
			get => _actualCount;
			set => SetField(ref _actualCount, value);
		}

		/// <summary>
		/// Операция передвижения товаров
		/// </summary>
		[Display(Name = "Операция передвижения товаров")]
		public virtual CounterpartyMovementOperation CounterpartyMovementOperation
		{
			get => _counterpartyMovementOperation;
			set => SetField(ref _counterpartyMovementOperation, value);
		}

		/// <summary>
		/// Направление
		/// </summary>
		[Display(Name = "Направление")]
		public virtual Direction? Direction
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

		#region Не сохраняемые

		/// <summary>
		/// Уже отгружено
		/// </summary>
		[Display(Name = "Уже отгружено")]
		public virtual decimal AmountUnloaded
		{
			get => _amountUnloaded;
			set => SetField(ref _amountUnloaded, value);
		}

		#endregion
	}
}

