using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Domain.Employees;
using System;
using Vodovoz.Domain.WageCalculation;

namespace VodovozBusinessTests.Domain.Logistic
{
	[TestFixture]
	public class RouteListTests
	{
		[Test(Description = "Если адрес перенесён из другого МЛ, то не удаляем и генерим сообщение")]
		public void TryRemoveAddress_IfAddressIsFromAnotherRL_ThenTheAddressWillNotBeDeletedAndMessageWillNotBeEmpty()
		{
			//arrange
			RouteList routeListSourceMock = Substitute.For<RouteList>();
			routeListSourceMock.Id.Returns(999);

			RouteListItem routeListItemRemovingMock = Substitute.For<RouteListItem>();
			routeListItemRemovingMock.WasTransfered.Returns(true);
			routeListItemRemovingMock.Order = Substitute.For<Order>();

			RouteListItem routeListItemSourceMock = Substitute.For<RouteListItem>();
			routeListItemSourceMock.RouteList.Returns(routeListSourceMock);

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			IRouteListItemRepository routeListItemRepository = Substitute.For<IRouteListItemRepository>();
			routeListItemRepository.GetTransferedFrom(uow, routeListItemRemovingMock).Returns(routeListItemSourceMock);

			RouteList routeListUnderTest = new RouteList {
				UoW = uow
			};
			routeListUnderTest.Addresses.Add(routeListItemRemovingMock);

			// act
			routeListUnderTest.TryRemoveAddress(routeListItemRemovingMock, out string msg, routeListItemRepository);

			// assert
			Assert.That(routeListUnderTest.Addresses.Count, Is.EqualTo(1));
			Assert.That(string.IsNullOrEmpty(msg), Is.False);
		}

		[Test(Description = "Если адрес - не перенос из другого МЛ, то удаляем без генерирования сообщения")]
		public void TryRemoveAddress_IfAddressIsNotTransfered_ThenTheAddressWillBeDeletedAndMessageWillNotBeGenerated()
		{
			//arrange
			RouteList routeListSourceMock = Substitute.For<RouteList>();
			routeListSourceMock.Id.Returns(999);

			RouteListItem routeListItemRemovingMock = Substitute.For<RouteListItem>();
			routeListItemRemovingMock.WasTransfered.Returns(false);
			routeListItemRemovingMock.Order = Substitute.For<Order>();

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			IRouteListItemRepository routeListItemRepository = Substitute.For<IRouteListItemRepository>();

			RouteList routeListUnderTest = new RouteList {
				UoW = uow
			};
			routeListUnderTest.Addresses.Add(routeListItemRemovingMock);

			// act
			routeListUnderTest.TryRemoveAddress(routeListItemRemovingMock, out string msg, routeListItemRepository);

			// assert
			Assert.That(routeListUnderTest.Addresses.Count, Is.EqualTo(0));
			Assert.That(string.IsNullOrEmpty(msg), Is.True);
		}

		[Test(Description = "Если автомобиль в собственности компании, не фура и водитель не выездной мастер, то его необходимо " +
			"отправлять на проверку километража")]
		public void OurCarNotTruckNotVisitingMaster_MustBeSentToMileageCheck()
		{
			//arrange
			RouteList routeList = new RouteList();
			routeList.Car = Substitute.For<Car>();
			routeList.Car.IsCompanyCar.Returns(true);
			routeList.Car.TypeOfUse.Returns(CarTypeOfUse.CompanyLargus);
			routeList.Driver = Substitute.For<Employee>();
			routeList.Driver.VisitingMaster.Returns(false);

			//act
			//assert
			Assert.That(routeList.NeedMileageCheck, Is.EqualTo(true));

		}

		[Test(Description = "Если автомобиль не в собственности компании, не фура и водитель не выездной мастер и с уровневым расчетом зарплаты, " +
			"то его не нужно отправлять на проверку километража")]
		public void MercenariesCarNotTruckDriverNotVisitingMasterDriverHaveLevelRatesWage_DontSentToMileageCheck()
		{
			//arrange
			RouteList routeList = new RouteList();
			routeList.Date = new DateTime(2019, 09, 25);
			routeList.Car = Substitute.For<Car>();
			routeList.Car.IsCompanyCar.Returns(false);
			routeList.Car.TypeOfUse.Returns(CarTypeOfUse.CompanyGAZelle);
			routeList.Driver = Substitute.For<Employee>();
			routeList.Driver.VisitingMaster.Returns(false);
			WageParameter wageParameter = Substitute.For<WageParameter>();
			wageParameter.WageParameterType.Returns(WageParameterTypes.RatesLevel);
			routeList.Driver.GetActualWageParameter(routeList.Date).Returns(wageParameter);

			//act
			//assert
			Assert.That(routeList.NeedMileageCheck, Is.EqualTo(false));
		}

		[Test(Description = "Если автомобиль не в собственности компании, не фура и водитель не выездной мастер без уровневого расчета зарплаты, " +
			"то его необходимо отправлять на проверку километража")]
		public void MercenariesCarNotTruckDriverNotVisitingMasterDriverWithoutLevelRatesWage_MustBeSentToMileageCheck()
		{
			//arrange
			RouteList routeList = new RouteList();
			routeList.Date = new DateTime(2019, 09, 25);
			routeList.Car = Substitute.For<Car>();
			routeList.Car.IsCompanyCar.Returns(false);
			routeList.Car.TypeOfUse.Returns(CarTypeOfUse.CompanyGAZelle);
			routeList.Driver = Substitute.For<Employee>();
			routeList.Driver.VisitingMaster.Returns(false);
			WageParameter wageParameter = Substitute.For<WageParameter>();
			wageParameter.WageParameterType.Returns(WageParameterTypes.OldRates);
			routeList.Driver.GetActualWageParameter(routeList.Date).Returns(wageParameter);

			//act
			//assert
			Assert.That(routeList.NeedMileageCheck, Is.EqualTo(true));
		}

		[Test(Description = "Если автомобиль фура то его не нужно отправлять на проверку километража")]
		public void CarIsTruck_DontSentToMileageCheck()
		{
			//arrange
			RouteList routeList = new RouteList();
			routeList.Car = Substitute.For<Car>();
			routeList.Car.TypeOfUse.Returns(CarTypeOfUse.CompanyTruck);

			//act
			//assert
			Assert.That(routeList.NeedMileageCheck, Is.EqualTo(false));
		}

		[Test(Description = "Если водитель выездной мастер то его не нужно отправлять на проверку километража")]
		public void DriverIsVisitingMaster_DontSentToMileageCheck()
		{
			//arrange
			RouteList routeList = new RouteList();
			routeList.Car = Substitute.For<Car>();
			routeList.Car.TypeOfUse.Returns(CarTypeOfUse.CompanyGAZelle);
			routeList.Driver = Substitute.For<Employee>();
			routeList.Driver.VisitingMaster.Returns(true);

			//act
			//assert
			Assert.That(routeList.NeedMileageCheck, Is.EqualTo(false));
		}
	}
}
