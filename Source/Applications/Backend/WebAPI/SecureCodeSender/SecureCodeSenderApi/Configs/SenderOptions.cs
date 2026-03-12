namespace SecureCodeSenderApi.Configs
{
	/// <summary>
	/// Настройки отправщика кодов
	/// </summary>
	public class SenderOptions
	{
		public const string Path = "SenderOptions";
		
		/// <summary>
		/// Лимит попыток отправок смс
		/// </summary>
		public int SendSmsAttemptsCountLimit { get; set; }
		/// <summary>
		/// Лимит попыток отправок в Telegram
		/// </summary>
		public int SendToTelegramAttemptsCountLimit { get; set; }
		/// <summary>
		/// Длина кода. Должно быть от 4 до 6 символов
		/// </summary>
		public int CodeLength { get; set; }
	}
}
