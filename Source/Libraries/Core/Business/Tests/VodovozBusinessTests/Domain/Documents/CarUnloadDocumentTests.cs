using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;

namespace VodovozBusinessTests.Domain.Documents
{
	[TestFixture]
	public class CarUnloadDocumentTests
	{
		[Test(Description = "Проверка, что при смене склада в строке документа рузгрузки, так же меняется входящий склад в операции перемещения")]
		public void UpdateWarehouse_OnChangeWarehouseInCarUnloadDocumentItem_IncomingWarehouseInWarehouseMovementOperationForAllItemsAlsoChanges()
		{
			// arrange
			Warehouse warehouseMock01 = Substitute.For<Warehouse>();
			Warehouse warehouseMock02 = Substitute.For<Warehouse>();
			CarUnloadDocument unloadDocumentUnderTest = new CarUnloadDocument {
				Warehouse = warehouseMock01,
				Items = new List<CarUnloadDocumentItem> {
					new CarUnloadDocumentItem {
						GoodsAccountingOperation = new GoodsAccountingOperation {
							IncomingWarehouse = warehouseMock01
						}
					},
					new CarUnloadDocumentItem {
						GoodsAccountingOperation = new GoodsAccountingOperation {
							IncomingWarehouse = warehouseMock01
						}
					},
					new CarUnloadDocumentItem {
						GoodsAccountingOperation = new GoodsAccountingOperation {
							IncomingWarehouse = null
						}
					}
				}
			};

			// act
			unloadDocumentUnderTest.Warehouse = warehouseMock02;

			// assert
			Assert.That(unloadDocumentUnderTest.Items.All(i => i.GoodsAccountingOperation.IncomingWarehouse == warehouseMock02), Is.True);
		}
	}
}