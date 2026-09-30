using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultOrderDocumentSentConsumer : IConsumer<Fault<OrderDocumentSentEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultOrderDocumentSentExceptionHandler _faultHandler;

		public FaultOrderDocumentSentConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultOrderDocumentSentExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}

		public async Task Consume(ConsumeContext<Fault<OrderDocumentSentEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий об отправке документа"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}

