using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class UndeliveryTransferAbsenceReasonJournalViewModel : FilterableSingleEntityJournalViewModelBase<UndeliveryTransferAbsenceReason,
		UndeliveryTransferAbsenceReasonViewModel, UndeliveryTransferAbsenceReasonJournalNode, UndeliveryTransferAbsenceReasonJournalFilterViewModel>
	{
		public UndeliveryTransferAbsenceReasonJournalViewModel(UndeliveryTransferAbsenceReasonJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Журнал причин отсутствий переноса";
			UpdateOnChanges(typeof(UndeliveryTransferAbsenceReason));
		}

		protected override Func<IUnitOfWork, IQueryOver<UndeliveryTransferAbsenceReason>> ItemsSourceQueryFunction => (uow) =>
		{
			UndeliveryTransferAbsenceReason undeliveryTransferAbsenceReasonAlias = null;
			UndeliveryTransferAbsenceReasonJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => undeliveryTransferAbsenceReasonAlias);

			if(FilterViewModel.CreateEventDateFrom != null && FilterViewModel.CreateEventDateTo != null)
			{
				itemsQuery.Where(x => x.CreateDate >= FilterViewModel.CreateEventDateFrom.Value.Date &&
									  x.CreateDate <= FilterViewModel.CreateEventDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			if(!FilterViewModel.ShowArchive)
			{
				itemsQuery.Where(x => !x.IsArchive);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => undeliveryTransferAbsenceReasonAlias.Name,
				() => undeliveryTransferAbsenceReasonAlias.Id)
			);

			itemsQuery
				.SelectList(list => list
					.Select(() => undeliveryTransferAbsenceReasonAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => undeliveryTransferAbsenceReasonAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => undeliveryTransferAbsenceReasonAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
				)
				.TransformUsing(Transformers.AliasToBean<UndeliveryTransferAbsenceReasonJournalNode>());

			return itemsQuery;
		};

		protected override Func<UndeliveryTransferAbsenceReasonViewModel> CreateDialogFunction =>
			() => new UndeliveryTransferAbsenceReasonViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<UndeliveryTransferAbsenceReasonJournalNode, UndeliveryTransferAbsenceReasonViewModel> OpenDialogFunction =>
			node => new UndeliveryTransferAbsenceReasonViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);

	}
}
