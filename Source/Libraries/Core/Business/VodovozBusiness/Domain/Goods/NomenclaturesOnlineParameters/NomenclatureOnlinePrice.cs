using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Domain.Goods.NomenclaturesOnlineParameters
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "онлайн цены номенклатуры для ИПЗ",
		Accusative = "онлайн цену номенклатуры для ИПЗ",
		Nominative = "онлайн цена номенклатуры для ИПЗ")]
	[HistoryTrace]
	public abstract class NomenclatureOnlinePrice : PropertyChangedBase, IDomainObject
	{
		private decimal? _priceWithoutDiscount;
		private NomenclatureOnlineParameters _nomenclatureOnlineParameters;
		private NomenclaturePriceBase _nomenclaturePrice;

		public virtual int Id { get; set; }
		
		[Display(Name = "Онлайн параметры номенклатуры")]
		public virtual NomenclatureOnlineParameters NomenclatureOnlineParameters
		{
			get => _nomenclatureOnlineParameters;
			set => SetField(ref _nomenclatureOnlineParameters, value);
		}
		
		[Display(Name = "Стоимость со скидкой")]
		public virtual NomenclaturePriceBase NomenclaturePrice
		{
			get => _nomenclaturePrice;
			set => SetField(ref _nomenclaturePrice, value);
		}

		[Display (Name = "Стоимость без скидки")]
		public virtual decimal? PriceWithoutDiscount
		{
			get => _priceWithoutDiscount;
			set => SetField(ref _priceWithoutDiscount, value);
		}

		public abstract GoodsOnlineParameterType Type { get; }
	}
}
