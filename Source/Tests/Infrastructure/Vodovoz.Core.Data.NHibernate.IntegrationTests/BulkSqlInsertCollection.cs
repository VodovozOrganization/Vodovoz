using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Тесты с БД идут последовательно: журнал SQL и глобальные настройки сервера общие.
	/// </summary>
	[CollectionDefinition(Name, DisableParallelization = true)]
	public class BulkSqlInsertCollection : ICollectionFixture<BulkSqlInsertFixture>
	{
		public const string Name = "BulkSqlInsert";
	}
}
