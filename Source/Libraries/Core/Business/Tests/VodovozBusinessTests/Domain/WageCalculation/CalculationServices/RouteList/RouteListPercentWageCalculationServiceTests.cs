using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture]
	public class RouteListPercentWageCalculationServiceTests
	{
		[Test(Description = "Расчёт ЗП для МЛ с заказами на доставку (не сервисными)")]
		public void WageCalculationForRouteListWithDelivery()
		{
			// arrange
			PercentWageParameterItem percentWage = new PercentWageParameterItem {
				PercentWageType = PercentWageTypes.RouteList,
				RouteListPercent = 50
			};

			var orderItemWageCalculationSource1 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource1.ActualCount.Returns(10);
			orderItemWageCalculationSource1.Price.Returns(50);
			orderItemWageCalculationSource1.DiscountMoney.Returns(100);

			var orderItemWageCalculationSource2 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource2.InitialCount.Returns(5);
			orderItemWageCalculationSource2.Price.Returns(100);
			orderItemWageCalculationSource2.DiscountMoney.Returns(50);

			var orderItemWageCalculationSource3 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource3.ActualCount.Returns(15);
			orderItemWageCalculationSource3.Price.Returns(100);
			orderItemWageCalculationSource3.DiscountMoney.Returns(300);

			var orderItemWageCalculationSource4 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource4.InitialCount.Returns(1);
			orderItemWageCalculationSource4.Price.Returns(1000);
			orderItemWageCalculationSource4.DiscountMoney.Returns(500);

			var orderDepositItemWageCalculationSource1 = Substitute.For<IOrderDepositItemWageCalculationSource>();
			orderDepositItemWageCalculationSource1.ActualCount.Returns(3);
			orderDepositItemWageCalculationSource1.Deposit.Returns(150);

			var orderDepositItemWageCalculationSource2 = Substitute.For<IOrderDepositItemWageCalculationSource>();
			orderDepositItemWageCalculationSource2.InitialCount.Returns(5);
			orderDepositItemWageCalculationSource2.Deposit.Returns(75);

			var orderDepositItemWageCalculationSource3 = Substitute.For<IOrderDepositItemWageCalculationSource>();
			orderDepositItemWageCalculationSource3.ActualCount.Returns(4);
			orderDepositItemWageCalculationSource3.Deposit.Returns(400);

			var orderDepositItemWageCalculationSource4 = Substitute.For<IOrderDepositItemWageCalculationSource>();
			orderDepositItemWageCalculationSource4.InitialCount.Returns(1);
			orderDepositItemWageCalculationSource4.Deposit.Returns(100);

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.OrderItemsSource.Returns(
				new List<IOrderItemWageCalculationSource> {
					orderItemWageCalculationSource1,
					orderItemWageCalculationSource2
				}
			);

			routeListItemWageCalculationSource1.OrderDepositItemsSource.Returns(
				new List<IOrderDepositItemWageCalculationSource> {
					orderDepositItemWageCalculationSource1,
					orderDepositItemWageCalculationSource2
				}
			);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.OrderItemsSource.Returns(
				new List<IOrderItemWageCalculationSource> {
					orderItemWageCalculationSource3,
					orderItemWageCalculationSource4
				}
			);

			routeListItemWageCalculationSource2.OrderDepositItemsSource.Returns(
				new List<IOrderDepositItemWageCalculationSource> {
					orderDepositItemWageCalculationSource3,
					orderDepositItemWageCalculationSource4
				}
			);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {
					routeListItemWageCalculationSource1,
					routeListItemWageCalculationSource2
				}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListPercentWageCalculationService(
				percentWage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(12.5));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Расчёт ЗП для МЛ с заказами на обслуживание")]
		public void WageCalculationForServiceRouteList()
		{
			// arrange
			PercentWageParameterItem percentWage = new PercentWageParameterItem {
				PercentWageType = PercentWageTypes.Service
			};

			var orderItemWageCalculationSource1 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource1.ActualCount.Returns(12);
			orderItemWageCalculationSource1.Price.Returns(500);
			orderItemWageCalculationSource1.PercentForMaster.Returns(10);
			orderItemWageCalculationSource1.IsMasterNomenclature.Returns(true);

			var orderItemWageCalculationSource2 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource2.InitialCount.Returns(13);
			orderItemWageCalculationSource2.Price.Returns(1020);
			orderItemWageCalculationSource2.IsMasterNomenclature.Returns(true);

			var orderItemWageCalculationSource3 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource3.ActualCount.Returns(11);
			orderItemWageCalculationSource3.Price.Returns(10);
			orderItemWageCalculationSource3.IsMasterNomenclature.Returns(false);

			var orderItemWageCalculationSource4 = Substitute.For<IOrderItemWageCalculationSource>();
			orderItemWageCalculationSource4.ActualCount.Returns(1);
			orderItemWageCalculationSource4.Price.Returns(1000);
			orderItemWageCalculationSource4.PercentForMaster.Returns(50);
			orderItemWageCalculationSource4.IsMasterNomenclature.Returns(true);

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.OrderItemsSource.Returns(
				new List<IOrderItemWageCalculationSource> {
					orderItemWageCalculationSource1,
					orderItemWageCalculationSource2,
					orderItemWageCalculationSource3
				}
			);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.OrderItemsSource.Returns(
				new List<IOrderItemWageCalculationSource> {
					orderItemWageCalculationSource4
				}
			);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {
					routeListItemWageCalculationSource1,
					routeListItemWageCalculationSource2
				}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListPercentWageCalculationService(
				percentWage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(1100));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}
	}
}
