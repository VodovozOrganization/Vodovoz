using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Sms
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "смс уведомления при новом контрагенте",
		Nominative = "смс уведомление при новом контрагенте")]
	[EntityPermission]
	[HistoryTrace]
	public class NewClientSmsNotification : SmsNotification
	{
		public override SmsNotificationType SmsNotificationType => SmsNotificationType.NewClient;

		private Order order;
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value, () => Order);
		}

		private Counterparty counterparty;
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value, () => Counterparty);
		}
	}
}
