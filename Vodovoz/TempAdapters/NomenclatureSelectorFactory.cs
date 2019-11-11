using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Store;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
	public class NomenclatureSelectorFactory : INomenclatureSelectorFactory
	{
		public IEntitySelector CreateNomenclatureSelectorForWarehouse(Warehouse warehouse, IEnumerable<int> excludedNomenclatures)
		{
			NomenclatureFilterViewModel filter = new NomenclatureFilterViewModel(ServicesConfig.CommonServices.InteractiveService) { HidenByDefault = true };
			filter.RestrictedLoadedWarehouse = warehouse;
			filter.RestrictedIds = excludedNomenclatures;

			NomenclaturesJournalViewModel nomenclatureJournal = new NomenclaturesJournalViewModel(
					filter,
					UnitOfWorkFactory.GetDefaultFactory,
					ServicesConfig.CommonServices
				);
			nomenclatureJournal.SelectionMode = JournalSelectionMode.Multiple;
			return nomenclatureJournal;
		}
	}
}
