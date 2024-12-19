using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "цены",
		Nominative = "цена")]
	[HistoryTrace]
	public class NomenclaturePriceEntityBase : PropertyChangedBase, IDomainObject
	{
		private int _minCount = 1;
		private decimal _price;
		private NomenclatureEntity _nomenclature;

		#region Свойства

		public virtual int Id { get; set; }

		[Display(Name = "Номенклатура")]
		public virtual NomenclatureEntity Nomenclature
		{
			get => _nomenclature;
			set => SetField(ref _nomenclature, value);
		}

		[Display(Name = "Минимальное количество")]
		public virtual int MinCount
		{
			get => _minCount;
			set => SetField(ref _minCount, value);
		}

		[Display(Name = "Стоимость")]
		public virtual decimal Price
		{
			get => _price;
			set => SetField(ref _price, value);
		}

		public virtual NomenclaturePriceType Type { get; }

		#endregion

		public enum NomenclaturePriceType
		{
			[Display(Name = "Обычная")]
			General,
			[Display(Name = "Альтернативная")]
			Alternative
		}
	}
}

