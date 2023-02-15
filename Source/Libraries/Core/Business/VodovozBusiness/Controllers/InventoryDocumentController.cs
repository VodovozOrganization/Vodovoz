using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Stock;

namespace Vodovoz.Controllers
{
	public class InventoryDocumentController
	{
		private readonly InventoryDocument _inventoryDocument;
		private readonly IInteractiveService _interactiveService;

		public InventoryDocumentController(
			InventoryDocument inventoryDocument,
			IInteractiveService interactiveService)
		{
			_inventoryDocument = inventoryDocument ?? throw new ArgumentNullException(nameof(inventoryDocument));
			_interactiveService = interactiveService ?? throw new ArgumentNullException(nameof(interactiveService));
		}
		
		public bool ConfirmInventoryDocument()
		{
			//TODO проверка на расхождения в экземплярном учете
			
			if(_inventoryDocument.InstanceItems.Any(x => !string.IsNullOrWhiteSpace(x.DiscrepancyDescription)))
			{
				_interactiveService.ShowMessage(
					ImportanceLevel.Warning,
					"Невозможно подтвердить документ\n" +
					"Имеются расхождения в экземплярном учете");
				return false;
			}

			_inventoryDocument.InventoryDocumentStatus = InventoryDocumentStatus.Confirmed;
			return true;
		}
		
		public void FillNomenclatureItemsFromStock(IUnitOfWork uow, Dictionary<int, decimal> selectedNomenclature)
		{
			var inStock = selectedNomenclature;

			if(inStock.Count == 0)
			{
				return;
			}

			var nomenclatures = uow.GetById<Nomenclature>(inStock.Select(p => p.Key).ToArray());

			_inventoryDocument.ObservableNomenclatureItems.Clear();

			foreach(var itemInStock in inStock)
			{
				var item = CreateNewNomenclatureItem();
				FillBulkItem(item, nomenclatures.First(x => x.Id == itemInStock.Key), itemInStock.Value, 0);
				_inventoryDocument.ObservableNomenclatureItems.Add(item);
			}
		}
		
		public void FillNomenclatureItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			NomenclatureCategory[] nomenclatureTypeToInclude,
			NomenclatureCategory[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			if(_inventoryDocument.Warehouse == null)
			{
				return;
			}

			var selectedNomenclature = stockRepository.NomenclatureInStock(
				uow,
				_inventoryDocument.Warehouse.Id,
				nomenclaturesToInclude,
				nomenclaturesToExclude,
				nomenclatureTypeToInclude,
				nomenclatureTypeToExclude,
				productGroupToInclude,
				productGroupToExclude);

			FillNomenclatureItemsFromStock(uow, selectedNomenclature);
		}
		
		public void UpdateNomenclatureItemsFromStock(
			IUnitOfWork uow,
			IStockRepository stockRepository,
			int[] nomenclaturesToInclude,
			int[] nomenclaturesToExclude,
			NomenclatureCategory[] nomenclatureTypeToInclude,
			NomenclatureCategory[] nomenclatureTypeToExclude,
			int[] productGroupToInclude,
			int[] productGroupToExclude)
		{
			if(_inventoryDocument.Warehouse == null)
			{
				return;
			}

			var inStock = stockRepository.NomenclatureInStock(
				uow,
				_inventoryDocument.Warehouse.Id,
				nomenclaturesToInclude,
				nomenclaturesToExclude,
				nomenclatureTypeToInclude,
				nomenclatureTypeToExclude,
				productGroupToInclude,
				productGroupToExclude,
				_inventoryDocument.TimeStamp);

			foreach(var itemInStock in inStock)
			{
				var item = _inventoryDocument.NomenclatureItems.FirstOrDefault(x => x.Nomenclature.Id == itemInStock.Key);
				if(item != null)
				{
					item.AmountInDB = itemInStock.Value;
				}
				else
				{
					item = CreateNewNomenclatureItem();
					FillBulkItem(item, uow.GetById<Nomenclature>(itemInStock.Key), itemInStock.Value, 0);
					_inventoryDocument.ObservableNomenclatureItems.Add(item);
				}
			}

			var itemsToRemove = new List<InventoryDocumentItem>();
			foreach(var item in _inventoryDocument.NomenclatureItems)
			{
				if(!inStock.ContainsKey(item.Nomenclature.Id))
				{
					itemsToRemove.Add(item);
				}
			}

			foreach(var item in itemsToRemove)
			{
				_inventoryDocument.ObservableNomenclatureItems.Remove(item);
			}
		}
		
		public void AddNomenclatureItem(Nomenclature nomenclature, decimal amountInDb, decimal amountInFact)
		{
			var item = CreateNewNomenclatureItem();
			FillBulkItem(item, nomenclature, amountInDb, amountInFact);
			_inventoryDocument.ObservableNomenclatureItems.Add(item);
		}
		
		public void AddInstanceItem(InstanceInventoryDocumentItem instanceInventoryDocumentItem)
		{
			_inventoryDocument.ObservableInstanceItems.Add(instanceInventoryDocumentItem);
		}
		
		//TODO проверить маппинг, возможно стоит настроить там удаление строк и операций
		public void UpdateOperations(IUnitOfWork uow)
		{
			IList<InventoryDocumentItem> itemsToDelete = new List<InventoryDocumentItem>();
			foreach(var item in _inventoryDocument.NomenclatureItems)
			{
				if(item.Difference == 0 && item.WarehouseChangeOperation != null)
				{
					uow.Delete(item.WarehouseChangeOperation);
					item.WarehouseChangeOperation = null;
				}
				
				UpdateOperation(item);

				if(item.AmountInDB == 0 && item.AmountInFact == 0)
				{
					itemsToDelete.Add(item);
				}
			}

			foreach(var item in itemsToDelete)
			{
				uow.Delete(item);
				_inventoryDocument.NomenclatureItems.Remove(item);
			}
		}
		
		private void FillBulkItem(InventoryDocumentItem item, Nomenclature nomenclature, decimal amountInDb, decimal amountInFact)
		{
			item.Nomenclature = nomenclature;
			item.AmountInDB = amountInDb;
			item.AmountInFact = amountInFact;
			item.Document = _inventoryDocument;
		}

		private InventoryDocumentItem CreateNewNomenclatureItem()
		{
			switch(_inventoryDocument.InventoryDocumentType)
			{
				case InventoryDocumentType.WarehouseInventory:
					return new WarehouseBulkInventoryDocumentItem();
				case InventoryDocumentType.EmployeeInventory:
					return new EmployeeBulkInventoryDocumentItem();
				case InventoryDocumentType.CarInventory:
					return new CarBulkInventoryDocumentItem();
				default:
					throw new InvalidOperationException("Неизвестный тип документа");
			}
		}
		
		private void UpdateOperation(InventoryDocumentItem item)
		{
			if(item.Difference != 0)
			{
				item.UpdateOperation(_inventoryDocument.TimeStamp);
			}
		}
	}
}
