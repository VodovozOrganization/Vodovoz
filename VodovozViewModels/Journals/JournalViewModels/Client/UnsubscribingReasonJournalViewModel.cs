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
	public class UnsubscribingReasonJournalViewModel : SingleEntityJournalViewModelBase<UnsubscribingReason, UnsubscribingReasonViewModel,
		UnsubscribingReasonJournalNode>
	{
		public UnsubscribingReasonJournalViewModel(IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(unitOfWorkFactory, commonServices)
		{
			TabName = "Причины отписки от рассылки";

			UpdateOnChanges(typeof(UnsubscribingReason));
		}

		protected override Func<IUnitOfWork, IQueryOver<UnsubscribingReason>> ItemsSourceQueryFunction => (uow) =>
		{
			UnsubscribingReason unsubscribingReasonNameAlias = null;
			UnsubscribingReasonJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => unsubscribingReasonNameAlias);

			itemsQuery.Where(GetSearchCriterion(
				() => unsubscribingReasonNameAlias.Id,
				() => unsubscribingReasonNameAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => unsubscribingReasonNameAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => unsubscribingReasonNameAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => unsubscribingReasonNameAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.OrderBy(() => unsubscribingReasonNameAlias.Name).Asc
				.TransformUsing(Transformers.AliasToBean<UnsubscribingReasonJournalNode>());

			return itemsQuery;
		};

		protected override Func<UnsubscribingReasonViewModel> CreateDialogFunction => () =>
			new UnsubscribingReasonViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<UnsubscribingReasonJournalNode, UnsubscribingReasonViewModel> OpenDialogFunction =>
			(node) => new UnsubscribingReasonViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
