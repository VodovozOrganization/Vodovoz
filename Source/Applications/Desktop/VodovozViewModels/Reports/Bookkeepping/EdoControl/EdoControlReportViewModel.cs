using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.ViewModels;
using QS.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters;
using Vodovoz.Reports.Editing.Modifiers;
using Vodovoz.ViewModels.ReportsParameters.Profitability;

namespace Vodovoz.ViewModels.Reports.Bookkeepping.EdoControl
{
	public class EdoControlReportViewModel : DialogTabViewModelBase
	{
		private readonly IIncludeExcludeBookkeeppingReportsFilterFactory _includeExcludeBookkeeppingReportsFilterFactory;
		private LeftRightListViewModel<GroupingNode> _groupViewModel;
		private DateTime? _startDate;
		private DateTime? _endDate;

		public EdoControlReportViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IIncludeExcludeBookkeeppingReportsFilterFactory includeExcludeBookkeeppingReportsFilterFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_includeExcludeBookkeeppingReportsFilterFactory = includeExcludeBookkeeppingReportsFilterFactory ?? throw new ArgumentNullException(nameof(includeExcludeBookkeeppingReportsFilterFactory));

			Title = "Контроль за ЭДО";

			FilterViewModel = _includeExcludeBookkeeppingReportsFilterFactory.CreateEdoControlReportIncludeExcludeFilter(UoW);
		}

		public IncludeExludeFiltersViewModel FilterViewModel { get; }

		public virtual LeftRightListViewModel<GroupingNode> GroupingSelectViewModel
		{
			get => _groupViewModel;
			set => SetField(ref _groupViewModel, value);
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

		private IEnumerable<GroupingType> GetSelectedGroupings() =>
			GroupingSelectViewModel
			.GetRightItems()
			.Select(x => x.GroupType);
	}
}
