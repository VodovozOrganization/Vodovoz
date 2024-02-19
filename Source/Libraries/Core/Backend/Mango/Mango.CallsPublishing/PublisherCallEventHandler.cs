using Mango.Core.Dto;
using Mango.Core.Handlers;
using MassTransit;
using System;
using System.Threading.Tasks;

namespace Mango.CallsPublishing
{
	public class PublisherCallEventHandler : ICallEventHandler
	{
		private readonly IBus _messagingBus;

		public PublisherCallEventHandler(IBus messagingBus)
		{
			_messagingBus = messagingBus ?? throw new ArgumentNullException(nameof(messagingBus));
		}

		public async Task HandleAsync(MangoCallEvent callEvent)
		{
			await _messagingBus.Publish(callEvent);
		}
	}
}
