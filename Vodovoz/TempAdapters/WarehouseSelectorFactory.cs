using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.JournalViewModels;

namespace Vodovoz.TempAdapters
{
    public class WarehouseSelectorFactory : IEntityAutocompleteSelectorFactory
    {
        public Type EntityType => typeof(Warehouse);

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
	        var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
	        
            var warehouseJournal = new WarehouseJournalViewModel(
	            journalActions,
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                new SubdivisionRepository()
            )
            {
                SelectionMode = QS.Project.Journal.JournalSelectionMode.Single
            };
            return warehouseJournal;
        }

        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            return CreateAutocompleteSelector(multipleSelect);
        }
    }
}
