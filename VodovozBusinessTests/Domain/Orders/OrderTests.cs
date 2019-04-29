using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.Orders;
using Vodovoz.Repository;

namespace VodovozBusinessTests.Domain.Orders
{
	[TestFixture()]
	public class OrderTests
	{
		[TearDown]
		public void RemoveStaticGaps()
		{
			PromotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPointTestGap = null;
			OrganizationRepository.GetOrganizationByPaymentTypeTestGap = null;
		}

		#region Рекламные наборы

		[Ignore("Непонятно как. Пока игнор.")]
		[Test(Description = "Удаление из списка промо-наборов заказа наборов, которые перестали присутствовать среди позиций заказа")]
		public void ClearPromotionSetsList_WhenNoAnyOrderItemsFromPromotionalSet_RemovesThisPromotionalSetFromList()
		{
			// arrange
			PromotionalSet promotionalSetMockNotExisting = Substitute.For<PromotionalSet>();
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock1 = Substitute.For<OrderItem>();
			orderItemMock1.Discount.Returns(100);
			orderItemMock1.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				UoW = Substitute.For<IUnitOfWork>(),
				Client = Substitute.For<Counterparty>(),
				DeliveryPoint = Substitute.For<DeliveryPoint>(),

				OrderItems = new List<OrderItem> {
					orderItemMock0,
					orderItemMock1,
					orderItemMock2,
					orderItemMock3
				},

				PromotionalSets = new List<PromotionalSet> {
					promotionalSetMockExisting,
					promotionalSetMockNotExisting
				}
			};

			// act
			//order.ClearPromotionSets();//если private переделать на public успех
			//order.SaveEntity();//???

			// assert
			Assert.That(orderUnderTest.PromotionalSets.Count, Is.EqualTo(1));
			Assert.That(Equals(orderUnderTest.PromotionalSets.FirstOrDefault(), promotionalSetMockExisting));
		}

		[Test(Description = "Передаваемая в метод одна из трёх строк заказа с одинаковой ссылкой на промо-набор, не вызывает удаления промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfThreeOrderItemsWithSamePromotionalSet_DoesNotRemoveAnyPromoSetsFromPromoSetsListAndFromOrderItemsIfFound()
		{
			// arrange
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock1 = Substitute.For<OrderItem>();
			orderItemMock1.Discount.Returns(100);
			orderItemMock1.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> {
					orderItemMock0,
					orderItemMock2,
					orderItemMock3
				},

				PromotionalSets = new List<PromotionalSet> { promotionalSetMockExisting }
			};

			// act
			orderUnderTest.TryToRemovePromotionalSet(orderItemMock1);

			// assert
			Assert.That(orderUnderTest.PromotionalSets.Count, Is.EqualTo(1));
		}

		[Test(Description = "Передаваемая в метод строка заказа без ссылки на промо-набор, не вызывает удаления промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOrderItemWithNoReferenceToPromotionalSet_DoesNotRemoveAnyPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock1 = Substitute.For<OrderItem>();
			orderItemMock1.Discount.Returns(100);
			orderItemMock1.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> {
					orderItemMock1,
					orderItemMock2,
					orderItemMock3
				},

				PromotionalSets = new List<PromotionalSet> { promotionalSetMockExisting }
			};

			// act
			orderUnderTest.TryToRemovePromotionalSet(orderItemMock0);

			// assert
			Assert.That(Equals(orderUnderTest.PromotionalSets.FirstOrDefault(), promotionalSetMockExisting));
		}

		[Test(Description = "Передаваемая в метод одна из двух строк заказа у которых одинаковые ссылки на промо-наборы, но у передаваемой скидка 0, не вызывает удаления промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfTwoOrderItemsAndBothHaveSamePromotionalSetButPassingOrderItemHasNoDiscount_DoesNotRemoveAnyPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> {
					orderItemMock0,
					orderItemMock2
				},

				PromotionalSets = new List<PromotionalSet> { promotionalSetMockExisting }
			};

			// act
			orderUnderTest.TryToRemovePromotionalSet(orderItemMock3);

			// assert
			Assert.That(Equals(orderUnderTest.PromotionalSets.FirstOrDefault(), promotionalSetMockExisting));
		}

