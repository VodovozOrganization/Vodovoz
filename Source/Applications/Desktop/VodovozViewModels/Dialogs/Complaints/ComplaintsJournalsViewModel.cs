using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Project.Services.FileDialog;
using QS.Services;
using System;
using Autofac;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.FilterViewModels;
using Vodovoz.Parameters;
using Vodovoz.Services;
using Vodovoz.TempAdapters;
using Vodovoz.ViewModels.Journals.JournalFactories;
using Vodovoz.ViewModels.TempAdapters;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;
using Vodovoz.Factories;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ComplaintsJournalsViewModel : TabViewModelBase
	{
		private JournalViewModelBase _journal;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ICommonServices _commonServices;
		private readonly INavigationManager _navigationManager;
		private readonly IComplaintsJournalFactory _complaintsJournalFactory;
		private readonly ComplaintFilterViewModel _filterViewModel;

		public ComplaintsJournalsViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IComplaintsJournalFactory complaintsJournalFactory,
			ComplaintFilterViewModel filterViewModel) : base(commonServices.InteractiveService, navigationManager)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_complaintsJournalFactory = complaintsJournalFactory ?? throw new ArgumentNullException(nameof(complaintsJournalFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			_filterViewModel.DisposeOnDestroy = false;

			Title = "2 Журнал рекламаций";
			ChangeView(typeof(ComplaintsJournalViewModel));
		}

		public override void Dispose()
		{
			_filterViewModel.Dispose();
			base.Dispose();
		}

		private void ChangeView(Type switchToType)
		{
			var newJournal = GetNewJournal(switchToType);
			UpdateJournal(newJournal);
		}

		private JournalViewModelBase GetNewJournal(Type switchToType)
		{
			if(switchToType == typeof(ComplaintsWithDepartmentsReactionJournalViewModel))
			{
				var withDepartmentsReactionComplaintsJournal = GetJournalWithDepartmentsReaction();
				withDepartmentsReactionComplaintsJournal.ChangeView += ChangeView;
				return withDepartmentsReactionComplaintsJournal;
			}

			var standartComplaintsJournal = GetJournalStandart();
			standartComplaintsJournal.ChangeView += ChangeView;
			return standartComplaintsJournal;
		}

		private void UpdateJournal(JournalViewModelBase newJournal)
		{
			Journal?.Dispose();
			Journal = newJournal;
		}

		public JournalViewModelBase Journal
		{
			get => _journal;
			set => SetField(ref _journal, value);
		}

		private ComplaintsJournalViewModel GetJournalStandart()
		{
			return _complaintsJournalFactory.GetStandartJournal(_filterViewModel, this);
		}

		private ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction()
		{
			return _complaintsJournalFactory.GetJournalWithDepartmentsReaction(_filterViewModel, this);
		}
	}
}
