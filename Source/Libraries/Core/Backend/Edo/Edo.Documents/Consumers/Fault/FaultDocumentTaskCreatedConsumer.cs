using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultDocumentTaskCreatedConsumer : IConsumer<Fault<DocumentTaskCreatedEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultDocumentTaskCreatedExceptionHandler _faultHandler;

		public FaultDocumentTaskCreatedConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultDocumentTaskCreatedExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<DocumentTaskCreatedEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий создания заявки по документу ЭДО"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
