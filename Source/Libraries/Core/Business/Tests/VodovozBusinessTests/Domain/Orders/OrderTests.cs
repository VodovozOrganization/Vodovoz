﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using QS.Dialog;
using Vodovoz.Domain.Contacts;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Cash;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Services;

namespace VodovozBusinessTests.Domain.Orders
{
	[TestFixture()]
	public class OrderTests
	{
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

		[Test(Description = "При добавлении промо-набора в заказ, адрес доставки которого не найден среди других заказов с промонаборами, возвращается true")]
		public void CanAddPromotionalSet_WhenAddPromotionalSetToTheOrderAndNoSameAddressFoundInAnotherOrdersWithPromoSets_ReturnsTrue()
		{
			// arrange
			Order orderUnderTest = new Order();

			var dict = new Dictionary<int, int[]>();
			var promotionalSetRepositoryMock = Substitute.For<IPromotionalSetRepository>();
			promotionalSetRepositoryMock.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(null, orderUnderTest).Returns(dict);

			// act
			var res = orderUnderTest.CanAddPromotionalSet(Substitute.For<PromotionalSet>(), promotionalSetRepositoryMock);

			// assert
			Assert.That(res, Is.True);
		}

		[Test(Description = "При добавлении промо-набора в заказ, адрес доставки которого найден среди других заказов с промонаборами, возвращается false")]
		public void CanAddPromotionalSet_WhenAddPromotionalSetToTheOrderAndSameAddressFoundInAnotherOrdersWithPromoSets_ReturnsFalse()
		{
			// arrange
			var intercativeServiceMock = Substitute.For<IInteractiveService>();
			Order orderUnderTest = new Order {
				UoW = Substitute.For<IUnitOfWork>(),
				Client = Substitute.For<Counterparty>(),
				DeliveryPoint = Substitute.For<DeliveryPoint>(),
				InteractiveService = intercativeServiceMock
			};
			
			var dict = new Dictionary<int, int[]>
			{
				{1, new[]{ 1, 2 }}
			};
			
			var promotionalSetRepositoryMock = Substitute.For<IPromotionalSetRepository>();
			promotionalSetRepositoryMock.GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(
				orderUnderTest.UoW, orderUnderTest).Returns(dict);

			// act
			var res = orderUnderTest.CanAddPromotionalSet(Substitute.For<PromotionalSet>(), promotionalSetRepositoryMock);

			// assert
			Assert.That(res, Is.False);
		}

		[Test(Description = "При добавлении второго промо-набора в заказ возвращается false")]
		public void CanAddPromotionalSet_WhenAddSecondPromotionalSetToTheOrder_ReturnsFalse()
		{
			// arrange
			var promotionalSetMock = Substitute.For<PromotionalSet>();
			var intercativeServiceMock = Substitute.For<IInteractiveService>();
			var promotionalSetRepositoryMock = Substitute.For<IPromotionalSetRepository>();

			Order orderUnderTest = new Order();
			orderUnderTest.PromotionalSets.Add(promotionalSetMock);
			orderUnderTest.InteractiveService = intercativeServiceMock;

			// act
			var result = orderUnderTest.CanAddPromotionalSet(promotionalSetMock, promotionalSetRepositoryMock);

			// assert
			Assert.That(result, Is.False);
		}

		#endregion Рекламные наборы

		#region Акции

