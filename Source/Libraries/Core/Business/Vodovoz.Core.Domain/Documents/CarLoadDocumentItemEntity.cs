using QS.DomainModel.Entity;
using QS.Extensions.Observable.Collections.List;
using QS.HistoryLog;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Core.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки талона погрузки",
		Nominative = "строка талона погрузки")]
	[HistoryTrace]
	public class CarLoadDocumentItemEntity : PropertyChangedBase, IDomainObject
	{
		private CarLoadDocumentEntity _document;
		private decimal _amount;
		private decimal? _expireDatePercent;
		private int? _orderId;
		private bool _isIndividualSetForOrder;
		private NomenclatureEntity _nomenclature;
		private IObservableList<CarLoadDocumentItemTrueMarkProductCode> _trueMarkCodes = new ObservableList<CarLoadDocumentItemTrueMarkProductCode>();

		public virtual int Id { get; set; }

		[Display(Name = "Талон погрузки")]
		public virtual CarLoadDocumentEntity Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}

		[Display(Name = "Количество")]
		public virtual decimal Amount
		{
			get => _amount;
			set => SetField(ref _amount, value);
		}

		[Display(Name = "Процент срока годности")]
		public virtual decimal? ExpireDatePercent
		{
			get => _expireDatePercent;
			set
			{
				SetField(ref _expireDatePercent, value);
			}
		}

		[Display(Name = "Номер заказа")]
		public virtual int? OrderId
		{
			get => _orderId;
			set => SetField(ref _orderId, value);
		}

		[Display(Name = "Отделить номенклатуры заказа при погрузке")]
		public virtual bool IsIndividualSetForOrder
		{
			get => _isIndividualSetForOrder;
			set => SetField(ref _isIndividualSetForOrder, value);
		}

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Коды ЧЗ товаров")]
		public virtual IObservableList<CarLoadDocumentItemTrueMarkProductCode> TrueMarkCodes
		{
			get => _trueMarkCodes;
			set => SetField(ref _trueMarkCodes, value);
		}
	}
}
