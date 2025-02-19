using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Settings
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "Настройки для установки организации по оплачено онлайн",
		Nominative = "Настройка для установки организации по оплачено онлайн",
		Prepositional = "Настройке для установки организации по оплачено онлайн",
		PrepositionalPlural = "Настройках для установки организации по оплачено онлайн"
	)]
	public class OnlinePaymentTypeOrganizationSettings : PaymentTypeOrganizationSettings
	{
		public virtual IList<PaymentFrom> PaymentsFrom { get; set; } =  new List<PaymentFrom>();
		public override PaymentType PaymentType => PaymentType.PaidOnline;
		
		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var paymentFrom in PaymentsFrom)
			{
				if(paymentFrom.OrganizationForOnlinePayments is null)
				{
					yield return new ValidationResult(
						$"Для источника оплаты {paymentFrom.Name} должна быть указана организация",
						new[] { nameof(paymentFrom) }
						);
				}
			}
		}
	}
}
