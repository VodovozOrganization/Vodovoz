using System;
using System.Reflection;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.Filter;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalSelector
{
	public class NomenclatureSelectorFactory<Nomenclature, NomenclaturesJournalViewModel> : IEntitySelectorFactory
		where NomenclaturesJournalViewModel : JournalViewModelBase, IEntitySelector
	{
		public NomenclatureSelectorFactory(ICommonServices commonServices, NomenclatureFilterViewModel filterViewModel)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			filter = filterViewModel;
		}

		private readonly ICommonServices commonServices;
		private NomenclatureFilterViewModel filter;

		public Type EntityType => typeof(Nomenclature);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			NomenclaturesJournalViewModel selectorViewModel = (NomenclaturesJournalViewModel)Activator
				.CreateInstance(typeof(NomenclaturesJournalViewModel), new object[] { filter, UnitOfWorkFactory.GetDefaultFactory, commonServices });
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
