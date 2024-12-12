using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Infrastructure.Report.SelectableParametersFilter;

namespace Vodovoz.ViewModels.ViewModels.Suppliers
{
	public class BalanceSummaryRow
	{
		public int Num { get; set; }
		public int EntityId { get; set; }
		public string NomTitle { get; set; }
		public string InventoryNumber { get; set; }
		public int Min { get; set; }
		public decimal Common => WarehousesBalances.Sum() + EmployeesBalances.Sum() + CarsBalances.Sum();
		public decimal Diff => Common - Min;
		public decimal? ReservedItemsAmount { get; set; } = 0;
		public decimal? AvailableItemsAmount => Common - ReservedItemsAmount;
		public decimal PurchasePrice { get; set; }
		public decimal Price { get; set; }
		public decimal AlternativePrice { get; set; }
		public List<decimal> WarehousesBalances { get; set; }
		public List<decimal> EmployeesBalances { get; set; }
		public List<decimal> CarsBalances { get; set; }
		public bool HasGreaterThanZeroBalance { get; private set; }
		public bool HasLessOrEqualZeroBalance { get; private set; }
		public bool HasLessThanMinBalance { get; private set; }
		public bool HasGreaterOrEqualThanMinBalance { get; private set; }
		
		public void FillStoragesBalance(
			StorageType storageType,
			IList<SelectableParameter> storages,
			int entityId,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> storagesResult,
			CancellationToken cancellationToken)
		{
			switch(storageType)
			{
				case StorageType.Warehouse:
					FillStoragesBalance(WarehousesBalances, storages, entityId, storagesResult, cancellationToken);
					break;
				case StorageType.Employee:
					FillStoragesBalance(EmployeesBalances, storages, entityId, storagesResult, cancellationToken);
					break;
				case StorageType.Car:
					FillStoragesBalance(CarsBalances, storages, entityId, storagesResult, cancellationToken);
					break;
			}
		}
		
		private void FillStoragesBalance(
			IList<decimal> storagesBalances,
			IList<SelectableParameter> storages,
			int entityId,
			IReadOnlyDictionary<NomenclatureStorageIds, BalanceBean> storagesResult,
			CancellationToken cancellationToken)
		{
			for(var i = 0; i < storages?.Count; i++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				
				var key = new NomenclatureStorageIds(entityId, (int)storages[i].Value);
				storagesResult.TryGetValue(key, out var tempBulkBalanceBean);
				var amount = tempBulkBalanceBean?.Amount ?? 0;

				FillBalanceParameters(amount);

				storagesBalances.Add(amount);
			}
		}

		private void FillBalanceParameters(decimal amount)
		{
			var zeroBalance = default(decimal);
			
			if(amount > zeroBalance)
			{
				HasGreaterThanZeroBalance = true;
			}

			if(amount <= zeroBalance)
			{
				HasLessOrEqualZeroBalance = true;
			}

			if(amount < Min)
			{
				HasLessThanMinBalance = true;
			}

			if(amount >= Min)
			{
				HasGreaterOrEqualThanMinBalance = true;
			}
		}
	}
}
