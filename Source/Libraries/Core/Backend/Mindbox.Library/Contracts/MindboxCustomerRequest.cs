using System.Text.Json.Serialization;

namespace Mindbox.Library.Contracts
{
	/// <summary>
	/// Тело запроса на создание/обновление клиента в Mindbox
	/// </summary>
	public sealed class MindboxCustomerRequest
	{
		[JsonPropertyName("customer")]
		public MindboxCustomer Customer { get; set; }
	}
}
