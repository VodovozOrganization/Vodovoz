using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> : IEntitySelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntitySelector
	{
		public NomenclatureSelectorFactory(ICommonServices commonServices, 
		                                   NomenclatureFilterViewModel filterViewModel,
		                                   IEntityAutocompleteSelectorFactory counterpartySelectorFactory)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			filter = filterViewModel;
		}

		protected readonly ICommonServices commonServices;
		protected readonly NomenclatureFilterViewModel filter;
		protected readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;

		public Type EntityType => typeof(Nomenclature);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			NomenclaturesJournalViewModel selectorViewModel = (NomenclaturesJournalViewModel)Activator
				.CreateInstance(typeof(NomenclaturesJournalViewModel), new object[] { filter, 
					UnitOfWorkFactory.GetDefaultFactory, commonServices, VodovozGtkServicesConfig.EmployeeService, 
					this, counterpartySelectorFactory});
			
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
