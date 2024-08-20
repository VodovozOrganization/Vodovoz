using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.TrueMark;

namespace VodovozBusiness.Domain.TrueMark
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "коды ЧЗ для товаров в талонах погрузки",
			Nominative = "код ЧЗ для товара в талоне погрузки")]
	public class CarLoadDocumentItemProductCode : PropertyChangedBase, IDomainObject
	{
		private CarLoadDocumentItem _carLoadDocumentItem;
		private int _sequenceNumber;
		private TrueMarkWaterIdentificationCode _trueMarkCode;
		private int _nomenclatureId;

		public virtual int Id { get; set; }

		[Display(Name = "Строка талона погрузки")]
		public virtual CarLoadDocumentItem CarLoadDocumentItem
		{
			get => _carLoadDocumentItem;
			set => SetField(ref _carLoadDocumentItem, value);
		}

		[Display(Name = "Порядковый номер")]
		public virtual int SequenceNumber
		{
			get => _sequenceNumber;
			set => SetField(ref _sequenceNumber, value);
		}

		[Display(Name = "Код честного знака")]
		public virtual TrueMarkWaterIdentificationCode TrueMarkCode
		{
			get => _trueMarkCode;
			set => SetField(ref _trueMarkCode, value);
		}

		[Display(Name = "Id номенклатуры товара")]
		public virtual int NomenclatureId
		{
			get => _nomenclatureId;
			set => SetField(ref _nomenclatureId, value);
		}
	}
}
