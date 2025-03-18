using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Responses
{
	/// <summary>
	/// Информация о статусе регистрации участника
	/// </summary>
	public class ProductInstanceInfoResponse
	{
		/// <summary>
		/// Статус экземпляра товара
		/// </summary>
		[JsonPropertyName("instanceStatuses")]
		public ProductInstanceStatus InstanceStatus { get; set; }

		/// <summary>
		/// Ошибки
		/// </summary>
		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }
	}
}
