using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.BulkDebtMailingReport;

namespace Vodovoz.Views.Reports
{
	public partial class BulkDebtMailingReportView : TabViewBase<BulkDebtMailingReportViewModel>
	{
		public BulkDebtMailingReportView(BulkDebtMailingReportViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			rangeBulkEmailEventDate.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.EventActionTimeFrom, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EventActionTimeTo, w => w.EndDateOrNull)
				.InitializeFromSource();

			entryCounterparty.SetEntityAutocompleteSelectorFactory(ViewModel.CounterpartySelectorFactory);
			entryCounterparty.Binding.AddBinding(ViewModel, vm => vm.Counterparty, w => w.Subject).InitializeFromSource();

			ybuttonCreateReport.Clicked += OnYbtnRunReportClicked;

			ybuttonCreateSummaryReport.Clicked += OnYbtnRunSummaryReportClicked;

			ybuttonSave.Clicked += (sender, args) => ViewModel.ExportCommand.Execute();

			ViewModel.PropertyChanged += OnViewModelPropertyChanged;

			ConfigureReportTreeView();
		}

		private void OnYbtnRunReportClicked(object sender, System.EventArgs e)
		{
			ViewModel.GenerateCommand.Execute();

			ConfigureReportTreeView();

			ytreeviewReport.ItemsDataSource = ViewModel.Report.Rows;
			ytreeviewReport.YTreeModel.EmitModelChanged();
		}

		private void OnYbtnRunSummaryReportClicked(object sender, System.EventArgs e)
		{
			ViewModel.GenerateSummaryCommand.Execute();

			ConfigureReportTreeView();

			ytreeviewReport.ItemsDataSource = ViewModel.SummaryReport.Rows;
			ytreeviewReport.YTreeModel.EmitModelChanged();
		}


		private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(ViewModel.IsReportSelected) ||
			   e.PropertyName == nameof(ViewModel.IsSummaryReportSelected))
			{
				ConfigureReportTreeView();
			}
		}

		private void ConfigureReportTreeView()
		{
			if(ViewModel.IsReportSelected)
			{
				ytreeviewReport.ColumnsConfig = FluentColumnsConfig<BulkDebtMailingReportRow>.Create()
					.AddColumn("№").AddNumericRenderer(r => ViewModel.Report.Rows.IndexOf(r) + 1)
					.AddColumn("Дата + Время").AddTextRenderer(r => r.ActionDatetimeString)
					.AddColumn("Статус").AddTextRenderer(r => r.StateString)
					.AddColumn("Код клиента").AddNumericRenderer(r => r.CounterpartyId)
					.AddColumn("Имя клиента").AddTextRenderer(n => n.CounterpartyName)
					.AddColumn("E-mail").AddTextRenderer(r => r.Email)
					.AddColumn("Телефон").AddTextRenderer(r => r.Phone)
					.AddColumn("")
					.Finish();
			}
			else
			{
				ytreeviewReport.ColumnsConfig = FluentColumnsConfig<BulkDebtMailingSummaryReportRow>.Create()
					.AddColumn("№").AddNumericRenderer(r => ViewModel.SummaryReport.Rows.IndexOf(r) + 1)
					.AddColumn("Дата").AddTextRenderer(r => r.ActionDatetimeString)
					.AddColumn("Статус").AddTextRenderer(r => r.StateString)
					.AddColumn("Количество").AddNumericRenderer(r => r.Count)
					.AddColumn("")
					.Finish();
			}

			ytreeviewReport.QueueDraw();
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
