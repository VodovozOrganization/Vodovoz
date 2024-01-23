using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.TempAdapters
{
	public class NomenclatureJournalFactory : INomenclatureJournalFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public NomenclatureJournalFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new System.ArgumentNullException(nameof(lifetimeScope));
		}

		public NomenclaturesJournalViewModel CreateNomenclaturesJournalViewModel(
			ILifetimeScope lifetimeScope, NomenclatureFilterViewModel filter = null, bool multiselect = false)
		{
			NomenclaturesJournalViewModel journalViewModel = null;

			if(filter is null)
			{
				journalViewModel = lifetimeScope.Resolve<NomenclaturesJournalViewModel>();
			}
			else
			{
				journalViewModel = lifetimeScope.Resolve<NomenclaturesJournalViewModel>(
					new TypedParameter(typeof(NomenclatureFilterViewModel), filter));
			}

			journalViewModel.SelectionMode = multiselect ? JournalSelectionMode.Multiple : JournalSelectionMode.Single;
			return journalViewModel;
		}

		public IEntitySelector CreateNomenclatureSelector(
			ILifetimeScope lifetimeScope, IEnumerable<int> excludedNomenclatures = null, bool multipleSelect = true)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictArchive = true,
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoods());

			return CreateNomenclaturesJournalViewModel(lifetimeScope, filter, multipleSelect);
		}

		public IEntitySelector CreateNomenclatureOfGoodsWithoutEmptyBottlesSelector(
			ILifetimeScope lifetimeScope, IEnumerable<int> excludedNomenclatures = null)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictArchive = true,
				x => x.AvailableCategories = Nomenclature.GetCategoriesForGoodsWithoutEmptyBottles());

			return CreateNomenclaturesJournalViewModel(lifetimeScope, filter);
		}


		public IEntitySelector CreateNomenclatureSelectorForFuelSelect(ILifetimeScope lifetimeScope)
		{
			var filter = new NomenclatureFilterViewModel();
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.fuel,
				x => x.RestrictArchive = false);

			return CreateNomenclaturesJournalViewModel(lifetimeScope, filter, true);
		}

		public IEntityAutocompleteSelectorFactory GetWaterJournalFactory(ILifetimeScope lifetimeScope)
		{
			return new WaterJournalFactory(lifetimeScope);
		}

		public IEntityAutocompleteSelectorFactory GetDefaultWaterSelectorFactory(ILifetimeScope lifetimeScope)
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

					return CreateNomenclaturesJournalViewModel(lifetimeScope, filter);
				});
		}

		public IEntityAutocompleteSelectorFactory GetNotArchiveEquipmentsSelectorFactory(ILifetimeScope lifetimeScope)
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

					return CreateNomenclaturesJournalViewModel(lifetimeScope, filter);
				});
		}

		public IEntityAutocompleteSelectorFactory CreateNomenclatureForFlyerJournalFactory(ILifetimeScope lifetimeScope) =>
			new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() =>
				{
					var filter = new NomenclatureFilterViewModel();
					filter.SetAndRefilterAtOnce(
						x => x.RestrictCategory = NomenclatureCategory.additional,
						x => x.RestrictArchive = false);

					return CreateNomenclaturesJournalViewModel(lifetimeScope, filter);
				}
			);

		public IEntityAutocompleteSelectorFactory GetDefaultNomenclatureSelectorFactory(
			ILifetimeScope lifetimeScope, NomenclatureFilterViewModel filter = null)
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() => CreateNomenclaturesJournalViewModel(lifetimeScope, filter));
		}

		public IEntityAutocompleteSelectorFactory GetRoboatsWaterJournalFactory()
		{
			return new EntityAutocompleteSelectorFactory<RoboatsWaterNomenclatureJournalViewModel>(
				typeof(Nomenclature),
				() => new RoboatsWaterNomenclatureJournalViewModel(_lifetimeScope.Resolve<IUnitOfWorkFactory>(), ServicesConfig.CommonServices)
				{
					SelectionMode = JournalSelectionMode.Single,
				});
		}

		public IEntityAutocompleteSelectorFactory GetDepositSelectorFactory(ILifetimeScope lifetimeScope)
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() => GetDepositJournal(lifetimeScope));
		}

		private NomenclaturesJournalViewModel GetDepositJournal(ILifetimeScope lifetimeScope)
		{
			var filter = new NomenclatureFilterViewModel { HidenByDefault = true };
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.deposit
			);

			var journalViewModel = CreateNomenclaturesJournalViewModel(lifetimeScope, filter);
			journalViewModel.HideButtons();
			
			return journalViewModel;
		}

		public IEntityAutocompleteSelectorFactory GetServiceSelectorFactory(ILifetimeScope lifetimeScope)
		{
			return new EntityAutocompleteSelectorFactory<NomenclaturesJournalViewModel>(
				typeof(Nomenclature),
				() => GetServiceJournal(lifetimeScope));
		}

		private NomenclaturesJournalViewModel GetServiceJournal(ILifetimeScope lifetimeScope)
		{
			var filter = new NomenclatureFilterViewModel { HidenByDefault = true };
			filter.SetAndRefilterAtOnce(
				x => x.RestrictCategory = NomenclatureCategory.service
			);

			var journalViewModel = CreateNomenclaturesJournalViewModel(lifetimeScope, filter);
			journalViewModel.HideButtons();
			
			return journalViewModel;
		}
	}
}
