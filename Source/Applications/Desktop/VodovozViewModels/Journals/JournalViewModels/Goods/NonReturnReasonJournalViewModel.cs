using System;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Goods;
using Vodovoz.ViewModels.ViewModels.Goods;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Goods
{
	public class NonReturnReasonJournalViewModel : EntityJournalViewModelBase<NonReturnReason, NonReturnReasonViewModel,
		NonReturnReasonJournalNode>
	{
		public NonReturnReasonJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager)
			: base(unitOfWorkFactory, interactiveService, navigationManager)
		{
			TabName = "Причины невозврата имущества";

			UpdateOnChanges(typeof(NonReturnReason));			
		}

		protected override IQueryOver<NonReturnReason> ItemsQuery(IUnitOfWork unitOfWork)
		{
			NonReturnReason nonReturnReasonAlias = null;
			NonReturnReasonJournalNode resultAlias = null;

			var itemsQuery = unitOfWork.Session.QueryOver(() => nonReturnReasonAlias);

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
		}
	}
}
