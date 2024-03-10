using Mango.Core.Dto;
using MassTransit;
using Microsoft.Extensions.Logging;
using Pacs.Core.Messages.Events;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
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
			if(!_seqValidator.ValidateCallSequence(mangoCallEvent))
			{
				_logger.LogTrace("Повторное событие Seq={Seq} звонка №{CallId} не зарегистрировано.", mangoCallEvent.Seq, mangoCallEvent.CallId);
				return;
			}

			_logger.LogInformation($"Регистрация звонка {mangoCallEvent.CallId}");
			var callEvent = CreateDomainEvent(mangoCallEvent);

			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				await uow.SaveAsync(callEvent);
				await uow.CommitAsync();
			}

			var transportEvent = CreateTransportEvent(mangoCallEvent);
			await _messageBus.Publish(transportEvent);
		}

		private CallEvent CreateTransportEvent(MangoCallEvent callEvent)
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

		private Vodovoz.Core.Domain.Pacs.CallEvent CreateDomainEvent(MangoCallEvent callEvent)
		{
			var domainEvent = new Vodovoz.Core.Domain.Pacs.CallEvent
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

	public class Call
	{
		public DateTime CreationTime { get; set; }
		public IList<SubCall> SubCalls { get; set; } = new List<SubCall>();

		public string EntryId { get; set; }
		public string CallId { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EndTime { get; set; }
		public string FromNumber { get; set; }
		public string FromExtension { get; set; }
		public string ToNumber { get; set; }
		public string ToExtension { get; set; }
		public string ToLineNumber { get; set; }
		public int? DisconnectReason { get; set; }
		public CallDirection? CallDirection { get; set; }
		public CallEntryResult? EntryResult { get; set; }
	}

	public class SubCall
	{
		public DateTime CreationTime { get; set; }
		public Call Call { get; set; }
		public IList<CallEvent> CallEvents{ get; set; }

		public string CallId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string FromNumber { get; set; }
		public string FromExtension { get; set; }
		public string ToNumber { get; set; }
		public string ToExtension { get; set; }
		public string ToLineNumber { get; set; }
		public string ToAcdGroup { get; set; }
		public int DisconnectReason { get; set; }
		public string TakenFromCallId { get; set; }

	}

	public class CallEvent
	{
		public DateTime CreationTime { get; set; }
		public SubCall SubCall { get; set; }
	
		
		public DateTime EventTime { get; set; }
		public CallState State { get; set; }
		public int CallSequence { get; set; }
		public bool FromWasTransfered { get; set; }
		public bool FromHoldInitiator { get; set; }
		public bool ToWasTransfered { get; set; }
		public bool ToHoldInitiator { get; set; }
		public CallTransferType? TransferType { get; set; }
	}
}
