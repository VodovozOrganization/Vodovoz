using Autofac;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using System;
using Vodovoz.Domain.Store;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalViewModels.Store;

namespace Vodovoz.ViewModels.Journals.JournalFactories
{
	public class WarehouseJournalFactory : IWarehouseJournalFactory
	{
		public IEntityAutocompleteSelectorFactory CreateSelectorFactory(
			ILifetimeScope lifetimeScope, Action<WarehouseJournalFilterViewModel> filterParams = null)
		{
			return new EntityAutocompleteSelectorFactory<WarehouseJournalViewModel>(
				typeof(Warehouse),
				() =>
				{
					WarehouseJournalViewModel journalViewModel;

					if(filterParams != null)
					{
						journalViewModel =
							lifetimeScope.Resolve<WarehouseJournalViewModel>(
								new TypedParameter(typeof(Action<WarehouseJournalFilterViewModel>),
									filterParams));
					}
					else
					{
						journalViewModel = lifetimeScope.Resolve<WarehouseJournalViewModel>();
					}

					journalViewModel.SelectionMode = JournalSelectionMode.Single;

					return journalViewModel;
				}
			);
		}
	}
}
