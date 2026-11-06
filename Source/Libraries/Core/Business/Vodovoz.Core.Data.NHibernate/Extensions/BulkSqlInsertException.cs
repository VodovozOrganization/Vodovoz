using System;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Ошибка массовой вставки после начала работы с БД.
	/// Транзакция UoW при этом принудительно откатывается вместе с остальными изменениями этого UoW.
	/// </summary>
	public class BulkSqlInsertException : Exception
	{
		public BulkSqlInsertException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
