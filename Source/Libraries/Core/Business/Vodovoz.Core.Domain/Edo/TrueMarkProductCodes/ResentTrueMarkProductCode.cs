using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes
{
	/// <summary>
	/// Код ЧЗ товара, созданный для повторной отправки отклоненного кода в другой задаче.
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "переотправленные коды ЧЗ товаров",
		Nominative = "переотправленный код ЧЗ товара")]
	public class ResentTrueMarkProductCode : TrueMarkProductCode
	{
	}
}
