using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Handlers;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Docflow.Consumers.Fault
{
	public class FaultOrderDocumentSendConsumer : IConsumer<Fault<OrderDocumentSendEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultOrderDocumentSendExceptionHandler _faultHandler;

		public FaultOrderDocumentSendConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultOrderDocumentSendExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<OrderDocumentSendEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий отправки УПД по ЭДО"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
