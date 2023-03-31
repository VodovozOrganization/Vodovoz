using QS.DomainModel.UoW;
using QS.Project.Journal;
using QS.Services;
using System;
using Vodovoz.FilterViewModels;
using Vodovoz.ViewModels.TempAdapters;
using QS.Navigation;
using QS.ViewModels;
using Vodovoz.Journals.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.ViewModels.Dialogs.Complaints
{
	public class ComplaintsJournalsViewModel : TabViewModelBase
	{
		private JournalViewModelBase _journal;
		private readonly IComplaintsJournalFactory _complaintsJournalFactory;
		private readonly ComplaintFilterViewModel _filterViewModel;

		public ComplaintsJournalsViewModel(
			ICommonServices commonServices,
			INavigationManager navigationManager,
			IComplaintsJournalFactory complaintsJournalFactory,
			ComplaintFilterViewModel filterViewModel) : base(commonServices.InteractiveService, navigationManager)
		{
			_complaintsJournalFactory = complaintsJournalFactory ?? throw new ArgumentNullException(nameof(complaintsJournalFactory));
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));

			_filterViewModel.DisposeOnDestroy = false;

			Title = "Журнал рекламаций";
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

			var standartComplaintsJournal = GetStandartJournal();
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

		private ComplaintsJournalViewModel GetStandartJournal()
		{
			var journal = _complaintsJournalFactory.GetStandartJournal(_filterViewModel);
			journal.ParentTab = this;
			return journal;
		}

		private ComplaintsWithDepartmentsReactionJournalViewModel GetJournalWithDepartmentsReaction()
		{
			var journal = _complaintsJournalFactory.GetJournalWithDepartmentsReaction(_filterViewModel);
			journal.ParentTab = this;
			return journal;
		}
	}
}
