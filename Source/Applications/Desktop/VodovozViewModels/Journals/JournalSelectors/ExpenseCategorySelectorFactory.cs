using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public class ExpenseCategorySelectorFactory : IEntitySelectorFactory
	{
		public ExpenseCategorySelectorFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}
		protected readonly ILifetimeScope _lifetimeScope;

		public Type EntityType => typeof(ExpenseCategory);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			ExpenseCategoryJournalViewModel selectorViewModel = _lifetimeScope.Resolve<ExpenseCategoryJournalViewModel>();
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}
	}
}
