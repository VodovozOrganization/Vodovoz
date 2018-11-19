using NSubstitute;
using NUnit.Framework;
using System;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;

namespace VodovozBusinessTests.Tools.Orders
{
	[TestFixture(TestOf = typeof(OrderStateKey))]
	public class OrderStateKeyTests
	{
		[Test()]
		public void CompareWithDeliveryPriceRuleTestCase()
		{
			var deliveryPriceMock = Substitute.For<IDeliveryPriceRule>();
			//требуемые условия
			deliveryPriceMock.Water19LCount.Returns(25);
			//коефициенты для пересчета в 19л бутыли
			deliveryPriceMock.EqualsCount6LFor19L.Returns(10);
			deliveryPriceMock.EqualsCount600mlFor19L.Returns(48);

			//проверка на меньшее кол-во бутылей (17) из требуемых 25. Должен вернуть false
			OrderStateKey testedStateKey1 = new OrderStateKey {
				Water19LCount = 10,
				DisposableWater19LCount = 5,
				DisposableWater6LCount = 10,
				DisposableWater600mlCount = 48
			};


			//проверка равное кол-во бутылей (25) из требуемых 25. Должен вернуть true
			OrderStateKey testedStateKey2 = new OrderStateKey {
				Water19LCount = 10,
				DisposableWater19LCount = 5,
				DisposableWater6LCount = 50,
				DisposableWater600mlCount = 240
			};

			//проверка на сумму из неполных частей бутылей (25,87) из 25. Должен вернуть true
			OrderStateKey testedStateKey3 = new OrderStateKey {
				Water19LCount = 20,
				DisposableWater19LCount = 4,
				DisposableWater6LCount = 9,
				DisposableWater600mlCount = 47
			};


			Assert.IsTrue(
				testedStateKey1.CompareWithDeliveryPriceRule(deliveryPriceMock) == false &&
				testedStateKey2.CompareWithDeliveryPriceRule(deliveryPriceMock) == true &&
				testedStateKey3.CompareWithDeliveryPriceRule(deliveryPriceMock) == true
				);
		}
	}
}
