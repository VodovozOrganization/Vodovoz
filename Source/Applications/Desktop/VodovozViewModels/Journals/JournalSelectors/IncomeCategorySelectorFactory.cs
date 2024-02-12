using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using System.Threading.Tasks;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;

namespace Vodovoz.ViewModels.Journals.JournalSelectors
{
	public class IncomeCategorySelectorFactory : IEntitySelectorFactory
	{
		protected ILifetimeScope LifetimeScope;

		public IncomeCategorySelectorFactory(
			ILifetimeScope lifetimeScope)
		{
			LifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public Type EntityType => typeof(IncomeCategory);

		public IEntitySelector CreateSelector(bool multipleSelect = false)
		{
			var selectorViewModel = LifetimeScope.Resolve<IncomeCategoryJournalViewModel>();

			selectorViewModel.SelectionMode = JournalSelectionMode.Single;

			return selectorViewModel;
		}

		public void Dispose()
		{
			LifetimeScope = null;
		}
	}
}
