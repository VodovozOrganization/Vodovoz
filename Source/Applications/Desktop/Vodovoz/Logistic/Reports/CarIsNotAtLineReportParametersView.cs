using Gamma.ColumnConfig;
using Gtk;
using QS.Views.Dialog;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.Presentation.ViewModels.Logistic.Reports;
using Vodovoz.ViewModels.Bookkeeping.Reports.EdoControl;
using static Vodovoz.Presentation.ViewModels.Logistic.Reports.CarIsNotAtLineReport;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Logistic.Reports
{
	public partial class CarIsNotAtLineReportParametersView
		: TabViewBase<CarIsNotAtLineReportParametersViewModel>
	{
		private const int _hpanedDefaultPosition = 530;
		private const int _hpanedMinimalPosition = 16;

		public CarIsNotAtLineReportParametersView(
			CarIsNotAtLineReportParametersViewModel viewModel)
			: base(viewModel)
		{
			Build();

			Initialize();
		}

		private void Initialize()
		{
			hpanedMain.Position = _hpanedDefaultPosition;

			datepickerDate.Binding
				.AddBinding(ViewModel, vm => vm.Date, w => w.Date)
				.InitializeFromSource();

			datepickerDate.IsEditable = true;

			yspinbuttonDaysCount.Binding
				.AddBinding(ViewModel, vm => vm.CountDays, w => w.ValueAsInt)
				.InitializeFromSource();

			vboxFilter.Remove(includeexcludefiltergroupview1);
			includeexcludefiltergroupview1 = new Presentation.Views.IncludeExcludeFilterGroupView(ViewModel.IncludeExludeFilterGroupViewModel);
			includeexcludefiltergroupview1.Show();
			vboxFilter.Add(includeexcludefiltergroupview1);

			ybuttonGenerate.Binding
				.AddBinding(ViewModel, vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanAbortReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonGenerate.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);
			ybuttonInfo.BindCommand(ViewModel.ShowInfoCommand);

			ConfigureDataTreeView();

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		private void ConfigureDataTreeView()
		{
			ytreeReportRows.ColumnsConfig = FluentColumnsConfig<Row>.Create()
				.AddColumn("№ п/п").AddNumericRenderer(x => x.Id)
				.AddColumn("Дата начала простоя").AddTextRenderer(x => x.DowntimeStartedAtString).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("Тип авто").AddTextRenderer(x => x.CarTypeWithGeographicalGroup)
				.AddColumn("Госномер").AddTextRenderer(x => x.RegistationNumber)
				.AddColumn("Время / описание поломки").AddTextRenderer(x => x.TimeAndBreakdownReason).WrapWidth(200).WrapMode(WrapMode.WordChar)
				.AddColumn("Планируемая дата выпуска\nавтомобиля на линию").AddTextRenderer(x => x.PlannedReturnToLineDateString)
				.AddColumn("Основания переноса даты").AddTextRenderer(x => x.PlannedReturnToLineDateAndReschedulingReason).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.Finish();

			ytreeReportRows.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.Rows : Enumerable.Empty<Row>(), w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeReportRows.EnableGridLines = TreeViewGridLines.Both;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			vboxFilter.Visible = !vboxFilter.Visible;

			hpanedMain.Position = vboxFilter.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = vboxFilter.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
