using System.Text.Json.Serialization;

namespace Mailganer.Api.Client.Dto
{
	/// <summary>
	/// Описание информации о времени жизни записи (TTL) для домена
	/// </summary>
	public class TtlDto
	{
		/// <summary>
		/// Имя домена
		/// </summary>
		[JsonPropertyName("domain")]
		public string Domain { get; set; }

		/// <summary>
		/// Дата окончания действия TTL для домена
		/// </summary>
		[JsonPropertyName("ttl_date")]
		public string TtlDate { get; set; }
	}
}
