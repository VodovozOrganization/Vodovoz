using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Core;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalViewModels;
using Vodovoz.SearchViewModels;

namespace Vodovoz.TempAdapters
{
	public class NomenclatureSelectorFactory : INomenclatureSelectorFactory
	{
		public IEntitySelector CreateNomenclatureSelectorForWarehouse(Warehouse warehouse, IEnumerable<int> excludedNomenclatures)
		{
			NomenclatureStockFilterViewModel nomenclatureStockFilter = new NomenclatureStockFilterViewModel(new WarehouseRepository());
			nomenclatureStockFilter.ExcludedNomenclatureIds = excludedNomenclatures;
			nomenclatureStockFilter.RestrictWarehouse = warehouse;

			NomenclatureStockBalanceJournalViewModel vm = new NomenclatureStockBalanceJournalViewModel(
				nomenclatureStockFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				CriterionSearchFactory.GetMultipleEntryCriterionSearchViewModel()
			);

			vm.SelectionMode = JournalSelectionMode.Multiple;

			return vm;
		}
	}
}
