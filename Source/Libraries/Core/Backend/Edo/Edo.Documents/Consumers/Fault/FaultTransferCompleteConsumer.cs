using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Documents.Consumers.Fault
{
	public class FaultTransferCompleteConsumer : IConsumer<Fault<TransferCompleteEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultTransferCompleteExceptionHandler _faultHandler;

		public FaultTransferCompleteConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultTransferCompleteExceptionHandler faultHandler)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<TransferCompleteEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Сервис обработки упавших событий завершения трансфера"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
