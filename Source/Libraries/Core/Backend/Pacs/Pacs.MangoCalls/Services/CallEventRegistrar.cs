using Core.Infrastructure;
using Mango.Core.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Events;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.MangoCalls.Services
{
	internal class CallEventRegistrar : ICallEventRegistrar
	{
		private readonly ILogger<CallEventRegistrar> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly CallEventHandlerFactory _callEventHandlerFactory;
		private readonly IBus _messageBus;

		public CallEventRegistrar(
			ILogger<CallEventRegistrar> logger,
			IUnitOfWorkFactory uowFactory,
			CallEventHandlerFactory callEventHandlerFactory,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_callEventHandlerFactory = callEventHandlerFactory ?? throw new ArgumentNullException(nameof(callEventHandlerFactory));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task RegisterCallEvents(IEnumerable<MangoCallEvent> mangoCallEvents)
		{
			var callEventHanlers = new Dictionary<string, CallEventHandler>();
			var pacsCallEvents = new List<PacsCallEvent>();

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				foreach(var callEvent in mangoCallEvents)
				{
					var callEventHandler = callEventHanlers.GetOrAdd(callEvent.EntryId, (entryId) => 
						_callEventHandlerFactory.CreateCallEventHandler(entryId, uow)
					);


					var call = await callEventHandler.HandleCallEvent(callEvent);
					if(call.CallDirection == CallDirection.Incoming)
					{
						pacsCallEvents.Add(new PacsCallEvent { Call = call });
					}
				}

				await uow.CommitAsync();

				var publishTasks = pacsCallEvents.Select(x => _messageBus.Publish(x));
				await Task.WhenAll(publishTasks);
			}
		}

		public async Task RegisterSummaryEvent(MangoSummaryEvent summaryEvent)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var callEventHandler = _callEventHandlerFactory.CreateCallEventHandler(summaryEvent.EntryId, uow);
				var call = await callEventHandler.HandleSummaryEvent(summaryEvent);
				_logger.LogInformation($"Summary event | entryId: {summaryEvent.EntryId}, endTime: {summaryEvent.EndTime} ({call.EndTime})");
				await uow.CommitAsync();

				if(call.CallDirection == CallDirection.Incoming)
				{
					await _messageBus.Publish(new PacsCallEvent { Call = call });
				}
			}
		}
	}
}
