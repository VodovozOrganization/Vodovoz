﻿using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Domain.Settings
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по оплате через МП водителя (QR-код)",
		Nominative = "Настройка для установки организации по оплате через МП водителя (QR-код)",
		Prepositional = "Настройке для установки организации по оплате через МП водителя (QR-код)",
		PrepositionalPlural = "Настройках для установки организации по оплате через МП водителя (QR-код)"
	)]
	public class DriverAppQrPaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public override PaymentType PaymentType => PaymentType.DriverApplicationQR;
	}
}
