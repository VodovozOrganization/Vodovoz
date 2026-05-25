namespace CustomerOrdersApi.Library.Config
{
	/// <summary>
	/// Настройки кэша
	/// </summary>
	public class CacheOptions
	{
		public const string Path = "CacheOptions";
		
		/// <summary>
		/// Строка подключения Garnet
		/// </summary>
		public string Garnet { get; set; }
		/// <summary>
		/// Время хранения проверки в корзине в минутах
		/// </summary>
		public int UserBasketCheckExpireTimeInMinutes { get; set; }
	}
}
