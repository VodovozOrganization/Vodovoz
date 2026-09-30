using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.RequestsForCall
{
	/// <summary>
	/// Тип заявки на звонок
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
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
