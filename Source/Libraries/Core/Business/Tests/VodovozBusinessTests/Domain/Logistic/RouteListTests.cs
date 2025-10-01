using System;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using System.Collections;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Settings.Common;

namespace VodovozBusinessTests.Domain.Logistic
{
	[TestFixture]
	public class RouteListTests
	{
		[OneTimeSetUp]
		public void Init()
		{
			var generalSettingsSettingsMock = Substitute.For<IGeneralSettings>();
			generalSettingsSettingsMock.GetCanAddForwardersToLargus.Returns(true);
			generalSettingsSettingsMock.GetCanAddForwardersToMinivan.Returns(true);

			RouteList.SetGeneralSettingsSettingsGap(generalSettingsSettingsMock);
		}

		[OneTimeTearDown]
		public void Cleanup()
		{
			RouteList.SetGeneralSettingsSettingsGap(null);
		}

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
			routeListItemRepository.GetTransferredFrom(uow, routeListItemRemovingMock).Returns(routeListItemSourceMock);

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

		static IEnumerable NeedMileageCheckParams()
		{
			//Company GAZelle
			var carVersionMock1 = Substitute.For<CarVersion>();
			carVersionMock1.CarOwnType.Returns(CarOwnType.Company);
			var carModelMock1 = Substitute.For<CarModel>();
			carModelMock1.CarTypeOfUse.Returns(CarTypeOfUse.GAZelle);
			var carMock1 = Substitute.For<Car>();
			carMock1.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock1);
			carMock1.CarModel.Returns(carModelMock1);

			//Company Largus
			var carMock2 = Substitute.For<Car>();
			var carVersionMock2 = Substitute.For<CarVersion>();
			carVersionMock2.CarOwnType.Returns(CarOwnType.Company);
			var carModelMock2 = Substitute.For<CarModel>();
			carModelMock2.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			carMock2.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock2);
			carMock2.CarModel.Returns(carModelMock2);

			//Company Truck
			var carMock3 = Substitute.For<Car>();
			var carVersionMock3 = Substitute.For<CarVersion>();
			carVersionMock3.CarOwnType.Returns(CarOwnType.Company);
			var carModelMock3 = Substitute.For<CarModel>();
			carModelMock3.CarTypeOfUse.Returns(CarTypeOfUse.Truck);
			carMock3.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock3);
			carMock3.CarModel.Returns(carModelMock3);

			//Driver GAZelle
			var carMock4 = Substitute.For<Car>();
			var carVersionMock4 = Substitute.For<CarVersion>();
			carVersionMock4.CarOwnType.Returns(CarOwnType.Driver);
			var carModelMock4 = Substitute.For<CarModel>();
			carModelMock4.CarTypeOfUse.Returns(CarTypeOfUse.GAZelle);
			carMock4.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock4);
			carMock4.CarModel.Returns(carModelMock4);

			//Driver Largus
			var carMock5 = Substitute.For<Car>();
			var carVersionMock5 = Substitute.For<CarVersion>();
			carVersionMock5.CarOwnType.Returns(CarOwnType.Driver);
			var carModelMock5 = Substitute.For<CarModel>();
			carModelMock5.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			carMock5.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock5);
			carMock5.CarModel.Returns(carModelMock5);

			//Driver Truck
			var carMock6 = Substitute.For<Car>();
			var carVersionMock6 = Substitute.For<CarVersion>();
			carVersionMock6.CarOwnType.Returns(CarOwnType.Driver);
			var carModelMock6 = Substitute.For<CarModel>();
			carModelMock6.CarTypeOfUse.Returns(CarTypeOfUse.Truck);
			carMock6.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock6);
			carMock6.CarModel.Returns(carModelMock6);

			//Raskat GAZelle
			var carMock7 = Substitute.For<Car>();
			var carVersionMock7 = Substitute.For<CarVersion>();
			carVersionMock7.CarOwnType.Returns(CarOwnType.Raskat);
			var carModelMock7 = Substitute.For<CarModel>();
			carModelMock7.CarTypeOfUse.Returns(CarTypeOfUse.GAZelle);
			carMock7.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock7);
			carMock7.CarModel.Returns(carModelMock7);

			//Raskat Largus
			var carMock8 = Substitute.For<Car>();
			var carVersionMock8 = Substitute.For<CarVersion>();
			carVersionMock8.CarOwnType.Returns(CarOwnType.Raskat);
			var carModelMock8 = Substitute.For<CarModel>();
			carModelMock8.CarTypeOfUse.Returns(CarTypeOfUse.Largus);
			carMock8.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock8);
			carMock8.CarModel.Returns(carModelMock8);

			//Raskat Truck
			var carMock9 = Substitute.For<Car>();
			var carVersionMock9 = Substitute.For<CarVersion>();
			carVersionMock9.CarOwnType.Returns(CarOwnType.Raskat);
			var carModelMock9 = Substitute.For<CarModel>();
			carModelMock9.CarTypeOfUse.Returns(CarTypeOfUse.Truck);
			carMock9.GetActiveCarVersionOnDate(Arg.Any<DateTime>()).Returns(carVersionMock9);
			carMock9.CarModel.Returns(carModelMock9);

			yield return new TestCaseData(carMock1).Returns(true).SetName($"{carVersionMock1.CarOwnType} {carModelMock1.CarTypeOfUse}");
			yield return new TestCaseData(carMock2).Returns(true).SetName($"{carVersionMock2.CarOwnType} {carModelMock2.CarTypeOfUse}");
			yield return new TestCaseData(carMock3).Returns(false).SetName($"{carVersionMock3.CarOwnType} {carModelMock3.CarTypeOfUse}");
			yield return new TestCaseData(carMock4).Returns(false).SetName($"{carVersionMock4.CarOwnType} {carModelMock4.CarTypeOfUse}");
			yield return new TestCaseData(carMock5).Returns(false).SetName($"{carVersionMock5.CarOwnType} {carModelMock5.CarTypeOfUse}");
			yield return new TestCaseData(carMock6).Returns(false).SetName($"{carVersionMock6.CarOwnType} {carModelMock6.CarTypeOfUse}");
			yield return new TestCaseData(carMock7).Returns(false).SetName($"{carVersionMock7.CarOwnType} {carModelMock7.CarTypeOfUse}");
			yield return new TestCaseData(carMock8).Returns(false).SetName($"{carVersionMock8.CarOwnType} {carModelMock8.CarTypeOfUse}");
			yield return new TestCaseData(carMock9).Returns(false).SetName($"{carVersionMock9.CarOwnType} {carModelMock9.CarTypeOfUse}");
		}

		[TestCaseSource(nameof(NeedMileageCheckParams))]
		[Test(Description = "Если машина - собственность компании, но не фура, то её нужно отправлять на проверку километража. Остальные машины - не нужно")]
		public bool NeedMileageCheck_Test(Car car)
		{
			RouteList routeList = new RouteList();
			routeList.Car = car;
			return routeList.NeedMileageCheck;
		}
	}
}