		[Test(Description = "Если в заказе не указан контрагент, то возвращаем false")]
		public void CanAddStockBottle_IfNoCounterpartyInOrder_ReturnsTrue()
		{
			// arrange
			Order orderUnderTest = new Order();
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			IOrderRepository orderRepository = Substitute.For<IOrderRepository>();
			orderRepository.GetFirstRealOrderForClientForActionBottle(uow, orderUnderTest, null).ReturnsNull();

			// act
			var res = orderUnderTest.CanAddStockBottle(orderRepository);

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
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			orderUnderTest.UoW = uow;
			IOrderRepository orderRepository = Substitute.For<IOrderRepository>();
			orderRepository.GetFirstRealOrderForClientForActionBottle(uow, orderUnderTest, client).ReturnsNull();

			// act
			var res = orderUnderTest.CanAddStockBottle(orderRepository);

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
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			orderUnderTest.UoW = uow;
			IOrderRepository orderRepository = Substitute.For<IOrderRepository>();
			if(result) {
				orderRepository.GetFirstRealOrderForClientForActionBottle(uow, orderUnderTest, client).ReturnsNull();
			} else {
				orderRepository.GetFirstRealOrderForClientForActionBottle(uow, orderUnderTest, client).Returns(firstOrder);
			}

			// act
			var res = orderUnderTest.CanAddStockBottle(orderRepository);

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
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			orderUnderTest.UoW = uow;
			IOrderRepository orderRepository = Substitute.For<IOrderRepository>();
			orderRepository.GetFirstRealOrderForClientForActionBottle(uow, firstOrder, client).Returns(Substitute.For<Order>());

			// act
			var res = orderUnderTest.CanAddStockBottle(orderRepository);

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
			Order testOrder = new Order {
				Id = 1,
				DeliveryDate = DateTime.Now,
				OrderItems = orderItems
			};
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			testOrder.UoW = uow;
			IRouteListItemRepository routeListItemRepository = Substitute.For<IRouteListItemRepository>();
			routeListItemRepository.HasRouteListItemsForOrder(uow, testOrder).Returns(false);
			ICashRepository cashRepository = Substitute.For<ICashRepository>();
			cashRepository.GetIncomePaidSumForOrder(uow, testOrder.Id).Returns(111);
			cashRepository.GetExpenseReturnSumForOrder(uow, testOrder.Id).Returns(111);

			var standartNom = Substitute.For<IStandartNomenclatures>();
			standartNom.GetForfeitId().Returns(33);

			// act
			testOrder.UpdateBottlesMovementOperationWithoutDelivery(uow, standartNom, routeListItemRepository, cashRepository);

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

		[Test(Description = "Проверка созднания DepositOperation для закрывашек по контракту")]
		public void Check_DepositOperation_Creation_For_Contract_Closer()
		{
			// arrange
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();

			Order testOrder = new Order {
				OrderItems = new List<OrderItem>(),
				DeliveryDate = DateTime.Now,
				DepositOperations = new List<DepositOperation>()
			};
			testOrder.IsContractCloser = true;

			Nomenclature depositNomenclature = Substitute.For<Nomenclature>();
			depositNomenclature.TypeOfDepositCategory.Returns(TypeOfDepositCategory.BottleDeposit);
			OrderItem recivedDepositOrderItem = new OrderItem {
				Nomenclature = depositNomenclature,
				Count = 3,
				Price = 322
			};
			testOrder.OrderItems.Add(recivedDepositOrderItem);

			// act
			var operations = testOrder.UpdateDepositOperations(uow);

			// assert
			Assert.That(operations?.FirstOrDefault(), Is.EqualTo(null));
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
			DiscountReason discountReason = Substitute.For<DiscountReason>();
			var discountController = Substitute.For<IOrderDiscountsController>();

			// act
			discountController.SetCustomDiscountForOrder(
				discountReason, discountInMoney, DiscountUnits.money, testOrder.ObservableOrderItems);

			// assert
			for(int i = 0; i < OrderItems.Count; i++)
			{
				Assert.AreEqual(discountForOrderItems[i], testOrder.OrderItems[i].DiscountMoney);
			}
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
			var discountController = Substitute.For<IOrderDiscountsController>();

			// act
			discountController.SetCustomDiscountForOrder(
				discountReason, discountInPercent, DiscountUnits.percent, testOrder.ObservableOrderItems);

			// assert
			for(int i = 0; i < OrderItems.Count; i++)
			{
				Assert.AreEqual(discountForOrderItems[i], testOrder.OrderItems[i].DiscountMoney);
			}
		}

		#endregion

		[Test(Description = "Если кол-во отгруженных товаров по документам самовывоза совпадает с кол-вом товаров в заказе, то возвращается true")]
		public void IsFullyShippedSelfDeliveryOrder_IfQuantityOfUnloadedGoodsIsTheSameAsQuantityOfGoodsInOrder_ThenMethodReturnsTrue()
		{
			// arrange
			Nomenclature nomenclatureMock01 = Substitute.For<Nomenclature>();
			nomenclatureMock01.Category.Returns(NomenclatureCategory.bottle);
			nomenclatureMock01.Id.Returns(33);

			Nomenclature nomenclatureMock02 = Substitute.For<Nomenclature>();
			nomenclatureMock02.Category.Returns(NomenclatureCategory.equipment);
			nomenclatureMock02.Id.Returns(111);

			Nomenclature nomenclatureMock03 = Substitute.For<Nomenclature>();
			nomenclatureMock03.Category.Returns(NomenclatureCategory.water);
			nomenclatureMock03.Id.Returns(1);

			OrderItem orderItemMock01 = Substitute.For<OrderItem>();
			orderItemMock01.Nomenclature.Returns(nomenclatureMock01);
			orderItemMock01.Count.Returns(3);

			OrderItem orderItemMock02 = Substitute.For<OrderItem>();
			orderItemMock02.Nomenclature.Returns(nomenclatureMock02);
			orderItemMock02.Count.Returns(1);

			OrderItem orderItemMock03 = Substitute.For<OrderItem>();
			orderItemMock03.Nomenclature.Returns(nomenclatureMock03);
			orderItemMock03.Count.Returns(31);

			OrderEquipment orderEquipmentMock01 = Substitute.For<OrderEquipment>();
			orderEquipmentMock01.Nomenclature.Returns(nomenclatureMock02);
			orderEquipmentMock01.Count.Returns(1);
			orderEquipmentMock01.Direction.Returns(Direction.Deliver);

			OrderEquipment orderEquipmentMock02 = Substitute.For<OrderEquipment>();
			orderEquipmentMock02.Nomenclature.Returns(nomenclatureMock03);
			orderEquipmentMock02.Count.Returns(5);
			orderEquipmentMock02.Direction.Returns(Direction.PickUp);

			Order orderUnderTest = new Order {
				SelfDelivery = true,
				OrderItems = new List<OrderItem> { orderItemMock01, orderItemMock02, orderItemMock03 },
				OrderEquipments = new List<OrderEquipment> { orderEquipmentMock01, orderEquipmentMock02 }
			};

			SelfDeliveryDocument selfDeliveryDocumentMock = Substitute.For<SelfDeliveryDocument>();

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			ISelfDeliveryRepository repository = Substitute.For<ISelfDeliveryRepository>();
			repository.OrderNomenclaturesUnloaded(uow, orderUnderTest, selfDeliveryDocumentMock).Returns(
				new Dictionary<int, decimal> {
					{ 33, 3 },
					{ 111, 2 },
					{ 1, 31 }
				}
			);

			// act
			var res = orderUnderTest.IsFullyShippedSelfDeliveryOrder(uow, repository, selfDeliveryDocumentMock);

			// assert
			Assert.That(res, Is.True);
		}


		[Test(Description = "Создание новой операции перемещения бутылей в самовывозе и не учёт неустоек в подсчёте общего кол-ва возвращённых бутылей, если самовывоз не полностью оплачен")]
		public void UpdateBottlesMovementOperationWithoutDelivery_CreatesNewBottleMovementOperationAndIgnoreForfeits_WhenTheSelfDeliveryIsNotFullyPaid()
		{
			// arrange
			Order orderUnderTest = new Order {
				Id = 1,
				DeliveryDate = new DateTime(2000, 01, 02),
				SelfDelivery = true,
				IsContractCloser = false,
				ReturnedTare = 1
			};

			SelfDeliveryDocument selfDeliveryDocumentMock = Substitute.For<SelfDeliveryDocument>();

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			orderUnderTest.UoW = uow;
			IRouteListItemRepository routeListItemRepository = Substitute.For<IRouteListItemRepository>();
			routeListItemRepository.HasRouteListItemsForOrder(uow, orderUnderTest).Returns(false);
			IStandartNomenclatures standartNomenclatures = Substitute.For<IStandartNomenclatures>();
			ICashRepository cashRepository = Substitute.For<ICashRepository>();
			cashRepository.GetIncomePaidSumForOrder(uow, orderUnderTest.Id).Returns(111m);
			cashRepository.GetExpenseReturnSumForOrder(uow, orderUnderTest.Id).Returns(112m);

			// act
			orderUnderTest.UpdateBottlesMovementOperationWithoutDelivery(uow, standartNomenclatures, routeListItemRepository, cashRepository);

			// assert
			Assert.That(orderUnderTest.BottlesMovementOperation.Order, Is.EqualTo(orderUnderTest));
			Assert.That(orderUnderTest.BottlesMovementOperation.OperationTime.Year, Is.EqualTo(2000));
			Assert.That(orderUnderTest.BottlesMovementOperation.Returned, Is.EqualTo(1));
		}

		[Test(Description = "Создание новой операции перемещения бутылей в самовывозе и не учёт неустоек в подсчёте общего кол-ва возвращённых бутылей, если самовывоз не полностью оплачен")]
		public void UpdateBottlesMovementOperationWithoutDelivery_CreatesNewBottleMovementOperationAndNoIgnoreForfeits_WhenTheSelfDeliveryIsFullyPaid()
		{
			// arrange
			Nomenclature nomenclatureMock01 = Substitute.For<Nomenclature>();
			nomenclatureMock01.Id.Returns(100);
			OrderItem orderItem01 = new OrderItem {
				Nomenclature = nomenclatureMock01,
				Count = 15
			};
			Order orderUnderTest = new Order {
				Id = 1,
				DeliveryDate = new DateTime(2000, 01, 02),
				SelfDelivery = true,
				IsContractCloser = false,
				OrderItems = new List<OrderItem> { orderItem01 },
				ReturnedTare = 1
			};

			SelfDeliveryDocument selfDeliveryDocumentMock = Substitute.For<SelfDeliveryDocument>();

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			orderUnderTest.UoW = uow;
			IRouteListItemRepository routeListItemRepository = Substitute.For<IRouteListItemRepository>();
			routeListItemRepository.HasRouteListItemsForOrder(uow, orderUnderTest).Returns(false);
			IStandartNomenclatures standartNomenclatures = Substitute.For<IStandartNomenclatures>();
			standartNomenclatures.GetForfeitId().Returns(100);
			ICashRepository cashRepository = Substitute.For<ICashRepository>();
			cashRepository.GetIncomePaidSumForOrder(uow, orderUnderTest.Id).Returns(1m);
			cashRepository.GetExpenseReturnSumForOrder(uow, orderUnderTest.Id).Returns(1m);

			// act
			orderUnderTest.UpdateBottlesMovementOperationWithoutDelivery(uow, standartNomenclatures, routeListItemRepository, cashRepository);

			// assert
			Assert.That(orderUnderTest.BottlesMovementOperation.Order, Is.EqualTo(orderUnderTest));
			Assert.That(orderUnderTest.BottlesMovementOperation.OperationTime.Year, Is.EqualTo(2000));
			Assert.That(orderUnderTest.BottlesMovementOperation.Returned, Is.EqualTo(16));
		}

		[Test(Description = "Обновление существующей операции перемещения бутылей в самовывозе с обновлением полей даты, доставлено и возвращено, при условии полной оплаты в кассе")]
		public void UpdateBottlesMovementOperationWithoutDelivery_UpdatesExistingBottleMovementOperationAndNoIgnoreForfeits_WhenTheSelfDeliveryIsFullyPaid()
		{
			// arrange
			Nomenclature nomenclatureMock01 = Substitute.For<Nomenclature>();
			nomenclatureMock01.Category.Returns(NomenclatureCategory.equipment);
			nomenclatureMock01.Id.Returns(50);

			Nomenclature nomenclatureMock03 = Substitute.For<Nomenclature>();
			nomenclatureMock03.Category.Returns(NomenclatureCategory.water);
			nomenclatureMock03.IsDisposableTare.Returns(false);
			nomenclatureMock03.Id.Returns(14);

			OrderItem orderItem01 = new OrderItem {
				Nomenclature = nomenclatureMock01,
				Count = 7
			};

			OrderItem orderItem02 = new OrderItem {
				Nomenclature = nomenclatureMock03,
				Count = 41
			};

			Order orderUnderTest = new Order {
				Id = 1,
				BottlesMovementOperation = new BottlesMovementOperation {
					OperationTime = DateTime.Now,
					Delivered = -1,
					Returned = -1
				},
				DeliveryDate = new DateTime(1999, 05, 12),
				SelfDelivery = true,
				IsContractCloser = false,
				OrderItems = new List<OrderItem> { orderItem01, orderItem02 },
				ReturnedTare = 3
			};

			SelfDeliveryDocument selfDeliveryDocumentMock = Substitute.For<SelfDeliveryDocument>();

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			orderUnderTest.UoW = uow;
			IRouteListItemRepository routeListItemRepository = Substitute.For<IRouteListItemRepository>();
			routeListItemRepository.HasRouteListItemsForOrder(uow, orderUnderTest).Returns(false);
			IStandartNomenclatures standartNomenclatures = Substitute.For<IStandartNomenclatures>();
			standartNomenclatures.GetForfeitId().Returns(50);
			ICashRepository cashRepository = Substitute.For<ICashRepository>();
			cashRepository.GetIncomePaidSumForOrder(uow, 1).Returns(22);
			cashRepository.GetExpenseReturnSumForOrder(uow, 1).Returns(22);

			// act
			orderUnderTest.UpdateBottlesMovementOperationWithoutDelivery(uow, standartNomenclatures, routeListItemRepository, cashRepository);

			// assert
			Assert.That(orderUnderTest.BottlesMovementOperation.Order, Is.Null);
			Assert.That(orderUnderTest.BottlesMovementOperation.OperationTime.Year, Is.EqualTo(1999));
			Assert.That(orderUnderTest.BottlesMovementOperation.Delivered, Is.EqualTo(41));
			Assert.That(orderUnderTest.BottlesMovementOperation.Returned, Is.EqualTo(10));
		}

		static IEnumerable NomenclatureSettingsForVolume()
		{
			yield return new object[] { false, false, 0d };
			yield return new object[] { false, true, 1.2d };
			yield return new object[] { true, false, 0.7d };
			yield return new object[] { true, true, 1.9d };
		}
		[TestCaseSource(nameof(NomenclatureSettingsForVolume))]
		[Test(Description = "Считаем полный объём груза, либо отдельно товаров или оборудования в заказе")]
		public void FullVolume_WhenPassCommandToCalculateOrderItemsOrEquipmentOrBoth_CanCalculatesFullVolumeOrVolumeOfItemsOrEquipmentSeparately(bool countOrderItems, bool countOrderEquipment, double result)
		{
			// arrange
			Nomenclature nomenclatureMockOrderItem = Substitute.For<Nomenclature>();
			nomenclatureMockOrderItem.Volume.Returns(.35m);

			Nomenclature nomenclatureMockOrderEquipment = Substitute.For<Nomenclature>();
			nomenclatureMockOrderEquipment.Volume.Returns(.40m);

			OrderItem orderItem = new OrderItem {
				Nomenclature = nomenclatureMockOrderItem,
				Count = 2
			};

			OrderEquipment orderEquipment = new OrderEquipment {
				Nomenclature = nomenclatureMockOrderEquipment,
				Count = 3,
				Direction = Direction.Deliver
			};

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> { orderItem },
				OrderEquipments = new List<OrderEquipment> { orderEquipment },
			};

			// act
			var vol = orderUnderTest.FullVolume(countOrderItems, countOrderEquipment);

			// assert
			Assert.That(Math.Round(vol, 4), Is.EqualTo(Math.Round(result, 4)));
		}

