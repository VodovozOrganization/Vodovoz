using System;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Ошибка массовой вставки после начала работы с БД.
	/// UoW при этом закрывается, его транзакция откатывается вместе с остальными изменениями этого UoW.
	/// Работать с этим UoW дальше нельзя.
	/// </summary>
	public class BulkSqlInsertException : Exception
	{
		public BulkSqlInsertException(string message, Exception innerException = null)
			: base(message, innerException)
		{
		}
	}
}
