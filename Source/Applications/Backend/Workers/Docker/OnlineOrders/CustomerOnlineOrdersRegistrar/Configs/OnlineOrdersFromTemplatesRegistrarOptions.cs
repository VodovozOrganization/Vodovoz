namespace CustomerOnlineOrdersRegistrar.Configs
{
	/// <summary>
	/// Настройки регистратора автозаказов из шаблонов
	/// </summary>
	public class OnlineOrdersFromTemplatesRegistrarOptions
	{
		public const string SectionName = "OnlineOrdersFromTemplatesRegistrarOptions";
		
		/// <summary>
		/// Задержка между запусками в секундах
		/// </summary>
		public int DelayInSeconds { get; set; }
	}
}
