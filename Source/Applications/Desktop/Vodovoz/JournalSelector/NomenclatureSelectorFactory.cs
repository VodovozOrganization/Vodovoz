using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.FilterViewModels.Goods;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> : IEntitySelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntitySelector
	{
		public NomenclatureSelectorFactory(ICommonServices commonServices, 
		                                   NomenclatureFilterViewModel filterViewModel,
		                                   ICounterpartyJournalFactory counterpartySelectorFactory,
		                                   INomenclatureRepository nomenclatureRepository,
		                                   IUserRepository userRepository,
										   ILifetimeScope lifetimeScope)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
			filter = filterViewModel;
		}

		protected readonly ICommonServices commonServices;
		protected readonly INomenclatureRepository nomenclatureRepository;
		protected readonly IUserRepository userRepository;
		protected readonly NomenclatureFilterViewModel filter;
		protected readonly ICounterpartyJournalFactory counterpartySelectorFactory;
		private readonly ILifetimeScope _lifetimeScope;

		public Type EntityType => typeof(Nomenclature);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			var nomecnlatureJournalFactory = new NomenclatureJournalFactory(_lifetimeScope);
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
					null
				});
			
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
