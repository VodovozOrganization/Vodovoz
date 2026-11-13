using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Vodovoz.Testing.Database;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Общая база и схема из тестовых маппингов.
	/// </summary>
	public class BulkSqlInsertFixture : MariaDbFixture
	{
		protected override string DatabaseName => "bulk";

		protected override Assembly[] MappingAssemblies => new[] { typeof(BulkParentMap).Assembly };
	}
}
