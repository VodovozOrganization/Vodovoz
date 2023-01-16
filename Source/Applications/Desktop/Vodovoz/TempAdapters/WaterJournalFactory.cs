using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
    public class WaterJournalFactory : IEntityAutocompleteSelectorFactory
    {
        public Type EntityType => typeof(Nomenclature);
        public IEntitySelector CreateSelector(bool multipleSelect = false)
        {
            return CreateAutocompleteSelector(multipleSelect);
        }

        public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
        {
            NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
            nomenclatureFilter.RestrictCategory = NomenclatureCategory.fuel;
            nomenclatureFilter.RestrictArchive = false;

            var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
            var userRepository = new UserRepository();
            var counterpartyJournalFactory = new CounterpartyJournalFactory();

            var nomenclatureSelectorFactory =
                new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
                    ServicesConfig.CommonServices, nomenclatureFilter, counterpartyJournalFactory, nomenclatureRepository, userRepository);

            WaterJournalViewModel waterJournal = new WaterJournalViewModel(
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                new EmployeeService(),
				new NomenclatureJournalFactory(),
                counterpartyJournalFactory,
                nomenclatureRepository,
                userRepository
            );

            waterJournal.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
            return waterJournal;
        }
    }
}
