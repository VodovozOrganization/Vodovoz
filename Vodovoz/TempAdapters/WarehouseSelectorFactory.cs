using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
    public class WarehouseSelectorFactory : IWarehouseSelectorFactory
    {
        public IEntityAutocompleteSelectorFactory CreateWarehouseAutocompleteSelectorFactory()
        {
            return new DefaultEntityAutocompleteSelectorFactory<Warehouse, WarehouseJournalViewModel, WarehouseJournalFilterViewModel>(ServicesConfig.CommonServices);
        }
    }
}
