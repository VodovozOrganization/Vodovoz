using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.DB;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ComplaintKindJournalViewModel : EntityJournalViewModelBase<ComplaintKind, ComplaintKindViewModel, ComplaintKindJournalNode>
	{
		private readonly ComplaintKindJournalFilterViewModel _filterViewModel;

		public ComplaintKindJournalViewModel(
			ComplaintKindJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<ComplaintKindJournalFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			TabName = "Виды рекламаций";

			UpdateOnChanges(
				typeof(ComplaintKind),
				typeof(ComplaintObject));

			if(filterConfig != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterConfig);
			}

			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<ComplaintKind> ItemsQuery(IUnitOfWork uow)
		{
			ComplaintKind complaintKindAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintKindJournalNode resultAlias = null;
			Subdivision subdivisionsAlias = null;

			var itemsQuery = uow.Session.QueryOver(() => complaintKindAlias)
				.Left.JoinAlias(x => x.ComplaintObject, () => complaintObjectAlias)
				.Left.JoinAlias(x => x.Subdivisions, () => subdivisionsAlias);

			if(_filterViewModel.ComplaintObject != null)
			{
				itemsQuery.Where(x => x.ComplaintObject.Id == _filterViewModel.ComplaintObject.Id);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => complaintKindAlias.Id,
				() => complaintKindAlias.Name,
				() => complaintObjectAlias.Name)
			);

			var subdivisionsProjection = CustomProjections.GroupConcat(
				() => subdivisionsAlias.ShortName,
				orderByExpression: () => subdivisionsAlias.ShortName,
				separator: ", "
			);

			itemsQuery.SelectList(list => list
					.SelectGroup(() => complaintKindAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => complaintKindAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObject)
					.Select(subdivisionsProjection).WithAlias(() => resultAlias.Subdivisions)
				)
				.TransformUsing(Transformers.AliasToBean<ComplaintKindJournalNode>());

			return itemsQuery;
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;

			base.Dispose();
		}
	}
}
