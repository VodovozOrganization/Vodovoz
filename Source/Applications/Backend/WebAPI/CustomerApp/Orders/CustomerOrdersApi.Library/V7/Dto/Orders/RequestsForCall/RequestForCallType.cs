using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.RequestsForCall
{
	/// <summary>
	/// Тип заявки на звонок
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum RequestForCallType
	{
		/// <summary>
		/// Обычная
		/// </summary>
		General,
		/// <summary>
		/// Сервисная
		/// </summary>
		Service
	}
}
