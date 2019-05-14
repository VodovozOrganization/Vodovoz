using NUnit.Framework;
using System;
using Vodovoz.Domain.Orders;
using System.Collections;
using NSubstitute;

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
	}
}
