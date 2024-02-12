using System;
using System.Collections;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Services;

namespace VodovozBusinessTests.Domain.Logistic
{
	[TestFixture]
	public class RouteListItemTests
	{
		private static Order ForfeitWaterAndEmptyBottles(Order order, int waterCount, int forfeitCount, int emptyBottlesCount = 0)
		{
			Nomenclature forfeitNomenclature = Substitute.For<Nomenclature>();
			forfeitNomenclature.Category.Returns(NomenclatureCategory.bottle);
			forfeitNomenclature.Id.Returns(33);

			Nomenclature emptyBottleNomenclature = Substitute.For<Nomenclature>();
			emptyBottleNomenclature.Category.Returns(NomenclatureCategory.bottle);

			Nomenclature waterNomenclature = Substitute.For<Nomenclature>();
			waterNomenclature.Category.Returns(NomenclatureCategory.water);
			waterNomenclature.IsDisposableTare.Returns(false);

			order.AddNomenclature(forfeitNomenclature);
			order.OrderItems.LastOrDefault().SetActualCount(forfeitCount);
			order.AddNomenclature(emptyBottleNomenclature);
			order.OrderItems.LastOrDefault().SetActualCount(emptyBottlesCount);
			order.AddNomenclature(waterNomenclature);
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
			testRLItem.Order.DeliveryDate = DateTime.Now;
			var standartNom = Substitute.For<IStandartNomenclatures>();
			standartNom.GetForfeitId().Returns(33);

			// act
			testRLItem.Order.UpdateBottleMovementOperation(uow,standartNom,testRLItem.BottlesReturned);

			// assert
			Assert.AreEqual(returned, testRLItem.Order.BottlesMovementOperation.Returned);
			Assert.AreEqual(delivered, testRLItem.Order.BottlesMovementOperation.Delivered);
		}
		#endregion создание операций
	}
}
