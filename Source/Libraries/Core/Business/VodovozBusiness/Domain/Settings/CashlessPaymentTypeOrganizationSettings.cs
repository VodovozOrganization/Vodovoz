using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для установки организации по безналу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по безналу",
		Nominative = "Настройка для установки организации по безналу",
		Prepositional = "Настройке для установки организации по безналу",
		PrepositionalPlural = "Настройках для установки организации по безналу"
	)]
	public class CashlessPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Cashless;
	}
}
