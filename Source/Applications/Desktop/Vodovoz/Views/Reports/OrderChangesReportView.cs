using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using System.Linq;
using Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges;
using static Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges.OrderChangesReportViewModel;
using VodovozOrganization = Vodovoz.Domain.Organizations.Organization;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Reports
{
	public partial class OrderChangesReportView : TabViewBase<OrderChangesReportViewModel>
	{
		private const int _hpanedDefaultPosition = 420;
		private const int _hpanedMinimalPosition = 16;

		public OrderChangesReportView(OrderChangesReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			hpanedMain.Position = _hpanedDefaultPosition;
			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;

			datePeriodPicker.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			datePeriodPicker.PeriodChangedByUser += OnDatePeriodPickerPeriodChanged;

			ycheckbuttonArchive.Binding
				.AddBinding(ViewModel, vm => vm.IsOldMonitoring, w => w.Active)
				.InitializeFromSource();

			speciallistcomboboxOrganization.ItemsList = ViewModel.Organizations;
			speciallistcomboboxOrganization.SetRenderTextFunc<VodovozOrganization>(e => e.Name);
			speciallistcomboboxOrganization.Binding
				.AddBinding(ViewModel, vm => vm.SelectedOrganization, w => w.SelectedItem)
				.InitializeFromSource();

			ylabelDateWarning.Binding
				.AddBinding(ViewModel, vm => vm.IsWideDateRangeWarningMessageVisible, w => w.Visible)
				.InitializeFromSource();

			ybuttonCreateReport.Binding
				.AddFuncBinding(ViewModel, vm => !vm.IsReportGenerationInProgress, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.Binding
				.AddBinding(ViewModel, vm => vm.CanAbortReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.GenerateReportCommand);
			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortReportGenerationCommand);
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ConfigureChangeTypesTreeView();
			ConfigureIssueTypesTreeView();
			ConfigureReportRowsTreeView();
		}

		private void ConfigureChangeTypesTreeView()
		{
			ytreeviewChangeTypes.ColumnsConfig = FluentColumnsConfig<SelectableKeyValueNode>.Create()
				.AddColumn("✓").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("Тип").AddTextRenderer(x => x.Key)
				.Finish();

			ytreeviewChangeTypes.ItemsDataSource = ViewModel.ChangeTypes;
		}

		private void ConfigureIssueTypesTreeView()
		{
			ytreeviewIssueTypes.ColumnsConfig = FluentColumnsConfig<SelectableKeyValueNode>.Create()
				.AddColumn("✓").AddToggleRenderer(x => x.IsSelected)
				.AddColumn("Тип").AddTextRenderer(x => x.Key)
				.Finish();

			ytreeviewIssueTypes.Binding
				.AddBinding(ViewModel, vm => vm.CanChangeIssueTypesSelection, w => w.Sensitive)
				.InitializeFromSource();

			ytreeviewIssueTypes.ItemsDataSource = ViewModel.IssueTypes;
		}

		private void ConfigureReportRowsTreeView()
		{
			ytreeReportRows.ColumnsConfig = FluentColumnsConfig<OrderChangesReportRow>.Create()
				.AddColumn("№\nп/п").AddNumericRenderer(x => x.RowNumber)
				.AddColumn("Контрагент").AddTextRenderer(x => x.CounterpartyInfo).WrapWidth(150).WrapMode(WrapMode.WordChar)
				.AddColumn("Комментарий\nвод.телефона").AddTextRenderer(x => x.DriverPhoneComment).WrapWidth(100).WrapMode(WrapMode.WordChar)
				.AddColumn("Дата\nоплаты").AddTextRenderer(x => x.PaymentDateString)
				.AddColumn("Заказ").AddNumericRenderer(x => x.OrderId)
				.AddColumn("Сумма\nзаказа").AddTextRenderer(x => (x.OrderSum.HasValue ? x.OrderSum.Value : 0).ToString("F2"))
				.AddColumn("Дата\nдоставки").AddTextRenderer(x => x.TimeDeliveredString)
				.AddColumn("Время\nизменения").AddTextRenderer(x => x.ChangeTimeString)
				.AddColumn("Номенклатура").AddTextRenderer(x => x.NomenclatureName).WrapWidth(150).WrapMode(WrapMode.WordChar)
				.AddColumn("Старое\nзначение").AddTextRenderer(x => x.OldValueFull).WrapWidth(100).WrapMode(WrapMode.WordChar)
				.AddColumn("Новое\nзначение").AddTextRenderer(x => x.NewValueFull).WrapWidth(100).WrapMode(WrapMode.WordChar)
				.AddColumn("Водитель").AddTextRenderer(x => x.Driver).WrapWidth(100).WrapMode(WrapMode.WordChar)
				.AddColumn("Автор").AddTextRenderer(x => x.Author).WrapWidth(100).WrapMode(WrapMode.WordChar)
				.Finish();

			ytreeReportRows.Binding
				.AddSource(ViewModel)
				.AddFuncBinding(vm => vm.Report != null, w => w.Visible)
				.AddFuncBinding(vm => vm.Report != null ? vm.Report.Rows : Enumerable.Empty<OrderChangesReportRow>(), w => w.ItemsDataSource)
				.InitializeFromSource();

			ytreeReportRows.EnableGridLines = TreeViewGridLines.Both;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			yvboxFilterContainer.Visible = !yvboxFilterContainer.Visible;

			hpanedMain.Position = yvboxFilterContainer.Visible ? _hpanedDefaultPosition : _hpanedMinimalPosition;

			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = yvboxFilterContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}

		private void OnDatePeriodPickerPeriodChanged(object sender, System.EventArgs e)
		{
			ViewModel.UpdateReportGeneratingAvailabilitySettings();
		}

		public override void Destroy()
		{
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;
			datePeriodPicker.PeriodChanged -= OnDatePeriodPickerPeriodChanged;

			base.Destroy();
		}
	}
}
