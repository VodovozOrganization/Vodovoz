namespace Vodovoz.Settings.Sale
{
	/// <summary>
	/// Настройки шаблонов онлайн заказов
	/// </summary>
	public interface IOnlineOrderTemplateSettings : IRepeatTemplateSettings
	{
		/// <summary>
		/// Получение максимального количества "рабочих" шаблонов(не архивных)
		/// </summary>
		int GetMaximumNumberActiveOnlineOrderTemplates { get; }

		/// <summary>
		/// Получение количества дат следующих доставок при ответе CheckOrderTemplate <see cref="CheckOrderTemplateResponse"/>
		/// </summary>
		int GetNumberNextDeliveriesToDisplay { get; }
		
		/// <summary>
		/// Получение количества дат следующих доставок для недельных повторений при ответе ResumeOrderTemplate <see cref="ResumeOrderTemplateResponse"/>
		/// </summary>
		int GetNumberNextDeliveriesToDisplayByWeekly { get; }
		
		/// <summary>
		/// Получение количества дат следующих доставок для месячных повторений при ответе ResumeOrderTemplate <see cref="ResumeOrderTemplateResponse"/>
		/// </summary>
		int GetNumberNextDeliveriesToDisplayByMonthly { get; }
		
		/// <summary>
		/// Получение количества добавляемых дней для предполагаемой даты доставки автозаказа при запросе до 11
		/// </summary>
		int GetAddDaysToDeliveryDateBefore11Hours { get; }
		
		/// <summary>
		/// Получение количества добавляемых дней для предполагаемой даты доставки автозаказа при запросе после 11
		/// </summary>
		int GetAddDaysToDeliveryDateAfter11Hours { get; }
	}
}
