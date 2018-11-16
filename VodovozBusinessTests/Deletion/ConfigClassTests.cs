using System.Collections;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Deletion;
using QS.Deletion.Testing;

namespace VodovozBusinessTests.Deletion
{
	[TestFixture]
	[Category("Конфигурация удаления")]
	public class ConfigClassTests : DeleteConfigTestBase
	{
		static ConfigClassTests()
		{
			ConfigureOneTime.ConfigureNh();
			ConfigureOneTime.ConfogureDeletion();

			//Так как этот класс в общей библиотеке и пока никак не используется для удаления.
			IgnoreMissingClass.Add(typeof(QS.Project.Domain.UserBase));
			//Технические классы пока нигде не удаляется через удаление.
			IgnoreMissingClass.Add(typeof(Vodovoz.Domain.BaseParameter));
			IgnoreMissingClass.Add(typeof(Vodovoz.Domain.Logistic.CachedDistance));

			//Так как удалятся вместе треком засчет конфигурации базы. Показывать пользователю все удаляемые точки смысла нет.
			IgnoreMissingClass.Add(typeof(Vodovoz.Domain.Logistic.TrackPoint));
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

		public new static IEnumerable NhibernateMappedClasses => DeleteConfigTestBase.NhibernateMappedClasses;

		[Test, TestCaseSource(nameof(NhibernateMappedClasses))]
		public override void DeleteRuleExisitForNHMappedClasssTest(NHibernate.Mapping.PersistentClass mapping)
		{
			base.DeleteRuleExisitForNHMappedClasssTest(mapping);
		}

		public new static IEnumerable NhibernateMappedEntityRelation => DeleteConfigTestBase.NhibernateMappedEntityRelation;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelation))]
		public override void DeleteRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property property)
		{
			base.DeleteRuleExisitForNHMappedEntityRelationTest(mapping, property);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRule => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRule;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRule))]
		public override void DependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.DependenceRuleExisitForNHMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRuleCascadeRelated => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRuleCascadeRelated;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRuleCascadeRelated))]
		public override void CascadeDependenceRuleExisitForNHMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.CascadeDependenceRuleExisitForNHMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedCollection => DeleteConfigTestBase.NhibernateMappedCollection;

		[Test, TestCaseSource(nameof(NhibernateMappedCollection))]
		public override void NHMappedCollectionsAllInOneTest(PersistentClass mapping, Property property)
		{
			base.NHMappedCollectionsAllInOneTest(mapping, property);
		}
	}
}
