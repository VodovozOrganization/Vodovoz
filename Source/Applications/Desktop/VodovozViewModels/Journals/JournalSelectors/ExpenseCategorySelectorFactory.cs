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
		protected ILifetimeScope LifetimeScope;

		public ExpenseCategorySelectorFactory(ILifetimeScope lifetimeScope)
		{
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public Type EntityType => typeof(ExpenseCategory);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			ExpenseCategoryJournalViewModel selectorViewModel = LifetimeScope.Resolve<ExpenseCategoryJournalViewModel>();
			selectorViewModel.SelectionMode = JournalSelectionMode.Single;
			return selectorViewModel;
		}

		public void Dispose()
		{
			LifetimeScope = null;
		}
	}
}
