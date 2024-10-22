using NUnit.Framework;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
namespace VodovozBusinessTests.Domain.Goods
{
	[TestFixture]
	public class NomenclatureTests
	{
		[Test(Description = "Проверка, что при смене категории с 'вода в многооборотной таре' на любую другую категорию, объём тары должен выставлятся в Null")]
		public void NomenclatureInstantiating_IfChangeCategoryFromWaterToAnyOtherCategory_ThenTareVolumeShouldBeSetToNull()
		{
			// arrange
			Nomenclature nomenclatureUnderTest = new Nomenclature {
				Category = NomenclatureCategory.water,
				TareVolume = TareVolume.Vol19L
			};

			// act
			nomenclatureUnderTest.Category = NomenclatureCategory.deposit;

			// assert
			Assert.That(nomenclatureUnderTest.TareVolume.HasValue, Is.False);
		}
	}
}
