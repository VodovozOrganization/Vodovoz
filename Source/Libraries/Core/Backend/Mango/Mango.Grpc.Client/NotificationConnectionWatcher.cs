using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Mango.Grpc.Client
{
	public class NotificationConnectionWatcher
    {
        private readonly Channel _channel;
        private readonly Action<ChannelState> _stateChanged;

        public NotificationConnectionWatcher(Channel channel, Action<ChannelState> stateChanged)
        {
			this._channel = channel ?? throw new ArgumentNullException(nameof(channel));
            this._stateChanged = stateChanged ?? throw new ArgumentNullException(nameof(stateChanged));
            channel.WaitForStateChangedAsync(channel.State).ContinueWith(HandleStateChange);
        }

        private void HandleStateChange(Task task)
        {
            _stateChanged(_channel.State);
            if(_channel.State != ChannelState.Shutdown)
                _channel.WaitForStateChangedAsync(_channel.State).ContinueWith(HandleStateChange);
        }
    }
}
