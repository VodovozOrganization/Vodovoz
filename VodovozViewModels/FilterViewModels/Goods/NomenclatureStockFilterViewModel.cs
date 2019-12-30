using System;
using QS.Project.Filter;
using QS.Services;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Store;
using System.Collections.Generic;

namespace Vodovoz.FilterViewModels.Goods
{
	public class NomenclatureStockFilterViewModel : FilterViewModelBase<NomenclatureStockFilterViewModel>
	{
		public NomenclatureStockFilterViewModel(IWarehouseRepository warehouseRepository)
		{
			if(warehouseRepository == null) {
				throw new ArgumentNullException(nameof(warehouseRepository));
			}
			AvailableWarehouses = warehouseRepository.GetActiveWarehouse(UoW);
		}

		public IEnumerable<Warehouse> AvailableWarehouses { get; set; }

		public IEnumerable<int> ExcludedNomenclatureIds { get; set; }

		private Warehouse restrictWarehouse;
		public virtual Warehouse RestrictWarehouse {
			get => restrictWarehouse;
			set {
				if(SetField(ref restrictWarehouse, value, () => RestrictWarehouse)) {
					Warehouse = restrictWarehouse;
					OnPropertyChanged(nameof(CanChangeWarehouse));
				}
			}
		}

		public bool CanChangeWarehouse => RestrictWarehouse == null;

		private Warehouse warehouse;
		public virtual Warehouse Warehouse {
			get => warehouse;
			set => UpdateFilterField(ref warehouse, value, () => Warehouse);
		}


		private bool? restrictShowArchive;
		public virtual bool? RestrictShowArchive {
			get => restrictShowArchive;
			set {
				if(SetField(ref restrictShowArchive, value, () => RestrictShowArchive) && restrictShowArchive.HasValue) {
					ShowArchive = restrictShowArchive.Value;
					OnPropertyChanged(nameof(CanChangeShowArchive));
				}
			}
		}

		public bool CanChangeShowArchive => RestrictShowArchive == null;

		private bool showArchive;

		public virtual bool ShowArchive {
			get => showArchive;
			set => UpdateFilterField(ref showArchive, value, () => ShowArchive);
		}
	}
}
