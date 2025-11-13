using System;
using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusinessTests.Domain.Logistic
{
	[TestFixture]
	public class RouteListItemTests
	{
		private static IOrderContractUpdater _contractUpdater;
		
		[SetUp]
		private void Init()
		{
			_contractUpdater = Substitute.For<IOrderContractUpdater>();
		}
		
		private static Order ForfeitWaterAndEmptyBottles(Order order, int waterCount, int forfeitCount, int emptyBottlesCount = 0)
		{
			var uow = Substitute.For<IUnitOfWork>();
			Nomenclature forfeitNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Category.Returns(NomenclatureCategory.bottle);
			forfeitNomenclature.Id.Returns(33);

			Nomenclature emptyBottleNomenclature = Substitute.For<Nomenclature>();
			emptyBottleNomenclature.Category.Returns(NomenclatureCategory.bottle);

			Nomenclature waterNomenclature = Substitute.For<Nomenclature>();
			waterNomenclature.Category.Returns(NomenclatureCategory.water);
			waterNomenclature.IsDisposableTare.Returns(false);

			order.AddNomenclature(uow, _contractUpdater, forfeitNomenclature);
			order.OrderItems.LastOrDefault().SetActualCount(forfeitCount);
			order.AddNomenclature(uow, _contractUpdater, emptyBottleNomenclature);
			order.OrderItems.LastOrDefault().SetActualCount(emptyBottlesCount);
			order.AddNomenclature(uow, _contractUpdater, waterNomenclature);
			order.OrderItems.LastOrDefault().SetActualCount(waterCount);

			return order;
		}

		#region создание операций
		static IEnumerable WaterForfeitBottleOrderItems()
		{
			var order = new Order();

			yield return new object[] { ForfeitWaterAndEmptyBottles(order,10, 10), 10, 10 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(order, 7, 5), 7, 5 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(order, 0, 2), 0, 2 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(order, 3, 0), 3, 0 };
			yield return new object[] { ForfeitWaterAndEmptyBottles(order, 11, 11, 11), 11, 11 };
		}

		[TestCaseSource(nameof(WaterForfeitBottleOrderItems))]
		[Test(Description = "Проверка создания операции перемещения бутылей")]
		public void Check_Bottle_Movement_Operation_Creation(Order order, int delivered, int returned)
		{
			//arrange
			RouteListItem testRLItem = new RouteListItem();
			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			testRLItem.Order = order;
			testRLItem.Order.UoW = uow;
			testRLItem.Order.UpdateDeliveryDate(DateTime.Now, _contractUpdater, out var message);
			var standartNom = Substitute.For<Vodovoz.Settings.Nomenclature.INomenclatureSettings>();
			standartNom.ForfeitId.Returns(33);

			// act
			testRLItem.Order.UpdateBottleMovementOperation(uow,standartNom,testRLItem.BottlesReturned);

			// assert
			Assert.AreEqual(returned, testRLItem.Order.BottlesMovementOperation.Returned);
			Assert.AreEqual(delivered, testRLItem.Order.BottlesMovementOperation.Delivered);
		}
		#endregion создание операций
	}
}
