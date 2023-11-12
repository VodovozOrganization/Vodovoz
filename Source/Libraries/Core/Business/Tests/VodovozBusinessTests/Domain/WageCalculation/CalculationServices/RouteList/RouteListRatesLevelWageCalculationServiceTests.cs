using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.WageCalculation.AdvancedWageParameters;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture]
	public class RouteListRatesLevelWageCalculationServiceTests
	{
		#region Подготовка ставок

		Dictionary<(WageDistrict wageDistrict, CarTypeOfUse carTypeOfUse), WageDistrictLevelRate> ConfigureLevelRates(
			WageDistrict district1, WageDistrict district2)
		{
			var result = new Dictionary<(WageDistrict wageDistrict, CarTypeOfUse carTypeOfUse), WageDistrictLevelRate>();

			foreach(var carTypeOfUse in Car.GetCarTypesOfUseForRatesLevelWageCalculation())
			{
				WageDistrictLevelRate rate1 = Substitute.For<WageDistrictLevelRate>();
				rate1.WageDistrict.Returns(district1);
				rate1.CarTypeOfUse = carTypeOfUse;

				rate1.WageRates.Returns(
					new List<WageRate>
					{
						new WageRate
						{
							WageRateType = WageRateTypes.Address,
							ForDriverWithoutForwarder = 01,
							ForDriverWithForwarder = 02,
							ForForwarder = 03
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle19L,
							ForDriverWithoutForwarder = 04,
							ForDriverWithForwarder = 05,
							ForForwarder = 06
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle19LInBigOrder,
							ForDriverWithoutForwarder = 07,
							ForDriverWithForwarder = 08,
							ForForwarder = 09
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle6L,
							ForDriverWithoutForwarder = 10,
							ForDriverWithForwarder = 11,
							ForForwarder = 12
						},
						new WageRate
						{
							WageRateType = WageRateTypes.ContractCancelation,
							ForDriverWithoutForwarder = 13,
							ForDriverWithForwarder = 14,
							ForForwarder = 15
						},
						new WageRate
						{
							WageRateType = WageRateTypes.EmptyBottle19L,
							ForDriverWithoutForwarder = 16,
							ForDriverWithForwarder = 17,
							ForForwarder = 18
						},
						new WageRate
						{
							WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
							ForDriverWithoutForwarder = 19,
							ForDriverWithForwarder = 20,
							ForForwarder = 21
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Equipment,
							ForDriverWithoutForwarder = 22,
							ForDriverWithForwarder = 23,
							ForForwarder = 24
						},
						new WageRate
						{
							WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
							ForDriverWithoutForwarder = 60,
							ForDriverWithForwarder = 65,
							ForForwarder = 70
						},
						new WageRate
						{
							WageRateType = WageRateTypes.PackOfBottles600ml,
							ForDriverWithoutForwarder = 28,
							ForDriverWithForwarder = 29,
							ForForwarder = 30
						},
						new WageRate
						{
							WageRateType = WageRateTypes.PhoneCompensation,
							ForDriverWithoutForwarder = 31,
							ForDriverWithForwarder = 32,
							ForForwarder = 33
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle1500ml,
							ForDriverWithoutForwarder = 34,
							ForDriverWithForwarder = 35,
							ForForwarder = 36
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle500ml,
							ForDriverWithoutForwarder = 37,
							ForDriverWithForwarder = 38,
							ForForwarder = 39
						}
					}
				);

				WageDistrictLevelRate rate2 = Substitute.For<WageDistrictLevelRate>();
				rate2.WageDistrict.Returns(district2);
				rate2.CarTypeOfUse = carTypeOfUse;

				rate2.WageRates.Returns(
					new List<WageRate>
					{
						new WageRate
						{
							WageRateType = WageRateTypes.Address,
							ForDriverWithoutForwarder = 101,
							ForDriverWithForwarder = 102,
							ForForwarder = 103
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle19L,
							ForDriverWithoutForwarder = 104,
							ForDriverWithForwarder = 105,
							ForForwarder = 106
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle19LInBigOrder,
							ForDriverWithoutForwarder = 107,
							ForDriverWithForwarder = 108,
							ForForwarder = 109
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle6L,
							ForDriverWithoutForwarder = 110,
							ForDriverWithForwarder = 111,
							ForForwarder = 112
						},
						new WageRate
						{
							WageRateType = WageRateTypes.ContractCancelation,
							ForDriverWithoutForwarder = 113,
							ForDriverWithForwarder = 114,
							ForForwarder = 115
						},
						new WageRate
						{
							WageRateType = WageRateTypes.EmptyBottle19L,
							ForDriverWithoutForwarder = 116,
							ForDriverWithForwarder = 117,
							ForForwarder = 118
						},
						new WageRate
						{
							WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
							ForDriverWithoutForwarder = 119,
							ForDriverWithForwarder = 120,
							ForForwarder = 121
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Equipment,
							ForDriverWithoutForwarder = 122,
							ForDriverWithForwarder = 123,
							ForForwarder = 124
						},
						new WageRate
						{
							WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
							ForDriverWithoutForwarder = 100,
							ForDriverWithForwarder = 110,
							ForForwarder = 120
						},
						new WageRate
						{
							WageRateType = WageRateTypes.PackOfBottles600ml,
							ForDriverWithoutForwarder = 128,
							ForDriverWithForwarder = 129,
							ForForwarder = 130
						},
						new WageRate
						{
							WageRateType = WageRateTypes.PhoneCompensation,
							ForDriverWithoutForwarder = 131,
							ForDriverWithForwarder = 132,
							ForForwarder = 133
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle1500ml,
							ForDriverWithoutForwarder = 134,
							ForDriverWithForwarder = 135,
							ForForwarder = 136
						},
						new WageRate
						{
							WageRateType = WageRateTypes.Bottle500ml,
							ForDriverWithoutForwarder = 137,
							ForDriverWithForwarder = 138,
							ForForwarder = 139
						}
					}
				);

				result.Add((district1, carTypeOfUse), rate1);
				result.Add((district2, carTypeOfUse), rate2);
			}
			return result;
		}

		#endregion Подготовка ставок

		#region Уровневые ставки без доп параметров расчета зп

		[Test(Description = "Работа с несколькими строками МЛ. Расчёт ЗП для водителя без экспедитора.")]
		public void WageCalculationForDriverWithoutForwarderBySeveralAddresses()
		{
			// arrange
			WageDistrict district1 = Substitute.For<WageDistrict>();
			district1.Id.Returns(1);
			WageDistrict district2 = Substitute.For<WageDistrict>();
			district2.Id.Returns(2);

			var rates = ConfigureLevelRates(district1, district2);

			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.Bottle500mlCount.Returns(10);
			routeListItemWageCalculationSource1.Bottle1500mlCount.Returns(22);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.ContractCancelation.Returns(true);
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource2.Bottle600mlCount.Returns(60);
			routeListItemWageCalculationSource2.Bottle500mlCount.Returns(55);
			routeListItemWageCalculationSource2.Bottle1500mlCount.Returns(11);
			routeListItemWageCalculationSource2.FullBottle19LCount.Returns(135);
			routeListItemWageCalculationSource2.EmptyBottle19LCount.Returns(101);
			routeListItemWageCalculationSource2.Bottle6LCount.Returns(50);
			routeListItemWageCalculationSource2.DriverWageSurcharge.Returns(500);
			routeListItemWageCalculationSource2.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource2.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource2.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource2.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource2.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource2.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource2.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource3 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource3.IsValidForWageCalculation.Returns(false);
			routeListItemWageCalculationSource3.IsDelivered.Returns(false);

			var routeListItemWageCalculationSource4 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource4.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource4.Bottle600mlCount.Returns(601);
			routeListItemWageCalculationSource4.Bottle500mlCount.Returns(25);
			routeListItemWageCalculationSource4.Bottle1500mlCount.Returns(13);
			routeListItemWageCalculationSource4.FullBottle19LCount.Returns(15);
			routeListItemWageCalculationSource4.EmptyBottle19LCount.Returns(11);
			routeListItemWageCalculationSource4.Bottle6LCount.Returns(7);
			routeListItemWageCalculationSource4.DriverWageSurcharge.Returns(1511);
			routeListItemWageCalculationSource4.HasFirstOrderForDeliveryPoint.Returns(false);
			routeListItemWageCalculationSource4.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource4.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource4.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource4.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource4.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource4.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.driver);
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
			Assert.That(result.Wage, Is.EqualTo(2546 + 41287 + 0 + 2140));
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

			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.Bottle500mlCount.Returns(10);
			routeListItemWageCalculationSource1.Bottle1500mlCount.Returns(1);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource2.Bottle600mlCount.Returns(60);
			routeListItemWageCalculationSource2.Bottle500mlCount.Returns(55);
			routeListItemWageCalculationSource2.Bottle1500mlCount.Returns(12);
			routeListItemWageCalculationSource2.FullBottle19LCount.Returns(135);
			routeListItemWageCalculationSource2.EmptyBottle19LCount.Returns(101);
			routeListItemWageCalculationSource2.Bottle6LCount.Returns(50);
			routeListItemWageCalculationSource2.DriverWageSurcharge.Returns(500);
			routeListItemWageCalculationSource2.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource2.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource2.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource2.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource2.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource2.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource2.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource3 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource3.IsValidForWageCalculation.Returns(false);
			routeListItemWageCalculationSource3.IsDelivered.Returns(false);

			var routeListItemWageCalculationSource4 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource4.WageDistrictOfAddress.Returns(district1);
			routeListItemWageCalculationSource4.Bottle600mlCount.Returns(601);
			routeListItemWageCalculationSource4.Bottle500mlCount.Returns(25);
			routeListItemWageCalculationSource4.Bottle1500mlCount.Returns(17);
			routeListItemWageCalculationSource4.FullBottle19LCount.Returns(15);
			routeListItemWageCalculationSource4.EmptyBottle19LCount.Returns(11);
			routeListItemWageCalculationSource4.Bottle6LCount.Returns(7);
			routeListItemWageCalculationSource4.DriverWageSurcharge.Returns(1511);
			routeListItemWageCalculationSource4.HasFirstOrderForDeliveryPoint.Returns(false);
			routeListItemWageCalculationSource4.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource4.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource4.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource4.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource4.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource4.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.driver);
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
			Assert.That(result.Wage, Is.EqualTo(1997 + 41777 + 0 + 2368));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Работа с несколькими строками МЛ. Расчёт ЗП для водителя в качестве экспедитора.")]
		public void WageCalculationForDriverAsForwarderBySeveralAddresses()
		{
			// arrange
			WageDistrict districtMock1 = Substitute.For<WageDistrict>();
			districtMock1.Id.Returns(1);

			WageDistrict districtMock2 = Substitute.For<WageDistrict>();
			districtMock2.Id.Returns(2);

			var rates = ConfigureLevelRates(districtMock1, districtMock2);

			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(districtMock1);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.Bottle500mlCount.Returns(10);
			routeListItemWageCalculationSource1.Bottle1500mlCount.Returns(33);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource2 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource2.WageDistrictOfAddress.Returns(districtMock2);
			routeListItemWageCalculationSource2.Bottle600mlCount.Returns(60);
			routeListItemWageCalculationSource2.Bottle500mlCount.Returns(55);
			routeListItemWageCalculationSource2.Bottle1500mlCount.Returns(20);
			routeListItemWageCalculationSource2.FullBottle19LCount.Returns(135);
			routeListItemWageCalculationSource2.EmptyBottle19LCount.Returns(101);
			routeListItemWageCalculationSource2.Bottle6LCount.Returns(50);
			routeListItemWageCalculationSource2.DriverWageSurcharge.Returns(500);
			routeListItemWageCalculationSource2.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource2.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource2.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource2.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource2.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource2.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource2.IsDelivered.Returns(true);

			var routeListItemWageCalculationSource3 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource3.IsValidForWageCalculation.Returns(false);
			routeListItemWageCalculationSource3.IsDelivered.Returns(false);

			var routeListItemWageCalculationSource4 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource4.WageDistrictOfAddress.Returns(districtMock1);
			routeListItemWageCalculationSource4.Bottle600mlCount.Returns(601);
			routeListItemWageCalculationSource4.Bottle500mlCount.Returns(25);
			routeListItemWageCalculationSource4.Bottle1500mlCount.Returns(1);
			routeListItemWageCalculationSource4.FullBottle19LCount.Returns(15);
			routeListItemWageCalculationSource4.EmptyBottle19LCount.Returns(11);
			routeListItemWageCalculationSource4.Bottle6LCount.Returns(7);
			routeListItemWageCalculationSource4.DriverWageSurcharge.Returns(1511);
			routeListItemWageCalculationSource4.HasFirstOrderForDeliveryPoint.Returns(false);
			routeListItemWageCalculationSource4.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource4.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource4.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource4.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource4.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource4.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.forwarder);
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
			Assert.That(result.Wage, Is.EqualTo(3314 + 43220 + 0 + 1883));
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

			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = rates.Select(x => x.Value).ToList()
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district2);
			routeListItemWageCalculationSource1.Bottle600mlCount.Returns(100);
			routeListItemWageCalculationSource1.Bottle500mlCount.Returns(10);
			routeListItemWageCalculationSource1.Bottle1500mlCount.Returns(1);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(25);
			routeListItemWageCalculationSource1.Bottle6LCount.Returns(75);
			routeListItemWageCalculationSource1.DriverWageSurcharge.Returns(1000);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.NeedTakeOrDeliverEquipment.Returns(false);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.WageCalculationMethodic.Returns(rates[(district1, CarTypeOfUse.Largus)]);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.forwarder);
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
			Assert.That(result.Wage, Is.EqualTo(2162));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		#endregion Уровневые ставки без доп параметров расчета зп

		#region Уровневые ставки c учетом доп параметров расчета зп

		[Test(Description = "Расчёт ЗП для водителя без экспедитора.ДопПараметры(кол-во бутылей в заказе)")]
		public void WageCalculationForDriverWithoutForwarderWithBottlesCountAdvancedWageParameter()
		{
			// arrange
			WageDistrict district = Substitute.For<WageDistrict>();

			WageDistrictLevelRate rate = Substitute.For<WageDistrictLevelRate>();
			rate.WageDistrict.Returns(district);
			rate.WageRates.Returns(
				new List<WageRate> {
					new WageRate {
						WageRateType = WageRateTypes.Address,
						ForDriverWithoutForwarder = 1,
						ForDriverWithForwarder = 2,
						ForForwarder = 3,
						ChildrenParameters = new List<AdvancedWageParameter>
							{
									new BottlesCountAdvancedWageParameter
										{
											BottlesFrom = 2,
											LeftSing = ComparisonSings.LessOrEqual,
											RightSing = ComparisonSings.Less,
											BottlesTo = 1000,
											ForDriverWithForwarder = 10,
											ForDriverWithoutForwarder = 20,
											ForForwarder = 30
										},
									new BottlesCountAdvancedWageParameter
										{
											BottlesFrom = 1,
											LeftSing = ComparisonSings.MoreOrEqual,
											ForDriverWithForwarder = 100,
											ForDriverWithoutForwarder = 200,
											ForForwarder = 300
										}
							}
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle6L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.ContractCancelation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Equipment,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
						ForDriverWithoutForwarder = int.MaxValue,
						ForDriverWithForwarder = int.MaxValue,
						ForForwarder = int.MaxValue
					},
					new WageRate {
						WageRateType = WageRateTypes.PackOfBottles600ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.PhoneCompensation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle1500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					}
				}
			);
			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = new List<WageDistrictLevelRate> { rate }
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.HasFirstOrderForDeliveryPoint.Returns(true);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(false);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.driver);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> {routeListItemWageCalculationSource1}
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(20));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Расчёт ЗП для водителя с экспедитором.ДопПараметры(время доставки)")]
		public void WageCalculationForDriverForwarderWithDeliveryTimeAdvancedWageParameter()
		{
			// arrange
			WageDistrict district = Substitute.For<WageDistrict>();
			district.Id.Returns(1);
			WageDistrictLevelRate rate = Substitute.For<WageDistrictLevelRate>();
			rate.WageDistrict.Returns(district);
			rate.WageRates.Returns(
				new List<WageRate> {
					new WageRate {
						WageRateType = WageRateTypes.Bottle19L,
						ForDriverWithoutForwarder = 1,
						ForDriverWithForwarder = 2,
						ForForwarder = 3,
						ChildrenParameters = new List<AdvancedWageParameter>
							{
									new DeliveryTimeAdvancedWageParameter
										{
											StartTime = new TimeSpan(0, 0, 0),
											EndTime = new TimeSpan(13, 0, 0),
											ForDriverWithForwarder = 10,
											ForDriverWithoutForwarder = 20,
											ForForwarder = 30
										},
									new DeliveryTimeAdvancedWageParameter
										{
											StartTime = new TimeSpan(13,1,1),
											EndTime = new TimeSpan(23,59,59),
											ForDriverWithForwarder = 100,
											ForDriverWithoutForwarder = 200,
											ForForwarder = 300
										}
							}
					},
					new WageRate {
						WageRateType = WageRateTypes.Address,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle6L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.ContractCancelation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Equipment,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
						ForDriverWithoutForwarder = int.MaxValue,
						ForDriverWithForwarder = int.MaxValue,
						ForForwarder = int.MaxValue
					},
					new WageRate {
						WageRateType = WageRateTypes.PackOfBottles600ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.PhoneCompensation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle1500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					}
				}
			);
			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = new List<WageDistrictLevelRate> { rate }
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district);
			routeListItemWageCalculationSource1.FullBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);
			routeListItemWageCalculationSource1.DeliverySchedule.Returns((new TimeSpan(0, 0, 0), new TimeSpan(12, 0, 0)));

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.driver);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> { routeListItemWageCalculationSource1 }
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(500));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Расчёт ЗП для водителя в качестве экспедитора.ВложенныеДопПараметры(ко-во бутылей -> время доставки)")]
		public void WageCalculationForDriverAsForwarderWithNestedAdvancedWageParameters()
		{
			// arrange
			WageDistrict district = Substitute.For<WageDistrict>();
			district.Id.Returns(1);
			WageDistrictLevelRate rate = Substitute.For<WageDistrictLevelRate>();
			rate.WageDistrict.Returns(district);
			rate.WageRates.Returns(
				new List<WageRate> {
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19L,
						ForDriverWithoutForwarder = 1,
						ForDriverWithForwarder = 2,
						ForForwarder = 3,
						ChildrenParameters = new List<AdvancedWageParameter>
							{
									new BottlesCountAdvancedWageParameter
										{
											BottlesFrom = 0,
											LeftSing = ComparisonSings.Less,
											RightSing = ComparisonSings.Less,
											BottlesTo = 4,
											ForDriverWithForwarder = 10,
											ForDriverWithoutForwarder = 20,
											ForForwarder = 30,
											ChildrenParameters = new List<AdvancedWageParameter>
											{
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(0, 0, 0),
															EndTime = new TimeSpan(6, 0, 0),
															ForDriverWithForwarder = 100,
															ForDriverWithoutForwarder = 200,
															ForForwarder = 300
														},
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(6,1,0),
															EndTime = new TimeSpan(23,59,59),
															ForDriverWithForwarder = 1000,
															ForDriverWithoutForwarder = 2000,
															ForForwarder = 3000
														}
											}
										},
									new BottlesCountAdvancedWageParameter
										{
											BottlesFrom = 5,
											LeftSing = ComparisonSings.MoreOrEqual,
											ForDriverWithForwarder = 40,
											ForDriverWithoutForwarder = 50,
											ForForwarder = 60,
											ChildrenParameters = new List<AdvancedWageParameter>
											{
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(0, 0, 0),
															EndTime = new TimeSpan(6, 0, 0),
															ForDriverWithForwarder = 400,
															ForDriverWithoutForwarder = 500,
															ForForwarder = 600
														},
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(6,1,0),
															EndTime = new TimeSpan(23,59,59),
															ForDriverWithForwarder = 4000,
															ForDriverWithoutForwarder = 5000,
															ForForwarder = 6000
														}
											}
										}
							}
					},
					new WageRate {
						WageRateType = WageRateTypes.Address,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle6L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.ContractCancelation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Equipment,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
						ForDriverWithoutForwarder = int.MaxValue,
						ForDriverWithForwarder = int.MaxValue,
						ForForwarder = int.MaxValue
					},
					new WageRate {
						WageRateType = WageRateTypes.PackOfBottles600ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.PhoneCompensation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle1500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					}
				}
			);
			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = new List<WageDistrictLevelRate> { rate }
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);
			routeListItemWageCalculationSource1.DeliverySchedule.Returns((new TimeSpan(3, 0, 0), new TimeSpan(4, 0, 0)));

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.forwarder);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> { routeListItemWageCalculationSource1 }
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(30000));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		[Test(Description = "Расчёт ЗП для водителя в качестве экспедитора.ВложенныеДопПараметры(ко-во бутылей -> время доставки(провальные параметры))")]
		public void WageCalculationForDriverAsForwarderWithNestedFailedAdvancedWageParameters()
		{
			// arrange
			WageDistrict district = Substitute.For<WageDistrict>();
			district.Id.Returns(1);
			WageDistrictLevelRate rate = Substitute.For<WageDistrictLevelRate>();
			rate.WageDistrict.Returns(district);
			rate.WageRates.Returns(
				new List<WageRate> {
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19L,
						ForDriverWithoutForwarder = 1,
						ForDriverWithForwarder = 2,
						ForForwarder = 3,
						ChildrenParameters = new List<AdvancedWageParameter>
							{
									new BottlesCountAdvancedWageParameter
										{
											BottlesFrom = 0,
											LeftSing = ComparisonSings.Less,
											RightSing = ComparisonSings.Less,
											BottlesTo = 4,
											ForDriverWithForwarder = 10,
											ForDriverWithoutForwarder = 20,
											ForForwarder = 30,
											ChildrenParameters = new List<AdvancedWageParameter>
											{
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(0, 0, 0),
															EndTime = new TimeSpan(2, 0, 0),
															ForDriverWithForwarder = 100,
															ForDriverWithoutForwarder = 200,
															ForForwarder = 300
														},
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(6,1,0),
															EndTime = new TimeSpan(23,59,59),
															ForDriverWithForwarder = 1000,
															ForDriverWithoutForwarder = 2000,
															ForForwarder = 3000
														}
											}
										},
									new BottlesCountAdvancedWageParameter
										{
											BottlesFrom = 5,
											LeftSing = ComparisonSings.MoreOrEqual,
											ForDriverWithForwarder = 40,
											ForDriverWithoutForwarder = 50,
											ForForwarder = 60,
											ChildrenParameters = new List<AdvancedWageParameter>
											{
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(0, 0, 0),
															EndTime = new TimeSpan(2, 0, 0),
															ForDriverWithForwarder = 400,
															ForDriverWithoutForwarder = 500,
															ForForwarder = 600
														},
													new DeliveryTimeAdvancedWageParameter
														{
															StartTime = new TimeSpan(6,1,0),
															EndTime = new TimeSpan(23,59,59),
															ForDriverWithForwarder = 4000,
															ForDriverWithoutForwarder = 5000,
															ForForwarder = 6000
														}
											}
										}
							}
					},
					new WageRate {
						WageRateType = WageRateTypes.Address,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle6L,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.ContractCancelation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0,
					},
					new WageRate {
						WageRateType = WageRateTypes.EmptyBottle19LInBigOrder,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Equipment,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.MinBottlesQtyInBigOrder,
						ForDriverWithoutForwarder = int.MaxValue,
						ForDriverWithForwarder = int.MaxValue,
						ForForwarder = int.MaxValue
					},
					new WageRate {
						WageRateType = WageRateTypes.PackOfBottles600ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.PhoneCompensation,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle1500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					},
					new WageRate {
						WageRateType = WageRateTypes.Bottle500ml,
						ForDriverWithoutForwarder = 0,
						ForDriverWithForwarder = 0,
						ForForwarder = 0
					}
				}
			);
			RatesLevelWageParameterItem wage = new RatesLevelWageParameterItem {
				WageDistrictLevelRates = new WageDistrictLevelRates {
					IsArchive = false,
					LevelRates = new List<WageDistrictLevelRate> { rate }
				}
			};

			var routeListItemWageCalculationSource1 = Substitute.For<IRouteListItemWageCalculationSource>();
			routeListItemWageCalculationSource1.WageDistrictOfAddress.Returns(district);
			routeListItemWageCalculationSource1.EmptyBottle19LCount.Returns(50);
			routeListItemWageCalculationSource1.WasVisitedByForwarder.Returns(true);
			routeListItemWageCalculationSource1.WageCalculationMethodic.ReturnsNull();
			routeListItemWageCalculationSource1.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			routeListItemWageCalculationSource1.IsValidForWageCalculation.Returns(true);
			routeListItemWageCalculationSource1.IsDelivered.Returns(true);
			routeListItemWageCalculationSource1.DeliverySchedule.Returns((new TimeSpan(3, 0, 0), new TimeSpan(4, 0, 0)));

			IRouteListWageCalculationSource src = Substitute.For<IRouteListWageCalculationSource>();
			src.EmployeeCategory.Returns(EmployeeCategory.forwarder);
			src.ItemSources.Returns(
				new List<IRouteListItemWageCalculationSource> { routeListItemWageCalculationSource1 }
			);

			IRouteListWageCalculationService percentWageCalculationService = new RouteListRatesLevelWageCalculationService(
				wage,
				src
			);

			// act
			var result = percentWageCalculationService.CalculateWage();

			// assert
			Assert.That(result.Wage, Is.EqualTo(3000));
			Assert.That(result.FixedWage, Is.EqualTo(0));
			Assert.That(result.WageDistrictLevelRate, Is.Null);
		}

		#endregion Уровневые ставки c учетом доп параметров расчета зп
	}
}
