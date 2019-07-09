using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Logistic;

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
	}
}
