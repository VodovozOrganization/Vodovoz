using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Sms
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "смс уведомления при н/согл автопереносе в недовозе при переносе",
		Nominative = "смс уведомление при н/согл автопереносе в недовозе при переносе")]
	[EntityPermission]
	[HistoryTrace]
	public class UndeliveryNotApprovedSmsNotification : SmsNotification
	{
		private UndeliveredOrder _undeliveredOrder;
		
		public override SmsNotificationType SmsNotificationType => SmsNotificationType.UndeliveryNotApproved;
		
		[Display(Name = "Недовоз")]
		public virtual UndeliveredOrder UndeliveredOrder 
		{
			get => _undeliveredOrder;
			set => SetField(ref _undeliveredOrder, value);
		}

		private Counterparty counterparty;
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty 
		{
			get => counterparty;
			set => SetField(ref counterparty, value);
		}
	}
}