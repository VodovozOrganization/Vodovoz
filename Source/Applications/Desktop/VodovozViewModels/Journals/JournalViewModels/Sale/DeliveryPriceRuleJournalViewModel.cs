using NHibernate.Transform;
using NHibernate;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vodovoz.Domain.Contacts;
using Vodovoz.EntityRepositories;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.ViewModels.Sale;
using Vodovoz.EntityRepositories.Sale;

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

			TabName = "Типы телефонов";

			UpdateOnChanges(typeof(DeliveryPriceRule));
		}

		IUnitOfWorkFactory unitOfWorkFactory;

		protected override Func<IUnitOfWork, IQueryOver<DeliveryPriceRule>> ItemsSourceQueryFunction => (uow) => {

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
				.TransformUsing(Transformers.AliasToBean<DeliveryPriceRuleJournalNode>()).OrderBy(x => x.Id).Desc;

			query.Where(
			GetSearchCriterion<DeliveryPriceRule>(
				x => x.Id
			)
			);

			return query;
		};

		#region Properites
		//public List<string[]> DistrictSetsUsingThisRule => 
		//	districtRuleRepository.GetDistrictNameDistrictSetNameAndCreationDateByDeliveryPriceRule(UoW,nod);

		
		#endregion

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

			if(commonServices.UserService.GetCurrentUser(UoW).IsAdmin)
			{
				CreateDefaultDeleteAction();
			}
		}
	}

	public class DeliveryPriceRuleJournalNode : JournalEntityNodeBase<DeliveryPriceRule>
	{
		public int Water19LCount { get; set; }
		public int Water6LCount { get; set; }
		public int Water1500mlCount { get; set; }
		public int Water600mlCount { get; set; }
		public int Water500mlCount { get; set; }
		public string Name { get; set; }
		public decimal OrderMinSumEShopGoods { get; set; }

		public string Description => $"Если " +
			$"19л б. < {Water19LCount}шт. " +
			$"или 6л б. < {Water6LCount}шт. " +
			$"или 1500мл б. < {Water1500mlCount}шт. " +
			$"или 500мл б. < {Water500mlCount}шт.";
	}
}
