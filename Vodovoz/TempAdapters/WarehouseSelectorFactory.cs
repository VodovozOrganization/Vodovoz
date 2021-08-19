using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.TempAdapters
{
    public class WarehouseSelectorFactory : IEntityAutocompleteSelectorFactory
    {
        public Type EntityType => typeof(Warehouse);

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            var warehouseJournal = new WarehouseJournalViewModel(
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                new SubdivisionRepository(new ParametersProvider())
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
