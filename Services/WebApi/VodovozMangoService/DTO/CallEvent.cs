using System;
using System.Text.Json.Serialization;

namespace VodovozMangoService.DTO
{
    public class CallEvent
    {
        #region From Json
        public string entry_id { get; set; }
        public string call_id { get; set; }
        public long timestamp { get; set; }
        public uint seq { get; set; }
        public string call_state { get; set; }
        public string location { get; set; }
        public FromCaller from { get; set; }
        public ToCaller to { get; set; }
        public int disconnect_reason { get; set; }
        public string sip_call_id { get; set; }
        #endregion

        #region Calculated

        public CallState CallState => Enum.Parse<CallState>(call_state);
        
        [JsonIgnore]
        public DateTimeOffset Time => DateTimeOffset.FromUnixTimeSeconds(timestamp);
        
        [JsonIgnore]
        public TimeSpan Duration => DateTime.UtcNow - Time;

        #endregion
    }
}