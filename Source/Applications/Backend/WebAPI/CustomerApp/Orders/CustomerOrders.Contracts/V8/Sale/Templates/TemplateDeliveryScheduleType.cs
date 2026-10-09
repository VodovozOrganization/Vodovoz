using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum TemplateDeliveryScheduleType
	{
		/// <summary>
		/// Недельная(в конкретный день недели)
		/// </summary>
		Weekly,
		/// <summary>
		/// Месячная(в конкретное число месяца)
		/// </summary>
		Monthly
	}
}
