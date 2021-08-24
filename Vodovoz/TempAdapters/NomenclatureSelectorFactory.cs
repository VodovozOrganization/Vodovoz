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
			NomenclatureStockFilterViewModel nomenclatureStockFilter = new NomenclatureStockFilterViewModel(new WarehouseSelectorFactory());
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
		
		public IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true)
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictArchive = true;
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoods();
			
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			
			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomenclatureRepository, userRepository);

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				userRepository
			);

			vm.SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;

			return vm;
		}
		
		public IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(IEnumerable<int> excludedNomenclatures = null)
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictArchive = true;
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles();
			
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();
			
			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			
			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomenclatureRepository,
					userRepository);

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				userRepository
			);

			vm.SelectionMode = JournalSelectionMode.Single;

			return vm;
		}
		

		public IEntitySelector CreateNomenclatureSelectorForFuelSelect()
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictCategory = NomenclatureCategory.fuel;
			nomenclatureFilter.RestrictArchive = false;
			
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);
			
			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomenclatureRepository, userRepository);

			NomenclaturesJournalViewModel vm = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				userRepository
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
			var nomRep = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			var nomenclatureSelectorFactory =
				new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
					ServicesConfig.CommonServices, nomenclatureFilter, counterpartySelectorFactory, nomRep, userRepository);

			var journalViewModel = new NomenclaturesJournalViewModel(
				nomenclatureFilter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				nomenclatureSelectorFactory,
				counterpartySelectorFactory,
				nomRep,
				userRepository)
			{
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

					var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
					var userRepository = new UserRepository();

					var counterpartySelectorFactory =
						new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
							ServicesConfig.CommonServices);
			
					var nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, filter, counterpartySelectorFactory, nomenclatureRepository, userRepository);

					var journal = new NomenclaturesJournalViewModel(
						filter,
						UnitOfWorkFactory.GetDefaultFactory,
						ServicesConfig.CommonServices,
						new EmployeeService(),
						nomenclatureSelectorFactory,
						counterpartySelectorFactory,
						nomenclatureRepository,
						userRepository)
					{
						SelectionMode = JournalSelectionMode.Single
					};

					return journal;
				}
			);

		public IEntityAutocompleteSelectorFactory GetDefaultNomenclatureSelectorFactory()
		{
			var filter = new NomenclatureFilterViewModel();

			INomenclatureRepository nomenclatureRepository = new NomenclatureRepository(
				new NomenclatureParametersProvider(
					new ParametersProvider()));
			
			IUserRepository userRepository = new UserRepository();

			var counterpartySelectorFactory = new CounterpartyJournalFactory();

			return new NomenclatureAutoCompleteSelectorFactory<Nomenclature,NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				filter, counterpartySelectorFactory.CreateCounterpartyAutocompleteSelectorFactory(),
				nomenclatureRepository, userRepository);
		}
	}
}
