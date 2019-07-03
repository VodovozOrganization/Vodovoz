using System;
using System.Collections;
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
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Repositories.Orders;
using Vodovoz.Repository;
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
		}

		#region OrderItemsPacks

		private static IEnumerable<OrderItem> ForfeitWaterAndEmptyBottles(int waterCount, int forfeitCount, int emptyBottlesCount = 0)
		{
			Nomenclature forfeitNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Category.Returns(NomenclatureCategory.bottle);
			forfeitNomenclature.Id.Returns(33);

			Nomenclature emptyBottleNomenclature = Substitute.For<Nomenclature>();
			emptyBottleNomenclature.Category.Returns(NomenclatureCategory.bottle);

			Nomenclature waterNomenclature = Substitute.For<Nomenclature>();
			waterNomenclature.Category.Returns(NomenclatureCategory.water);
			waterNomenclature.IsDisposableTare.Returns(false);

			yield return new OrderItem { Nomenclature = forfeitNomenclature, Count = forfeitCount };
			yield return new OrderItem { Nomenclature = emptyBottleNomenclature, Count = emptyBottlesCount };
			yield return new OrderItem { Nomenclature = waterNomenclature, Count = waterCount };
		}

		private static IEnumerable<OrderItem> OrderItemsWithPriceAndCount(params (int, int)[] countAndPrice)
		{
			foreach(var i in countAndPrice) {
				yield return new OrderItem {
					ActualCount = i.Item1,
					Price = i.Item2
				};
			}
		}

		#endregion OrderItemsPacks

		#region OrderCreations

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
			yield return new object[] { OrderStatus.Closed, false };
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
		static IEnumerable WaterForfeitBottleOrderItems()
		{
			yield return new object[] { ForfeitWaterAndEmptyBottles(10, 10).ToList(), 10, 10 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(7, 5).ToList(), 7, 5 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(0, 2).ToList(), 0, 2 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(3, 0).ToList(), 3, 0 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(11, 11, 11).ToList(), 11, 11 };
		}
		[TestCaseSource(nameof(WaterForfeitBottleOrderItems))]
		[Test(Description = "Проверка создания операции перемещения бутылей")]
		public void Check_Bottle_Movement_Operation_Update_Without_Delivery(List<OrderItem> orderItems, int delivered, int returned)
		{
			// arrange
			Order testOrder = new Order();
			testOrder.DeliveryDate = DateTime.Now;
			testOrder.OrderItems = orderItems;
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			IRouteListItemRepository repository = Substitute.For<IRouteListItemRepository>();
			repository.HasRouteListItemsForOrder(uow, testOrder).Returns(false);

			var standartNom = Substitute.For<IStandartNomenclatures>();
			standartNom.GetForfeitId().Returns(33);

			// act
			testOrder.UpdateBottlesMovementOperationWithoutDelivery(uow, standartNom, repository);

			// assert
			Assert.AreEqual(returned, testOrder.BottlesMovementOperation.Returned);
			Assert.AreEqual(delivered, testOrder.BottlesMovementOperation.Delivered);
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

		private static IEnumerable OrderItemsAndDiscountInMoney()
		{
			yield return new object[] { OrderItemsWithPriceAndCount((5, 100), (1, 1000), (7, 300)).ToList(), new List<double> { 50, 100, 210 }, 360m };
			yield return new object[] { OrderItemsWithPriceAndCount((1, 100), (10, 200), (7, 5000)).ToList(), new List<double> { 10, 200, 3500 }, 3710m };
			yield return new object[] { OrderItemsWithPriceAndCount((8, 800), (5, 435), (5, 700)).ToList(), new List<double> { 640, 217.5, 350 }, 1207.5m };
		}
		[TestCaseSource(nameof(OrderItemsAndDiscountInMoney))]
		[Test(Description = "Проверка расчета скидки( в рублях )")]
		public void Check_Money_Discount_For_OrderItems(List<OrderItem> OrderItems, List<double> discountForOrderItems, decimal discountInMoney)
		{
			// arrange
			Order testOrder = new Order();
			testOrder.OrderItems = OrderItems;
			testOrder.ObservableOrderItems.ListContentChanged -= testOrder.ObservableOrderItems_ListContentChanged;
			testOrder.OrderItems.ToList().ForEach(x => x.IsDiscountInMoney = true);
			DiscountReason discountReason = Substitute.For<DiscountReason>();

			// act
			testOrder.SetDiscount(discountReason, discountInMoney, DiscountUnits.money);

			// assert
			for(int i = 0; i < OrderItems.Count; i++)
				Assert.AreEqual(discountForOrderItems[i], testOrder.OrderItems[i].DiscountMoney);
		}


		static IEnumerable OrderItemsAndDiscountInPercent()
		{
			yield return new object[] { OrderItemsWithPriceAndCount((5, 100), (1, 1000), (7, 300)).ToList(), new List<double> { 50, 100, 210 }, 10 };
			yield return new object[] { OrderItemsWithPriceAndCount((1, 100), (10, 200), (7, 5000)).ToList(), new List<double> { 50, 1000, 17500 }, 50 };
			yield return new object[] { OrderItemsWithPriceAndCount((8, 800), (5, 435), (5, 700)).ToList(), new List<double> { 6400, 2175, 3500 }, 110 };
		}
		[TestCaseSource(nameof(OrderItemsAndDiscountInPercent))]
		[Test(Description = "Проверка расчета скидки( в процентах )")]
		public void Check_Percent_Discount_For_OrderItems(List<OrderItem> OrderItems, List<double> discountForOrderItems, int discountInPercent)
		{
			// arrange
			Order testOrder = new Order();
			testOrder.OrderItems = OrderItems;
			testOrder.ObservableOrderItems.ListContentChanged -= testOrder.ObservableOrderItems_ListContentChanged;
			DiscountReason discountReason = Substitute.For<DiscountReason>();

			// act
			testOrder.SetDiscount(discountReason, discountInPercent, DiscountUnits.percent);

			// assert
			for(int i = 0; i < OrderItems.Count; i++)
				Assert.AreEqual(discountForOrderItems[i], testOrder.OrderItems[i].DiscountMoney);
		}

		#endregion

		[Ignore("Слишком много всего в сеттере для точки доставки. Пока не разгрузим - игнор")]
		[Test(Description = "Проверка обновления точки доставки в ДС на продажу оборудования при смене точки доставки в заказе")]
		public void UpdateDeliveryPointInSalesAgreement_OnChangeOfDeliveryPointInOrder_UpdatesDeliveryPointInEquipmentSalesAgreement()
		{
			// arrange
			Order testOrder = new Order();
			DeliveryPoint deliveryPointMock01 = Substitute.For<DeliveryPoint>();
			testOrder.DeliveryPoint = deliveryPointMock01;
			DeliveryPoint deliveryPointMock02 = Substitute.For<DeliveryPoint>();
			OrderAgreement salesAgreement = new OrderAgreement {
				Id = 1,
				AdditionalAgreement = new SalesEquipmentAgreement {
					DeliveryPoint = deliveryPointMock01
				}
			};
			OrderAgreement wsa = new OrderAgreement {
				Id = 2,
				AdditionalAgreement = new WaterSalesAgreement {
					DeliveryPoint = deliveryPointMock01
				}
			};
			testOrder.OrderDocuments = new List<OrderDocument> { salesAgreement, wsa };

			// act
			testOrder.DeliveryPoint = deliveryPointMock02;

			// assert
			Assert.That(testOrder.ObservableOrderDocuments.FirstOrDefault(d => d.Id == 1), Is.EqualTo(deliveryPointMock02));
		}

	}
}