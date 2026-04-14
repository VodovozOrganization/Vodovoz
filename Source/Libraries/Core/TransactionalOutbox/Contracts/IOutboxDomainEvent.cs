using System;

namespace TransactionalOutbox.Contracts
{
	/// <summary>
	/// Интерфейс, который должен реализовывать доменное событие, чтобы оно могло быть сохранено в транзакционном аутбоксе.
	/// </summary>
	public interface IOutboxDomainEvent
	{
		/// <summary>
		/// Получает ключ для дедупликации события.
		/// </summary>
		/// <returns>Ключ для дедупликации события.</returns>
		string GetDeduplicationKey();

		/// <summary>
		/// Получает идентификатор агрегата, к которому относится событие.
		/// </summary>
		/// <returns>Идентификатор агрегата.</returns>
		int GetAggregateId();
	}
}
