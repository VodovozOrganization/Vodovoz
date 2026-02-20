using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Строка талона погрузки автомобиля
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки талона погрузки",
		Nominative = "строка талона погрузки")]
	[HistoryTrace]
	public class CarLoadDocumentItemEntity : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private CarLoadDocumentEntity _document;
		private decimal _amount;
		private decimal? _expireDatePercent;
		private int? _orderId;
		private bool _isIndividualSetForOrder;
		private NomenclatureEntity _nomenclature;
		private IObservableList<CarLoadDocumentItemTrueMarkProductCode> _trueMarkCodes = new ObservableList<CarLoadDocumentItemTrueMarkProductCode>();

		/// <summary>
		/// Идентификатор<br/>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Талон погрузки, к которому относится строка
		/// </summary>
		[Display(Name = "Талон погрузки")]
		public virtual CarLoadDocumentEntity Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}

		/// <summary>
		/// Количество номенклатуры в строке талона погрузки
		/// </summary>
		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		/// <summary>
		/// Процент срока годности
		/// </summary>
		[Display(Name = "Процент срока годности")]
		public virtual decimal? ExpireDatePercent
		{
			get => _expireDatePercent;
			set
			{
				SetField(ref _expireDatePercent, value);
			}
		}

		/// <summary>
		/// Номер заказа, к которому относится строка талона погрузки
		/// </summary>
		[Display(Name = "Номер заказа")]
		public virtual int? OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		/// <summary>
		/// Отделить номенклатуры заказа при погрузке
		/// </summary>
		[Display(Name = "Отделить номенклатуры заказа при погрузке")]
		public virtual bool IsIndividualSetForOrder
		{
			get => _isIndividualSetForOrder;
			set => SetField(ref _isIndividualSetForOrder, value);
		}

		/// <summary>
		/// Номенклатура, к которой относится строка талона погрузки
		/// </summary>
		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			//Нельзя устанавливать, см. логику в CarLoadDocumentItem.cs
			protected set => SetField(ref _nomenclature, value);
		}

		/// <summary>
		/// Коды ЧЗ товаров, которые относятся к строке талона погрузки
		/// </summary>
		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<CarLoadDocumentItemTrueMarkProductCode> TrueMarkCodes
		{
			get => _trueMarkCodes;
			set => SetField(ref _trueMarkCodes, value);
		}
	}
}
