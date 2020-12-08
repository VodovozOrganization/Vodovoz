using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusinessTests.Domain.Orders
{
	[TestFixture()]
	public class OrderItemTests
	{
		private static IEnumerable DiscountByStockTestSource()
		{
			yield return new object[] { 1000m, 100m, 0m, 20m, 100m };
			yield return new object[] { 600m, 60m, 10m, 20m, 70m };
			yield return new object[] { 800m, 80m, 0m, 20m, 100m };
			yield return new object[] { 850m, 85m, 0m, 20m, 100m };
			yield return new object[] { 950m, 95m, 10m, 20m, 100m };
			yield return new object[] { 0m, 0m, 0m, 20m, 20m };
			yield return new object[] { 503.1m, 50.31m, 0m, 11.67m, 61.98m };
		}

		[Test(Description = "Проверка добавления скидки в процентах, для товара на сумму 1000, в котором скидка установлена деньгами")]
		[TestCaseSource(nameof(DiscountByStockTestSource))]
		public void SetDiscountByStockTest_Where_DiscountInMoney_is_true(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			OrderItem testedOrderItem = new OrderItem();

			testedOrderItem.Price = 50;
			testedOrderItem.Count = 20;
			testedOrderItem.DiscountByStock = existingDiscountByStock;
			testedOrderItem.Discount = discountPercent;
			testedOrderItem.DiscountMoney = discountMoney;
			testedOrderItem.IsDiscountInMoney = true;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(result));
		}

		[Test(Description = "Проверка добавления скидки в процентах, для товара на сумму 1000, в котором скидка установлена процентами")]
		[TestCaseSource(nameof(DiscountByStockTestSource))]
		public void SetDiscountByStockTest_Where_DiscountInMoney_is_false(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			OrderItem testedOrderItem = new OrderItem();

			testedOrderItem.Price = 50;
			testedOrderItem.Count = 20;
			testedOrderItem.DiscountByStock = existingDiscountByStock;
			testedOrderItem.Discount = discountPercent;
			testedOrderItem.DiscountMoney = discountMoney;
			testedOrderItem.IsDiscountInMoney = false;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(result));
		}

		private static IEnumerable DiscountByStockForAddAndDeleteTestSource()
		{
			yield return new object[] { 1000m, 100m, 0m, 20m, 100m };
			yield return new object[] { 1000m, 100m, 20m, 20m, 80m };
			yield return new object[] { 1000m, 100m, 20m, 10m, 80m };
			yield return new object[] { 500m, 50m, 0m, 20m, 50m };
			yield return new object[] { 700m, 70m, 20m, 20m, 50m };
			yield return new object[] { 900m, 90m, 0m, 20m, 90m };
			yield return new object[] { 950m, 95m, 20m, 20m, 75m };
			yield return new object[] { 950m, 95m, 10m, 20m, 85m };
			yield return new object[] { 0m, 0m, 0m, 20m, 0m };
		}

		[Test(Description = "Если добавить и потом удалить скидку по акции, сумма товара должна вернутся на первоначальный уровень. Скидка в процентах")]
		[TestCaseSource(nameof(DiscountByStockForAddAndDeleteTestSource))]
		public void SetDiscountByStockTest_SetDiscount_and_DeleteDiscount_Where_DiscountInMoney_is_false(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			OrderItem testedOrderItem = new OrderItem();

			testedOrderItem.Price = 50;
			testedOrderItem.Count = 20;
			testedOrderItem.DiscountByStock = existingDiscountByStock;
			testedOrderItem.Discount = discountPercent;
			testedOrderItem.DiscountMoney = discountMoney;
			testedOrderItem.IsDiscountInMoney = false;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);
			testedOrderItem.SetDiscountByStock(discountReason, 0);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(result));
		}

		[Test(Description = "Если добавить и потом удалить скидку по акции, сумма товара должна вернутся на первоначальный уровень. Скидка в деньгах")]
		[TestCaseSource(nameof(DiscountByStockForAddAndDeleteTestSource))]
		public void SetDiscountByStockTest_SetDiscount_and_DeleteDiscount_Where_DiscountInMoney_is_true(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			OrderItem testedOrderItem = new OrderItem();

			testedOrderItem.Price = 50;
			testedOrderItem.Count = 20;
			testedOrderItem.DiscountByStock = existingDiscountByStock;
			testedOrderItem.Discount = discountPercent;
			testedOrderItem.DiscountMoney = discountMoney;
			testedOrderItem.IsDiscountInMoney = true;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);
			testedOrderItem.SetDiscountByStock(discountReason, 0);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(result));
		}

		[Test(Description = "При установке актуального кол-ва в 0 НДС и сумма становятся 0")]
		public void PropActualCount_WhenSetTo0_NdsAndCurrentSumAreAlso0()
		{
			// arrange
			Order orderMock = Substitute.For<Order>();
			OrderItem orderItem = new OrderItem {
				Order = orderMock,
				Count = 1,
				Price = 100,
				ManualChangingDiscount = 10
			};

			// act
			orderItem.ActualCount = 0;

			// assert
			Assert.That(orderItem.ActualSum, Is.EqualTo(0));
			Assert.That(orderItem.IncludeNDS, Is.EqualTo(0));
		}

		static IEnumerable PercentDiscountValues()
		{
			yield return new object[] { -10m, 0m };
			yield return new object[] { 0m, 0m };
			yield return new object[] { 10m, 10m };
			yield return new object[] { 110m, 100m };
		}
		[Test(Description = "Проверка установки скидки в процентах")]
		[TestCaseSource(nameof(PercentDiscountValues))]
		public void ManualChangingDiscount_WhenSetPercentDiscount_ResultDiscountInRange0And100(decimal discount, decimal result)
		{
			// arrange
			OrderItem orderItem = new OrderItem {
				Count = 1,
				Price = 100,
				IsDiscountInMoney = false
			};

			// act
			orderItem.ManualChangingDiscount = discount;

			// assert
			Assert.That(orderItem.Discount, Is.EqualTo(result));
		}

		static IEnumerable MoneyDiscountValues()
		{
			yield return new object[] { -110m, 0m };
			yield return new object[] { 0m, 0m };
			yield return new object[] { 200m, 200m };
			yield return new object[] { 11110m, 2*5000m };
		}
		[Test(Description = "Проверка установки скидки в реальных деньгах")]
		[TestCaseSource(nameof(MoneyDiscountValues))]
		public void ManualChangingDiscount_WhenSetMoneyDiscount_ThenResultMoneyDiscountInRangeOf0AndMaxOrderSum(decimal discount, decimal result)
		{
			// arrange
			OrderItem orderItem = new OrderItem {
				Count = 2,
				Price = 5000,
				IsDiscountInMoney = true
			};

			// act
			orderItem.ManualChangingDiscount = discount;

			// assert
			Assert.That(orderItem.DiscountMoney, Is.EqualTo(result));
		}


		[Test(Description = "Проверка установки скидки в деньгах если не было указано количество")]
		public void ManualChangingDiscountInMoney_WhenCountIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			OrderItem orderItem = new OrderItem {
				Count = 0,
				Price = 5000,
				IsDiscountInMoney = true
			};

			// act
			orderItem.ManualChangingDiscount = discount;

			// assert
			Assert.That(orderItem.DiscountMoney, Is.EqualTo(0));
		}

		[Test(Description = "Проверка установки скидки в процентах если не была указана цена")]
		public void ManualChangingDiscountInPercent_WhenPriceIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			OrderItem orderItem = new OrderItem {
				Count = 0,
				Price = 5000
			};

			// act
			orderItem.ManualChangingDiscount = discount;

			// assert
			Assert.That(orderItem.Discount, Is.EqualTo(0));
		}

		[Test(Description = "Проверка установки скидки в деньгах если не была указана цена")]
		public void ManualChangingDiscountInMoney_WhenPriceIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			OrderItem orderItem = new OrderItem {
				Count = 30,
				Price = 0,
				IsDiscountInMoney = true
			};

			// act
			orderItem.ManualChangingDiscount = discount;

			// assert
			Assert.That(orderItem.DiscountMoney, Is.EqualTo(0));
		}

		[Test(Description = "Проверка установки скидки в процентах если не было указано количество")]
		public void ManualChangingDiscountInPercent_WhenCountIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			OrderItem orderItem = new OrderItem {
				Count = 30,
				Price = 0
			};

			// act
			orderItem.ManualChangingDiscount = discount;

			// assert
			Assert.That(orderItem.Discount, Is.EqualTo(0));
		}
	}
}
