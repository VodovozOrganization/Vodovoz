using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "коды ЧЗ товаров введенные вручную",
		Nominative = "код ЧЗ товара введенный вручную")]
	public class ManualTrueMarkProductCode : TrueMarkProductCode
	{
	}
}
