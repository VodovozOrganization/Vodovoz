/// <summary>
/// Настройки для отправки уведомлений об ЭДО
/// </summary>
public class EdoNotificationsOptions
{
	/// <summary>
	/// Название секции с настройками
	/// </summary>
	public const string SectionName = "EdoNotifications";

	/// <summary>
	/// Ссылка Битрикс
	/// </summary>
	public string BitrixWebhookUrl { get; set; }

	/// <summary>
	/// Email отправителя
	/// </summary>
	public string EmailForMailing { get; set; }
}
