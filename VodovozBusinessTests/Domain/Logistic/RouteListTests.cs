using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;
using System.Collections;

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

		static IEnumerable NeedMileageCheckParams()
		{
			var carMock1 = Substitute.For<Car>(); carMock1.TypeOfUse = CarTypeOfUse.CompanyGAZelle; carMock1.IsCompanyCar.Returns(true);
			var carMock2 = Substitute.For<Car>(); carMock2.TypeOfUse = CarTypeOfUse.CompanyLargus; carMock2.IsCompanyCar.Returns(true);
			var carMock3 = Substitute.For<Car>(); carMock3.TypeOfUse = CarTypeOfUse.CompanyTruck; carMock3.IsCompanyCar.Returns(true);
			var carMock4 = Substitute.For<Car>(); carMock4.TypeOfUse = CarTypeOfUse.DriverCar; carMock4.IsCompanyCar.Returns(false);

			yield return new TestCaseData(carMock1).Returns(true).SetName(carMock1.TypeOfUse.ToString());
			yield return new TestCaseData(carMock2).Returns(true).SetName(carMock2.TypeOfUse.ToString());
			yield return new TestCaseData(carMock3).Returns(false).SetName(carMock3.TypeOfUse.ToString());
			yield return new TestCaseData(carMock4).Returns(false).SetName(carMock4.TypeOfUse.ToString());
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
