using Mango.Core.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Events;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using CallState = Vodovoz.Core.Domain.Pacs.CallState;

namespace Pacs.MangoCalls.Services
{
	public class CallEventRegistrar : ICallEventRegistrar
	{
		private readonly ILogger<CallEventRegistrar> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ICallEventSequenceValidator _seqValidator;
		private readonly IBus _messageBus;

		public CallEventRegistrar(
			ILogger<CallEventRegistrar> logger,
			IUnitOfWorkFactory uowFactory,
			ICallEventSequenceValidator seqValidator,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_seqValidator = seqValidator ?? throw new ArgumentNullException(nameof(seqValidator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task RegisterCallEvent(MangoCallEvent mangoCallEvent)
		{
			if(_seqValidator.ValidateCallSequence(mangoCallEvent))
			{
				_logger.LogTrace("Повторное событие Seq={Seq} звонка №{CallId} не зарегистрировано.", mangoCallEvent.Seq, mangoCallEvent.CallId);
				return;
			}

			var CallEvent = CreateDomainEvent(mangoCallEvent);

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				await uow.SaveAsync(CallEvent);
				await uow.CommitAsync();
			}

			await _messageBus.Publish(CallEvent);
		}

		private CallEvent CreateDomainEvent(MangoCallEvent callEvent)
		{
			var domainEvent = new CallEvent
			{
				CallId = callEvent.CallId,
				CallSequence = callEvent.Seq,
				CallState = (CallState)Enum.Parse(typeof(CallState), callEvent.CallState),
				DisconnectReason = callEvent.DisconnectReason,
				EventTime = DateTimeOffset.FromUnixTimeSeconds(callEvent.Timestamp).LocalDateTime,
				FromNumber = callEvent.From.Number,
				FromExtension = callEvent.From.Extension,
				TakenFromCallId = callEvent.From.TakenFromCallId,
				ToNumber = callEvent.To.Number,
				ToExtension = callEvent.To.Extension,
			};
			return domainEvent;
		}
	}
}
