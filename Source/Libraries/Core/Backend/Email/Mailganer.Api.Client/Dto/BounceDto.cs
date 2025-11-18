using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	/// <summary>
	/// Описание события возврата письма (bounce)
	/// </summary>
	public class BounceDto
	{
		/// <summary>
		/// Дата события возврата
		/// </summary>
		[JsonPropertyName("date")]
		public string Date { get; set; }

		/// <summary>
		/// Текст сообщения или причина возврата письма
		/// </summary>
		[JsonPropertyName("bounce")]
		public string BounceMessage { get; set; }
	}
}
