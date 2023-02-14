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
				.Select(x => x.RuleName).WithAlias(() => resultAlias.RuleName)
				.Select(x => x.ToString()).WithAlias(() => resultAlias.RuleDescription))
				.TransformUsing(Transformers.AliasToBean<DeliveryPriceRuleJournalNode>()).OrderBy(x => x.Id).Desc;

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

		protected override Func<DeliveryPriceRuleJournalNode, DeliveryPriceRuleViewModel> OpenDialogFunction => node => new DeliveryPriceRuleViewModel(
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

	public class DeliveryPriceRuleJournalNode : JournalEntityNodeBase<DeliveryPriceRule>
	{
		public string RuleName { get; set; }
		public string RuleDescription { get; set; }
	}
}
