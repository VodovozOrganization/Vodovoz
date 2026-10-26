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
	}
}
