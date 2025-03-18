using System.Text.Json.Serialization;

namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Тип заказа в зависимости от цели приобретения воды клиентом
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum OrderReasonForLeavingDtoType
	{
		/// <summary>
		/// Для собственных нужд
		/// </summary>
		ForPersonal,
		/// <summary>
		/// Для перепродажи
		/// </summary>
		ForResale,
		/// <summary>
		/// Сетевой клиент
		/// </summary>
		Distributing
	}
}
