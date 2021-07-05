using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.ViewModels;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Store;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;

namespace Vodovoz.TempAdapters
{
	public class NomenclatureSelectorFactory : INomenclatureSelectorFactory
	{
		public IEntitySelector CreateNomenclatureSelectorForWarehouse(Warehouse warehouse, IEnumerable<int> excludedNomenclatures)
		{
			NomenclatureStockFilterViewModel nomenclatureStockFilter = new NomenclatureStockFilterViewModel(new WarehouseRepository())
			{
				ExcludedNomenclatureIds = excludedNomenclatures, RestrictWarehouse = warehouse
			};
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			NomenclatureStockBalanceJournalViewModel vm = new NomenclatureStockBalanceJournalViewModel(
				journalActions,
				nomenclatureStockFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			)
			{
				SelectionMode = JournalSelectionMode.Multiple
			};


			return vm;
		}
		
		public IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures)
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel
			{
				RestrictArchive = true, AvailableCategories = Nomenclature.GetCategoriesForGoods()
			};

			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
			
			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			
			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
					nomenclatureFilter, new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService),
					counterpartySelectorFactory, nomenclatureRepository, UserSingletonRepository.GetInstance());
			
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				journalActions,
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				UserSingletonRepository.GetInstance()
			)
			{
				SelectionMode = JournalSelectionMode.Multiple
			};

			return vm;
		}
		

		public IEntitySelector CreateNomenclatureSelectorForFuelSelect()
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictCategory = NomenclatureCategory.fuel;
			nomenclatureFilter.RestrictArchive = false;
			
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
			
			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			
			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
					nomenclatureFilter, new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService),
					counterpartySelectorFactory, nomenclatureRepository, UserSingletonRepository.GetInstance());
			
			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				journalActions,
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				UserSingletonRepository.GetInstance()
			)
			{
				SelectionMode = JournalSelectionMode.Multiple
			};

			return vm;
		}

		public IEntityAutocompleteSelectorFactory GetWaterJournalFactory()
		{
			return new WaterJournalFactory();
		}
	}
}
