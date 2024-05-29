using QS.Views.Dialog;
using System.ComponentModel;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Domain.Logistic;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;

namespace Vodovoz.Logistic.Reports
{
	[ToolboxItem(true)]
	public partial class CarIsNotAtLineReportParametersView
		: DialogViewBase<CarIsNotAtLineReportParametersViewModel>
	{
		private readonly IGenericRepository<CarEvent> _carEventRepository;

		public CarIsNotAtLineReportParametersView(
			CarIsNotAtLineReportParametersViewModel viewModel,
			IGenericRepository<CarEvent> carEventRepository)
			: base(viewModel)
		{
			Build();

			Initialize();
			_carEventRepository = carEventRepository;
		}

		private void Initialize()
		{
			datepickerDate.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();

			yspinbuttonDaysCount.Binding
				.AddBinding(ViewModel, vm => vm.CountDays, w => w.ValueAsInt)
				.InitializeFromSource();

			var includeExludeFilterGroupViewModel = new IncludeExludeFilterGroupViewModel();

			includeExludeFilterGroupViewModel.InitializeFor(ViewModel.UnitOfWork, _carEventRepository);

			includeexcludefiltergroupview1 = new Presentation.Views.IncludeExcludeFilterGroupView(includeExludeFilterGroupViewModel);

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
