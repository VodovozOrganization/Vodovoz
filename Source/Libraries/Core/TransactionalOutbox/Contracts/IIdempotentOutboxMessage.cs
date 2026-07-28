namespace TransactionalOutbox.Contracts
{
	/// <summary>
	///Обеспечивает идемпотентность сообщений Outbox
	/// </summary>
	public interface IIdempotentOutboxMessage
	{
		/// <summary>
		/// Получает идентификатор агрегата
		/// </summary>
		/// <returns>Идентификатор агрегата</returns>
		int GetAggregateId();

		/// <summary>
		/// Получает ключ для дедупликации
		/// </summary>
		/// <returns>Ключ для дедупликации</returns>
		string GetDeduplicationKey();
	}
}
