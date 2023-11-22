using Autofac;
using QS.Navigation;
using QS.Project.Filter;
using QS.ViewModels.Control.EEVM;
using QS.ViewModels.Dialog;
using System;
using Vodovoz.FilterViewModels.Organization;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Employees
{
	public class PremiumJournalFilterViewModel:FilterViewModelBase<PremiumJournalFilterViewModel>
	{
		private readonly INavigationManager _navigationManager;
		private readonly ILifetimeScope _lifetimeScope;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Subdivision _subdivision;
		private DialogViewModelBase _journalViewModel;

		public PremiumJournalFilterViewModel(
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; private set; }

		public virtual DateTime? StartDate
		{
			get => _startDate;
			set => UpdateFilterField(ref _startDate, value);
		}

		public virtual DateTime? EndDate
		{
			get => _endDate;
			set => UpdateFilterField(ref _endDate, value);
		}

		public virtual Subdivision Subdivision
		{
			get => _subdivision;
			set => UpdateFilterField(ref _subdivision, value);
		}

		public DialogViewModelBase JournalViewModel
		{
			get => _journalViewModel;
			set
			{
				_journalViewModel = value;

				var subdivisionViewModelEntryViewModelBuilder = new CommonEEVMBuilderFactory<PremiumJournalFilterViewModel>(value, this, UoW, _navigationManager, _lifetimeScope);

				SubdivisionViewModel = subdivisionViewModelEntryViewModelBuilder
					.ForProperty(x => x.Subdivision)
					.UseViewModelDialog<SubdivisionViewModel>()
					.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel, SubdivisionFilterViewModel>(
						filter =>
						{
						})
					.Finish();
			}
		}
	}
}
