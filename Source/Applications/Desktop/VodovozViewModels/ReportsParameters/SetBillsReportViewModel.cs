using QS.Report.ViewModels;
using System;
using System.Collections.Generic;
using DateTimeHelpers;
using QS.ViewModels.Control.EEVM;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.ViewModels.Organizations;
using QS.Navigation;
using Autofac;
using QS.DomainModel.UoW;
using QS.Report;

namespace Vodovoz.ViewModels.ReportsParameters
{
	public class SetBillsReportViewModel : ReportParametersViewModelBase
	{
		private readonly RdlViewerViewModel _rdlViewerViewModel;
		private readonly ILifetimeScope _lifetimeScope;
		private readonly IUnitOfWork _unitOfWork;
		private DateTime? _startDate;
		private DateTime? _endDate;
		private Subdivision _authorSubdivision;

		public SetBillsReportViewModel(
			RdlViewerViewModel rdlViewerViewModel,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IUnitOfWorkFactory unitOfWorkFactory)
			: base(rdlViewerViewModel)
		{
			_rdlViewerViewModel = rdlViewerViewModel ?? throw new ArgumentNullException(nameof(rdlViewerViewModel));
			NavigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			Title = "Отчет по выставленным счетам";

			_unitOfWork = unitOfWorkFactory.CreateWithoutRoot(Title);

			StartDate = DateTime.Today;
			EndDate = DateTime.Today;

			SubdivisionViewModel = CreateSubdivisionViewModel();
		}

		public DateTime? StartDate
		{
			get => _startDate;
			set => SetField(ref _startDate, value);
		}

		public DateTime? EndDate
		{
			get => _endDate;
			set => SetField(ref _endDate, value);
		}

		public Subdivision AuthorSubdivision
		{
			get => _authorSubdivision;
			set => SetField(ref _authorSubdivision, value);
		}

		public IEntityEntryViewModel SubdivisionViewModel { get; }

		public INavigationManager NavigationManager { get; }

		public override ReportInfo ReportInfo => new ReportInfo
		{
			Identifier = "Sales.SetBillsReport",
			Title = Title,
			Parameters = Parameters
		};

		protected override Dictionary<string, object> Parameters => new Dictionary<string, object>
		{
			{ "creationDate", DateTime.Now },
			{ "startDate", StartDate },
			{ "endDate", EndDate.Value.LatestDayTime() },
			{ "authorSubdivision", AuthorSubdivision?.Id }
		};
		
		private IEntityEntryViewModel CreateSubdivisionViewModel()
		{
			return new CommonEEVMBuilderFactory<SetBillsReportViewModel>(_rdlViewerViewModel, this, _unitOfWork, NavigationManager, _lifetimeScope)
				.ForProperty(x => x.AuthorSubdivision)
				.UseViewModelJournalAndAutocompleter<SubdivisionsJournalViewModel>()
				.UseViewModelDialog<SubdivisionViewModel>()
				.Finish();
		}
	}
}
