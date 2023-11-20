using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> :
			NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>, IEntityAutocompleteSelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
	{
		private readonly ILifetimeScope _lifetimeScope;

		public NomenclatureAutoCompleteSelectorFactory(
			ICommonServices commonServices, 
			NomenclatureFilterViewModel filterViewModel,
			ICounterpartyJournalFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository,
			ILifetimeScope lifetimeScope) 
			: base(commonServices, filterViewModel, counterpartySelectorFactory, nomenclatureRepository,
				userRepository, lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			var nomecnlatureJournalFactory = new NomenclatureJournalFactory(_lifetimeScope);
			var nomenclatureSettings = _lifetimeScope.Resolve<INomenclatureSettings>();
			NomenclaturesJournalViewModel selectorViewModel = (NomenclaturesJournalViewModel)Activator
			.CreateInstance(typeof(NomenclaturesJournalViewModel), new object[]
			{
				filter, 
				UnitOfWorkFactory.GetDefaultFactory,
				commonServices,
				VodovozGtkServicesConfig.EmployeeService,
				nomecnlatureJournalFactory,
				counterpartySelectorFactory,
				nomenclatureRepository,
				userRepository,
				nomenclatureSettings,
				null
			});
			
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
