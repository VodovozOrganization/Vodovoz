using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using QS.Project.Journal.Search;
using QS.Project.Journal.Search.Criterion;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Orders;

namespace Vodovoz.JournalViewModels
{
	public class PromotionalSetsJournalViewModel : SingleEntityJournalViewModelBase<PromotionalSet, PromotionalSetViewModel, PromotionalSetJournalNode, CriterionSearchModel>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;

		public PromotionalSetsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory, 
			ICommonServices commonServices,
			SearchViewModelBase<CriterionSearchModel> searchViewModel,
			bool hideJournalForOpenDialog = false, 
			bool hideJournalForCreateDialog = false)
		: base(unitOfWorkFactory, commonServices, searchViewModel, hideJournalForOpenDialog, hideJournalForCreateDialog)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Рекламные наборы";

			var threadLoader = DataLoader as ThreadDataLoader<PromotionalSetJournalNode>;
			threadLoader.MergeInOrderBy(x => x.IsArchive, false);
			threadLoader.MergeInOrderBy(x => x.Id, false);

			UpdateOnChanges(typeof(PromotionalSet));
		}

		protected override Func<IUnitOfWork, IQueryOver<PromotionalSet>> ItemsSourceQueryFunction => (uow) => {
			PromotionalSetJournalNode resultAlias = null;
			DiscountReason reasonAlias = null;

			var query = uow.Session.QueryOver<PromotionalSet>().Left.JoinAlias(x => x.PromoSetDiscountReason, () => reasonAlias);
			query.Where(CriterionSearchModel.ConfigureSearch()
				.AddSearchBy<PromotionalSet>(x => x.Id)
				.GetSearchCriterion()
			);

			var result = query.SelectList(list => list
									.Select(x => x.Id).WithAlias(() => resultAlias.Id)
									.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
									.Select(x => x.Name).WithAlias(() => resultAlias.Name)
									.Select(() => reasonAlias.Name).WithAlias(() => resultAlias.PromoSetDiscountReasonName))
									.TransformUsing(Transformers.AliasToBean<PromotionalSetJournalNode>())
									.OrderBy(x => x.Name).Asc;
			return result;
		};

		protected override Func<PromotionalSetViewModel> CreateDialogFunction => () => new PromotionalSetViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices
		);

		protected override Func<PromotionalSetJournalNode, PromotionalSetViewModel> OpenDialogFunction => node => new PromotionalSetViewModel(
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
