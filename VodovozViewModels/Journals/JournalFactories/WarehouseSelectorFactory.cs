using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class WarehouseSelectorFactory : IEntityAutocompleteSelectorFactory
    {
		private readonly WarehouseJournalFilterViewModel _filterViewModel;

		public WarehouseSelectorFactory(WarehouseJournalFilterViewModel filterViewModel = null)
		{
			_filterViewModel = filterViewModel;
		}
		
        public Type EntityType => typeof(Warehouse);

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            var warehouseJournal = new WarehouseJournalViewModel(
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                new SubdivisionRepository(new ParametersProvider()),
				_filterViewModel
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
