using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Sale;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Sale
{
	public class DeliveryPriceRuleJournalViewModel
		: EntityJournalViewModelBase<DeliveryPriceRule, DeliveryPriceRuleViewModel, DeliveryPriceRuleJournalNode>
	{
		public DeliveryPriceRuleJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			TabName = "Правила для цен доставки";

			VisibleDeleteAction = false;

			UpdateOnChanges(typeof(DeliveryPriceRule));
		}

		protected override IQueryOver<DeliveryPriceRule> ItemsQuery(IUnitOfWork unitOfWork)
		{
			DeliveryPriceRuleJournalNode resultAlias = null;

			var query = unitOfWork.Session.QueryOver<DeliveryPriceRule>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Water19LCount).WithAlias(() => resultAlias.Water19LCount)
				.Select(x => x.Water6LCount).WithAlias(() => resultAlias.Water6LCount)
				.Select(x => x.Water1500mlCount).WithAlias(() => resultAlias.Water1500mlCount)
				.Select(x => x.Water600mlCount).WithAlias(() => resultAlias.Water600mlCount)
				.Select(x => x.Water500mlCount).WithAlias(() => resultAlias.Water500mlCount)
				.Select(x => x.RuleName).WithAlias(() => resultAlias.Name)
				.Select(x => x.OrderMinSumEShopGoods).WithAlias(() => resultAlias.OrderMinSumEShopGoods))
				.TransformUsing(Transformers.AliasToBean<DeliveryPriceRuleJournalNode>());

			query.Where(GetSearchCriterion<DeliveryPriceRule>(x => x.Id));

			return query;
		}
	}
}
