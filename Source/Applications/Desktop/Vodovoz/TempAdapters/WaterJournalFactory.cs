using System;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Settings.Database;
using Vodovoz.ViewModels.Factories;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

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
            var nomenclatureFilter = new NomenclatureFilterViewModel();
            nomenclatureFilter.RestrictCategory = NomenclatureCategory.fuel;
            nomenclatureFilter.RestrictArchive = false;

            var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
            var userRepository = new UserRepository();
            var counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());

			WaterJournalViewModel waterJournal = new WaterJournalViewModel(
                UnitOfWorkFactory.GetDefaultFactory,
                ServicesConfig.CommonServices,
                new EmployeeService(),
				new NomenclatureJournalFactory(),
                counterpartyJournalFactory,
                nomenclatureRepository,
                userRepository,
				new NomenclatureOnlineParametersProvider(
					new SettingsController(
						UnitOfWorkFactory.GetDefaultFactory,
						new Logger<SettingsController>(new LoggerFactory()))));

			waterJournal.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
            return waterJournal;
        }
    }
}
