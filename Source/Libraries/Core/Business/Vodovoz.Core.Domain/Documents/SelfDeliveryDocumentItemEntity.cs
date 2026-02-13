using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Строка документа самовывоза
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]
	[HistoryTrace]
	public class SelfDeliveryDocumentItemEntity : PropertyChangedBase, IDomainObject
	{
		private decimal _amount;
		private IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> _trueMarkProductCodes = new ObservableList<SelfDeliveryDocumentItemTrueMarkProductCode>();
		private SelfDeliveryDocumentEntity _document;
		private NomenclatureEntity _nomenclature;
		private EquipmentEntity _equipment;
		private OrderItemEntity _orderItem;
		private OrderEquipmentEntity _orderEquipment;
		private decimal _amountInStock;
		private decimal _amountUnloaded;
		private CounterpartyMovementOperation _counterpartyMovementOperation;

		/// <summary>
		/// Идентификатор
		/// </summary>
		public virtual int Id { get; set; }

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
		/// Документ самовывоза, к которому относится строка
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
			//Нельзя устанавливать, см. логику в SelfDeliveryDocumentItem.cs
			protected set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Коды ЧЗ товаров, которые были отсканированы в строке документа самовывоза
		/// </summary>
		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<SelfDeliveryDocumentItemTrueMarkProductCode> TrueMarkProductCodes
		{
			get => _trueMarkProductCodes;
			set => SetField(ref _trueMarkProductCodes, value);
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
		/// Связанный товар
		/// </summary>
		[Display(Name = "Связанный товар")]
		public virtual OrderItemEntity OrderItem
		{
			get => _orderItem;
			set => SetField(ref _orderItem, value);
		}

		/// <summary>
		/// Связанное оборудование
		/// </summary>
		[Display(Name = "Связанное оборудование")]
		public virtual OrderEquipmentEntity OrderEquipment
		{
			get => _orderEquipment;
			set => SetField(ref _orderEquipment, value);
		}

		#region Не сохраняемые

		/// <summary>
		/// Количество на складе
		/// </summary>
		[Display(Name = "Количество на складе")]
		public virtual decimal AmountInStock
		{
			get => _amountInStock;
			set => SetField(ref _amountInStock, value);
		}

		/// <summary>
		/// Уже отгружено
		/// </summary>
		[Display(Name = "Уже отгружено")]
		public virtual decimal AmountUnloaded
		{
			get => _amountUnloaded;
			set => SetField(ref _amountUnloaded, value);
		}

		/// <summary>
		/// Операция передвижения товара контрагента
		/// </summary>
		[Display(Name = "Операция передвижения товара контрагента")]
		public virtual CounterpartyMovementOperation CounterpartyMovementOperation
		{
			get => _counterpartyMovementOperation;
			set => SetField(ref _counterpartyMovementOperation, value);
		}

		#endregion
	}
}
