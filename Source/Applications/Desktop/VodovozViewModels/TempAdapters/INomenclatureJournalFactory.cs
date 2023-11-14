using Autofac;
using QS.Project.Journal.EntitySelector;
using System.Collections.Generic;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
	public interface INomenclatureJournalFactory
	{
		IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true);
		IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(IEnumerable<int> excludedNomenclatures = null);
		IEntitySelector CreateNomenclatureSelectorForFuelSelect();
		IEntityAutocompleteSelectorFactory GetWaterJournalFactory(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory GetRoboatsWaterJournalFactory();
		IEntityAutocompleteSelectorFactory GetDefaultWaterSelectorFactory();
		IEntityAutocompleteSelectorFactory GetDepositSelectorFactory();
		IEntityAutocompleteSelectorFactory GetServiceSelectorFactory();
		IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory();
		IEntityAutocompleteSelectorFactory GetDefaultNomenclatureSelectorFactory(NomenclatureFilterViewModel filter = null);
		IEntityAutocompleteSelectorFactory GetNotArchiveEquipmentsSelectorFactory();
		NomenclaturesJournalViewModel CreateNomenclaturesJournalViewModel(
			NomenclatureFilterViewModel filter = null, bool multiselect = false);
	}
}
