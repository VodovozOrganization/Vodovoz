using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
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
			_journalFilterViewModel = journalFilterViewModel
				?? throw new ArgumentNullException(nameof(journalFilterViewModel));

			if(journalFilterViewModelConfig != null)
			{
				_journalFilterViewModel.ConfigureWithoutFiltering(journalFilterViewModelConfig);
			}

			JournalFilter = _journalFilterViewModel;
			_journalFilterViewModel.OnFiltered += OnFiltered;
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
