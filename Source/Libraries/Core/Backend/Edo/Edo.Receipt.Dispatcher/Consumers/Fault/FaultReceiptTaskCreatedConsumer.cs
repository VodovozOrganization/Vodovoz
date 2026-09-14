using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Receipt.Dispatcher.Consumers.Fault
{
	public class FaultReceiptTaskCreatedConsumer : IConsumer<Fault<ReceiptTaskCreatedEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultReceiptTaskCreatedExceptionHandler _faultHandler;

		public FaultReceiptTaskCreatedConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultReceiptTaskCreatedExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<ReceiptTaskCreatedEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий создания заявки по чеку"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
