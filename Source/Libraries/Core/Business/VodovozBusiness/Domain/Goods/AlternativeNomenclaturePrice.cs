using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "альтернативные цены",
		Nominative = "альтернативная цена")]
	public class AlternativeNomenclaturePrice : NomenclaturePriceBase
	{
		public override NomenclaturePriceType Type => NomenclaturePriceType.Alternative;
	}
}
