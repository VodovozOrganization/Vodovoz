namespace Vodovoz.Settings.Orders
{
	/// <summary>
	/// Настройки закрытия поставок
	/// </summary>
	public interface IClosingDeliveriesSettings
	{
		/// <summary>
		/// Почта, на которую будут приходить уведомления о закрытии поставок нашимм сотрудникам. Несколько адресов можно указать через точку с запятой.
		/// </summary>
		string ClosingDeliveriesNotificationEmailsTo { get; }

		/// <summary>
		/// Дней сверх просрочки до закрытия поставок
		/// </summary>
		int DaysBeforeClosingDeliveries { get; }

		/// <summary>
		/// Обновить почту для уведомлений о закрытии поставок.
		/// </summary>
		/// <param name="value"></param>
		void UpdateClosingDeliveriesNotificationEmails(string value);

		/// <summary>
		/// Обновить количество дней сверх просрочки до закрытия поставок.
		/// </summary>
		/// <param name="daysBeforeClosingDeliveries"></param>
		void UpdateDaysBeforeClosingDeliveries(int daysBeforeClosingDeliveries);
	}
}
