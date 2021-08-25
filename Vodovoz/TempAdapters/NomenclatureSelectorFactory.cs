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
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
	public class NomenclatureSelectorFactory : INomenclatureSelectorFactory
	{
		public IEntitySelector CreateNomenclatureSelectorForWarehouse(Warehouse warehouse, IEnumerable<int> excludedNomenclatures)
		{
			NomenclatureStockFilterViewModel nomenclatureStockFilter = new NomenclatureStockFilterViewModel(new WarehouseSelectorFactory());
			nomenclatureStockFilter.ExcludedNomenclatureIds = excludedNomenclatures;
			nomenclatureStockFilter.RestrictWarehouse = warehouse;
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
		
		public IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true)
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictArchive = true;
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoods();

			return CreateNomenclaturesJournal(nomenclatureFilter, multipleSelect);
		}
		
		public IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(IEnumerable<int> excludedNomenclatures = null)
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictArchive = true;
			nomenclatureFilter.AvailableCategories = Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles();

			return CreateNomenclaturesJournal(nomenclatureFilter);
		}
		

		public IEntitySelector CreateNomenclatureSelectorForFuelSelect()
		{
			NomenclatureFilterViewModel nomenclatureFilter = new NomenclatureFilterViewModel();
			nomenclatureFilter.RestrictCategory = NomenclatureCategory.fuel;
			nomenclatureFilter.RestrictArchive = false;

			return CreateNomenclaturesJournal(nomenclatureFilter, true);
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

			return CreateNomenclatureAutocompleteSelectorFactory(nomenclatureFilter);
		}

		public IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory()
		{
			var filter = new NomenclatureFilterViewModel
			{
				RestrictCategory = NomenclatureCategory.additional, RestrictArchive = false
			};

			return CreateNomenclatureAutocompleteSelectorFactory(filter);
		}

		public IEntityAutocompleteSelectorFactory CreateNomenclatureAutocompleteSelectorFactory(
			NomenclatureFilterViewModel filterViewModel = null, bool multipleSelect = false) =>
				new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
					typeof(Nomenclature),
					() => CreateNomenclaturesJournal(filterViewModel, multipleSelect)
				);

		public NomenclaturesJournalViewModel CreateNomenclaturesJournal(
			NomenclatureFilterViewModel filterViewModel = null, bool multipleSelect = false)
		{
			var filter = filterViewModel ?? new NomenclatureFilterViewModel(); 

			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();

			var counterpartySelectorFactory =
				new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel, CounterpartyJournalFilterViewModel>(
					ServicesConfig.CommonServices);

			var journalActions = new EntitiesJournalActionsViewModel(ServicesConfig.InteractiveService);

			var journal = new NomenclaturesJournalViewModel(
				journalActions,
				filter,
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				this,
				counterpartySelectorFactory,
				nomenclatureRepository,
				userRepository)
			{
				SelectionMode = multipleSelect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single
			};

			return journal;
		}
	}
}
