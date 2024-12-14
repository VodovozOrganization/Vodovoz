using Gtk;
using QS.Dialog;
using QS.Views.Dialog;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.Store.Reports;

namespace Vodovoz.Store.Reports
{
	[ToolboxItem(true)]
	public partial class DefectiveItemsReportView : DialogViewBase<DefectiveItemsReportViewModel>
	{
		private int _hpanedDefaultPosition = 680;
		private int _hpanedMinimalPosition = 16;
		private readonly IGuiDispatcher _guiDispatcher;

		public DefectiveItemsReportView(
			DefectiveItemsReportViewModel viewModel,
			IGuiDispatcher guiDispatcher)
			: base(viewModel)
		{
			_guiDispatcher = guiDispatcher
				?? throw new ArgumentNullException(nameof(guiDispatcher));

			Build();

			Initialize();

			UpdateSliderArrow();
		}

		private void Initialize()
		{
			yEnumCmbSource.ItemsEnum = ViewModel.DefectSourceType;
			yEnumCmbSource.AddEnumToHideList(ViewModel.HiddenDefectSources);
			yEnumCmbSource.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.DefectSource, w => w.SelectedItemOrNull)
				.InitializeFromSource();

			datePeriod.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			entityentryDriver.ViewModel = ViewModel.DriverViewModel;

			buttonRun.BindCommand(ViewModel.GenerateReportCommand);

			hpaned1.Position = _hpanedDefaultPosition;

			UpdateSliderArrow();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;
		}

		private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.Report))
			{
				_guiDispatcher.RunInGuiTread(() =>
				{
					RefreshReportPreview();
					QueueDraw();
				});
			}
		}

		private void RefreshReportPreview()
		{

		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			scrolledwindow1.Visible = !scrolledwindow1.Visible;

			hpaned1.Position = scrolledwindow1.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = scrolledwindow1.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
