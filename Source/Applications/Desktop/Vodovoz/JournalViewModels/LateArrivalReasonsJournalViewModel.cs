using System;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Services;
using Vodovoz.Domain.Logistic;
using Vodovoz.ViewModels.Logistic;
using Vodovoz.JournalNodes;
using NHibernate;

namespace Vodovoz.JournalViewModels
{
	public class LateArrivalReasonsJournalViewModel : SingleEntityJournalViewModelBase<LateArrivalReason, LateArrivalReasonViewModel, LateArrivalReasonsJournalNode>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public LateArrivalReasonsJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices,
			bool hideJournalForOpenDialog = false, bool hideJournalForCreateDialog = false)
			: base(unitOfWorkFactory, commonServices, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Причины опозданий водителей";

			var threadLoader = DataLoader as ThreadDataLoader<LateArrivalReasonsJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Id, false);

			UpdateOnChanges(typeof(LateArrivalReason));
		}

		protected override Func<IUnitOfWork, IQueryOver<LateArrivalReason>> ItemsSourceQueryFunction => (uow) => {
			LateArrivalReasonsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<LateArrivalReason>();

			query.Where(
				GetSearchCriterion<LateArrivalReason>(
					x => x.Id
				)
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive))
									.TransformUsing(Transformers.AliasToBean<LateArrivalReasonsJournalNode>())
									.OrderBy(x => x.Name).Asc;
			return result;
		};

		protected override Func<LateArrivalReasonViewModel> CreateDialogFunction => () => new LateArrivalReasonViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<LateArrivalReasonsJournalNode, LateArrivalReasonViewModel> OpenDialogFunction => node => new LateArrivalReasonViewModel(
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