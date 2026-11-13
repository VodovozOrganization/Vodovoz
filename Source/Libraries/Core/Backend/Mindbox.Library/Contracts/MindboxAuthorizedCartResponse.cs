using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Ответ Mindbox на операцию CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartResponse
	{
		/// <summary>
		/// Статус операции
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Результат расчёта заказа
		/// </summary>
		[JsonPropertyName("order")]
		public MindboxAuthorizedCartResultOrder Order { get; set; }
	}
}
