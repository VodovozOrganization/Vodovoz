using System.Data.Common;

namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Интерфейс для создания соединений с базой данных, используемых в транзакционном аутбоксе.
	/// </summary>
	public interface IOutboxConnectionFactory
	{
		DbConnection CreateConnection();
	}
}
