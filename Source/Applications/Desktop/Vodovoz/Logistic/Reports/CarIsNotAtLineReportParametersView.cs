using QS.Views.Dialog;
using System;
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
			_carEventRepository = carEventRepository ?? throw new ArgumentNullException(nameof(carEventRepository));

			Build();

			Initialize();
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

			vbox1.Remove(includeexcludefiltergroupview1);
			includeexcludefiltergroupview1 = new Presentation.Views.IncludeExcludeFilterGroupView(includeExludeFilterGroupViewModel);
			includeexcludefiltergroupview1.Show();
			vbox1.Add(includeexcludefiltergroupview1);

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);
		}
	}
}
