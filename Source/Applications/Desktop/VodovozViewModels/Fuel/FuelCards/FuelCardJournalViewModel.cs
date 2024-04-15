using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.ViewModels.Fuel.FuelCards
{
	public class FuelCardJournalViewModel : EntityJournalViewModelBase<FuelCard, FuelCardViewModel, FuelCard>
	{
		private readonly FuelCardJournalFilterViewModel _filterViewModel;

		public FuelCardJournalViewModel(
			FuelCardJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<FuelCardJournalFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			Title = "Топливные карты";

			VisibleDeleteAction = false;

			UpdateOnChanges(typeof(FuelCard));

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

		protected override IQueryOver<FuelCard> ItemsQuery(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<FuelCard>();

			if(!_filterViewModel.IsShowArchived)
			{
				query.Where(f => !f.IsArchived);
			}

			query.Where(GetSearchCriterion<FuelCard>(
				x => x.Id,
				x => x.CardNumber
			));

			return query;
		}
	}
}
