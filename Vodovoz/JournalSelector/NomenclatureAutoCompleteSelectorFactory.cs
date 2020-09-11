using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.FilterViewModels.Goods;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> :
			NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>, IEntityAutocompleteSelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
	{
		public NomenclatureAutoCompleteSelectorFactory(
			ICommonServices commonServices, 
			NomenclatureFilterViewModel filterViewModel,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory) 
			: base(commonServices, filterViewModel, counterpartySelectorFactory)
		{
			//this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			//filter = filterViewModel;
		}

		//private readonly ICommonServices commonServices;
		//private readonly NomenclatureFilterViewModel filter;

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
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
