using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.ViewModels.Journals.FilterViewModels.Employees;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.ViewModels.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class PremiumTemplateJournalViewModel : SingleEntityJournalViewModelBase<PremiumTemplate, PremiumTemplateViewModel,
		PremiumTemplateJournalNode>
	{
		private readonly IUnitOfWorkFactory unitOfWorkFactory;
		public PremiumTemplateJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			this.unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));

			TabName = "Журнал шаблонов премий";

			UpdateOnChanges(typeof(PremiumTemplate));
		}

		protected override Func<IUnitOfWork, IQueryOver<PremiumTemplate>> ItemsSourceQueryFunction => (uow) =>
		{
			PremiumTemplate premiumTemplateAlias = null;
			PremiumTemplateJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => premiumTemplateAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => premiumTemplateAlias.Id,
				() => premiumTemplateAlias.Reason,
				() => premiumTemplateAlias.PremiumMoney)
			);

			itemsQuery.SelectList(list => list
					.SelectGroup(() => premiumTemplateAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => premiumTemplateAlias.PremiumMoney).WithAlias(() => resultAlias.PremiumMoney)
					.Select(() => premiumTemplateAlias.Reason).WithAlias(() => resultAlias.Reason)
				)
				.TransformUsing(Transformers.AliasToBean<PremiumTemplateJournalNode>());

			return itemsQuery;
		};

		protected override Func<PremiumTemplateViewModel> CreateDialogFunction => () =>
			new PremiumTemplateViewModel(EntityUoWBuilder.ForCreate(), unitOfWorkFactory, commonServices);
		protected override Func<PremiumTemplateJournalNode, PremiumTemplateViewModel> OpenDialogFunction =>
			(node) => new PremiumTemplateViewModel(EntityUoWBuilder.ForOpen(node.Id), unitOfWorkFactory, commonServices);
	}
}
