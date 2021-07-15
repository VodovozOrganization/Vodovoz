using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using QS.Project.Journal.EntitySelector;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ComplaintKindJournalViewModel : FilterableSingleEntityJournalViewModelBase<ComplaintKind, ComplaintKindViewModel, ComplaintKindJournalNode, ComplaintKindJournalFilterViewModel>
	{
		private readonly IEntityAutocompleteSelectorFactory _employeeSelectorFactory;
		public ComplaintKindJournalViewModel(ComplaintKindJournalFilterViewModel filterViewModel, IUnitOfWorkFactory unitOfWorkFactory, ICommonServices commonServices, IEntityAutocompleteSelectorFactory employeeSelectorFactory)
			: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			_employeeSelectorFactory = employeeSelectorFactory ?? throw new ArgumentNullException(nameof(employeeSelectorFactory));

			TabName = "Виды рекламаций";

			UpdateOnChanges(typeof(ComplaintKind));
		}

		protected override Func<IUnitOfWork, IQueryOver<ComplaintKind>> ItemsSourceQueryFunction => (uow) =>
		{
			ComplaintKind complaintKindAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintKindJournalNode resultAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => complaintKindAlias)
				.Left.JoinAlias(x => x.ComplaintObject, () => complaintObjectAlias);

			if(FilterViewModel.ComplaintObject != null)
			{
				itemsQuery.Where(x => x.ComplaintObject.Id == FilterViewModel.ComplaintObject.Id);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => complaintKindAlias.Id,
				() => complaintKindAlias.Name,
				() => complaintObjectAlias.Name)
			);

			itemsQuery.SelectList(list => list
					.Select(() => complaintKindAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObject)
				)
				.TransformUsing(Transformers.AliasToBean<ComplaintKindJournalNode>());

			return itemsQuery;
		};

		protected override void CreateNodeActions()
		{
			CreateDefaultAddActions();
			CreateDefaultEditAction();
			CreateDefaultSelectAction();
		}

		protected override Func<ComplaintKindViewModel> CreateDialogFunction => () =>
			new ComplaintKindViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices, _employeeSelectorFactory);

		protected override Func<ComplaintKindJournalNode, ComplaintKindViewModel> OpenDialogFunction =>
			(node) => new ComplaintKindViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices, _employeeSelectorFactory);
	}
}
