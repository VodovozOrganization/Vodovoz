using System.Collections;
using NSubstitute;
using NUnit.Framework;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

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

			yield return new object[] { 1000m, 100m, 0m, 20m, 100m };
			yield return new object[] { 600m, 60m, 10m, 20m, 68m };
			yield return new object[] { 800m, 80m, 0m, 20m, 84m };
			yield return new object[] { 850m, 85m, 0m, 20m, 88m };
			yield return new object[] { 950m, 95m, 10m, 20m, 96m };
			yield return new object[] { 0m, 0m, 0m, 20m, 20m };
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

			yield return new object[] { 1000m, 100m, 0m, 20m, 100m };
			yield return new object[] { 1000m, 100m, 20m, 20m, 100m };
			yield return new object[] { 1000m, 100m, 20m, 10m, 100m };
			yield return new object[] { 500m, 50m, 0m, 20m, 50m };
			yield return new object[] { 700m, 70m, 20m, 20m, 70m };
			yield return new object[] { 900m, 90m, 0m, 20m, 90m };
			yield return new object[] { 950m, 95m, 20m, 20m, 95m };
			yield return new object[] { 950m, 95m, 10m, 20m, 95m };
			yield return new object[] { 0m, 0m, 0m, 20m, 0m };
		}

		[Test(Description = "Если добавить и потом удалить скидку по акции, сумма товара должна вернутся на первоначальный уровень. Скидка в процентах")]
		[TestCaseSource(nameof(DiscountByStockForAddAndDeleteTestSource))]
		public void SetDiscountByStockTest_SetDiscount_and_DeleteDiscount_Where_DiscountInMoney_is_false(decimal discountMoney, decimal discountPercent, decimal existingDiscountByStock, decimal discountForAdd, decimal result)
		{
			// arrange
			DiscountReason discountReason = new DiscountReason();
			var order = new Order();

			discountReason.Value = discountPercent;
			discountReason.ValueType = DiscountUnits.percent;

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				new Nomenclature(),
				20,
				50);

			testedOrderItem.SetDiscount(discountPercent);

			testedOrderItem.SetDiscountByStock(discountReason, existingDiscountByStock);

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
			DiscountReason discountReason = new DiscountReason();
			var order = new Order();


			OrderItem testedOrderItem = OrderItem.CreateForSaleWithDiscount(
				order,
				new Nomenclature(),
				20,
				50,
				true,
				discountMoney,
				discountReason,
				null);

			testedOrderItem.SetDiscountByStock(discountReason, existingDiscountByStock);

			// act
			testedOrderItem.SetDiscountByStock(discountReason, discountForAdd);
			testedOrderItem.SetDiscountByStock(discountReason, 0m);

			// assert
			Assert.That(decimal.Round(testedOrderItem.Discount, 2), Is.EqualTo(result));
		}

		public static IEnumerable PreparePropActualCount_WhenSetTo0_NdsAndCurrentSumAreAlso0()
		{
			// Case1

			Order order1 = new Order();
			yield return new object[] { order1, 0m };

			// Case2

			Organization organization2 = new Organization();

			CounterpartyContract counterpartyContract2 = new CounterpartyContract()
			{
				Organization = organization2
			};

			Order order2 = new Order()
			{
				Contract = counterpartyContract2
			};

			yield return new object[] { order2, null };

			// Case3

			Organization organization3 = new Organization();

			CounterpartyContract counterpartyContract3 = new CounterpartyContract()
			{
				Organization = organization3
			};

			Order order3 = new Order()
			{
				Contract = counterpartyContract3
			};

			yield return new object[] { order3, 0m };
		}

		[TestCaseSource(nameof(PreparePropActualCount_WhenSetTo0_NdsAndCurrentSumAreAlso0))]
		[Test(Description = "При установке актуального кол-ва в 0 НДС и сумма становятся 0")]
		public void PropActualCount_WhenSetTo0_NdsAndCurrentSumAreAlso0(Order order, decimal? includeNdsExpected)
		{
			// arrange
			Nomenclature nomenclature = Substitute.For<Nomenclature>();

			OrderItem testedOrderItem = OrderItem.CreateForSale(
				order,
				nomenclature,
				1,
				100);

			testedOrderItem.SetManualChangingDiscount(10);

			// act
			testedOrderItem.SetActualCount(0);

			// assert
			Assert.That(testedOrderItem.ActualSum, Is.EqualTo(0m));
			Assert.That(testedOrderItem.IncludeNDS, Is.EqualTo(includeNdsExpected));
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

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.DiscountMoney, Is.EqualTo(0m));
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

			// act
			testedOrderItem.SetManualChangingDiscount(discount);

			// assert
			Assert.That(testedOrderItem.Discount, Is.EqualTo(0));
		}
	}
}
