using System.Collections.Generic;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
	public class NomenclatureJournalFactory : INomenclatureJournalFactory
	{
		public NomenclaturesJournalViewModel CreateNomenclaturesJournalViewModel(
			NomenclatureFilterViewModel filter = null, bool multiselect = false)
		{
			var nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider(new ParametersProvider()));
			var userRepository = new UserRepository();
			var counterpartyJournalFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());

			var vm = new NomenclaturesJournalViewModel(
				filter ?? new NomenclatureFilterViewModel(),
				UnitOfWorkFactory.GetDefaultFactory,
				ServicesConfig.CommonServices,
				new EmployeeService(),
				new NomenclatureJournalFactory(),
				counterpartyJournalFactory,
				nomenclatureRepository,
				userRepository,
				null
			);

			vm.SelectionMode = multiselect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return vm;
		}

		public IEntitySelector CreateNomenclatureSelector(IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictArchive = true,
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods());

			return CreateNomenclaturesJournalViewModel(filter, multipleSelect);
		}

		public IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(IEnumerable<int> excludedNomenclatures = null)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictArchive = true,
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles());

			return CreateNomenclaturesJournalViewModel(filter);
		}


		public IEntitySelector CreateNomenclatureSelectorForFuelSelect()
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.fuel,
				x => x.RestrictArchive = false);

			return CreateNomenclaturesJournalViewModel(filter, true);
		}

		public IEntityAutocompleteSelectorFactory GetWaterJournalFactory()
		{
			return new WaterJournalFactory();
		}

		public IEntityAutocompleteSelectorFactory GetDefaultWaterSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() =>
				{
					var filter = new NomenclatureFilterViewModel { HidenByDefault = true };
					filter.SetAndRefilterAtOnce(
						x => x.RestrictCategory = NomenclatureCategory.water,
						x => x.RestrictDilers = true
					);

					return CreateNomenclaturesJournalViewModel(filter);
				});
		}

		public IEntityAutocompleteSelectorFactory GetNotArchiveEquipmentsSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() =>
				{
					var filter = new NomenclatureFilterViewModel { HidenByDefault = true };
					filter.SetAndRefilterAtOnce(
						x => x.RestrictCategory = NomenclatureCategory.equipment,
						x => x.RestrictArchive = false
					);

					return CreateNomenclaturesJournalViewModel(filter);
				});
		}

		public IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory() =>
			new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() =>
				{
					var filter = new NomenclatureFilterViewModel();
					filter.SetAndRefilterAtOnce(
						x => x.RestrictCategory = NomenclatureCategory.additional,
						x => x.RestrictArchive = false);

					return CreateNomenclaturesJournalViewModel(filter);
				}
			);

		public IEntityAutocompleteSelectorFactory GetDefaultNomenclatureSelectorFactory(NomenclatureFilterViewModel filter = null)
		{
			if(filter == null)
			{
				filter = new NomenclatureFilterViewModel();
			}

			INomenclatureRepository nomenclatureRepository = new NomenclatureRepository(
				new NomenclatureParametersProvider(new ParametersProvider()));

			IUserRepository userRepository = new UserRepository();

			var counterpartySelectorFactory = new CounterpartyJournalFactory(Startup.AppDIContainer.BeginLifetimeScope());

			return new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(ServicesConfig.CommonServices,
				filter, counterpartySelectorFactory, nomenclatureRepository, userRepository);
		}

		public IEntityAutocompleteSelectorFactory GetRoboatsWaterJournalFactory()
		{
			return new EntityAutocompleteSelectorFactory<RoboatsWaterNomenclatureJournalViewModel>(
				typeof(Nomenclature),
				() => new RoboatsWaterNomenclatureJournalViewModel(UnitOfWorkFactory.GetDefaultFactory, ServicesConfig.CommonServices)
				{
					SelectionMode = JournalSelectionMode.Single,
				});
		}

		public IEntityAutocompleteSelectorFactory GetDepositSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				GetDepositJournal);
		}

		private NomenclaturesJournalViewModel GetDepositJournal()
		{
			var filter = new NomenclatureFilterViewModel { HidenByDefault = true };
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.deposit
			);

			var journalViewModel = CreateNomenclaturesJournalViewModel(filter);
			journalViewModel.HideButtons();
			
			return journalViewModel;
		}

		public IEntityAutocompleteSelectorFactory GetServiceSelectorFactory()
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				GetServiceJournal);
		}

		private NomenclaturesJournalViewModel GetServiceJournal()
		{
			var filter = new NomenclatureFilterViewModel { HidenByDefault = true };
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.service
			);

			var journalViewModel = CreateNomenclaturesJournalViewModel(filter);
			journalViewModel.HideButtons();
			
			return journalViewModel;
		}
	}
}
