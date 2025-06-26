using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using System.Reflection;
using Vodovoz.Core.Domain.Goods.Recomendations;

namespace Vodovoz.ViewModels.Goods
{
	public class RecomendationsJournalViewModel : EntityJournalViewModelBase<Recomendation, RecomendationViewModel, RecomendationJournalNode>
	{
		private readonly RecomendationsJournalFilterViewModel _journalFilterViewModel;

		public RecomendationsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			RecomendationsJournalFilterViewModel journalFilterViewModel,
			Action<RecomendationsJournalFilterViewModel> journalFilterViewModelConfig = null)
			: base(
				  unitOfWorkFactory,
				  interactiveService,
				  navigationManager,
				  deleteEntityService,
				  currentPermissionService)
		{
			Title = "Журнал " + typeof(Recomendation).GetCustomAttribute<AppellativeAttribute>().GenitivePlural;

			_journalFilterViewModel = journalFilterViewModel
				?? throw new ArgumentNullException(nameof(journalFilterViewModel));

			if(journalFilterViewModelConfig != null)
			{
				_journalFilterViewModel.ConfigureWithoutFiltering(journalFilterViewModelConfig);
			}

			JournalFilter = _journalFilterViewModel;
			_journalFilterViewModel.OnFiltered += OnFiltered;
		}

		protected override void CreateNodeActions()
		{
			bool canCreate = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(Recomendation)).CanCreate;
			bool canEdit = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(Recomendation)).CanUpdate;
			bool canDelete = CurrentPermissionService == null || CurrentPermissionService.ValidateEntityPermission(typeof(Recomendation)).CanDelete;

			var addAction = new JournalAction("Добавить",
					(selected) => canCreate,
					(selected) => VisibleCreateAction,
					(selected) => CreateEntityDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => canEdit && selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<RecomendationJournalNode>().ToList().ForEach(EditEntityDialog)
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		private void OnFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<Recomendation> ItemsQuery(IUnitOfWork uow)
		{
			Recomendation recomendationAlias = null;
			RecomendationJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => recomendationAlias);

			if(_journalFilterViewModel.PersonType != null)
			{
				query.Where(r => r.PersonType == _journalFilterViewModel.PersonType);
			}

			if(_journalFilterViewModel.RoomType != null)
			{
				query.Where(r => r.RoomType == _journalFilterViewModel.RoomType);
			}

			if(Search.SearchValues?.Length > 0)
			{
				query.Where(GetSearchCriterion(
					() => recomendationAlias.Id,
					() => recomendationAlias.Name));
			}

			return query
				.SelectList(list => list
					.Select(x => x.Id).WithAlias(() => resultAlias.Id)
					.Select(x => x.Name).WithAlias(() => resultAlias.Name)
					.Select(x => x.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(x => x.RoomType).WithAlias(() => resultAlias.RoomType)
					.Select(x => x.PersonType).WithAlias(() => resultAlias.PersonType))
				.TransformUsing(Transformers.AliasToBean<RecomendationJournalNode>());
		}

		public override void Dispose()
		{
			_journalFilterViewModel.OnFiltered -= OnFiltered;
			base.Dispose();
		}
	}
}
