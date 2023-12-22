using Autofac;
using QS.Project.Journal.EntitySelector;
using System.Collections.Generic;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
	public interface INomenclatureJournalFactory
	{
		IEntitySelector CreateNomenclatureSelector(
			ILifetimeScope lifetimeScope, IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true);
		IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(
			ILifetimeScope lifetimeScope, IEnumerable<int> excludedNomenclatures = null);
		IEntitySelector CreateNomenclatureSelectorForFuelSelect(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory GetWaterJournalFactory(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory GetRoboatsWaterJournalFactory();
		IEntityAutocompleteSelectorFactory GetDefaultWaterSelectorFactory(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory GetDepositSelectorFactory(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory GetServiceSelectorFactory(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory(ILifetimeScope lifetimeScope);
		IEntityAutocompleteSelectorFactory GetDefaultNomenclatureSelectorFactory(
			ILifetimeScope lifetimeScope, NomenclatureFilterViewModel filter = null);
		IEntityAutocompleteSelectorFactory GetNotArchiveEquipmentsSelectorFactory(ILifetimeScope lifetimeScope);
		NomenclaturesJournalViewModel CreateNomenclaturesJournalViewModel(
			ILifetimeScope lifetimeScope, NomenclatureFilterViewModel filter = null, bool multiselect = false);
	}
}
