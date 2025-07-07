using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	/// <summary>
	/// Настройки для установки организации по оплате через SMS (QR-код)
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по оплате через SMS (QR-код)",
		Nominative = "Настройка для установки организации по оплате через SMS (QR-код)",
		Prepositional = "Настройке для установки организации по оплате через SMS (QR-код)",
		PrepositionalPlural = "Настройках для установки организации по оплате через SMS (QR-код)"
	)]
	public class SmsQrPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.SmsQR;
	}
}
