using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Первичная заявка на отправку документов заказа по ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов заказа по ЭДО",
		NominativePlural = "заявки на отправку документов заказа по ЭДО"
	)]
	public class PrimaryEdoRequest : FormalEdoRequest
	{
		public override EdoRequestType DocumentRequestType => EdoRequestType.Primary;
	}
}
