using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для установки организации по терминалу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по терминалу",
		Nominative = "Настройка для установки организации по терминалу",
		Prepositional = "Настройке для установки организации по терминалу",
		PrepositionalPlural = "Настройках для установки организации по терминалу"
	)]
	public class TerminalPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.Terminal;
	}
}
