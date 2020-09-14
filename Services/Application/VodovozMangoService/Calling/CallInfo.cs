using VodovozMangoService.DTO;

namespace VodovozMangoService.Calling
{
    public class CallInfo
    {
        public CallInfo(CallEvent callEvent)
        {
            LastEvent = callEvent;
        }

        public CallInfo()
        {
        }

        public uint Seq => LastEvent?.seq ?? 0;
        public CallEvent LastEvent;
        public CallInfo OnHoldCall;
    }
}