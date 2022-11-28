using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport;

namespace Vodovoz.Views.Reports
{
	public partial class EdoUpdReportView : TabViewBase<EdoUpdReportViewModel>
	{
		public EdoUpdReportView(EdoUpdReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			rangeDate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DateFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.DateTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			yenumcomboboxReportType.ItemsEnum = typeof(EdoUpdReportViewModel.EdoUpdReportType);
			yenumcomboboxReportType.Binding.AddBinding(ViewModel, s => s.ReportType, w => w.SelectedItem).InitializeFromSource();

			ybuttonCreateReport.Clicked += OnYbtnRunReportClicked;
			ybuttonSave.Clicked += (sender, args) => ViewModel.ExportCommand.Execute();

			ConfigureReportTreeView();
		}

		private void OnYbtnRunReportClicked(object sender, System.EventArgs e)
		{
			ViewModel.GenerateCommand.Execute();

			ytreeviewReport.ItemsDataSource = ViewModel.Report.Rows;
			ytreeviewReport.YTreeModel.EmitModelChanged();
		}

		private void ConfigureReportTreeView()
		{
			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<EdoUpdReportRow>.Create()
				.AddColumn("№").AddNumericRenderer(r => ViewModel.Report.Rows.IndexOf(r) + 1)
				.AddColumn("ИНН").AddTextRenderer(r => r.Inn)
				.AddColumn("Название контрагента").AddTextRenderer(r => r.CounterpartyName)
				.AddColumn("№ Заказа").AddNumericRenderer(r => r.OrderId)
				.AddColumn("Дата").AddTextRenderer(r => r.UpdDateString)
				.AddColumn("GTIN").AddTextRenderer(r => r.Gtin)
				.AddColumn("Кол-во").AddNumericRenderer(r => r.Count)
				.AddColumn("Цена").AddNumericRenderer(r => r.Price)
				.AddColumn("Стоимость строки с НДС").AddNumericRenderer(r => r.Sum)
				.AddColumn("Статус УПД в ЭДО").AddTextRenderer(r => r.EdoDocFlowStatusString)
				.AddColumn("Статус прямого вывода из оборота в Честном Знаке").AddTextRenderer(r => r.TrueMarkApiErrorString)
				.AddColumn("")
				.Finish();
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			parametersContainer.Visible = !parametersContainer.Visible;
			UpdateSliderArrow();
		}

		private void UpdateSliderArrow()
		{
			arrowSlider.ArrowType = parametersContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}
	}
}
