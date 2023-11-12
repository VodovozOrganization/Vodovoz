using NUnit.Framework;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;
using NSubstitute;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Domain.Employees;
using Vodovoz.Core.Domain.Employees;

namespace VodovozBusinessTests.Domain.WageCalculation.CalculationServices.RouteList
{
	[TestFixture()]
	public class RouteListOldRatesWageCalculationServiceTests
	{
		[Test(Description = "Если маршрутный лист для фуры, расчет должен вернуть 0")]
		public void CalculateWage_ForTruck_Return_0()
		{
			//arrange
			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(true);
			var service = new RouteListOldRatesWageCalculationService(new OldRatesWageParameterItem(), source);

			//act
			var result = service.CalculateWage();

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.FixedWage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Если маршрутный лист для фуры, расчет за адрес должен вернуть 0")]
		public void CalculateWageForRouteListItem_ForTruck_Return_0()
		{
			//arrange
			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(true);
			var service = new RouteListOldRatesWageCalculationService(new OldRatesWageParameterItem(), source);

			var itemSource = Substitute.For<IRouteListItemWageCalculationSource>();

			//act
			var result = service.CalculateWageForRouteListItem(itemSource);

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем компании с экспедитором, для всего маршрутного листа")]
		public void CalculateWage_ForDriverWithOurCarWithForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(true);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(true);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Address).ForDriverWithForwarder;

			//act
			var result = service.CalculateWage();

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.FixedWage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем компании с экспедитором, для адреса")]
		public void CalculateWageForRouteListItem_ForDriverWithOurCarWithForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(true);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(true);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Address).ForDriverWithForwarder;

			//act
			var result = service.CalculateWageForRouteListItem(sourceItem);

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем компании без экспедитора, для всего маршрутного листа")]
		public void CalculateWage_ForDriverWithOurCarWithoutForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(true);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(false);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Address).ForDriverWithForwarder;

			//act
			var result = service.CalculateWage();

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.FixedWage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем компании без экспедитора, для адреса")]
		public void CalculateWageForRouteListItem_ForDriverWithOurCarWithoutForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(true);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(false);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForOurs(source.RouteListDate, WageRateTypes.Address).ForDriverWithForwarder;

			//act
			var result = service.CalculateWageForRouteListItem(sourceItem);

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем не в собственности компании, без экспедитора, для всего маршрутного листа")]
		public void CalculateWage_ForDriverWithMercenariesCarWithoutForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(false);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(false);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithoutForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithoutForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithoutForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithoutForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Address).ForDriverWithoutForwarder;

			//act
			var result = service.CalculateWage();

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.FixedWage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем не в собственности компании, без экспедитора, для адреса")]
		public void CalculateWageForRouteListItem_ForDriverWithMercenariesCarWithoutForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(false);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(false);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithoutForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithoutForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithoutForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithoutForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Address).ForDriverWithoutForwarder;

			//act
			var result = service.CalculateWageForRouteListItem(sourceItem);

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем не в собственности компании, с экспедитором, для всего маршрутного листа")]
		public void CalculateWage_ForDriverWithMercenariesCarWithForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(false);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(true);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Address).ForDriverWithForwarder;

			//act
			var result = service.CalculateWage();

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.FixedWage, Is.EqualTo(0)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

		[Test(Description = "Проверка расчета зарплаты для водителя с автомобилем не в собственности компании, с экспедитором, для адреса")]
		public void CalculateWageForRouteListItem_ForDriverWithMercenariesCarWithForwarder()
		{
			//arrange
			var wageParameter = new OldRatesWageParameterItem();

			var source = Substitute.For<IRouteListWageCalculationSource>();
			source.IsTruck.Returns(false);
			source.HasAnyCompletedAddress.Returns(true);
			source.EmployeeCategory.Returns(EmployeeCategory.driver);
			source.DriverOfOurCar.Returns(false);
			source.RouteListDate.Returns(new DateTime(2019, 09, 25));

			var sourceItem = Substitute.For<IRouteListItemWageCalculationSource>();
			sourceItem.Bottle600mlCount.Returns(5);
			sourceItem.Bottle6LCount.Returns(5);
			sourceItem.FullBottle19LCount.Returns(5);
			sourceItem.EmptyBottle19LCount.Returns(5);
			sourceItem.ContractCancelation.Returns(false);
			sourceItem.DriverWageSurcharge.Returns(0);
			sourceItem.HasFirstOrderForDeliveryPoint.Returns(true);
			sourceItem.IsDelivered.Returns(true);
			sourceItem.NeedTakeOrDeliverEquipment.Returns(false);
			sourceItem.WasVisitedByForwarder.Returns(true);
			sourceItem.IsDelivered.Returns(true);

			source.ItemSources.Returns(new List<IRouteListItemWageCalculationSource> {
				{ sourceItem }
			});

			var service = new RouteListOldRatesWageCalculationService(wageParameter, source);

			decimal expectedValue = wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle19L).ForDriverWithForwarder * sourceItem.FullBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.EmptyBottle19L).ForDriverWithForwarder * sourceItem.EmptyBottle19LCount;
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Bottle6L).ForDriverWithForwarder * sourceItem.Bottle6LCount;
			expectedValue += Math.Truncate(wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.PackOfBottles600ml).ForDriverWithForwarder / 36 * sourceItem.Bottle600mlCount);
			expectedValue += wageParameter.GetRateForMercenaries(source.RouteListDate, WageRateTypes.Address).ForDriverWithForwarder;

			//act
			var result = service.CalculateWageForRouteListItem(sourceItem);

			//assert
			AssertsAccumulator.Create
				.Accumulate(() => Assert.That(result.Wage, Is.EqualTo(expectedValue)))
				.Accumulate(() => Assert.That(result.WageDistrictLevelRate, Is.EqualTo(null)))
				.Release();
		}

	}
}
