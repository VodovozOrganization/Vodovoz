using System;
using QS.Project.Journal.EntitySelector;
using System.Collections.Generic;
using QS.Project.Journal;
using Vodovoz.Domain.Store;

namespace Vodovoz.TempAdapters
{
	public interface INomenclatureSelectorFactory
	{
		IEntitySelector CreateNomenclatureSelectorForWarehouse(Warehouse warehouse, IEnumerable<int> excludedNomenclatures);
		IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures);
		IEntitySelector CreateNomenclatureSelectorForFuelSelect();

		IEntityAutocompleteSelectorFactory GetWaterJournalFactory();
	}
}
