using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Handlers;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Docflow.Consumers.Fault
{
	public class FaultTransferDocumentSendConsumer : IConsumer<Fault<TransferDocumentSendEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultTransferDocumentSendExceptionHandler _faultHandler;

		public FaultTransferDocumentSendConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultTransferDocumentSendExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<TransferDocumentSendEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий отправки трансфера"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
