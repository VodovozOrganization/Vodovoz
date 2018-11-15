using System.Collections;
using NUnit.Framework;
using QS.Deletion;
using QS.Deletion.Testing;

namespace VodovozBusinessTests.Deletion
{
	[TestFixture]
	public class ConfigClassTests : DeleteConfigTestBase
	{
		static ConfigClassTests()
		{
			ConfigureOneTime.ConfigureNh();
			ConfigureOneTime.ConfogureDeletion();
		}

		public new static IEnumerable AllDeleteItems => DeleteConfigTestBase.AllDeleteItems;

		[Test, TestCaseSource(nameof(AllDeleteItems))]
		public override void DeleteItemsTypesTest(IDeleteRule info, DeleteDependenceInfo dependence)
		{
			base.DeleteItemsTypesTest(info, dependence);
		}

		public new static IEnumerable AllClearItems => DeleteConfigTestBase.AllClearItems;

		[Test, TestCaseSource(nameof(AllClearItems))]
		public override void ClearItemsTypesTest(IDeleteRule info, ClearDependenceInfo dependence)
		{
			base.ClearItemsTypesTest(info, dependence);
		}
	}
}
