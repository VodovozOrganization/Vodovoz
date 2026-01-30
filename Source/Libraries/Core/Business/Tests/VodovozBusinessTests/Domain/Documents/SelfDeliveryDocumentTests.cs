using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Operations;

namespace VodovozBusinessTests.Domain.Documents
{
	[TestFixture]
	public class SelfDeliveryDocumentTests
	{
		[Test(Description = "Если сперва отгружаем воду, затем товары двумя разными документами самовывоза, то в операциях перемещения бутылей не обнуляется кол-во бутылей")]
		public void UpdateReceptions_WhenShipWaterFirstThenGoodsWithTwoDifferentSelfDeliveryDocuments_BottlesCountWillNotBeSetToZeroInBottleMovementOperation()
		{
			// arrange
			Order order = new Order {
				ReturnedTare = 1
			};
			Nomenclature nomenclatureMock = Substitute.For<Nomenclature>();
			nomenclatureMock.Id.Returns(99);
			Warehouse warehouseMock01 = Substitute.For<Warehouse>();
			warehouseMock01.CanReceiveBottles.Returns(true);
			warehouseMock01.CanReceiveEquipment.Returns(false);
			Warehouse warehouseMock02 = Substitute.For<Warehouse>();
			warehouseMock02.CanReceiveBottles.Returns(false);
			warehouseMock02.CanReceiveEquipment.Returns(true);

			SelfDeliveryDocument selfDelivery01 = new SelfDeliveryDocument {
				TimeStamp = new DateTime(2000, 01, 01, 12, 00, 00),
				Order = order,
				Warehouse = warehouseMock01,
				ReturnedItems = new List<SelfDeliveryDocumentReturned> {
					new SelfDeliveryDocumentReturned {
						Amount = 1,
						Nomenclature = nomenclatureMock,
						GoodsAccountingOperation = Substitute.For<WarehouseBulkGoodsAccountingOperation>(),
						CounterpartyMovementOperation = Substitute.For<CounterpartyMovementOperation>()
					},
					Substitute.For<SelfDeliveryDocumentReturned>()
				}
			};

			SelfDeliveryDocument selfDelivery02 = new SelfDeliveryDocument {
				TimeStamp = new DateTime(2000, 01, 01, 12, 10, 00),
				Order = order,
				Warehouse = warehouseMock02,
				ReturnedItems = new List<SelfDeliveryDocumentReturned> {
					new SelfDeliveryDocumentReturned {
						Amount = 0,
						Nomenclature = nomenclatureMock,
						GoodsAccountingOperation = Substitute.For<WarehouseBulkGoodsAccountingOperation>()
					},
					new SelfDeliveryDocumentReturned {
						Amount = 0,
						Nomenclature = Substitute.For<Nomenclature>(),
						GoodsAccountingOperation = Substitute.For<WarehouseBulkGoodsAccountingOperation>()
					}
				}
			};

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			uow.GetById<Nomenclature>(112).Returns(Substitute.For<Nomenclature>());
			uow.GetById<Nomenclature>(99).Returns(nomenclatureMock);

			INomenclatureRepository nomenclatureRepository = Substitute.For<INomenclatureRepository>();
			nomenclatureRepository.GetDefaultBottleNomenclature(uow).Returns(nomenclatureMock);

			IBottlesRepository bottlesRepository = Substitute.For<IBottlesRepository>();
			bottlesRepository.GetEmptyBottlesFromClientByOrder(uow, nomenclatureRepository, order, 1).ReturnsForAnyArgs(order.ReturnedTare.Value);

			// act
			selfDelivery01.TareToReturn = 2;
			selfDelivery01.InitializeDefaultValues(uow, nomenclatureRepository);
			selfDelivery01.UpdateReceptions(
				uow,
				new List<GoodsReceptionVMNode>(),
				nomenclatureRepository,
				bottlesRepository
			);

			selfDelivery02.InitializeDefaultValues(uow, nomenclatureRepository);
			selfDelivery02.UpdateReceptions(
				uow,
				new List<GoodsReceptionVMNode> {
					new GoodsReceptionVMNode {
						Amount = 1,
						NomenclatureId = 112,
						Category = NomenclatureCategory.equipment
					}
				},
				nomenclatureRepository,
				bottlesRepository
			);

			// assert
			Assert.That(order.ReturnedTare, Is.EqualTo(3));
		}

		[Test(Description = "При обновлении количества номенклатур на возврат в самовывозе, происходит обновление кол-ва в строке на возврат и в складской операции")]
		public void UpdateReturnedOperations_WhenUpdateAmountOfNomenclatures_ThenUpdatesAmountInSelfDeliveryDocumentReturnedAndAmountInWarehouseMovementOperation()
		{
			// arrange
			Nomenclature nomenclatureMock = Substitute.For<Nomenclature>();
			nomenclatureMock.Id.Returns(10);
			Dictionary<int, decimal> returnedNomenclatures = new Dictionary<int, decimal> { { 10, 6 } };

			SelfDeliveryDocument selfDelivery = new SelfDeliveryDocument {
				TimeStamp = new DateTime(2000, 01, 01, 12, 00, 00),
				Order = Substitute.For<Order>(),
				Warehouse = Substitute.For<Warehouse>(),
				ReturnedItems = new List<SelfDeliveryDocumentReturned> {
					new SelfDeliveryDocumentReturned {
						Amount = 1,
						Nomenclature = nomenclatureMock,
						GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation {
							Amount = 1
						},
						CounterpartyMovementOperation = Substitute.For<CounterpartyMovementOperation>()
					}
				}
			};

			// act
			selfDelivery.UpdateReturnedOperations(Substitute.For<IUnitOfWork>(), returnedNomenclatures);

			// assert
			Assert.That(selfDelivery.ReturnedItems.Count, Is.EqualTo(1));
			Assert.That(selfDelivery.ReturnedItems.FirstOrDefault(s => s.Nomenclature == nomenclatureMock).Amount, Is.EqualTo(6));
			Assert.That(selfDelivery.ReturnedItems.FirstOrDefault(s => s.Nomenclature == nomenclatureMock).GoodsAccountingOperation.Amount, Is.EqualTo(6));
		}

