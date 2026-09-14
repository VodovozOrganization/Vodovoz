using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;

namespace Edo.Receipt.Sender.Consumers.Fault
{
	public class FaultReceiptReadyToSendConsumer : IConsumer<Fault<ReceiptReadyToSendEvent>>
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly FaultReceiptReadyToSendExceptionHandler _faultHandler;

		public FaultReceiptReadyToSendConsumer(
			IUnitOfWorkFactory uowFactory,
			FaultReceiptReadyToSendExceptionHandler faultHandler
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_faultHandler = faultHandler ?? throw new ArgumentNullException(nameof(faultHandler));
		}
		
		public async Task Consume(ConsumeContext<Fault<ReceiptReadyToSendEvent>> context)
		{
			var fault = context.Message;

			using(var uow = _uowFactory.CreateWithoutRoot("Обработка упавших событий о готовности чека к отправке"))
			{
				await _faultHandler.HandleAsync(uow, fault, context.CancellationToken);
				await Task.CompletedTask;
			}
		}
	}
}
