using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Documents;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Transfer.Dispatcher.Consumers.Fault
{
	public class FaultTransferDocumentAcceptedConsumer : IConsumer<Fault<TransferDocumentAcceptedEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultTransferDocumentAcceptedExceptionHandler _faultHandler;

		public FaultTransferDocumentAcceptedConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultTransferDocumentAcceptedExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<TransferDocumentAcceptedEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий подтверждения трансфера"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
