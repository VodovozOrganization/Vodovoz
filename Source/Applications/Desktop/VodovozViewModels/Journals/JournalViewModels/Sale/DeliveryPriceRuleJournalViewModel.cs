using NHibernate.Transform;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ViewModels.Sale;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Sale
{
	public class DeliveryPriceRuleJournalViewModel : SingleEntityJournalViewModelBase<DeliveryPriceRule, DeliveryPriceRuleViewModel, DeliveryPriceRuleJournalNode>
	{
		private readonly IDistrictRuleRepository districtRuleRepository;

		public DeliveryPriceRuleJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDistrictRuleRepository districtRuleRepository)
			: base(unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			this.districtRuleRepository = districtRuleRepository ?? throw new ArgumentNullException(nameof(districtRuleRepository));

			TabName = "Правила для цен доставки";

			UpdateOnChanges(typeof(DeliveryPriceRule));
		}

		private IUnitOfWorkFactory unitOfWorkFactory;

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPriceRule>> ItemsSourceQueryFunction => (uow) =>
		{
			DeliveryPriceRuleJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<DeliveryPriceRule>()
				.SelectList(list => list
				.Select(x => x.Id).WithAlias(() => resultAlias.Id)
				.Select(x => x.Water19LCount).WithAlias(() => resultAlias.Water19LCount)
				.Select(x => x.Water6LCount).WithAlias(() => resultAlias.Water6LCount)
				.Select(x => x.Water1500mlCount).WithAlias(() => resultAlias.Water1500mlCount)
				.Select(x => x.Water600mlCount).WithAlias(() => resultAlias.Water600mlCount)
				.Select(x => x.Water500mlCount).WithAlias(() => resultAlias.Water500mlCount)
				.Select(x => x.RuleName).WithAlias(() => resultAlias.Name)
				.Select(x => x.OrderMinSumEShopGoods).WithAlias(() => resultAlias.OrderMinSumEShopGoods))
				.TransformUsing(Transformers.AliasToBean<DeliveryPriceRuleJournalNode>()).OrderBy(x => x.Id).Asc;

			query.Where(
			GetSearchCriterion<DeliveryPriceRule>(
				x => x.Id
			)
			);

			return query;
		};

		protected override Func<DeliveryPriceRuleViewModel> CreateDialogFunction => () => new DeliveryPriceRuleViewModel(
			EntityUoWBuilder.ForCreate(),
			unitOfWorkFactory,
			commonServices,
			districtRuleRepository
		);

		protected override Func<DeliveryPriceRuleJournalNode, DeliveryPriceRuleViewModel> OpenDialogFunction =>
			node => new DeliveryPriceRuleViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				unitOfWorkFactory,
				commonServices,
				districtRuleRepository
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
