using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Mango.Service.Calling
{
	public class Subscription
	{
		public readonly Channel<NotificationMessage> Queue;
		public readonly uint Extension;

		public Subscription(uint extension)
		{
			var options = new UnboundedChannelOptions();
			options.SingleReader = true;
			Queue = Channel.CreateUnbounded<NotificationMessage>(options);
			Extension = extension;
		}
	}
}