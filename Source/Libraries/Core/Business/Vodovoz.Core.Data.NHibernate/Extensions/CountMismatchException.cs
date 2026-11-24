using System;

namespace Vodovoz.Core.Data.NHibernate.Extensions
{
	/// <summary>
	/// Расхождение количества. Наружу выходит только как <see cref="BulkSqlInsertException"/> без вложенного исключения
	/// </summary>
	internal class CountMismatchException : Exception
	{
		public CountMismatchException(string message) : base(message)
		{
		}
	}
}
