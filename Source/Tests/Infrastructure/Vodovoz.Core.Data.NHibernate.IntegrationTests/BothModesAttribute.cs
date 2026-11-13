using System.Collections.Generic;
using System.Reflection;
using Vodovoz.Core.Data.NHibernate.Extensions;
using Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain;
using Xunit.Sdk;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Запускает тест для обоих методов: BulkSqlInsert и BulkSqlInsertAsync.
	/// </summary>
	public sealed class BothModesAttribute : DataAttribute
	{
		public override IEnumerable<object[]> GetData(MethodInfo testMethod)
		{
			yield return new object[] { InsertMode.Sync };
			yield return new object[] { InsertMode.Async };
		}
	}
}
