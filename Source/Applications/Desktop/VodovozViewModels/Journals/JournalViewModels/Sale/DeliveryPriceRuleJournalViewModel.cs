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
		private readonly IDistrictRuleRepository _districtRuleRepository;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public DeliveryPriceRuleJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IDistrictRuleRepository districtRuleRepository)
			: base(unitOfWorkFactory, commonServices)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_districtRuleRepository = districtRuleRepository ?? throw new ArgumentNullException(nameof(districtRuleRepository));

			TabName = "Правила для цен доставки";

			UpdateOnChanges(typeof(DeliveryPriceRule));
		}


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
			null,
			EntityUoWBuilder.ForCreate(),
			_unitOfWorkFactory,
			commonServices,
			_districtRuleRepository
		);

		protected override Func<DeliveryPriceRuleJournalNode, DeliveryPriceRuleViewModel> OpenDialogFunction =>
			node => new DeliveryPriceRuleViewModel(
				null,
				EntityUoWBuilder.ForOpen(node.Id),
				_unitOfWorkFactory,
				commonServices,
				_districtRuleRepository
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
