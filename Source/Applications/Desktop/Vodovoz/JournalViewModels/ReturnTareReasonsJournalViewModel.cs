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
	public class ReturnTareReasonsJournalViewModel : SingleEntityJournalViewModelBase<ReturnTareReason, ReturnTareReasonViewModel, ReturnTareReasonsJournalNode>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public ReturnTareReasonsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false)
			: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = typeof(ReturnTareReason).GetClassUserFriendlyName().NominativePlural.CapitalizeSentence();

			var threadLoader = DataLoader as ThreadDataLoader<ReturnTareReasonsJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Id, false);

			UpdateOnChanges(typeof(ReturnTareReason));
		}

		protected override Func<IUnitOfWork, IQueryOver<ReturnTareReason>> ItemsSourceQueryFunction => (uow) => {
			ReturnTareReasonsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<ReturnTareReason>();

			query.Where(
				GetSearchCriterion<ReturnTareReason>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive))
									.TransformUsing(Transformers.AliasToBean<ReturnTareReasonsJournalNode>())
									.OrderBy(x => x.Name).Asc;
			return result;
		};

		protected override Func<ReturnTareReasonViewModel> CreateDialogFunction => () => new ReturnTareReasonViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<ReturnTareReasonsJournalNode, ReturnTareReasonViewModel> OpenDialogFunction => node => new ReturnTareReasonViewModel(
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
