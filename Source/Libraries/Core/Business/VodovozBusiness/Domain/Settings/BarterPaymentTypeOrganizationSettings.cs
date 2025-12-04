using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для установки организации по бартеру
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по бартеру",
		Nominative = "Настройка для установки организации по бартеру",
		Prepositional = "Настройке для установки организации по бартеру",
		PrepositionalPlural = "Настройках для установки организации по бартеру"
	)]
	public class BarterPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Barter;
	}
}
