using System;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Journals.JournalViewModels;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> :
			NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>, IEntityAutocompleteSelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntityAutocompleteSelector
	{
		public NomenclatureAutoCompleteSelectorFactory(ICommonServices commonServices, NomenclatureFilterViewModel filterViewModel) : base(commonServices, filterViewModel)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			filter = filterViewModel;
		}

		private readonly ICommonServices commonServices;
		private NomenclatureFilterViewModel filter;

		public IEntityAutocompleteSelector CreateAutocompleteSelector(bool multipleSelect = false)
		{
			NomenclaturesJournalViewModel selectorViewModel = (NomenclaturesJournalViewModel)Activator
			.CreateInstance(typeof(NomenclaturesJournalViewModel), new object[] { filter, UnitOfWorkFactory.GetDefaultFactory, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
