using Autofac;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using System;
using Vodovoz.Domain.Cash;
using Vodovoz.ViewModels.Journals.JournalViewModels.Cash;
using Vodovoz.ViewModels.TempAdapters;
using Vodovoz.ViewModels.ViewModels.Cash;

namespace Vodovoz.TempAdapters
{
	public class IncomeCategorySelectorFactory : IIncomeCategorySelectorFactory
	{
		private readonly ILifetimeScope _lifetimeScope;

		public IncomeCategorySelectorFactory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public IEntityAutocompleteSelectorFactory CreateSimpleIncomeCategoryAutocompleteSelectorFactory()
		{
			var commonServices = _lifetimeScope.Resolve<ICommonServices>();
			
			var incomeCategoryAutocompleteSelectorFactory =
				new SimpleEntitySelectorFactory<IncomeCategory, IncomeCategoryViewModel>(
					() =>
					{
						var incomeCategoryJournalViewModel =
							new SimpleEntityJournalViewModel<IncomeCategory, IncomeCategoryViewModel>(
								x => x.Name,
								() => _lifetimeScope.Resolve<IncomeCategoryViewModel>(new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForCreate())),
								node => _lifetimeScope.Resolve<IncomeCategoryViewModel>(new TypedParameter(typeof(IEntityUoWBuilder), EntityUoWBuilder.ForOpen(node.Id))),
								_lifetimeScope.Resolve<IUnitOfWorkFactory>(),
								commonServices
							)
							{
								SelectionMode = JournalSelectionMode.Single
							};
						return incomeCategoryJournalViewModel;
					});
			return incomeCategoryAutocompleteSelectorFactory;
		}

		public IEntityAutocompleteSelectorFactory CreateDefaultIncomeCategoryAutocompleteSelectorFactory()
		{
			var incomeCategoryJournalViewModel = _lifetimeScope.Resolve<IncomeCategoryJournalViewModel>();

			return new EntityAutocompleteSelectorFactory<IncomeCategoryJournalViewModel>(
				typeof(IncomeCategory),
				() => {
					incomeCategoryJournalViewModel.SelectionMode = JournalSelectionMode.Single;
					return incomeCategoryJournalViewModel;
				});
		}
	}
}
