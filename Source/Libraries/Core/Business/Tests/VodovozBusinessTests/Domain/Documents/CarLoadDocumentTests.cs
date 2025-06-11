using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Logistic;
using System.Linq;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Core.Domain.Warehouses;

namespace VodovozBusinessTests.Domain.Documents
{
	[TestFixture]
	public class CarLoadDocumentTests
	{
		[Test(Description = "При заполнении талона погрузки по МЛ, создаются строки талона корректно")]
		public void FillFromRouteList_WhenPassARouteList_CarLoadItemsCreatingCorrectly()
		{
			// arrange
			Vodovoz.Domain.Logistic.RouteList routeListMock01 = Substitute.For<Vodovoz.Domain.Logistic.RouteList>();
			Warehouse warehouseMock01 = Substitute.For<Warehouse>();
			ISubdivisionRepository subdivisionRepositoryMock01 = Substitute.For<ISubdivisionRepository>();
			Nomenclature nomenclatureMock01 = Substitute.For<Nomenclature>();
			nomenclatureMock01.Id.Returns(101);
			Nomenclature nomenclatureMock02 = Substitute.For<Nomenclature>();
			nomenclatureMock02.Id.Returns(102);
			Nomenclature nomenclatureMock03 = Substitute.For<Nomenclature>();
			nomenclatureMock03.Id.Returns(103);

			List<GoodsInRouteListResultWithSpecialRequirements> listOfGoods = new List<GoodsInRouteListResultWithSpecialRequirements> {
				new GoodsInRouteListResultWithSpecialRequirements {
					NomenclatureId = nomenclatureMock01.Id,
					Amount = 10
				},
				new GoodsInRouteListResultWithSpecialRequirements {
					NomenclatureId = nomenclatureMock02.Id,
					Amount = 20
				},
				new GoodsInRouteListResultWithSpecialRequirements {
					NomenclatureId = nomenclatureMock03.Id,
					Amount = 30
				}
			};

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();
			uowMock.GetById<Nomenclature>(Arg.Any<int[]>()).Returns(
				new List<Nomenclature> {
					nomenclatureMock01,
					nomenclatureMock02,
					nomenclatureMock03
				}
			);

			IRouteListRepository routeListRepositoryMock = Substitute.For<IRouteListRepository>();
			routeListRepositoryMock.GetGoodsAndEquipsInRLWithSpecialRequirements(uowMock, routeListMock01, subdivisionRepositoryMock01, warehouseMock01).Returns(listOfGoods);

			CarLoadDocument loadDocumentUnderTest = new CarLoadDocument {
				Warehouse = warehouseMock01,
				RouteList = routeListMock01
			};

			// act
			loadDocumentUnderTest.FillFromRouteList(uowMock, routeListRepositoryMock, subdivisionRepositoryMock01, true);

			// assert
			Assert.That(loadDocumentUnderTest.Items.Count, Is.EqualTo(3));
			Assert.That(loadDocumentUnderTest.Items[0].Document, Is.EqualTo(loadDocumentUnderTest));
			Assert.That(loadDocumentUnderTest.Items[0].Nomenclature, Is.EqualTo(nomenclatureMock01));
			Assert.That(loadDocumentUnderTest.Items[0].AmountInRouteList, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock01.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[0].Amount, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock01.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[1].Document, Is.EqualTo(loadDocumentUnderTest));
			Assert.That(loadDocumentUnderTest.Items[1].Nomenclature, Is.EqualTo(nomenclatureMock02));
			Assert.That(loadDocumentUnderTest.Items[1].AmountInRouteList, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock02.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[1].Amount, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock02.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[2].Document, Is.EqualTo(loadDocumentUnderTest));
			Assert.That(loadDocumentUnderTest.Items[2].Nomenclature, Is.EqualTo(nomenclatureMock03));
			Assert.That(loadDocumentUnderTest.Items[2].AmountInRouteList, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock03.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[2].Amount, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock03.Id).Amount));
		}

		[Test(Description = "Корректное обновление колонки 'В маршрутном листе'")]
		public void UpdateInRouteListAmount_WhenCall_UpdatesQuantityCorrectly()
		{
			// arrange
			Vodovoz.Domain.Logistic.RouteList routeListMock01 = Substitute.For<Vodovoz.Domain.Logistic.RouteList>();

			Nomenclature nomenclatureMock01 = Substitute.For<Nomenclature>();
			nomenclatureMock01.Id.Returns(101);
			Nomenclature nomenclatureMock02 = Substitute.For<Nomenclature>();
			nomenclatureMock02.Id.Returns(102);
			Nomenclature nomenclatureMock03 = Substitute.For<Nomenclature>();
			nomenclatureMock03.Id.Returns(103);

			List<GoodsInRouteListResultWithSpecialRequirements> listOfGoods = new List<GoodsInRouteListResultWithSpecialRequirements> {
				new GoodsInRouteListResultWithSpecialRequirements {
					NomenclatureId = nomenclatureMock01.Id,
					Amount = 1
				},
				new GoodsInRouteListResultWithSpecialRequirements {
					NomenclatureId = nomenclatureMock02.Id,
					Amount = 2
				},
				new GoodsInRouteListResultWithSpecialRequirements {
					NomenclatureId = nomenclatureMock03.Id,
					Amount = 3
				}
			};

			IUnitOfWork uowMock = Substitute.For<IUnitOfWork>();

			IRouteListRepository routeListRepositoryMock = Substitute.For<IRouteListRepository>();
			routeListRepositoryMock.GetGoodsAndEquipsInRLWithSpecialRequirements(uowMock, routeListMock01, null).Returns(listOfGoods);

			CarLoadDocument loadDocumentUnderTest = new CarLoadDocument {
				RouteList = routeListMock01,
				Items = new List<CarLoadDocumentItem> {
					new CarLoadDocumentItem {
						Nomenclature = nomenclatureMock01,
						AmountInRouteList = 11
					},
					new CarLoadDocumentItem {
						Nomenclature = nomenclatureMock02,
						AmountInRouteList = 12
					},
					new CarLoadDocumentItem {
						Nomenclature = nomenclatureMock03,
						AmountInRouteList = 13
					}
				}
			};

			// act
			loadDocumentUnderTest.UpdateInRouteListAmount(uowMock, routeListRepositoryMock);

			// assert
			Assert.That(loadDocumentUnderTest.Items.Count, Is.EqualTo(3));
			Assert.That(loadDocumentUnderTest.Items[0].AmountInRouteList, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock01.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[1].AmountInRouteList, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock02.Id).Amount));
			Assert.That(loadDocumentUnderTest.Items[2].AmountInRouteList, Is.EqualTo(listOfGoods.FirstOrDefault(x => x.NomenclatureId == nomenclatureMock03.Id).Amount));
		}
	}
}
