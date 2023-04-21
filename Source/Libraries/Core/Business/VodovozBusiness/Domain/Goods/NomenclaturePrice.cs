using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "обычные цены",
		Nominative = "обычная цена")]
	public class NomenclaturePrice : NomenclaturePriceBase
	{
		public override NomenclaturePriceType Type => NomenclaturePriceType.General;
	}
}
