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
			//	Скидка без учёта скидки по акции "Бутыль" расчитывается по формуле:
			//		originalExistingPercent =  100 * (existingPercent - DiscountByStock) / (100 - DiscountByStock),
			//		где existingPercent это уже имеющийся общий процент скидки, а DiscountByStock процент скидки по акции "Бутыль".
			//	Итоговоя скидка resultDiscount = originalExistingPercent + (100 - originalExistingPercent) / 100 * discountPercent.

			// originalExistingPercent = 100 * (100 - 0) / (100 - 0) = 100
			// resultDiscount = 100 + (100 - 100) / 100 * 20 = 100
			yield return new object[] { 1000m, 100m, 0m, 20m, 100m };

			// originalExistingPercent = 100 * (60 - 10) / (100 - 10) = 55.555
			// resultDiscount = 55.555 + (100 - 55.555) / 100 * 20 = 64.44
			yield return new object[] { 600m, 60m, 10m, 20m, 64.44m };

			// originalExistingPercent = 100 * (80 - 0) / (100 - 0) = 80
			// resultDiscount = 80 + (100 - 80) / 100 * 20 = 84
			yield return new object[] { 800m, 80m, 0m, 20m, 84m };

			// originalExistingPercent = 100 * (85 - 0) / (100 - 0) = 85
			// resultDiscount = 85 + (100 - 85) / 100 * 20 = 88
			yield return new object[] { 850m, 85m, 0m, 20m, 88m };

			// originalExistingPercent = 100 * (95 - 10) / (100 - 10) = 94.444
			// resultDiscount = 94.444 + (100 - 94.444) / 100 * 20 = 95.56
			yield return new object[] { 950m, 95m, 10m, 20m, 95.56m };

			// originalExistingPercent = 100 * (0 - 0) / (100 - 0) = 0
			// resultDiscount = 0 + (100 - 0) / 100 * 20 = 20
			yield return new object[] { 0m, 0m, 0m, 20m, 20m };

			// originalExistingPercent = 100 * (50.31 - 0) / (100 - 0) = 50,31
			// resultDiscount = 50,31 + (100 - 50,31) / 100 * 11.67 = 56.11
			yield return new object[] { 503.1m, 50.31m, 0m, 11.67m, 56.11m };
		}

		[Test(Description = "Проверка добавления скидки в процентах, для товара на сумму 1000, в котором скидка установлена деньгами")]
		[TestCaseSource(nameof(DiscountByStockTestSource))]
		public void SetDiscountByStockTest_Where_DiscountInMoney_is_true(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSaleWithDiscount(
				order,
				nomenclature,
				20,
				50,
				true,
				discountMoney,
				discountReason,
				null);

			//testedOrderItem.Price = 50;
			//testedOrderItem.Count = 20;
			//testedOrderItem.DiscountByStock = existingDiscountByStock;
			//testedOrderItem.Discount = discountPercent;
			//testedOrderItem.DiscountMoney = discountMoney;
			//testedOrderItem.IsDiscountInMoney = true;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);

			// assert
			Assert.That(decimal.Round(testedOrderItem.Discount,2), Is.EqualTo(result));
		}

		[Test(Description = "Проверка добавления скидки в процентах, для товара на сумму 1000, в котором скидка установлена процентами")]
		[TestCaseSource(nameof(DiscountByStockTestSource))]
		public void SetDiscountByStockTest_Where_DiscountInMoney_is_false(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSaleWithDiscount(
				order,
				nomenclature,
				20,
				50,
				false,
				discountPercent,
				discountReason,
				null);

			//testedOrderItem.Price = 50;
			//testedOrderItem.Count = 20;
			//testedOrderItem.DiscountByStock = existingDiscountByStock;
			//testedOrderItem.Discount = discountPercent;
			//testedOrderItem.DiscountMoney = discountMoney;
			//testedOrderItem.IsDiscountInMoney = false;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);

			// assert
			Assert.That(decimal.Round(testedOrderItem.Discount, 2), Is.EqualTo(result));
		}

		private static IEnumerable DiscountByStockForAddAndDeleteTestSource()
		{
			//	Скидка без учёта скидки по акции "Бутыль" расчитывается по формуле:
			//		originalExistingPercent =  100 * (existingPercent - DiscountByStock) / (100 - DiscountByStock),
			//		где existingPercent это уже имеющийся общий процент скидки, а DiscountByStock процент скидки по акции "Бутыль".

			// originalExistingPercent = 100 * (100 - 0) / (100 - 0) = 100
			yield return new object[] { 1000m, 100m, 0m, 20m, 100m };

			// originalExistingPercent = 100 * (100 - 20) / (100 - 20) = 100
			yield return new object[] { 1000m, 100m, 20m, 20m, 100m };

			// originalExistingPercent = 100 * (100 - 20) / (100 - 20) = 100
			yield return new object[] { 1000m, 100m, 20m, 10m, 100m };

			// originalExistingPercent = 100 * (50 - 0) / (100 - 0) = 50
			yield return new object[] { 500m, 50m, 0m, 20m, 50m };

			// originalExistingPercent = 100 * (70 - 20) / (100 - 20) = 62.5
			yield return new object[] { 700m, 70m, 20m, 20m, 62.5m };

			// originalExistingPercent = 100 * (90 - 0) / (100 - 0) = 90
			yield return new object[] { 900m, 90m, 0m, 20m, 90m };

			// originalExistingPercent = 100 * (95 - 20) / (100 - 20) = 93.75
			yield return new object[] { 950m, 95m, 20m, 20m, 93.75m };

			// originalExistingPercent = 100 * (95 - 10) / (100 - 10) = 94.44
			yield return new object[] { 950m, 95m, 10m, 20m, 94.44m };

			// originalExistingPercent = 100 * (0 - 0) / (100 - 0) = 0
			yield return new object[] { 0m, 0m, 0m, 20m, 0m };
		}

		[Test(Description = "Если добавить и потом удалить скидку по акции, сумма товара должна вернутся на первоначальный уровень. Скидка в процентах")]
		[TestCaseSource(nameof(DiscountByStockForAddAndDeleteTestSource))]
		public void SetDiscountByStockTest_SetDiscount_and_DeleteDiscount_Where_DiscountInMoney_is_false(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSaleWithDiscount(
				order,
				nomenclature,
				20,
				50,
				false,
				discountPercent,
				discountReason,
				null);

			//testedOrderItem.Price = 50;
			//testedOrderItem.Count = 20;
			//testedOrderItem.DiscountByStock = existingDiscountByStock;
			//testedOrderItem.Discount = discountPercent;
			//testedOrderItem.DiscountMoney = discountMoney;
			//testedOrderItem.IsDiscountInMoney = false;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);
			testedOrderItem.SetDiscountByStock(discountReason, 0);

			// assert
			Assert.That(decimal.Round(testedOrderItem.Discount, 2), Is.EqualTo(result));
		}

		[Test(Description = "Если добавить и потом удалить скидку по акции, сумма товара должна вернутся на первоначальный уровень. Скидка в деньгах")]
		[TestCaseSource(nameof(DiscountByStockForAddAndDeleteTestSource))]
		public void SetDiscountByStockTest_SetDiscount_and_DeleteDiscount_Where_DiscountInMoney_is_true(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSaleWithDiscount(
				order,
				nomenclature,
				20,
				50,
				true,
				discountMoney,
				discountReason,
				null);

			//testedOrderItem.Price = 50;
			//testedOrderItem.Count = 20;
			//testedOrderItem.DiscountByStock = existingDiscountByStock;
			//testedOrderItem.Discount = discountPercent;
			//testedOrderItem.DiscountMoney = discountMoney;
			//testedOrderItem.IsDiscountInMoney = true;

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);
			testedOrderItem.SetDiscountByStock(discountReason, 0);

			// assert
			Assert.That(decimal.Round(testedOrderItem.Discount,2), Is.EqualTo(result));
		}

		[Test(Description = "При установке актуального кол-ва в 0 НДС и сумма становятся 0")]
		public void PropActualCount_WhenSetTo0_NdsAndCurrentSumAreAlso0()
		{
			// arrange
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				1,
				100);

			testedOrderItem.SetManualChangingDiscount(10);

			//OrderItem orderItem = new OrderItem {
			//	Order = orderMock,
			//	Count = 1,
			//	Price = 100,
			//	ManualChangingDiscount = 10
			//};

			// act
			testedOrderItem.SetActualCount(0);

			// assert
			Assert.That(testedOrderItem.ActualSum, Is.EqualTo(0));
			Assert.That(testedOrderItem.IncludeNDS, Is.EqualTo(null));
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
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				1,
				100);

			testedOrderItem.SetIsDiscountInMoney(false);

			//OrderItem orderItem = new OrderItem {
			//	Count = 1,
			//	Price = 100,
			//	IsDiscountInMoney = false
			//};

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(result));
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
			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				2,
				5000);

			testedOrderItem.SetIsDiscountInMoney(true);

			//OrderItem orderItem = new OrderItem {
			//	Count = 2,
			//	Price = 5000,
			//	IsDiscountInMoney = true
			//};

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.DiscountMoney, Is.EqualTo(result));
		}


		[Test(Description = "Проверка установки скидки в деньгах если не было указано количество")]
		public void ManualChangingDiscountInMoney_WhenCountIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				0,
				5000);

			testedOrderItem.SetIsDiscountInMoney(true);

			//OrderItem orderItem = new OrderItem {
			//	Count = 0,
			//	Price = 5000,
			//	IsDiscountInMoney = true
			//};

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.DiscountMoney, Is.EqualTo(0));
		}

		[Test(Description = "Проверка установки скидки в процентах если не была указана цена")]
		public void ManualChangingDiscountInPercent_WhenPriceIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				0,
				5000);

			//OrderItem orderItem = new OrderItem {
			//	Count = 0,
			//	Price = 5000
			//};

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(0));
		}

		[Test(Description = "Проверка установки скидки в деньгах если не была указана цена")]
		public void ManualChangingDiscountInMoney_WhenPriceIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				30,
				0);

			testedOrderItem.SetIsDiscountInMoney(true);

			//OrderItem orderItem = new OrderItem {
			//	Count = 30,
			//	Price = 0,
			//	IsDiscountInMoney = true
			//};

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.DiscountMoney, Is.EqualTo(0));
		}

		[Test(Description = "Проверка установки скидки в процентах если не было указано количество")]
		public void ManualChangingDiscountInPercent_WhenCountIsZero_ThenResultDiscountSetToZero()
		{
			// arrange
			decimal discount = 200;

			Nomenclature nomenclature = Substitute.For<Nomenclature>();
			Order order = Substitute.For<Order>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				30,
				0);

			//OrderItem orderItem = new OrderItem {
			//	Count = 30,
			//	Price = 0
			//};

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(0));
		}
	}
}
