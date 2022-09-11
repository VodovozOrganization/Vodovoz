using System.Text.Json.Serialization;

namespace DriverAPI.Library.DTOs
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum ActionDtoType
	{
		OpenOrderInfoPanel,
		OpenOrderDeliveryPanel,
		OpenOrderReceiptionPanel,
		CompleteOrderClicked
	}
}
