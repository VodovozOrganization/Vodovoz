using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public class IncomeCategorySelectorFactory : IEntitySelectorFactory
	{
		public IncomeCategorySelectorFactory(
			ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		protected readonly ILifetimeScope _lifetimeScope;

		public Type EntityType => typeof(IncomeCategory);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			var selectorViewModel = _lifetimeScope.Resolve<IncomeCategoryJournalViewModel>();

			selectorViewModel.SelectionMode = JournalSelectionMode.Single;

			return selectorViewModel;
		}
	}
}
