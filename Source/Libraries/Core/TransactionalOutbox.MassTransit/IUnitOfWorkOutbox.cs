using MassTransit;
using QS.DomainModel.UoW;
using System;

namespace TransactionalOutbox.MassTransit
{
	/// <summary>
	/// Публикация в транзакцию явно указанного <see cref="IUnitOfWork"/>: сообщение пишется в таблицы outbox
	/// и уходит после его Commit. Нужен там, где UoW не из DI-scope:
	/// <list type="bullet">
	/// <item>на десктопе (у диалогов свои UoW, IPublishEndpoint из DI запрещён);</item>
	/// <item>для второй шины приложения (другой vhost; после перехода на один vhost RabbitMQ этот случай уйдёт);</item>
	/// <item>на бэкенде для UoW, созданного через фабрику.</item>
	/// </list>
	/// На бэкенде для основной шины и UoW из DI достаточно обычного <see cref="IPublishEndpoint"/> из DI:
	/// он пишет в этот же механизм (<see cref="UnitOfWorkScopedBusContextProvider"/>).
	/// </summary>
	/// <typeparam name="TBus">Шина, через которую будет отправлено сообщение</typeparam>
	public interface IUnitOfWorkOutbox<TBus>
		where TBus : class, IBus
	{
		/// <summary>
		/// Конечная точка публикации, сохраняющая сообщения в транзакцию <paramref name="unitOfWork"/>
		/// </summary>
		IPublishEndpoint GetPublishEndpoint(IUnitOfWork unitOfWork);

		/// <summary>
		/// Провайдер конечных точек отправки, сохраняющий сообщения в транзакцию <paramref name="unitOfWork"/>
		/// </summary>
		ISendEndpointProvider GetSendEndpointProvider(IUnitOfWork unitOfWork);

		/// <summary>
		/// Удаляет из текущей транзакции <paramref name="unitOfWork"/> ранее опубликованное сообщение
		/// </summary>
		bool TryRemove(IUnitOfWork unitOfWork, Guid messageId);
	}
}
