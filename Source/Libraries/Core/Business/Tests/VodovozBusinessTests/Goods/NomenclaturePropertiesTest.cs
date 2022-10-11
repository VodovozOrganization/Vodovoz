using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using NUnit.Framework;
using Vodovoz.Domain.Goods;

namespace VodovozBusinessTests.Goods
{
	[TestFixture()]
	public class NomenclaturePropertiesTest
	{
		[Test(Description = "Все значения имеют атрибуты названия и GUID")]
		public void AllItemsHaveAttributeTest([Values] NomenclatureProperties item)
		{
			Assert.IsNotNull(item.GetAttribute<DisplayAttribute>(), $"{item} не имеет атрибута DisplayAttribute");
			Assert.IsNotNull(item.GetAttribute<OnlineStoreGuidAttribute>(), $"{item} не имеет атрибута OnlineStoreGuidAttribute");
			Assert.That(typeof(Nomenclature).GetProperty(item.ToString()), Is.Not.Null,
				"Для работы выгрузки в интернет магазин для каждого значения этого перечисления в Nomenclature должно имется свойство с таким же названием.");
		}
	}
}
