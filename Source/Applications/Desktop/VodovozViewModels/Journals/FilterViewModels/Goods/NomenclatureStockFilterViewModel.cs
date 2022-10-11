using System;
using QS.Project.Filter;
using Vodovoz.Domain.Store;
using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;

namespace Vodovoz.FilterViewModels.Goods
{
	public class NomenclatureStockFilterViewModel : FilterViewModelBase<NomenclatureStockFilterViewModel>
	{
		public NomenclatureStockFilterViewModel(IEntityAutocompleteSelectorFactory warehouseSelectorFactory)
		{
			WarehouseSelectorFactory = warehouseSelectorFactory ?? throw new ArgumentNullException(nameof(warehouseSelectorFactory));
		}

		public IEntityAutocompleteSelectorFactory WarehouseSelectorFactory { get; }

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