		static IEnumerable NomenclatureSettingsForReverseVolume()
		{
			yield return new object[] { false, false, 0.03645m, 0d };
			yield return new object[] { false, true, 0.03645m, 1.2d };
			yield return new object[] { true, false, 0.03645m, 0.18225d };
			yield return new object[] { true, true, 0.03645m, 1.38225d };
			yield return new object[] { true, true, 0.1m, 1.7d };
		}
		[TestCaseSource(nameof(NomenclatureSettingsForReverseVolume))]
		[Test(Description = "Считаем полный объём возвращаемого груза, либо отдельно тары или оборудования от клиента в заказе ")]
		public void FullReverseVolume_WhenPassCommandToCalculateOrderBottlesReturnOrEquipmentOrBoth_CanCalculatesFullReverseVolumeOrVolumeOfBottlesReturnOrEquipmentSeparately(bool countBottlesReturn, bool countOrderEquipment, decimal bottlesVolume, double result)
		{
			// arrange
			Nomenclature nomenclatureMockOrderEquipment = Substitute.For<Nomenclature>();
			nomenclatureMockOrderEquipment.Volume.Returns(.40m);
			nomenclatureMockOrderEquipment.Category = NomenclatureCategory.equipment;

			OrderEquipment orderEquipment = new OrderEquipment
			{
				Nomenclature = nomenclatureMockOrderEquipment,
				Count = 3,
				Direction = Direction.PickUp
			};

			Order orderUnderTest = new Order
			{
				BottlesReturn = 5,
				OrderEquipments = new List<OrderEquipment> { orderEquipment },
			};

			// act
			var vol = orderUnderTest.FullReverseVolume(countBottlesReturn, countOrderEquipment,bottlesVolume);

			// assert
			Assert.That(Math.Round(vol, 4), Is.EqualTo(Math.Round(result, 4)));
		}

