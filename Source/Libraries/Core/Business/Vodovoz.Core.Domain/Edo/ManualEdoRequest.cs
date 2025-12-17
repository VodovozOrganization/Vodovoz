using QS.DomainModel.Entity;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Ручная заявка на отправку документов заказа по ЭДО
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "заявка на отправку документов заказа по ЭДО",
		NominativePlural = "заявки на отправку документов заказа по ЭДО"
	)]
	public class ManualEdoRequest : FormalEdoRequest
	{
		public override EdoRequestType DocumentRequestType => EdoRequestType.Manual;
	}
}
