using NHibernate;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.Logistic.MileagesWriteOff
{
	public class MileageWriteOffReasonJournalViewModel : EntityJournalViewModelBase<MileageWriteOffReason, MileageWriteOffReasonViewModel, MileageWriteOffReason>
	{
		private readonly MileageWriteOffReasonJournalFilterViewModel _filterViewModel;

		public MileageWriteOffReasonJournalViewModel(
			MileageWriteOffReasonJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<MileageWriteOffReasonJournalFilterViewModel> filterConfig = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			Title = "Причины списания километража";

			VisibleDeleteAction = false;

			UpdateOnChanges(typeof(MileageWriteOffReason));

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

		protected override IQueryOver<MileageWriteOffReason> ItemsQuery(IUnitOfWork uow)
		{
			var query = uow.Session.QueryOver<MileageWriteOffReason>();

			if(!_filterViewModel.IsShowArchived)
			{
				query.Where(f => !f.IsArchived);
			}

			query.Where(GetSearchCriterion<MileageWriteOffReason>(
				x => x.Id,
				x => x.Name,
				x => x.Description
			));

			return query;
		}
	}
}
