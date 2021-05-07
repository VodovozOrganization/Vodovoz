using System.Text.Json.Serialization;

namespace DriverAPI.Library.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum APIRouteListAddressStatus
    {
        EnRoute,
        Completed,
        Canceled,
        Overdue,
        Transfered
    }
}
