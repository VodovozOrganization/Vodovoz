using System.Data.Common;

namespace TransactionalOutbox.Abstractions
{
	public interface IOutboxConnectionFactory
	{
		DbConnection CreateConnection();
	}
}
