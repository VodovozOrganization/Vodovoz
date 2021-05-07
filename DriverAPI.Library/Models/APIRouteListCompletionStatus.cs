using System.Text.Json.Serialization;

namespace DriverAPI.Library.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum  APIRouteListCompletionStatus
    {
        Completed,
        Incompleted
    }
}
