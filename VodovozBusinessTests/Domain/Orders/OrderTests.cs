using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace VodovozBusinessTests.Domain.Orders
{
	[TestFixture()]
	public class OrderTests
	{
		PromotionalSet promotionalSetMockExisting, promotionalSetMockNotExisting;

		Order CreateOrderWithPromoSetUnderTest()
		{
			promotionalSetMockNotExisting = Substitute.For<PromotionalSet>();
			promotionalSetMockExisting = Substitute.For<PromotionalSet>();
			promotionalSetMockExisting.Id = 909;

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Id.Returns(0);
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock1 = Substitute.For<OrderItem>();
			orderItemMock1.Id.Returns(1);
			orderItemMock1.Discount.Returns(100);
			orderItemMock1.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Id.Returns(2);
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Id.Returns(3);
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				Client = Substitute.For<Counterparty>(),

				OrderItems = new System.Collections.Generic.List<OrderItem> {
					orderItemMock0,
					orderItemMock1,
					orderItemMock2,
					orderItemMock3
				},

				PromotionalSets = new System.Collections.Generic.List<PromotionalSet> {
					promotionalSetMockExisting,
					promotionalSetMockNotExisting
				}
			};

			return orderUnderTest;
		}

		[Ignore("Непонятно как. Пока игнор.")]
		[Test(Description = "Удаление из списка промо-наборов заказа наборов, которые перестали присутствовать среди позиций заказа")]
		public void ClearPromotionSetsList_WhenNoAnyOrderItemsFromPromotionalSet_RemovesThisPromotionalSetFromList()
		{
			// arrange
			var order = CreateOrderWithPromoSetUnderTest();


			// act
			//order.ClearPromotionSets();//если private переделать на public успех
			//order.SaveEntity();//???

			// assert
			Assert.AreEqual(order.PromotionalSets.Count, 1);
			Assert.AreSame(order.PromotionalSets.FirstOrDefault(), promotionalSetMockExisting);
		}

		[Test(Description = "Передаваемая в метод одна из трёх строк заказа с одинаковой ссылкой на промо-набор, не вызывает удаления промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfThreeOrderItemsWithSamePromotionalSet_DoesNotRemoveAnyPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			var order = CreateOrderWithPromoSetUnderTest();

			// act
			var oi = order.OrderItems.FirstOrDefault(x => x.PromoSet == promotionalSetMockExisting && x.Discount > 0);
			order.OrderItems.Remove(oi);
			order.TryToRemovePromotionalSet(oi);

			// assert
			Assert.AreEqual(order.PromotionalSets.Count, 2);
			Assert.IsNotNull(order.OrderItems.LastOrDefault().PromoSet);
		}

		[Test(Description = "Передаваемая в метод строка заказа без ссылки на промо-набор, не вызывает удаления промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOrderItemWithNoReferenceToPromotionalSet_DoesNotRemoveAnyPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			var order = CreateOrderWithPromoSetUnderTest();
			var oi = order.OrderItems.FirstOrDefault(x => x.PromoSet == null);
			order.OrderItems.Remove(oi);

			// act
			order.TryToRemovePromotionalSet(oi);

			// assert
			Assert.AreSame(order.PromotionalSets[0], promotionalSetMockExisting);
			Assert.AreSame(order.PromotionalSets[1], promotionalSetMockNotExisting);
			Assert.IsNotNull(order.OrderItems.LastOrDefault().PromoSet);
		}

		[Test(Description = "Передаваемая в метод одна из двух строк заказа с у которых одинаковые ссылки на промо-наборы, но у передаваемой скидка 0, не вызывает удаления промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfTwoOrderItemsAndBothHaveSamePromotionalSetButPassingOrderItemHasNoDiscount_DoesNotRemoveAnyPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			var order = CreateOrderWithPromoSetUnderTest();
			order.OrderItems.RemoveAt(1);

			// act
			var oi = order.OrderItems.FirstOrDefault(x => x.PromoSet == promotionalSetMockExisting && x.Discount == 0);
			order.OrderItems.Remove(oi);
			order.TryToRemovePromotionalSet(oi);

			// assert
			Assert.AreSame(order.PromotionalSets[0], promotionalSetMockExisting);
			Assert.AreSame(order.PromotionalSets[1], promotionalSetMockNotExisting);
			Assert.IsNotNull(order.OrderItems.LastOrDefault().PromoSet);
		}

		[Test(Description = "Передаваемая в метод одна из двух строк заказа с у которых одинаковые ссылки на промо-наборы, но у передаваемой есть скидка, вызывает удаление этого промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfTwoOrderItemsAndBothHaveSamePromotionalSetButPassingOrderItemHasAnyDiscount_RemovesThisPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			var order = CreateOrderWithPromoSetUnderTest();
			order.OrderItems.RemoveAt(1);

			// act
			var oi = order.OrderItems.FirstOrDefault(x => x.PromoSet == promotionalSetMockExisting && x.Discount != 0);
			order.OrderItems.Remove(oi);
			order.TryToRemovePromotionalSet(oi);

			// assert
			Assert.AreEqual(order.PromotionalSets.Count, 1);
			Assert.AreSame(order.PromotionalSets[0], promotionalSetMockNotExisting);
			Assert.IsNull(order.OrderItems.LastOrDefault().PromoSet);
		}
	}
}