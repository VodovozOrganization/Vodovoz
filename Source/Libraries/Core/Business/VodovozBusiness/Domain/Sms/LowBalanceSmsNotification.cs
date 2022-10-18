using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Sms
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "смс уведомления при низком балансе на счете",
		Nominative = "смс уведомление при низком балансе на счете")]
	[EntityPermission]
	[HistoryTrace]
	public class LowBalanceSmsNotification : SmsNotification
	{
		public override SmsNotificationType SmsNotificationType => SmsNotificationType.LowBalance;

		private decimal balance;
		[Display(Name = "Баланс на счете")]
		public virtual decimal Balance {
			get => balance;
			set => SetField(ref balance, value, () => Balance);
		}
	}
}
