using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Тесты истории идут последовательно: журнал SQL и текущий пользователь общие.
	/// </summary>
	[CollectionDefinition(Name, DisableParallelization = true)]
	public class BulkSqlInsertHistoryCollection : ICollectionFixture<BulkSqlInsertHistoryFixture>
	{
		public const string Name = "BulkSqlInsertHistory";
	}
}
