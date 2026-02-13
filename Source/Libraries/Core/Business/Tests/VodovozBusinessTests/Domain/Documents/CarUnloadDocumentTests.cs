using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;

namespace VodovozBusinessTests.Domain.Documents
{
	[TestFixture]
	public class CarUnloadDocumentTests
	{
		[Test(Description = "Проверка, что при смене склада в строке документа рузгрузки, так же меняется входящий склад в операции перемещения")]
		public void UpdateWarehouse_OnChangeWarehouseInCarUnloadDocumentItem_IncomingWarehouseInWarehouseMovementOperationForAllItemsAlsoChanges()
		{
			// arrange
			var warehouseMock01 = Substitute.For<Warehouse>();
			var warehouseMock02 = Substitute.For<Warehouse>();
			var unloadDocumentUnderTest = new CarUnloadDocument {
				Warehouse = warehouseMock01,
				Items = new List<CarUnloadDocumentItem> {
					new CarUnloadDocumentItem {
						GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation {
							Warehouse = warehouseMock01
						}
					},
					new CarUnloadDocumentItem {
						GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation {
							Warehouse = warehouseMock01
						}
					},
					new CarUnloadDocumentItem {
						GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation {
							Warehouse = null
						}
					}
				}
			};

			// act
			unloadDocumentUnderTest.Warehouse = warehouseMock02;

			// assert
			Assert.That(
				unloadDocumentUnderTest.Items.All(i => i.GoodsAccountingOperation.Warehouse == warehouseMock02),
				Is.True);
		}
	}
}
