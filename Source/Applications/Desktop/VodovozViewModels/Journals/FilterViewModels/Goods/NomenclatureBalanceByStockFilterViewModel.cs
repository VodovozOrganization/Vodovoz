using System;
using System.Collections.Generic;
using QS.Project.Filter;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories.Store;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Goods
{
	public class NomenclatureBalanceByStockFilterViewModel : FilterViewModelBase<NomenclatureBalanceByStockFilterViewModel>
	{
		private Warehouse _warehouse;
		private Nomenclature _nomenclature;
		public bool CanChangeWarehouse { get; set; }
		public bool CanChangeNomenclature { get; set; }

		public NomenclatureBalanceByStockFilterViewModel(IWarehouseRepository warehouseRepository)
		{
			if(warehouseRepository == null)
			{
				throw new ArgumentNullException(nameof(warehouseRepository));
			}
			AvailableWarehouses = warehouseRepository.GetActiveWarehouse(UoW);
		}

		public virtual IList<Warehouse> AvailableWarehouses { get; }

		public virtual Warehouse Warehouse
		{
			get => _warehouse;
			set => UpdateFilterField(ref _warehouse, value);
		}

		public virtual Nomenclature Nomenclature
		{
			get => _nomenclature;
			set => UpdateFilterField(ref _nomenclature, value);
		}
	}
}
