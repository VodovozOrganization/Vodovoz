using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Sms
{
	/// <summary>
	/// Смс уведомление о том, что курьер в пути
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "смс уведомления о том, что курьер в пути",
		Nominative = "смс уведомление о том, что курьер в пути")]
	[EntityPermission]
	[HistoryTrace]
	public class CourierOnTheWaySmsNotification : SmsNotification
	{
		private Order _order;
		private Counterparty _counterparty;
		private Employee _driver;

		/// <summary>
		/// Тип смс уведомления
		/// </summary>
		public override SmsNotificationType SmsNotificationType => SmsNotificationType.CourierOnTheWay;

		/// <summary>
		/// Заказ, по которому создано смс уведомление
		/// </summary>
		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => _order;
			set => SetField(ref _order, value);
		}

		/// <summary>
		/// Контрагент, по которому создано смс уведомление
		/// </summary>
		[Display(Name = "Контрагент")]
		public virtual Counterparty Counterparty {
			get => _counterparty;
			set => SetField(ref _counterparty, value);
		}

		/// <summary>
		/// Водитель, по которому создано смс уведомление
		/// </summary>
		[Display(Name = "Водитель")]
		public virtual Employee Driver {
			get => _driver;
			set => SetField(ref _driver, value);
		}
	}
}
