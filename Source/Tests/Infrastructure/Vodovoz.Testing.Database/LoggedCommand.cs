using System.Data.Common;

namespace Vodovoz.Testing.Database
{
	/// <summary>
	/// Выполненная через NHibernate команда: текст, соединение и транзакция.
	/// </summary>
	public class LoggedCommand
	{
		public LoggedCommand(string text, DbConnection connection, DbTransaction transaction)
		{
			Text = text;
			Connection = connection;
			Transaction = transaction;
		}

		public string Text { get; }
		public DbConnection Connection { get; }
		public DbTransaction Transaction { get; }
	}
}
