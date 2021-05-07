using System.Text.Json.Serialization;

namespace DriverAPI.Library.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum APIRouteListStatus
    {
        New,
        Confirmed,
        InLoading,
        EnRoute,
        Delivered,
        OnClosing,
        MileageCheck,
        Closed
    }
}
