using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для установки организации по контрактной документации
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по контрактной документации",
		Nominative = "Настройка для установки организации по контрактной документации",
		Prepositional = "Настройке для установки организации по контрактной документации",
		PrepositionalPlural = "Настройках для установки организации по контрактной документации"
	)]
	public class ContractDocPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.ContractDocumentation;
	}
}
