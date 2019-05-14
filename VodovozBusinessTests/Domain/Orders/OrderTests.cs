using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Repositories.Orders;
using Vodovoz.Repository;
using Vodovoz.Domain.Goods;
using System.Collections;
using Vodovoz.Repository.Logistics;
using Vodovoz.Services;

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
			RouteListItemRepository.HasRouteListItemsForOrderTestGap = null;
		}

		#region OrderCreations

		private Order CreateTestOrderWithForfeitAndBottles()
		{
			Order testOrder = new Order {
				OrderItems = new List<OrderItem>(),
				DeliveryDate = DateTime.Now
			};

			Nomenclature forfeitNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Id.Returns(33);
			OrderItem forfeitOrderItem = new OrderItem {
				Nomenclature = forfeitNomenclature,
				Count = 3
			};
			testOrder.OrderItems.Add(forfeitOrderItem);

			Nomenclature emptyBottleNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Category.Returns(NomenclatureCategory.bottle);
			OrderItem emptyBottleOrderItem = new OrderItem {
				Nomenclature = emptyBottleNomenclature,
				Count = 5
			};
			testOrder.OrderItems.Add(emptyBottleOrderItem);

			Nomenclature waterNomenclature = new Nomenclature {
				Category = NomenclatureCategory.water,
				TareVolume = TareVolume.Vol19L,
				IsDisposableTare = false
			};
			OrderItem waterOrderItem = new OrderItem {
				Nomenclature = waterNomenclature,
				Count = 1
			};
			testOrder.OrderItems.Add(waterOrderItem);


			return testOrder;
		}

		private Order CreateTestOrderWithEquipmentRefundAndRecivedDeposit()
		{
			Order testOrder = new Order {
				OrderItems = new List<OrderItem>(),
				DeliveryDate = DateTime.Now,
				OrderDepositItems = new List<OrderDepositItem>(),
				DepositOperations = new List<DepositOperation>()
			};

			Nomenclature depositNomenclature = Substitute.For<Nomenclature>();
			depositNomenclature.TypeOfDepositCategory.Returns(TypeOfDepositCategory.EquipmentDeposit);
			OrderItem recivedDepositOrderItem = new OrderItem {
				Nomenclature = depositNomenclature,
				ActualCount = 2,
				Count = 3,
				Price = 150m,
			};
			testOrder.OrderItems.Add(recivedDepositOrderItem);

			OrderDepositItem refundOrderDepositItem = new OrderDepositItem {
				ActualCount = 3,
				Count = 4,
				Deposit = 500m,
				DepositType = DepositType.Equipment
			};
			testOrder.OrderDepositItems.Add(refundOrderDepositItem);

			return testOrder;
		}

		private Order CreateTestOrderWithBottleRefundAndRecivedDeposit()
		{
			Order testOrder = new Order {
				OrderItems = new List<OrderItem>(),
				DeliveryDate = DateTime.Now,
				OrderDepositItems = new List<OrderDepositItem>(),
				DepositOperations = new List<DepositOperation>()
			};

			Nomenclature depositNomenclature = Substitute.For<Nomenclature>();
			depositNomenclature.TypeOfDepositCategory.Returns(TypeOfDepositCategory.BottleDeposit);
			OrderItem recivedDepositOrderItem = new OrderItem {
				Nomenclature = depositNomenclature,
				ActualCount = 2,
				Count = 3,
				Price = 322,
			};
			testOrder.OrderItems.Add(recivedDepositOrderItem);

			OrderDepositItem refundBottleOrderDepositItem = new OrderDepositItem {
				ActualCount = 1,
				Count = 6,
				Deposit = 47m,
				DepositType = DepositType.Bottles
			};
			testOrder.OrderDepositItems.Add(refundBottleOrderDepositItem);

			return testOrder;
		}

		private Order CreateTestOrderWithClient()
		{
			Order testOrder = new Order();
			testOrder.DeliverySchedule = Substitute.For<DeliverySchedule>();
			testOrder.Contract = Substitute.For<CounterpartyContract>();

			Counterparty testClient = new Counterparty();
			testOrder.Client = testClient;
			testClient.DefaultDocumentType = DefaultDocumentType.torg12;
			testClient.PaymentMethod = PaymentType.cashless;

			DeliveryPoint testDeliveryPoint = new DeliveryPoint();
			testDeliveryPoint.Id = 45;
			testClient.DeliveryPoints.Add(testDeliveryPoint);

			return testOrder;
		}

		private Order CreateTestOrderWithOrderItems()
		{
			Order testOrder = new Order();
			testOrder.OrderItems = new List<OrderItem>();

			Nomenclature forfeitNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Category.Returns(x => NomenclatureCategory.service);
			OrderItem forfeitOrderItem = new OrderItem {
				Nomenclature = forfeitNomenclature,
				ActualCount = 2,
				Price = 1000
			};
			testOrder.OrderItems.Add(forfeitOrderItem);

			Nomenclature waterNomenclature = Substitute.For<Nomenclature>();
			waterNomenclature.Category.Returns(x => NomenclatureCategory.bottle);
			OrderItem waterOrderItem = new OrderItem {
				Nomenclature = waterNomenclature,
				ActualCount = 10,
				Price = 50
			};
			testOrder.OrderItems.Add(waterOrderItem);

			return testOrder;
		}

		#endregion

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

		#region Акции

		[Test(Description = "Если в заказе не указан контрагент, то возвращаем false")]
		public void CanAddStockBottle_IfNoCounterpartyInOrder_ReturnsTrue()
		{
			// arrange
			Order orderUnderTest = new Order();

			// act
			var res = orderUnderTest.CanAddStockBottle();

			// assert
			Assert.That(res, Is.False);
		}

		[Test(Description = "Если в заказе контрагент без первичного заказа, то возвращаем true")]
		public void CanAddStockBottle_IfCounterpartyWithoutFirstOrder_ReturnsTrue()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			client.FirstOrder.ReturnsNull();

			Order orderUnderTest = new Order { Client = client };
			OrderRepository.GetFirstRealOrderForClientTestGap = (uow, c) => null;

			// act
			var res = orderUnderTest.CanAddStockBottle();

			// assert
			Assert.That(res, Is.True);
		}

		static IEnumerable AllOrderStatusesForActionBottleAndResults()
		{
			yield return new object[] { OrderStatus.Accepted, false };
			yield return new object[] { OrderStatus.Canceled, true };
			yield return new object[] { OrderStatus.Closed, false};
			yield return new object[] { OrderStatus.DeliveryCanceled, true };
			yield return new object[] { OrderStatus.InTravelList, false };
			yield return new object[] { OrderStatus.NewOrder, true };
			yield return new object[] { OrderStatus.NotDelivered, true };
			yield return new object[] { OrderStatus.OnLoading, false };
			yield return new object[] { OrderStatus.OnTheWay, false };
			yield return new object[] { OrderStatus.Shipped, false };
			yield return new object[] { OrderStatus.UnloadingOnStock, false };
			yield return new object[] { OrderStatus.WaitForPayment, false };
		}

		[TestCaseSource(nameof(AllOrderStatusesForActionBottleAndResults))]
		[Test(Description = "Если в заказе контрагент с первичным заказом и статус заказа как в AllOrderStatusesForActionBottleAndResults(), возвращаем результат из AllOrderStatusesForActionBottleAndResults()")]
		public void CanAddActionBottle_IfCounterpartyWithReferenceToFirstOrderAndStatusOfTheFirstOrderIsTestStatus_ReturnsResult(OrderStatus testStatus, bool result)
		{
			// arrange
			var firstOrder = Substitute.For<Order>();
			firstOrder.OrderStatus.Returns(testStatus);
			var client = Substitute.For<Counterparty>();
			client.FirstOrder.Returns(firstOrder);

			Order orderUnderTest = new Order { Client = client };
			OrderRepository.GetFirstRealOrderForClientTestGap = (uow, c) => null;

			// act
			var res = orderUnderTest.CanAddStockBottle();

			// assert
			Assert.That(res, Is.EqualTo(result));
		}

		[Test(Description = "Если в заказе контрагент с первичным заказом и статус этого заказа не подходящий, но в репозитории нашёлся заказ с подходящим статусом, возвращаем false")]
		public void CanAddActionBottle_IfCounterpartyWithReferenceToFirstOrderButTheFirstOrderNotInValidStatusAndFoundOrderWithValidSatusInRepository_ReturnsFalse()
		{
			// arrange
			var firstOrder = Substitute.For<Order>();
			firstOrder.OrderStatus.Returns(OrderStatus.NewOrder);
			var client = Substitute.For<Counterparty>();
			client.FirstOrder.Returns(firstOrder);

			Order orderUnderTest = new Order { Client = client };
			OrderRepository.GetFirstRealOrderForClientTestGap = (uow, c) => new Order();

			// act
			var res = orderUnderTest.CanAddStockBottle();

			// assert
			Assert.That(res, Is.False);
		}

		#endregion
		
		#region UpdateOperationTests

		[Test(Description = "Проверка создания операции перемещения бутылей в заказе с неустойками")]
		public void Check_Bottle_Movement_Operation_Update_Without_Delivery()
		{
			// arrange
			Order testOrder = CreateTestOrderWithForfeitAndBottles();
			RouteListItemRepository.HasRouteListItemsForOrderTestGap = (order) => false;
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			var standartNom = Substitute.For<IStandartNomenclatures>();
			standartNom.GetForfeitId().Returns(33);

			// act
			testOrder.UpdateBottlesMovementOperationWithoutDelivery(uow, standartNom);

			// assert
			Assert.AreEqual(3, testOrder.BottlesMovementOperation.Returned);
			Assert.AreEqual(1, testOrder.BottlesMovementOperation.Delivered);
		}

		[Test(Description = "Проверка созднания DepositOperation для оборудования")]
		public void Check_DepositOperation_Creation_For_Equipment()
		{
			// arrange
			Order testOrder = CreateTestOrderWithEquipmentRefundAndRecivedDeposit();
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();

			// act
			var operations = testOrder.UpdateDepositOperations(uow);


			var EquipmentDeposit = operations
							.OfType<DepositOperation>()
							.FirstOrDefault(x => x.DepositType == DepositType.Equipment);


			// assert
			Assert.AreEqual(300m, EquipmentDeposit.ReceivedDeposit);
			Assert.AreEqual(1500m, EquipmentDeposit.RefundDeposit);
		}

		[Test(Description = "Проверка созднания DepositOperation для бутылей")]
		public void Check_DepositOperation_Creation_For_Bottles()
		{
			// arrange
			Order testOrder = CreateTestOrderWithBottleRefundAndRecivedDeposit();
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();

			// act
			var operations = testOrder.UpdateDepositOperations(uow);


			var BottleDeposit = operations
							.OfType<DepositOperation>()
							.FirstOrDefault(x => x.DepositType == DepositType.Bottles);

			// assert
			Assert.AreEqual(644, BottleDeposit.ReceivedDeposit);
			Assert.AreEqual(47, BottleDeposit.RefundDeposit);
		}

		#endregion

		#region OrderItemsDiscountTests

		[Test(Description = "Проверка расчета скидки( в рублях )")]
		public void Check_Money_Discount_For_OrderItems()
		{
			// arrange
			Order testOrder = CreateTestOrderWithOrderItems();
			testOrder.ObservableOrderItems.ListContentChanged -= testOrder.ObservableOrderItems_ListContentChanged;
			testOrder.OrderItems.ToList().ForEach(x => x.IsDiscountInMoney = true); //FIXME : костыль из-за метода Order.CalculateAndSetDiscount()
			DiscountReason discountReason = Substitute.For<DiscountReason>();

			// act
			testOrder.SetDiscount(discountReason, 500m, DiscountUnits.money);

			// assert
			Assert.AreEqual(400m, testOrder.OrderItems[0].DiscountMoney);
			Assert.AreEqual(100m, testOrder.OrderItems[1].DiscountMoney);
		}

		[Test(Description = "Проверка расчета скидки( в процентах )")]
		public void Check_Perscnt_Discount_For_OrderItems()
		{
			// arrange
			Order testOrder = CreateTestOrderWithOrderItems();
			testOrder.ObservableOrderItems.ListContentChanged -= testOrder.ObservableOrderItems_ListContentChanged;
			DiscountReason discountReason = Substitute.For<DiscountReason>();

			// act
			testOrder.SetDiscount(discountReason, 20m, DiscountUnits.percent);

			// assert
			Assert.AreEqual(400m, testOrder.OrderItems[0].DiscountMoney);
			Assert.AreEqual(100m, testOrder.OrderItems[1].DiscountMoney);
		}

		#endregion
	}
}