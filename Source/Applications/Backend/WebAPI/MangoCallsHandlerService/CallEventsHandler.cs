using Mango.Api.Handlers;
using Mango.Core.Dto;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MangoCallsHandlerService
{
	public class CallEventsHandler : ICallEventHandler
	{
		private readonly ILogger<CallEventsHandler> _logger;

		public CallEventsHandler(ILogger<CallEventsHandler> logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		public async Task HandleAsync(CallEvent callEvent)
		{
			_logger.LogInformation($"Call: {callEvent.CallId}, " +
				$"State: {callEvent.CallState}, " +
				$"Seq: {callEvent.Seq}, " +
				$"From: {callEvent.From.Number}:{callEvent.From.Extension}" +
				$"To: {callEvent.To.Number}:{callEvent.To.Extension} Group:{callEvent.To.AcdGroup}" +
				$"");

			return;
		}
	}
}
