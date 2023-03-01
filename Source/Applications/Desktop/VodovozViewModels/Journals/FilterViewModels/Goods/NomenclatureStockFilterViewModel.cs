using System;
using QS.Project.Filter;
using Vodovoz.Domain.Store;
using System.Collections.Generic;
using QS.Project.Journal.EntitySelector;
using Vodovoz.ViewModels.Journals.JournalFactories;

namespace Vodovoz.FilterViewModels.Goods
{
	public class NomenclatureStockFilterViewModel : FilterViewModelBase<NomenclatureStockFilterViewModel>
	{
		private readonly IWarehouseJournalFactory _warehouseJournalFactory;
		private Warehouse _restrictWarehouse;
		private Warehouse _warehouse;
		private bool? _restrictShowArchive;
		private bool _showArchive;

		public NomenclatureStockFilterViewModel(IWarehouseJournalFactory warehouseJournalFactory)
		{
			_warehouseJournalFactory = warehouseJournalFactory ?? throw new ArgumentNullException(nameof(warehouseJournalFactory));
			WarehouseSelectorFactory = _warehouseJournalFactory.CreateSelectorFactory();
		}

		public IEntityAutocompleteSelectorFactory WarehouseSelectorFactory { get; }

		public IEnumerable<int> ExcludedNomenclatureIds { get; set; }

		public virtual Warehouse RestrictWarehouse {
			get => _restrictWarehouse;
			set {
				if(SetField(ref _restrictWarehouse, value, () => RestrictWarehouse)) {
					Warehouse = _restrictWarehouse;
					OnPropertyChanged(nameof(CanChangeWarehouse));
				}
			}
		}

		public bool CanChangeWarehouse => RestrictWarehouse == null;

		public virtual Warehouse Warehouse {
			get => _warehouse;
			set => UpdateFilterField(ref _warehouse, value, () => Warehouse);
		}

		public virtual bool? RestrictShowArchive {
			get => _restrictShowArchive;
			set {
				if(SetField(ref _restrictShowArchive, value, () => RestrictShowArchive) && _restrictShowArchive.HasValue) {
					ShowArchive = _restrictShowArchive.Value;
					OnPropertyChanged(nameof(CanChangeShowArchive));
				}
			}
		}

		public bool CanChangeShowArchive => RestrictShowArchive == null;

		public virtual bool ShowArchive {
			get => _showArchive;
			set => UpdateFilterField(ref _showArchive, value, () => ShowArchive);
		}
	}
}
