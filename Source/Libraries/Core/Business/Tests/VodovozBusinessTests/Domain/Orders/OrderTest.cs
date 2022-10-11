using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Repository.Logistics;

namespace VodovozBusinessTests.Domain.Orders
{
	[TestFixture]
	public class OrderTest
	{

		[TearDown]
		public void SelfDeliveryWithForfeitInit()
		{
			RouteListItemRepository.HasRouteListItemsForOrderTestGap = null;
		}

		#region OrderCreations

		private Order CreateTestOrderWithForfeitAndBottles()
		{
			Order testOrder = new Order 
			{
				OrderItems = new List<OrderItem>() ,
				DeliveryDate = DateTime.Now
			};

			Nomenclature forfeitNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Id.Returns(33);
			OrderItem forfeitOrderItem = new OrderItem 
			{
				Nomenclature = forfeitNomenclature,
				Count = 3
			};
			testOrder.OrderItems.Add(forfeitOrderItem);

			Nomenclature emptyBottleNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Category.Returns(NomenclatureCategory.bottle);
			OrderItem emptyBottleOrderItem = new OrderItem 
			{
				Nomenclature = emptyBottleNomenclature,
				Count = 5
			};
			testOrder.OrderItems.Add(emptyBottleOrderItem);

			Nomenclature waterNomenclature = new Nomenclature 
			{
				Category = NomenclatureCategory.water,
				TareVolume = TareVolume.Vol19L,
				IsDisposableTare = false
			};
			OrderItem waterOrderItem = new OrderItem 
			{
				Nomenclature = waterNomenclature,
				Count = 1
			};
			testOrder.OrderItems.Add(waterOrderItem);


			return testOrder;
		}

		private Order CreateTestOrderWithEquipmentRefundAndRecivedDeposit()
		{
			Order testOrder = new Order 
			{
				OrderItems = new List<OrderItem>(),
				DeliveryDate = DateTime.Now,
				OrderDepositItems = new List<OrderDepositItem>(),
				DepositOperations = new List<DepositOperation>()
			};

			Nomenclature depositNomenclature = Substitute.For<Nomenclature>();
			depositNomenclature.TypeOfDepositCategory.Returns(TypeOfDepositCategory.EquipmentDeposit);
			OrderItem recivedDepositOrderItem = new OrderItem 
			{
				Nomenclature = depositNomenclature,
				ActualCount = 2,
				Count = 3,
				Price = 150m,
			};
			testOrder.OrderItems.Add(recivedDepositOrderItem);

			OrderDepositItem refundOrderDepositItem = new OrderDepositItem 
			{
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
			Order testOrder = new Order 
			{
				OrderItems = new List<OrderItem>(),
				DeliveryDate = DateTime.Now,
				OrderDepositItems = new List<OrderDepositItem>(),
				DepositOperations = new List<DepositOperation>()
			};

			Nomenclature depositNomenclature = Substitute.For<Nomenclature>();
			depositNomenclature.TypeOfDepositCategory.Returns(TypeOfDepositCategory.BottleDeposit);
			OrderItem recivedDepositOrderItem = new OrderItem 
			{
				Nomenclature = depositNomenclature,
				ActualCount = 2,
				Count = 3,
				Price = 322,
			};
			testOrder.OrderItems.Add(recivedDepositOrderItem);

			OrderDepositItem refundBottleOrderDepositItem = new OrderDepositItem 
			{
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
			OrderItem forfeitOrderItem = new OrderItem 
			{
				Nomenclature = forfeitNomenclature,
				ActualCount = 2 ,
				Price = 1000
			};
			testOrder.OrderItems.Add(forfeitOrderItem);

			Nomenclature waterNomenclature = Substitute.For<Nomenclature>();
			waterNomenclature.Category.Returns(x => NomenclatureCategory.bottle);
			OrderItem waterOrderItem= new OrderItem 
			{
				Nomenclature = waterNomenclature,
				ActualCount = 10,
				Price = 50 
			};
			testOrder.OrderItems.Add(waterOrderItem);

			return testOrder;
		}

		#endregion

		#region TestArrangeMethods

		private void SetTestGapForRouteListItemRepository()
		{
			RouteListItemRepository.HasRouteListItemsForOrderTestGap = (order) => false;
		}

		#endregion

		#region UpdateOperationTest

		[Test(Description = "Проверка создания операции перемещения бутылей в заказе с неустойками")]
		public void Check_SelfDelivery_Bottle_Movement_Operation_Update_With_Forfeit()
		{
			// arrange
			Order testOrder = CreateTestOrderWithForfeitAndBottles();
			SetTestGapForRouteListItemRepository();
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			var standartNom = Substitute.For<IStandartNomenclatures>();
			standartNom.GetForfeitId().Returns(33);

			// act
			testOrder.UpdateBottlesMovementOperationWithoutDelivery(uow , standartNom);

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
			testOrder.UpdateDepositOperations(uow);


			var EquipmentDeposit = testOrder.DepositOperations
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
			testOrder.UpdateDepositOperations(uow);


			var BottleDeposit = testOrder.DepositOperations
							.OfType<DepositOperation>()
							.FirstOrDefault(x => x.DepositType == DepositType.Bottles);
							
			// assert
			Assert.AreEqual(644, BottleDeposit.ReceivedDeposit);
			Assert.AreEqual(47, BottleDeposit.RefundDeposit);
		}

		#endregion

		#region DiscountTests

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
			Assert.AreEqual(400m , testOrder.OrderItems[0].DiscountMoney);
			Assert.AreEqual(100m , testOrder.OrderItems[1].DiscountMoney);
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
			Assert.AreEqual(400m , testOrder.OrderItems[0].DiscountMoney);
			Assert.AreEqual(100m , testOrder.OrderItems[1].DiscountMoney);
		}

		#endregion
	}
}
