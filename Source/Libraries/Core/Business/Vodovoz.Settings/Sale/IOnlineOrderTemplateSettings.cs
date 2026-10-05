namespace Vodovoz.Settings.Sale
{
	/// <summary>
	/// Настройки шаблонов онлайн заказов
	/// </summary>
	public interface IOnlineOrderTemplateSettings
	{
		/// <summary>
		/// Получение максимального количества активных шаблонов
		/// </summary>
		int GetMaximumNumberActiveOnlineOrderTemplates { get; }
	}
}