		static IEnumerable NomenclatureSettingsForWeight()
		{
			yield return new object[] { false, false, 0.0d };
			yield return new object[] { false, true, 1.6d };
			yield return new object[] { true, false, 2.4d };
			yield return new object[] { true, true, 4.0d };
		}
		[TestCaseSource(nameof(NomenclatureSettingsForWeight))]
		[Test(Description = "Считаем полный объём груза, либо отдельно товаров или оборудования в заказе")]
		public void FullWeight_WhenPassCommandToCalculateOrderItemsOrEquipmentOrBoth_CalculatesFullWeightOrWeightOfItemsOrEquipmentSeparately(bool countOrderItems, bool countOrderEquipment, double result)
		{
			// arrange
			Nomenclature nomenclatureMockOrderItem = Substitute.For<Nomenclature>();
			nomenclatureMockOrderItem.Weight.Returns(.3m);

			Nomenclature nomenclatureMockOrderEquipment = Substitute.For<Nomenclature>();
			nomenclatureMockOrderEquipment.Weight.Returns(1.6m);

			OrderItem orderItem = new OrderItem {
				Nomenclature = nomenclatureMockOrderItem,
				Count = 8
			};

			OrderEquipment orderEquipment = new OrderEquipment {
				Nomenclature = nomenclatureMockOrderEquipment,
				Count = 1,
				Direction = Direction.Deliver
			};

			Order orderUnderTest = new Order {
				OrderItems = new List<OrderItem> { orderItem },
				OrderEquipments = new List<OrderEquipment> { orderEquipment },
			};

			// act
			var weight = orderUnderTest.FullWeight(countOrderItems, countOrderEquipment);

			// assert
			Assert.That(Math.Round(weight, 4), Is.EqualTo(Math.Round(result, 4)));
		}

