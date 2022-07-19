using System;
using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.ViewModels.Journals.JournalNodes.Client;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Client
{
	public class BulkEmailEventReasonJournalViewModel : SingleEntityJournalViewModelBase<BulkEmailEventReason, BulekEmailEventReasonViewModel,
		BulkEmailEventReasonJournalNode>
	{
		public BulkEmailEventReasonJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Причины отписки от рассылки";

			UpdateOnChanges(typeof(BulkEmailEventReason));
		}

		protected override Func<IUnitOfWork, IQueryOver<BulkEmailEventReason>> ItemsSourceQueryFunction => (uow) =>
		{
			BulkEmailEventReason bulkEmailEventReasonNameAlias = null;
			BulkEmailEventReasonJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => bulkEmailEventReasonNameAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => bulkEmailEventReasonNameAlias.Id,
				() => bulkEmailEventReasonNameAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => bulkEmailEventReasonNameAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => bulkEmailEventReasonNameAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => bulkEmailEventReasonNameAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.OrderBy(() => bulkEmailEventReasonNameAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<BulkEmailEventReasonJournalNode>());

			return itemsQuery;
		};

		protected override Func<BulekEmailEventReasonViewModel> CreateDialogFunction => () =>
			new BulekEmailEventReasonViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<BulkEmailEventReasonJournalNode, BulekEmailEventReasonViewModel> OpenDialogFunction =>
			(node) => new BulekEmailEventReasonViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
