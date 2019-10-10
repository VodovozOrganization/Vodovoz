using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture]
	public class RouteListRatesLevelWageCalculationServiceTests
	{
		#region Подготовка ставок

		Dictionary<WageDistrict, WageDistrictLevelRate> ConfigureLevelRates(WageDistrict district1, WageDistrict district2)
		{
			WageDistrictLevelRate rate1 = Substitute.For<WageDistrictLevelRate>();
			rate1.WageDistrict.Returns(district1);
			rate1.WageRates.Returns(
				new List<WageRate> {
					new WageRate {
						WageRateType = WageRateTypes.Address,
						ForDriverWithoutForwarder = 01,
						ForDriverWithForwarder = 02,
						ForForwarder = 03
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19L,
						ForDriverWithoutForwarder = 04,
						ForDriverWithForwarder = 05,
						ForForwarder = 06
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19LInBigOrder,
						ForDriverWithoutForwarder = 07,
						ForDriverWithForwarder = 08,
						ForForwarder = 09
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle6L,
						ForDriverWithoutForwarder = 10,
						ForDriverWithForwarder = 11,
						ForForwarder = 12
					},
					new WageRate {
						WageRateType = WageRateTypes.ContractCancelation,
						ForDriverWithoutForwarder = 13,
						ForDriverWithForwarder = 14,
						ForForwarder = 15
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19L,
						ForDriverWithoutForwarder = 16,
						ForDriverWithForwarder = 17,
						ForForwarder = 18
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
						ForDriverWithoutForwarder = 19,
						ForDriverWithForwarder = 20,
						ForForwarder = 21
					},
					new WageRate {
						WageRateType = WageRateTypes.Equipment,
						ForDriverWithoutForwarder = 22,
						ForDriverWithForwarder = 23,
						ForForwarder = 24
					},
					new WageRate {
						WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
						ForDriverWithoutForwarder = 60,
						ForDriverWithForwarder = 65,
						ForForwarder = 70
					},
					new WageRate {
						WageRateType = WageRateTypes.PackOfBottles600ml,
						ForDriverWithoutForwarder = 28,
						ForDriverWithForwarder = 29,
						ForForwarder = 30
					},
					new WageRate {
						WageRateType = WageRateTypes.PhoneCompensation,
						ForDriverWithoutForwarder = 31,
						ForDriverWithForwarder = 32,
						ForForwarder = 33
					}
				}
			);

			WageDistrictLevelRate rate2 = Substitute.For<WageDistrictLevelRate>();
			rate2.WageDistrict.Returns(district2);
			rate2.WageRates.Returns(
				new List<WageRate> {
					new WageRate {
						WageRateType = WageRateTypes.Address,
						ForDriverWithoutForwarder = 101,
						ForDriverWithForwarder = 102,
						ForForwarder = 103
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19L,
						ForDriverWithoutForwarder = 104,
						ForDriverWithForwarder = 105,
						ForForwarder = 106
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19LInBigOrder,
						ForDriverWithoutForwarder = 107,
						ForDriverWithForwarder = 108,
						ForForwarder = 109
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle6L,
						ForDriverWithoutForwarder = 110,
						ForDriverWithForwarder = 111,
						ForForwarder = 112
					},
					new WageRate {
						WageRateType = WageRateTypes.ContractCancelation,
						ForDriverWithoutForwarder = 113,
						ForDriverWithForwarder = 114,
						ForForwarder = 115
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19L,
						ForDriverWithoutForwarder = 116,
						ForDriverWithForwarder = 117,
						ForForwarder = 118
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
						ForDriverWithoutForwarder = 119,
						ForDriverWithForwarder = 120,
						ForForwarder = 121
					},
					new WageRate {
						WageRateType = WageRateTypes.Equipment,
						ForDriverWithoutForwarder = 122,
						ForDriverWithForwarder = 123,
						ForForwarder = 124
					},
					new WageRate {
						WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
						ForDriverWithoutForwarder = 100,
						ForDriverWithForwarder = 110,
						ForForwarder = 120
					},
					new WageRate {
						WageRateType = WageRateTypes.PackOfBottles600ml,
						ForDriverWithoutForwarder = 128,
						ForDriverWithForwarder = 129,
						ForForwarder = 130
					},
					new WageRate {
						WageRateType = WageRateTypes.PhoneCompensation,
						ForDriverWithoutForwarder = 131,
						ForDriverWithForwarder = 132,
						ForForwarder = 133
					}
				}
			);
			return new Dictionary<WageDistrict, WageDistrictLevelRate> {
				{ district1, rate1 },
				{ district2, rate2 }
			};
		}

		#endregion Подготовка ставок

		[Test(Description = "Работа с несколькими строками МЛ. Расчёт ЗП для водителя без экспедитора.")]
		public void WageCalculationForDriverWithoutForwarderBySeveralAddresses()
		{
			// arrange
			WageDistrict district1 = Substitute.For<WageDistrict>();
			district1.Id.Returns(1);
			WageDistrict district2 = Substitute.For<WageDistrict>();
			district2.Id.Returns(2);

			var rates = ConfigureLevelRates(district1, district2);

			RatesLevelWageParameter wage = new RatesLevelWageParameter {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource2.Bottle600mlCount.Returns(60);
			routeListItemWageCalculationSource2.FullBottle19LCount.Returns(135);
			routeListItemWageCalculationSource2.EmptyBottle19LCount.Returns(101);
			routeListItemWageCalculationSource2.Bottle6LCount.Returns(50);
			routeListItemWageCalculationSource2.DriverWageSurcharge.Returns(500);
			routeListItemWageCalculationSource2.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource2.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource2.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource2.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource2.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource3 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource3.IsDelivered.Returns(false);

			var routeListItemWageCalculationSource4 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource4.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource4.Bottle600mlCount.Returns(601);
			routeListItemWageCalculationSource4.FullBottle19LCount.Returns(15);
			routeListItemWageCalculationSource4.EmptyBottle19LCount.Returns(11);
			routeListItemWageCalculationSource4.Bottle6LCount.Returns(7);
			routeListItemWageCalculationSource4.DriverWageSurcharge.Returns(1511);
			routeListItemWageCalculationSource4.HasFirstOrderForDeliveryPoint.Returns(false);
			routeListItemWageCalculationSource4.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource4.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource4.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource4.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(Vodovoz.Domain.Employees.EmployeeCategory.driver);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {
					routeListItemWageCalculationSource1,
					routeListItemWageCalculationSource2,
					routeListItemWageCalculationSource3,
					routeListItemWageCalculationSource4
				}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(1428 + 32278 + 0 + 773));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Работа с несколькими строками МЛ. Расчёт ЗП для водителя с экспедитором.")]
		public void WageCalculationForDriverWithForwarderBySeveralAddresses()
		{
			// arrange
			WageDistrict district1 = Substitute.For<WageDistrict>();
			district1.Id.Returns(1);
			WageDistrict district2 = Substitute.For<WageDistrict>();
			district2.Id.Returns(2);

			var rates = ConfigureLevelRates(district1, district2);

			RatesLevelWageParameter wage = new RatesLevelWageParameter {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource2.Bottle600mlCount.Returns(60);
			routeListItemWageCalculationSource2.FullBottle19LCount.Returns(135);
			routeListItemWageCalculationSource2.EmptyBottle19LCount.Returns(101);
			routeListItemWageCalculationSource2.Bottle6LCount.Returns(50);
			routeListItemWageCalculationSource2.DriverWageSurcharge.Returns(500);
			routeListItemWageCalculationSource2.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource2.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource2.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource2.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource2.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource3 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource3.IsDelivered.Returns(false);

			var routeListItemWageCalculationSource4 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource4.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource4.Bottle600mlCount.Returns(601);
			routeListItemWageCalculationSource4.FullBottle19LCount.Returns(15);
			routeListItemWageCalculationSource4.EmptyBottle19LCount.Returns(11);
			routeListItemWageCalculationSource4.Bottle6LCount.Returns(7);
			routeListItemWageCalculationSource4.DriverWageSurcharge.Returns(1511);
			routeListItemWageCalculationSource4.HasFirstOrderForDeliveryPoint.Returns(false);
			routeListItemWageCalculationSource4.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource4.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource4.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource4.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(Vodovoz.Domain.Employees.EmployeeCategory.driver);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {
					routeListItemWageCalculationSource1,
					routeListItemWageCalculationSource2,
					routeListItemWageCalculationSource3,
					routeListItemWageCalculationSource4

				}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(1582 + 32567 + 0 + 823));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Работа с несколькими строками МЛ. Расчёт ЗП для водителя в качестве экспедитора.")]
		public void WageCalculationForDriverAsForwarderBySeveralAddresses()
		{
			// arrange
			WageDistrict district1 = Substitute.For<WageDistrict>();
			district1.Id.Returns(1);
			WageDistrict district2 = Substitute.For<WageDistrict>();
			district2.Id.Returns(2);

			var rates = ConfigureLevelRates(district1, district2);

			RatesLevelWageParameter wage = new RatesLevelWageParameter {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource2.Bottle600mlCount.Returns(60);
			routeListItemWageCalculationSource2.FullBottle19LCount.Returns(135);
			routeListItemWageCalculationSource2.EmptyBottle19LCount.Returns(101);
			routeListItemWageCalculationSource2.Bottle6LCount.Returns(50);
			routeListItemWageCalculationSource2.DriverWageSurcharge.Returns(500);
			routeListItemWageCalculationSource2.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource2.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource2.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource2.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource2.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource3 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource3.IsDelivered.Returns(false);

			var routeListItemWageCalculationSource4 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource4.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource4.Bottle600mlCount.Returns(601);
			routeListItemWageCalculationSource4.FullBottle19LCount.Returns(15);
			routeListItemWageCalculationSource4.EmptyBottle19LCount.Returns(11);
			routeListItemWageCalculationSource4.Bottle6LCount.Returns(7);
			routeListItemWageCalculationSource4.DriverWageSurcharge.Returns(1511);
			routeListItemWageCalculationSource4.HasFirstOrderForDeliveryPoint.Returns(false);
			routeListItemWageCalculationSource4.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource4.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource4.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource4.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(Vodovoz.Domain.Employees.EmployeeCategory.forwarder);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {
					routeListItemWageCalculationSource1,
					routeListItemWageCalculationSource2,
					routeListItemWageCalculationSource3,
					routeListItemWageCalculationSource4
				}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(1736 + 32855 + 0 + 872));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Расчёт ЗП для водителя в качестве экспедитора по ранее сохранённой ставке.")]
		public void WageCalculationForDriverAsForwarderUsingSavedWageDistrictLevelRate()
		{
			// arrange
			WageDistrict district1 = Substitute.For<WageDistrict>();
			district1.Id.Returns(1);
			WageDistrict district2 = Substitute.For<WageDistrict>();
			district2.Id.Returns(2);

			var rates = ConfigureLevelRates(district1, district2);

			RatesLevelWageParameter wage = new RatesLevelWageParameter {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.Returns(rates[district1]);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(Vodovoz.Domain.Employees.EmployeeCategory.forwarder);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {
					routeListItemWageCalculationSource1
				}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(1736));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}
	}
}
