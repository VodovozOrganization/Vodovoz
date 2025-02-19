using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class NonReturnReasonJournalViewModel : SingleEntityJournalViewModelBase<NonReturnReason, NonReturnReasonViewModel,
		NonReturnReasonJournalNode>
	{
		public NonReturnReasonJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Причины невозврата имущества";

			UpdateOnChanges(typeof(NonReturnReason));			
		}

		protected override Func<IUnitOfWork, IQueryOver<NonReturnReason>> ItemsSourceQueryFunction => (uow) =>
		{
			NonReturnReason nonReturnReasonAlias = null;
			NonReturnReasonJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => nonReturnReasonAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => nonReturnReasonAlias.Id,
				() => nonReturnReasonAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => nonReturnReasonAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => nonReturnReasonAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => nonReturnReasonAlias.NeedForfeit).WithAlias(() => resultAlias.NeedForfeit)
				)
				.TransformUsing(Transformers.AliasToBean<NonReturnReasonJournalNode>());

			return itemsQuery;
		};

		protected override Func<NonReturnReasonViewModel> CreateDialogFunction => () =>
			new NonReturnReasonViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<NonReturnReasonJournalNode, NonReturnReasonViewModel> OpenDialogFunction =>
			(node) => new NonReturnReasonViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
