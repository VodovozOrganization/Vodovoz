using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	/// <summary>
	/// Описание события отписки пользователя
	/// </summary>
	public class UserUnsubscribeDto
	{
		/// <summary>
		/// Дата отписки
		/// </summary>
		[JsonPropertyName("date")]
		public string Date { get; set; }

		/// <summary>
		/// Домен, с которого пользователь отписался
		/// </summary>
		[JsonPropertyName("domain")]
		public string Domain { get; set; }
	}
}
