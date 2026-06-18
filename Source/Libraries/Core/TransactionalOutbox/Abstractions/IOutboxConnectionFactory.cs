using System.Data.Common;

namespace TransactionalOutbox.Abstractions
{
	/// <summary>
	/// Интерфейс для создания соединений с базой данных, используемых в транзакционном аутбоксе.
	/// </summary>
	public interface IOutboxConnectionFactory
	{
		/// <summary>
		/// Создает и возвращает новое соединение с базой данных.
		/// </summary>
		/// <returns>Новое соединени с базой данных</returns>
		DbConnection CreateConnection();
	}
}
