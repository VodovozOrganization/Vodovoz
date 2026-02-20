using MassTransit;
using System;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;

namespace Edo.Documents.Consumers
{
	/// <summary>
	/// Обработчик события завершения документооборота по заказу
	/// </summary>
	/// <remarks>
	/// Перенаправляет событие в WithdrawalEdoRequestHandler для проверки необходимости
	/// создания заявки на вывод из оборота (ситуация 2).
	/// </remarks>
	public class OrderDocflowCompletedConsumer : IConsumer<OrderDocflowCompletedEvent>
	{
		private readonly WithdrawalEdoRequestHandler _withdrawalEdoRequestHandler;

		public OrderDocflowCompletedConsumer(WithdrawalEdoRequestHandler withdrawalEdoRequestHandler)
		{
			_withdrawalEdoRequestHandler = withdrawalEdoRequestHandler 
				?? throw new ArgumentNullException(nameof(withdrawalEdoRequestHandler));
		}

		public async Task Consume(ConsumeContext<OrderDocflowCompletedEvent> context)
		{
			await _withdrawalEdoRequestHandler.HandleOrderDocflowCompleted(
				context.Message.DocumentId, 
				context.CancellationToken);
		}
	}
}
