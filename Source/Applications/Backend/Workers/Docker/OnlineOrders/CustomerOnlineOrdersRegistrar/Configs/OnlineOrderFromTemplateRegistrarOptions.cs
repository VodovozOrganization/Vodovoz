namespace CustomerOnlineOrdersRegistrar.Configs
{
	/// <summary>
	/// Настройки регистратора автозаказов из шаблонов
	/// </summary>
	public class OnlineOrderFromTemplateRegistrarOptions
	{
		public const string SectionName = "OnlineOrderFromTemplateRegistrarOptions";
		
		/// <summary>
		/// Задержка между запусками в секундах
		/// </summary>
		public int DelayInSeconds { get; set; }
	}
}
