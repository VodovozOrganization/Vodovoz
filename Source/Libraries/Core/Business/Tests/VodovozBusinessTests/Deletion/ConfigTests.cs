using System.Collections;
using NHibernate.Mapping;
using NUnit.Framework;
using QS.Deletion;
using QS.Deletion.Configuration;
using QS.Deletion.Testing;
using Vodovoz.Core.Domain.Warehouses;

namespace VodovozBusinessTests.Deletion
{
	[TestFixture]
	[Category("Конфигурация удаления")]
	public class ConfigTests : DeleteConfigTestBase
	{
		static ConfigTests()
		{
			ConfigureOneTime.ConfigureNh();
			ConfigureOneTime.ConfigureDeletion();

			AddIgnoredClass(typeof(QS.Project.Domain.UserBase), "Этот класс в общей библиотеке и пока никак не используется для удаления.");
			AddIgnoredClass(typeof(Vodovoz.Domain.Logistic.TrackPoint), "Удалятся вместе треком засчет конфигурации базы. Показывать пользователю все удаляемые точки смысла нет.");
			AddIgnoredClass(typeof(Vodovoz.Subdivision), "Один из основных классов программы. Удалятся не должен.");
			AddIgnoredClass(typeof(Vodovoz.Domain.Orders.Order), "Один из основных классов программы. Удалятся не должен.");
			AddIgnoredClass(typeof(Vodovoz.Domain.Employees.Employee), "Один из основных классов программы. Удалятся не должен.");
			AddIgnoredClass(typeof(Vodovoz.Domain.Client.Counterparty), "Один из основных классов программы. Удалятся не должен.");
			AddIgnoredClass(typeof(Vodovoz.Domain.Client.DeliveryPoint), "Один из основных классов программы. Удалятся не должен.");
			AddIgnoredClass(typeof(Warehouse), "Один из основных классов программы. Удалятся не должен.");

			//Технические классы пока нигде не удаляется через удаление.
			AddIgnoredClass(typeof(Vodovoz.Domain.Logistic.CachedDistance), "Технический клас пока нигде не удаляется через удаление.");
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
		public override void DeleteRuleExistForNhMappedClassTest(NHibernate.Mapping.PersistentClass mapping)
		{
			base.DeleteRuleExistForNhMappedClassTest(mapping);
		}

		public new static IEnumerable NhibernateMappedEntityRelation => DeleteConfigTestBase.NhibernateMappedEntityRelation;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelation))]
		public override void DeleteRuleExistForNhMappedEntityRelationTest(PersistentClass mapping, Property property)
		{
			base.DeleteRuleExistForNhMappedEntityRelationTest(mapping, property);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRule => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRule;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRule))]
		public override void DependenceRuleExistForNhMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.DependenceRuleExistForNhMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedEntityRelationWithExistRuleCascadeRelated => DeleteConfigTestBase.NhibernateMappedEntityRelationWithExistRuleCascadeRelated;

		[Test, TestCaseSource(nameof(NhibernateMappedEntityRelationWithExistRuleCascadeRelated))]
		public override void CascadeDependenceRuleExistForNhMappedEntityRelationTest(PersistentClass mapping, Property property, IDeleteRule related)
		{
			base.CascadeDependenceRuleExistForNhMappedEntityRelationTest(mapping, property, related);
		}

		public new static IEnumerable NhibernateMappedCollection => DeleteConfigTestBase.NhibernateMappedCollection;

		[Test, TestCaseSource(nameof(NhibernateMappedCollection))]
		public override void NHMappedCollectionsAllInOneTest(PersistentClass mapping, Property property)
		{
			base.NHMappedCollectionsAllInOneTest(mapping, property);
		}
	}
}
