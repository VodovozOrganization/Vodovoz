using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "обычные цены",
		Nominative = "обычная цена")]
	public class NomenclaturePriceEntity : NomenclaturePriceEntityBase
	{
		public override NomenclaturePriceType Type => NomenclaturePriceType.General;
	}
}
