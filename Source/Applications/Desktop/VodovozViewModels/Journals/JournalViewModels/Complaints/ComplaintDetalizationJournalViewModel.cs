using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ComplaintDetalizationJournalViewModel
		: FilterableSingleEntityJournalViewModelBase<
			ComplaintDetalization,
			ComplaintDetalizationViewModel,
			ComplaintDetalizationJournalNode,
			ComplaintDetalizationJournalFilterViewModel>
	{
		private ComplaintDetalization _selectedComplaintDetalization;

		public ComplaintDetalizationJournalViewModel(
					ComplaintDetalizationJournalFilterViewModel filterViewModel,
					IUnitOfWorkFactory unitOfWorkFactory,
					ICommonServices commonServices)
					: base(filterViewModel, unitOfWorkFactory, commonServices)
		{
			TabName = "Детализации видов рекламаций";

			UpdateOnChanges(
				typeof(ComplaintKind),
				typeof(ComplaintObject),
				typeof(ComplaintDetalization));
		}

		public ComplaintDetalization SelectedComplaintDetalization
		{
			get => _selectedComplaintDetalization;
			set
			{
				if(SetField(ref _selectedComplaintDetalization, value))
				{
					OnPropertyChanged(nameof(CanSelect));
				}
			}
		}

		public bool CanSelect => !SelectedComplaintDetalization.IsArchive;

		protected override Func<IUnitOfWork, IQueryOver<ComplaintDetalization>> ItemsSourceQueryFunction => (unitOfWork) =>
		{
			ComplaintDetalization complaintDetalizationAlias = null;
			ComplaintKind complaintKindAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintDetalizationJournalNode resultAlias = null;

			var itemsQuery = unitOfWork.Session.QueryOver(() => complaintDetalizationAlias)
				.Left.JoinAlias(x => x.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintKindAlias.ComplaintObject, () => complaintObjectAlias);

			if(FilterViewModel.ComplaintObject != null)
			{
				itemsQuery.Where(() => complaintObjectAlias.Id == FilterViewModel.ComplaintObject.Id);
			}

			if(FilterViewModel.ComplaintKind != null)
			{
				itemsQuery.Where(() => complaintKindAlias.Id == FilterViewModel.ComplaintKind.Id);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => complaintDetalizationAlias.Id,
				() => complaintDetalizationAlias.Name,
				() => complaintKindAlias.Name,
				() => complaintObjectAlias.Name));

			itemsQuery.OrderBy(x => x.IsArchive).Asc
				.SelectList(list =>
					list.SelectGroup(() => complaintDetalizationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => complaintDetalizationAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => complaintDetalizationAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
						.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObject)
						.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.ComplaintKind))
				.TransformUsing(Transformers.AliasToBean<ComplaintDetalizationJournalNode>());

			return itemsQuery;
		};

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateSelectAction();
			CreateDefaultAddActions();
			CreateDefaultEditAction();
		}

		private void CreateSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any() && selected.All(x => !(x as ComplaintDetalizationJournalNode).IsArchive),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => OnItemsSelected(selected)
			);
			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple)
			{
				RowActivatedAction = selectAction;
			}
			NodeActionsList.Add(selectAction);
		}

		protected override Func<ComplaintDetalizationViewModel> CreateDialogFunction =>
			() => new ComplaintDetalizationViewModel(
			   EntityUoWBuilder.ForCreate(),
			   UnitOfWorkFactory,
			   commonServices,
			   null,
			   FilterViewModel.RestrictComplaintObject,
			   FilterViewModel.RestrictComplaintKind);

		protected override Func<ComplaintDetalizationJournalNode, ComplaintDetalizationViewModel> OpenDialogFunction =>
			(node) => new ComplaintDetalizationViewModel(
				EntityUoWBuilder.ForOpen(node.Id),
				UnitOfWorkFactory,
				commonServices,
				null,
				FilterViewModel.RestrictComplaintObject,
				FilterViewModel.RestrictComplaintKind);
	}
}
