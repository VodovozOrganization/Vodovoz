using System;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.TempAdapters
{
    public class WaterJournalFactory : IEntityAutocompleteSelectorFactory
    {
	    private INomenclatureSelectorFactory _nomenclatureSelectorFactory = new NomenclatureSelectorFactory();
	    
        public Type EntityType => typeof(Nomenclature);
        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            return CreateAutocompleteSelector(multipleSelect);
        }

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            var nomenclatureFilter = new NomenclatureFilterViewModel();
            nomenclatureFilter.RestrictCategory = NomenclatureCategory.water;
            nomenclatureFilter.RestrictArchive = false;
			
            var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
            var userRepository = new UserRepository();

            var counterpartySelectorFactory =
                new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
                    ServicesConfig.CommonServices);
			
            var nomenclatureSelectorFactory = _nomenclatureSelectorFactory.CreateNomenclatureAutocompleteSelectorFactory(nomenclatureFilter);

            var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);
			
            WaterJournalViewModel waterJournal = new WaterJournalViewModel(
	            journalActions,
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                new EmployeeService(),
                nomenclatureSelectorFactory,
                counterpartySelectorFactory,
                nomenclatureRepository,
                userRepository	
            );

            return waterJournal;
        }
    }
}