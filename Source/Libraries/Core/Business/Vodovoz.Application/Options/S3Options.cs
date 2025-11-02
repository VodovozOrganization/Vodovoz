namespace Vodovoz.Application.Options
{
	/// <summary>
	/// Настройки подключения к хранилищу S3
	/// </summary>
	public class S3Options
	{
		/// <summary>
		/// Url для доступа к Api хранилища
		/// </summary>
		public string ServiceUrl { get; set; }

		/// <summary>
		/// Ключ доступа
		/// </summary>
		public string AccessKey { get; set; }

		/// <summary>
		/// Секретный ключ
		/// </summary>
		public string SecretKey { get; set; }
	}
}
