namespace VodovozMangoService.DTO
{
    public class CallEvent
    {
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
    }
}