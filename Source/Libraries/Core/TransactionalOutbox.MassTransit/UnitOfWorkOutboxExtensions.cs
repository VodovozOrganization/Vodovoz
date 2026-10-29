using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionalOutbox.MassTransit
{
	public static class UnitOfWorkOutboxExtensions
	{
		/// <summary>
		/// Публикация без изменения данных: сообщение сохраняется в outbox в отдельной короткой транзакции
		/// и уходит после её коммита. Если данные меняются, публикуйте в их UoW через GetPublishEndpoint(uow)
		/// </summary>
		public static async Task PublishInNewTransactionAsync<TBus, T>(
			this IUnitOfWorkOutbox<TBus> outbox,
			IUnitOfWorkFactory unitOfWorkFactory,
			T message,
			CancellationToken cancellationToken = default)
			where TBus : class, IBus
			where T : class
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}

			using(var unitOfWork = unitOfWorkFactory.CreateWithoutRoot($"Публикация {typeof(T).Name}"))
			{
				await outbox.GetPublishEndpoint(unitOfWork).Publish(message, cancellationToken);
				await unitOfWork.CommitAsync(cancellationToken);
			}
		}
	}
}
