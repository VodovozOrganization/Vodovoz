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

		/// <summary>
		/// Идентификатор точки контакта, в которой создан контрагент
		/// </summary>
		[JsonPropertyName("pointOfContact")]
		public string PointOfContact { get; set; }

		/// <summary>
		/// Идентификатор устройства из ИПЗ. Передаётся в URL и не попадает в тело запроса
		/// </summary>
		[JsonIgnore]
		public string DeviceUuid { get; set; }
	}
}
