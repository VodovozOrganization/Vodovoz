using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class DiscountReasonJournalViewModel
		 : EntityJournalViewModelBase<DiscountReason, DiscountReasonViewModel, DiscountReasonJournalNode>
	{

		public DiscountReasonJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Журнал оснований для скидки";

			UpdateOnChanges(typeof(DiscountReason));
		}

		protected override IQueryOver<DiscountReason> ItemsQuery(IUnitOfWork unitOfWork)
		{
			DiscountReason drAlias = null;
			DiscountReasonJournalNode drNodeAlias = null;

			var query = unitOfWork.Session.QueryOver(() => drAlias);

			query.Where(GetSearchCriterion(
				() => drAlias.Id,
				() => drAlias.Name));

			return query.SelectList(list => list
					.Select(dr => dr.Id).WithAlias(() => drNodeAlias.Id)
					.Select(dr => dr.Name).WithAlias(() => drNodeAlias.Name)
					.Select(dr => dr.IsArchive).WithAlias(() => drNodeAlias.IsArchive))
				.OrderBy(dr => dr.IsArchive).Asc
				.OrderBy(dr => dr.Name).Asc
				.TransformUsing(Transformers.AliasToBean<DiscountReasonJournalNode>());
		}
	}
}
