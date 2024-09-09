using Core.Infrastructure;
using Mango.Core.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pacs.Core.Messages.Events;
using Pacs.MangoCalls.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.MangoCalls.Services
{
	internal class CallEventRegistrar : ICallEventRegistrar
	{
		private readonly ILogger _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CallEventHandlerFactory _callEventHandlerFactory;
		private readonly RetrySettings _retryOptions;
		private readonly IBus _messageBus;

		public CallEventRegistrar(
			ILoggerFactory loggerFactory,
			IUnitOfWorkFactory uowFactory,
			CallEventHandlerFactory callEventHandlerFactory,
			IOptions<RetrySettings> retryOptions,
			IBus messageBus)
		{
			_logger = loggerFactory.CreateLogger("Events");
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_callEventHandlerFactory = callEventHandlerFactory ?? throw new ArgumentNullException(nameof(callEventHandlerFactory));
			_retryOptions = retryOptions?.Value ?? throw new ArgumentNullException(nameof(retryOptions));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task RegisterCallEvents(IEnumerable<MangoCallEvent> mangoCallEvents)
		{
			var retryCounter = _retryOptions.RetryCount;
			var needRetry = false;

			do
			{
				if(needRetry)
				{
					_logger.LogInformation("Повтор попытки записи");
				}

				try
				{
					var callEventHanlers = new Dictionary<string, CallEventHandler>();
					var pacsCallEvents = new List<PacsCallEvent>();

					using(var uow = _uowFactory.CreateWithoutRoot())
					{
						foreach(var callEvent in mangoCallEvents)
						{
							_logger.LogTrace("Call event | entryId: {entryId}", callEvent.EntryId);
							var callEventHandler = callEventHanlers.GetOrAdd(callEvent.EntryId, (entryId) =>
								_callEventHandlerFactory.CreateCallEventHandler(entryId, uow)
							);

							var call = await callEventHandler.HandleCallEvent(callEvent);

							if(call.CallDirection == CallDirection.Incoming)
							{
								pacsCallEvents.Add(new PacsCallEvent { Call = call });
							}
						}

						_logger.LogInformation("За итерацию обработано {PacsCallEventsCount} из {MangoCallEventsCount}", pacsCallEvents?.Count, mangoCallEvents?.Count());

						await uow.CommitAsync();

						var publishTasks = pacsCallEvents.Select(x => _messageBus.Publish(x));
						await Task.WhenAll(publishTasks);
					}
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка записи события");

					if(retryCounter > 0)
					{
						needRetry = true;
						await Task.Delay(TimeSpan.FromSeconds(1 + 3 - retryCounter));
					}
				}
			}
			while(needRetry);
		}

		public async Task RegisterSummaryEvent(MangoSummaryEvent summaryEvent)
		{
			var retryCounter = _retryOptions.RetryCount;
			var needRetry = false;
			_logger.LogTrace("Summary event | entryId: {entryId}", summaryEvent.EntryId);
			do
			{
				if(needRetry)
				{
					_logger.LogInformation("Повтор попытки записи");
				}

				try
				{
					using(var uow = _uowFactory.CreateWithoutRoot())
					{
						var callEventHandler = _callEventHandlerFactory.CreateCallEventHandler(summaryEvent.EntryId, uow);
						var call = await callEventHandler.HandleSummaryEvent(summaryEvent);
						await uow.CommitAsync();

						if(call.CallDirection == CallDirection.Incoming)
						{
							await _messageBus.Publish(new PacsCallEvent { Call = call });
						}
					}
					needRetry = false;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка записи события");

					if(retryCounter > 0)
					{
						needRetry = true;
						await Task.Delay(_retryOptions.Delay + TimeSpan.FromSeconds(_retryOptions.RetryCount - retryCounter));
					}
				}
			}
			while(needRetry);
		}
	}
}
