using System;

namespace Edo.Transfer.Routine.Options
{
	public class ClosingDocumentsOrdersUpdSendSettings
	{
		/// <summary>
		/// Интервал отправки УПД по заказам Закр.Док
		/// </summary>
		public TimeSpan Interval { get; set; }

		/// <summary>
		/// Максимальное количество дней с даты доставки заказа для отправки УПД по Закр.Док
		/// </summary>
		public int MaxDaysFromDeliveryDate { get; set; }
	}
}
