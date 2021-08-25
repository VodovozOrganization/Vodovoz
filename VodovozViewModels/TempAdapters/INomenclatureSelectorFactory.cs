using QS.Project.Journal.EntitySelector;
using System.Collections.Generic;
using Vodovoz.Domain.Store;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
	public interface INomenclatureSelectorFactory
	{
		IEntitySelector CreateNomenclatureSelectorForWarehouse(Warehouse warehouse, IEnumerable<int> excludedNomenclatures);
		IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true);
		IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(IEnumerable<int> excludedNomenclatures = null);
		IEntitySelector CreateNomenclatureSelectorForFuelSelect();
		IEntityAutocompleteSelectorFactory GetWaterJournalFactory();
		IEntityAutocompleteSelectorFactory GetDefaultWaterSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory();
		IEntityAutocompleteSelectorFactory CreateNomenclatureAutocompleteSelectorFactory(
			NomenclatureFilterViewModel filterViewModel = null, bool multipleSelect = false);
		NomenclaturesJournalViewModel CreateNomenclaturesJournal(
			NomenclatureFilterViewModel filterViewModel = null, bool multipleSelect = false);
	}
}
