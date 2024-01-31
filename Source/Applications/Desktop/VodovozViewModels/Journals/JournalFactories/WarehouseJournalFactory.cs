using System;
using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Parameters;
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
