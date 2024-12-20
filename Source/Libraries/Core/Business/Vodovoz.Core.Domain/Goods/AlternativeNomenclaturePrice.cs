using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Goods
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "альтернативные цены",
		Nominative = "альтернативная цена")]
	public class AlternativeNomenclaturePriceEntity : NomenclaturePriceEntityBase
	{
		public override NomenclaturePriceType Type => NomenclaturePriceType.Alternative;
	}
}
