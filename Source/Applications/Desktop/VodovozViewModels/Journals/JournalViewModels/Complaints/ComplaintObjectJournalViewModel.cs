using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using NHibernate.Criterion;
using QS.Project.DB;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ComplaintObjectJournalViewModel : FilterableSingleEntityJournalViewModelBase<ComplaintObject, ComplaintObjectViewModel, ComplaintObjectJournalNode, ComplaintObjectJournalFilterViewModel>
	{
		public ComplaintObjectJournalViewModel(
			ComplaintObjectJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			Action<ComplaintObjectJournalFilterViewModel> filterConfig = null)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Объекты рекламаций";

			UpdateOnChanges(
				typeof(ComplaintObject),
				typeof(ComplaintKind)
			);

			if(filterConfig != null)
			{
				FilterViewModel.SetAndRefilterAtOnce(filterConfig);
			}
		}

		protected override Func<IUnitOfWork, IQueryOver<ComplaintObject>> ItemsSourceQueryFunction => (uow) =>
		{
			ComplaintObject complaintObjectAlias = null;
			ComplaintKind complaintKindAlias = null;
			ComplaintObjectJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => complaintObjectAlias);

			var complaintKindSubquery = QueryOver.Of(() => complaintKindAlias)
				.Where(() => complaintKindAlias.ComplaintObject.Id == complaintObjectAlias.Id && !complaintKindAlias.IsArchive)
				.Select(CustomProjections.GroupConcat(() => complaintKindAlias.Name,
					orderByExpression: () => complaintKindAlias.Name, separator: ", "));

			if(FilterViewModel.CreateDateFrom != null && FilterViewModel.CreateDateTo != null)
			{
				itemsQuery.Where(x => x.CreateDate >= FilterViewModel.CreateDateFrom.Value.Date &&
									  x.CreateDate <= FilterViewModel.CreateDateTo.Value.Date.Add(new TimeSpan(0, 23, 59, 59)));
			}

			if(!FilterViewModel.IsArchive)
			{
				itemsQuery.Where(x => !x.IsArchive);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => complaintObjectAlias.Id,
				() => complaintObjectAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => complaintObjectAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => complaintObjectAlias.CreateDate).WithAlias(() => resultAlias.CreateDate)
					.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.Name)
					.SelectSubQuery(complaintKindSubquery).WithAlias(() => resultAlias.ComplaintKinds)
					.Select(() => complaintObjectAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.TransformUsing(Transformers.AliasToBean<ComplaintObjectJournalNode>());

			return itemsQuery;
		};

		protected override Func<ComplaintObjectViewModel> CreateDialogFunction => () =>
			new ComplaintObjectViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices);

		protected override Func<ComplaintObjectJournalNode, ComplaintObjectViewModel> OpenDialogFunction =>
			(node) => new ComplaintObjectViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices);
	}
}
