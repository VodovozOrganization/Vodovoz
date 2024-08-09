using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.NHibernateProjections.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class OrdersRatingReasonsJournalViewModel
		: EntityJournalViewModelBase<OrderRatingReason, OrderRatingReasonViewModel, OrdersRatingReasonsJournalNode>
	{
		public OrdersRatingReasonsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			VisibleDeleteAction = false;
		}

		protected override IQueryOver<OrderRatingReason> ItemsQuery(IUnitOfWork uow)
		{
			OrdersRatingReasonsJournalNode resultAlias = null;
			OrderRatingReason reasonAlias = null;

			var query = uow.Session.QueryOver(() => reasonAlias);

			query.SelectList(list => list
					.Select(r => r.Id).WithAlias(() => resultAlias.Id)
					.Select(r => r.Name).WithAlias(() => resultAlias.Name)
					.Select(r => r.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(OrderRatingReasonProjections.GetOrderRatingsForReason()).WithAlias(() => resultAlias.AvailableForRatings)
				)
				.TransformUsing(Transformers.AliasToBean<OrdersRatingReasonsJournalNode>());

			return query;
		}
	}
}
