using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalNodes;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.JournalViewModels
{
    public class ReturnTareReasonCategoriesJournalViewModel : SingleEntityJournalViewModelBase<ReturnTareReasonCategory, ReturnTareReasonCategoryViewModel, ReturnTareReasonCategoriesJournalNode>
    {
        private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public ReturnTareReasonCategoriesJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false)
			: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = typeof(ReturnTareReasonCategory).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();;

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
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<ReturnTareReasonCategoriesJournalNode, ReturnTareReasonCategoryViewModel> OpenDialogFunction => node => new ReturnTareReasonCategoryViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			unitOfWorkFactory,
			commonServices
	   	);

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}
    }
}
