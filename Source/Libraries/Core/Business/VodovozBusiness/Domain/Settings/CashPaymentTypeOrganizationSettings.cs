using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для установки организации по наличке
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по наличке",
		Nominative = "Настройка для установки организации по наличке",
		Prepositional = "Настройке для установки организации по наличке",
		PrepositionalPlural = "Настройках для установки организации по наличке"
	)]
	public class CashPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Cash;
	}
}
