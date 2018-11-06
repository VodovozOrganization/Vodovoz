using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using NUnit.Framework;
using Vodovoz.Domain.Goods;

namespace VodovozBusinessTests.Goods
{
	[TestFixture()]
	public class NomenclaturePropertiesTest
	{
		[Test(Description = "Все значения имеют атрибуты названия и GUID")]
		public void AllItemsHaveAttributeTestCase()
		{
			foreach(var item in Enum.GetValues(typeof(NomenclatureProperties)).Cast<NomenclatureProperties>()) {
				Assert.IsNotNull(item.GetAttribute<DisplayAttribute>(), $"{item} не имеет атрибута DisplayAttribute");
				Assert.IsNotNull(item.GetAttribute<OnlineStoreGuidAttribute>(), $"{item} не имеет атрибута OnlineStoreGuidAttribute");
			}
		}
	}
}