		[Test(Description = "При добавлении номенклатуры на возврат, происходит создание строки на возврат в самовывозе и создание соответствующей строки номенклатуры на возврат")]
		public void UpdateReturnedOperations_WhenAddNewNomenclatureToReturn_CreateNewSelfDeliveryDocumentReturnedAndNewWarehouseMovementOperation()
		{
			// arrange
			Nomenclature nomenclatureMock = Substitute.For<Nomenclature>();
			nomenclatureMock.Id.Returns(15);
			Dictionary<int, decimal> returnedNomenclatures = new Dictionary<int, decimal> { { 15, 4 } };

			SelfDeliveryDocument selfDelivery = new SelfDeliveryDocument {
				TimeStamp = new DateTime(2000, 01, 01, 12, 00, 00),
				Order = Substitute.For<Order>(),
				Warehouse = Substitute.For<Warehouse>(),
				ReturnedItems = new List<SelfDeliveryDocumentReturned>()
			};

			IUnitOfWork uow = Substitute.For<IUnitOfWork>();
			uow.GetById<Nomenclature>(15).Returns(nomenclatureMock);

			// act
			selfDelivery.UpdateReturnedOperations(uow, returnedNomenclatures);

			// assert
			Assert.That(selfDelivery.ReturnedItems.Count, Is.EqualTo(1));
			Assert.That(selfDelivery.ReturnedItems.FirstOrDefault(s => s.Nomenclature == nomenclatureMock).Amount, Is.EqualTo(4));
			Assert.That(selfDelivery.ReturnedItems.FirstOrDefault(s => s.Nomenclature == nomenclatureMock).GoodsAccountingOperation.Amount, Is.EqualTo(4));
		}

		[Test(Description = "При выставлении в существующей номенклатуре на возврат кол-ва = 0, происходит её удаление")]
		public void UpdateReturnedOperations_WhenSetAmountOfExistingNomenclatureToZero_ThenDeleteItsSelfDeliveryDocumentReturnedObject()
		{
			// arrange
			//Order order = Substitute.For<Order>();
			Nomenclature nomenclatureMock = Substitute.For<Nomenclature>();
			nomenclatureMock.Id.Returns(19);
			Dictionary<int, decimal> returnedNomenclatures = new Dictionary<int, decimal> { { 19, 0 } };

			SelfDeliveryDocument selfDelivery = new SelfDeliveryDocument {
				TimeStamp = new DateTime(2000, 01, 01, 12, 00, 00),
				Order = Substitute.For<Order>(),
				Warehouse = Substitute.For<Warehouse>(),
				ReturnedItems = new List<SelfDeliveryDocumentReturned> {
					new SelfDeliveryDocumentReturned {
						Amount = 2,
						Nomenclature = nomenclatureMock,
						GoodsAccountingOperation = Substitute.For<WarehouseBulkGoodsAccountingOperation>()
					}
				}
			};

			// act
			selfDelivery.UpdateReturnedOperations(Substitute.For<IUnitOfWork>(), returnedNomenclatures);

			// assert
			Assert.That(selfDelivery.ReturnedItems.Count, Is.EqualTo(0));
		}

		[Test(Description = "При добавлении номенклатуры на возврат, без указания кол-ва, не происходит создания записи с этой номенклатурой и складской операции")]
		public void UpdateReturnedOperations_WhenAddNewNomenclatureToReturnWithZeroAmount_ThenSelfDeliveryDocumentReturnedAndWarehouseMovementOperationWillNotBeCreated()
		{
			// arrange
			Nomenclature nomenclatureMock = Substitute.For<Nomenclature>();
			nomenclatureMock.Id.Returns(33);
			Dictionary<int, decimal> returnedNomenclatures = new Dictionary<int, decimal> { { 33, 0 } };

			SelfDeliveryDocument selfDelivery = new SelfDeliveryDocument {
				TimeStamp = new DateTime(2000, 01, 01, 12, 00, 00),
				Order = Substitute.For<Order>(),
				Warehouse = Substitute.For<Warehouse>(),
				ReturnedItems = new List<SelfDeliveryDocumentReturned>()
			};

			// act
			selfDelivery.UpdateReturnedOperations(Substitute.For<IUnitOfWork>(), returnedNomenclatures);

			// assert
			Assert.That(selfDelivery.ReturnedItems.Count, Is.EqualTo(0));
		}
	}
}
