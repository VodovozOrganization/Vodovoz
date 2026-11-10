using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Тело запроса операции CalculateAuthorizedCart
	/// </summary>
	public sealed class MindboxAuthorizedCartRequest
	{
		/// <summary>
		/// Клиент, для которого рассчитывается заказ
		/// </summary>
		[JsonPropertyName("customer")]
		public MindboxAuthorizedCartCustomer Customer { get; set; }

		/// <summary>
		/// Заказ
		/// </summary>
		[JsonPropertyName("order")]
		public MindboxAuthorizedCartOrder Order { get; set; }
	}
}