		[Test(Description = "Возврат Null если отсутствует клиент в заказе")]
		public void GetContact_WhenNoClientInOrder_ThenReturnsNull()
		{
			// arrange
			Order order = new Order {
				Client = null
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.Null);
		}

		[Test(Description = "Возврат Null если самовывоз и у клиента нет контактов")]
		public void GetContact_WhenSelfdeliveryAndNoContactsInClient_ThenReturnsNull()
		{
			// arrange
			var client = Substitute.For<Counterparty>();

			Order order = new Order {
				Client = client,
				SelfDelivery = true
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.Null);
		}

		[Test(Description = "Возврат номера мобильного телефона клиента если самовывоз и у клиента есть мобильный номер")]
		public void GetContact_WhenSelfdeliveryAndClientHasMobilePhoneNumber_ThenReturnsClientsMobilePhoneNumber()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			client.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "8121234567" },
					new Phone { Number = "9211234567" }
				}
			);

			Order order = new Order {
				Client = client,
				SelfDelivery = true
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("+79211234567"));
		}

		[Test(Description = "Возврат электронного адреса клиента если самовывоз и у клиента есть не мобильный номер и эл.адрес")]
		public void GetContact_WhenSelfdeliveryAndClientHasNotMobilePhoneNumberAndEMail_ThenReturnsClientsEMail()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			client.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "8121234567" }
				}
			);
			client.Emails.Returns<IList<Email>>(
				new List<Email> {
					new Email { Address = "123@dsd.dss" }
				}
			);

			Order order = new Order {
				Client = client,
				SelfDelivery = true
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("123@dsd.dss"));
		}

		[Test(Description = "Возврат электронного адреса клиента если самовывоз и у клиента есть мобильный номер и эл.адрес")]
		public void GetContact_WhenSelfdeliveryAndClientHasMobilePhoneNumberAndEMail_ThenReturnsClientsEMail()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			client.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "9211234567" }
				}
			);
			client.Emails.Returns<IList<Email>>(
				new List<Email> {
					new Email { Address = "123@dsd.dss" }
				}
			);

			Order order = new Order {
				Client = client,
				SelfDelivery = true
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("+79211234567"));
		}

		[Test(Description = "Возврат мобильного телефона точки доставки если не самовывоз, у точки доставки есть мобильный номер и у клиента есть эл.адрес")]
		public void GetContact_WhenNotSelfdeliveryAndClientHasEMailAndDeliveryPointHasMobilePhoneNumber_ThenReturnsDeliveryPointsMobilePhoneNumber()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			client.Emails.Returns<IList<Email>>(
				new List<Email> {
					new Email { Address = "123@dsd.dss" }
				}
			);

			var dp = Substitute.For<DeliveryPoint>();
			dp.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "9211234567" }
				}
			);

			Order order = new Order {
				Client = client,
				DeliveryPoint = dp
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("+79211234567"));
		}

		[Test(Description = "Возврат электронного адреса клиента если не самовывоз, у точки доставки есть НЕ мобильный номер и у клиента есть эл.адрес")]
		public void GetContact_WhenNotSelfdeliveryAndClientHasEMailAndDeliveryPointHasNotMobilePhoneNumber_ThenReturnsClientsEMail()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			client.Emails.Returns<IList<Email>>(
				new List<Email> {
					new Email { Address = "123@dsd.dss" }
				}
			);

			var dp = Substitute.For<DeliveryPoint>();
			dp.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "8121234567" }
				}
			);

			Order order = new Order {
				Client = client,
				DeliveryPoint = dp
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("123@dsd.dss"));
		}

		[Test(Description = "Возврат любого номера из точки доставки если не самовывоз, у точки доставки есть НЕ мобильный номер и у клиента нет контактов")]
		public void GetContact_WhenNotSelfdeliveryAndClientHasNoContactsAndDeliveryPointHasNotMobilePhoneNumber_ThenReturnsDeliveryPointsNotMobilePhoneNumber()
		{
			// arrange
			var client = Substitute.For<Counterparty>();
			var dp = Substitute.For<DeliveryPoint>();
			dp.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "8121234567" }
				}
			);

			Order order = new Order {
				Client = client,
				DeliveryPoint = dp
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("+78121234567"));
		}

		[Test(Description = "Возврат телефона для чеков точки доставки если он есть")]
		public void GetContact_WhenDeliveryPointNumberForReceiptsExist_ThenReturnsNumberForReceipts()
		{
			// arrange
			var client = Substitute.For<Counterparty>();

			var pt = Substitute.For<PhoneType>();
			pt.PhonePurpose = PhonePurpose.ForReceipts;

			var dp = Substitute.For<DeliveryPoint>();
			dp.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "1234567", PhoneType = pt}
				}
			);

			Order order = new Order {
				Client = client,
				DeliveryPoint = dp
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("+71234567"));
		}

		[Test(Description = "Возврат e-mail не для чеков или счетов, если отсутствуют мобильные телефоны и телефоны и email для чеков и счетов")]
		public void GetContact_WhenNoAnyMobilePhonesANDNoAnyPhonesOREmailsForBillsORReceipts_ThenReturnsEmailNotForReceiptsORBills()
		{
			// arrange
			var client = Substitute.For<Counterparty>();

			var et = Substitute.For<EmailType>();
			et.EmailPurpose = EmailPurpose.Default;

			client.Emails.Returns<IList<Email>>(
			new List<Email> {
				new Email { Address = "123@dsd.dss",
					EmailType = et }
				}
			);

			client.Phones.Returns<IList<Phone>>(
			new List<Phone> {
				new Phone { Number = "8121234567",}
				}
			);

			var dp = Substitute.For<DeliveryPoint>();
			dp.Phones.Returns<IList<Phone>>(
				new List<Phone> {
					new Phone { Number = "8121234567",}
				}
			);

			Order order = new Order {
				Client = client,
				DeliveryPoint = dp
			};

			// act
			var contact = order.GetContact();

			// assert
			Assert.That(contact, Is.EqualTo("123@dsd.dss"));
		}
	}
}
