namespace Mango.Contracts.V1.Options
{
	public class MangoOptions
	{
		public const string SectionName = "Mango";
		
		/// <summary>
		/// Ключ из ЛК манго
		/// </summary>
		public string ApiKey { get; set; } = string.Empty;
		
		/// <summary>
		/// Солт из ЛК манго
		/// </summary>
		public string ApiSalt { get; set; } = string.Empty;
		
		/// <summary>
		/// Ссылка для запроса групп
		/// </summary>
		public string GroupsUrl { get; set; } = "https://app.mango-office.ru/vpbx/groups";
		
		/// <summary>
		/// Ссылка для запроса звонков
		/// </summary>
		public string CallsUrl { get; set; } = "https://app.mango-office.ru/vpbx/stats/calls/request/";
		
		/// <summary>
		/// Ссылка для получения результатов по звонкам
		/// </summary>
		public string CallsResult { get; set; } = "https://app.mango-office.ru/vpbx/stats/calls/result/";

		/// <summary>
		/// Количество звонков в ответе
		/// </summary>
		public int Limit { get; set; } = 100;

		/// <summary>
		/// Смещение по звонкам
		/// </summary>
		public int Offset { get; set; } = 0;
	}
}
