using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultOrderDocumentAcceptedConsumer : IConsumer<Fault<OrderDocumentAcceptedEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultOrderDocumentAcceptedExceptionHandler _faultHandler;

		public FaultOrderDocumentAcceptedConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultOrderDocumentAcceptedExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<OrderDocumentAcceptedEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий подтверждения клиентского документа ЭДО"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
