using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Статус экземпляров товаров
	/// </summary>
	public class ProductInstanceStatus
	{
		/// <summary>
		/// Код
		/// </summary>
		[JsonPropertyName("IdentificationCode")]
		public string IdentificationCode { get; set; }

		/// <summary>
		/// Статус
		/// </summary>
		[JsonPropertyName("status")]
		public ProductInstanceStatusEnum? Status { get; set; }

		/// <summary>
		/// Инн владельца товара
		/// </summary>
		[JsonPropertyName("ownerInn")]
		public string OwnerInn { get; set; }

		/// <summary>
		/// Название владельца
		/// </summary>
		[JsonPropertyName("ownerName")]
		public string OwnerName { get; set; }
	}
}
