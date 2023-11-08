using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;
using DomainCallEvent = Vodovoz.Core.Domain.Pacs.CallEvent;
using MangoCallEvent = Mango.Core.Dto.CallEvent;

namespace Pacs.Mango.Services
{
	public class CallEventSaver : ICallEventSaver
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		public CallEventSaver(IUnitOfWorkFactory uowFactory)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
		}

		public async Task SaveCallEvent(MangoCallEvent callEvent)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var domainCallEvent = CreateDomainEvent(callEvent);

				await uow.SaveAsync(domainCallEvent);
				await uow.CommitAsync();
			}
		}

		private DomainCallEvent CreateDomainEvent(MangoCallEvent callEvent)
		{
			var domainEvent = new DomainCallEvent
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
				ToExtension = callEvent.To.Extension
			};
			return domainEvent;
		}
	}
}
