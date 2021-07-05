using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using QS.ViewModels;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.JournalViewModels
{
    public class ReturnTareReasonCategoriesJournalViewModel : SingleEntityJournalViewModelBase<ReturnTareReasonCategory, ReturnTareReasonCategoryViewModel, ReturnTareReasonCategoriesJournalNode>
    {
		public ReturnTareReasonCategoriesJournalViewModel(
			EntitiesJournalActionsViewModel journalActionsViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			bool hideJournalForOpenDialog = false,
			bool hideJournalForCreateDialog = false)
			: base(journalActionsViewModel, unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			TabName = "Категории причин забора тары";

			var threadLoader = DataLoader as ThreadDataLoader<ReturnTareReasonCategoriesJournalNode>;
			threadLoader.MergeInOrderBy(x => x.Id, false);

			UpdateOnChanges(typeof(ReturnTareReasonCategory));
		}

		protected override Func<IUnitOfWork, IQueryOver<ReturnTareReasonCategory>> ItemsSourceQueryFunction => (uow) => {
			ReturnTareReasonCategoriesJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<ReturnTareReasonCategory>();
			
			query.Where(
				GetSearchCriterion<ReturnTareReasonCategory>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name))
									.TransformUsing(Transformers.AliasToBean<ReturnTareReasonCategoriesJournalNode>())
									.OrderBy(x => x.Name).Asc;
			return result;
		};

		protected override Func<ReturnTareReasonCategoryViewModel> CreateDialogFunction => () => new ReturnTareReasonCategoryViewModel(
			EntitiesJournalActionsViewModel,
			EntityUoWBuilder.ForCreate(),
			UnitOfWorkFactory,
			CommonServices
		);

		protected override Func<JournalEntityNodeBase, ReturnTareReasonCategoryViewModel> OpenDialogFunction =>
			node => new ReturnTareReasonCategoryViewModel(
				EntitiesJournalActionsViewModel,
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				CommonServices
	   	);
		
		protected override void InitializeJournalActionsViewModel()
		{
			EntitiesJournalActionsViewModel.Initialize(
				SelectionMode, EntityConfigs, this, HideJournal, OnItemsSelected,
				true, true, true, false);
		}
    }
}