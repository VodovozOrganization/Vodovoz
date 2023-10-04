using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Mango.Grpc.Client
{
    public class NotificationConnectionWatcher
    {
        private readonly Channel channel;
        private readonly Action<ChannelState> stateChanged;

        public NotificationConnectionWatcher(Channel channel, Action<ChannelState> stateChanged)
        {
			this.channel = channel ?? throw new ArgumentNullException(nameof(channel));
            this.stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
            channel.WaitForStateChangedAsync(channel.State).ContinueWith(HandleStateChange);
        }

        private void HandleStateChange(Task task)
        {
            stateChanged(channel.State);
            if(channel.State != ChannelState.Shutdown)
                channel.WaitForStateChangedAsync(channel.State).ContinueWith(HandleStateChange);
        }
    }
}