		[Test(Description = "Передаваемая в метод одна из двух строк заказа с у которых одинаковые ссылки на промо-наборы, но у передаваемой есть скидка, вызывает удаление этого промонаборов из заказа.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfTwoOrderItemsAndBothHaveSamePromotionalSetButPassingOrderItemHasAnyDiscount_RemovesThisPromoSetsFromPromoSetsListIfFound()
		{
			// arrange
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> {
					orderItemMock0,
					orderItemMock3
				},

				PromotionalSets = new List<PromotionalSet> { promotionalSetMockExisting }
			};

			// act
			orderUnderTest.TryToRemovePromotionalSet(orderItemMock2);

			// assert
			Assert.That(orderUnderTest.PromotionalSets.Count, Is.EqualTo(0));
		}

		[Test(Description = "Передаваемая в метод одна из двух строк заказа с у которых одинаковые ссылки на промо-наборы, но у передаваемой есть скидка, вызывает удаление ссылки на промо-набор у строки заказа со скидкой = 0.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfTwoOrderItemsAndBothHaveSamePromotionalSetButPassingOrderItemHasAnyDiscount_ClearsReferenceToPromoSetInExistingOrderItemIfFound()
		{
			// arrange
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> { orderItemMock0, orderItemMock3 },
				PromotionalSets = new List<PromotionalSet> { promotionalSetMockExisting }
			};

			// act
			orderUnderTest.TryToRemovePromotionalSet(orderItemMock2);

			// assert
			Assert.That(orderUnderTest.OrderItems.LastOrDefault().PromoSet, Is.Null);
		}

		[Test(Description = "Передаваемая в метод одна из двух строк заказа у которых одинаковые ссылки на промо-наборы, но у передаваемой нет скидки, не вызывает удаления ссылки на промо-набор у строки заказа со скидкой не 0.")]
		public void TryToRemovePromotionalSet_WhenPassOneOfTwoOrderItemsAndBothHaveSamePromotionalSetButPassingOrderItemDoesNotHaveAnyDiscount_DoesNotClearReferenceToPromoSetInExistingOrderItemIfFound()
		{
			// arrange
			PromotionalSet promotionalSetMockExisting = Substitute.For<PromotionalSet>();

			OrderItem orderItemMock0 = Substitute.For<OrderItem>();
			orderItemMock0.Discount.Returns(0);
			orderItemMock0.PromoSet.ReturnsNull();

			OrderItem orderItemMock2 = Substitute.For<OrderItem>();
			orderItemMock2.Discount.Returns(100);
			orderItemMock2.PromoSet.Returns(promotionalSetMockExisting);

			OrderItem orderItemMock3 = Substitute.For<OrderItem>();
			orderItemMock3.Discount.Returns(0);
			orderItemMock3.PromoSet.Returns(promotionalSetMockExisting);

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> { orderItemMock0, orderItemMock2 },
				PromotionalSets = new List<PromotionalSet> { promotionalSetMockExisting }
			};

			// act
			orderUnderTest.TryToRemovePromotionalSet(orderItemMock3);

			// assert
			Assert.That(orderUnderTest.OrderItems.LastOrDefault().PromoSet, Is.Not.Null);
		}

		[Test(Description = "При добавлении промо-набора в заказ, адрес доставки которого не найден среди других заказов с промонаборами, возвращается true и нет сообщения")]
		public void CanAddPromotionalSet_WhenAddPromotionalSetToTheOrderAndNoSameAddressFoundInAnotherOrdersWithPromoSets_ReturnsTrueAndNoMessage()
		{
			// arrange
			Order orderUnderTest = new Order();
			PromotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPointTestGap = (uow, o, ignore) => new Dictionary<int, int[]>();

			// act
			var res = orderUnderTest.CanAddPromotionalSet(Substitute.For<PromotionalSet>(), out string mess);

			// assert
			Assert.That(res, Is.True);
			Assert.That(mess, Is.Empty);
		}

		[Test(Description = "При добавлении промо-набора в заказ, адрес доставки которого найден среди других заказов с промонаборами, возвращается false и сообщение")]
		public void CanAddPromotionalSet_WhenAddPromotionalSetToTheOrderAndSameAddressFoundInAnotherOrdersWithPromoSets_ReturnsFalseAndMessage()
		{
			// arrange
			Order orderUnderTest = new Order {
				UoW = Substitute.For<IUnitOfWork>(),
				Client = Substitute.For<Counterparty>(),
				DeliveryPoint = Substitute.For<DeliveryPoint>()
			};
			PromotionalSetRepository.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPointTestGap = (uow, o, ignore) => new Dictionary<int, int[]> { { 1, new[] { 1, 2 } } };

			// act
			var res = orderUnderTest.CanAddPromotionalSet(Substitute.For<PromotionalSet>(), out string mess);

			// assert
			Assert.That(res, Is.False);
			Assert.That(mess, Is.Not.Empty);
		}
		#endregion Рекламные наборы
	}
}