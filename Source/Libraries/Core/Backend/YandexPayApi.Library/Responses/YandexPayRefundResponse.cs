using System.Text.Json.Serialization;
using YandexPayApi.Library.Models;

namespace YandexPayApi.Library.Responses
{
	/// <summary>
	/// Ответ от Yandex Pay API при создании возврата
	/// </summary>
	public class YandexPayRefundResponse
	{
		/// <summary>
		/// Информация об операции возврата
		/// </summary>
		[JsonPropertyName("operation")]
		public YandexPayRefundOperation Operation { get; set; }
	}
}
