namespace GeoCoderApi.Client.Options
{
	public class GeoCoderApiOptions
	{
		/// <summary>
		/// Базовый адрес Api геокодера
		/// </summary>
		public string BaseUri { get; set; }

		/// <summary>
		/// Токен доступа к Api
		/// </summary>
		public string ApiToken { get; set; }
	}
}
