using System.Collections.Concurrent;

namespace VodovozMangoService.Calling
{
    public class Subscription
    {
        public readonly BlockingCollection<NotificationMessage> Queue = new BlockingCollection<NotificationMessage>();
        public readonly uint Extension;
        public CallInfo CurrentCall;

        public Subscription(uint extension)
        {
            Extension = extension;
        }
    }
}