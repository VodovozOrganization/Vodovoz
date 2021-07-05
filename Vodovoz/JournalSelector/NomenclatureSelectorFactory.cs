using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using QS.ViewModels;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> : IEntitySelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntitySelector
	{
		public NomenclatureSelectorFactory(ICommonServices commonServices, 
		                                   NomenclatureFilterViewModel filterViewModel,
		                                   EntitiesJournalActionsViewModel journalActionsViewModel,
		                                   IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
		                                   INomenclatureRepository nomenclatureRepository,
		                                   IUserRepository userRepository)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			filter = filterViewModel;
			this.journalActionsViewModel = journalActionsViewModel;
		}

		protected readonly ICommonServices commonServices;
		protected readonly INomenclatureRepository nomenclatureRepository;
		protected readonly IUserRepository userRepository;
		protected readonly NomenclatureFilterViewModel filter;
		protected readonly EntitiesJournalActionsViewModel journalActionsViewModel;
		protected readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;

		public Type EntityType => typeof(Nomenclature);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			NomenclaturesJournalViewModel selectorViewModel = (NomenclaturesJournalViewModel)Activator
				.CreateInstance(typeof(NomenclaturesJournalViewModel), new object[] { journalActionsViewModel, filter, 
					UnitOfWorkFactory.GetDefaultFactory, commonServices, VodovozGtkServicesConfig.EmployeeService, 
					this, counterpartySelectorFactory, nomenclatureRepository, userRepository});
			
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
