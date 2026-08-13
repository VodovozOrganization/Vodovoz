using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Common;
using Vodovoz.ViewModels.ViewModels.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class DiscountReasonJournalViewModel
		 : EntityJournalViewModelBase<DiscountReasonBase, DiscountReasonViewModel, DiscountReasonJournalNode>
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

		protected override IQueryOver<DiscountReasonBase> ItemsQuery(IUnitOfWork unitOfWork)
		{
			DiscountReasonBase drAlias = null;
			DiscountReasonJournalNode drNodeAlias = null;

			var query = unitOfWork.Session.QueryOver(() => drAlias);
			
			var discountReasonTypeProjection = Projections.Conditional(
				new[]
				{
					new ConditionalProjectionCase(
						Restrictions.Where(() => drAlias.DiscountReasonType == DiscountReasonType.PromoCode),
						Projections.Select(() => typeof(PromoCodeDiscount))),
					new ConditionalProjectionCase(
						Restrictions.Where(() => drAlias.DiscountReasonType == DiscountReasonType.AutoOrder),
						Projections.Select(() => typeof(AutoOrderDiscount))),
					new ConditionalProjectionCase(
						Restrictions.Where(() => drAlias.DiscountReasonType == DiscountReasonType.FirstOnlineOrderDiscount),
						Projections.Select(() => typeof(FirstOnlineOrderDiscount)))
				},
				Projections.Select(() => typeof(DiscountReason)));

			query.Where(GetSearchCriterion(
				() => drAlias.Id,
				() => drAlias.Name));

			return query.SelectList(list => list
					.Select(dr => dr.Id).WithAlias(() => drNodeAlias.Id)
					.Select(discountReasonTypeProjection).WithAlias(() => drNodeAlias.EntityType)
					.Select(dr => dr.Name).WithAlias(() => drNodeAlias.Name)
					.Select(dr => dr.IsArchive).WithAlias(() => drNodeAlias.IsArchive))
				.OrderBy(dr => dr.IsArchive).Asc
				.OrderBy(dr => dr.Name).Asc
				.TransformUsing(Transformers.AliasToBean<DiscountReasonJournalNode>());
		}
		
		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<DiscountReasonViewModel, IEntityViewModelContext>(
				this, EntityViewModelContext.Create(typeof(DiscountReason), UnitOfWorkFactory));
		}

		protected override void EditEntityDialog(DiscountReasonJournalNode node)
		{
			NavigationManager.OpenViewModel<DiscountReasonViewModel, IEntityViewModelContext>(
				this, EntityViewModelContext.Create(node.EntityType, UnitOfWorkFactory, node.Id));
		}
	}
}
