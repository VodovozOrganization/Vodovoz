using System.Text.Json.Serialization;

namespace DriverAPI.Library.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum APIActionType
    {
        OpenOrderInfoPanel,
        OpenOrderDeliveryPanel,
        OpenOrderReceiptionPanel,
        CompleteOrderClicked
    }
}
