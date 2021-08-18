using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
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
			NomenclatureStockFilterViewModel nomenclatureStockFilter = new NomenclatureStockFilterViewModel(new WarehouseRepository());
			nomenclatureStockFilter.ExcludedNomenclatureIds = excludedNomenclatures;
			nomenclatureStockFilter.RestrictWarehouse = warehouse;

			NomenclatureStockBalanceJournalViewModel vm = new NomenclatureStockBalanceJournalViewModel(
				nomenclatureStockFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices
			);

			vm.SelectionMode = JournalSelectionMode.Multiple;

			return vm;
		}
		
		public IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures = null)
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictArchive = true;
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
			
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
			
			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			
			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomenclatureRepository,
					UserSingletonRepository.GetInstance());

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				UserSingletonRepository.GetInstance()
			);

			vm.SelectionMode = JournalSelectionMode.Multiple;

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
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomenclatureRepository,
					UserSingletonRepository.GetInstance());

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				UserSingletonRepository.GetInstance()
			);

			vm.SelectionMode = JournalSelectionMode.Multiple;

			return vm;
		}

		public IEntityAutocompleteSelectorFactory GetWaterJournalFactory()
		{
			return new WaterJournalFactory();
		}

		public IEntityAutocompleteSelectorFactory GetDefaultWaterSelectorFactory()
		{
			var nomenclatureFilter = new NomenclatureFilterViewModel {HidenByDefault = true};
			nomenclatureFilter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.water,
				x => x.RestrictDilers = true
			);

			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			var nomRep = new NomenclatureRepository(new NomenclatureParametersProvider());

			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomRep,
					UserSingletonRepository.GetInstance());

			var journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomRep,
				UserSingletonRepository.GetInstance()
			) {
				SelectionMode = JournalSelectionMode.Single,
			};
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(typeof(Nomenclature), () => journalViewModel);
		}
		
		public IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory() =>
			new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() =>
				{
					var filter = new NomenclatureFilterViewModel
					{
						RestrictCategory = NomenclatureCategory.additional, RestrictArchive = false
					};

					var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
			
					var counterpartySelectorFactory =
						new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
							ServicesConfig.CommonServices);
			
					var nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, filter, counterpartySelectorFactory, nomenclatureRepository,
							UserSingletonRepository.GetInstance());

					var journal = new NomenclaturesJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						new EmployeeService(),
						nomenclatureSelectorFactory,
						counterpartySelectorFactory,
						nomenclatureRepository,
						UserSingletonRepository.GetInstance()
					)
					{
						SelectionMode = JournalSelectionMode.Single
					};

					return journal;
				}
			);

		public IEntityAutocompleteSelectorFactory GetDefaultNomenclatureSelectorFactory()
		{
			var filter = new NomenclatureFilterViewModel();

			INomenclatureRepository nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
			IUserRepository userRepository = UserSingletonRepository.GetInstance();

			var counterpartySelectorFactory = new CounterpartyJournalFactory();

			return new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				filter, counterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
				nomenclatureRepository, userRepository);
		}
	}
}
