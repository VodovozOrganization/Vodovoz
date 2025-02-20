﻿using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Settings
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по оплачено онлайн",
		Nominative = _nominative,
		Prepositional = "Настройке для установки организации по оплачено онлайн",
		PrepositionalPlural = "Настройках для установки организации по оплачено онлайн"
	)]
	public class OnlinePaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		private const string _nominative = "Настройка для установки организации по оплачено онлайн";
		public virtual PaymentFrom PaymentFrom { get; set; }
		public virtual string CriterionForOrganization { get; set; }
		public override PaymentType PaymentType => PaymentType.PaidOnline;
		
		public override string ToString()
		{
			return PaymentFrom != null ? $"{_nominative} в {PaymentFrom.Name}" : _nominative;
		}
	}
}
