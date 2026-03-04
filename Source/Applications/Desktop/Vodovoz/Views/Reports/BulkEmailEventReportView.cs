using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.BulkEmailEventReport;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.Views.Reports
{
	public partial class BulkEmailEventReportView : TabViewBase<BulkEmailEventReportViewModel>
	{
		public BulkEmailEventReportView(BulkEmailEventReportViewModel viewModel) : base(viewModel)
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

			entryBulkEmailEventReason.SetEntityAutocompleteSelectorFactory(ViewModel.BulkEmailEventReasonSelectorFactory);
			entryBulkEmailEventReason.Binding.AddBinding(ViewModel, vm => vm.BulkEmailEventReason, w => w.Subject).InitializeFromSource();

			ybuttonCreateReport.Clicked += OnYbtnRunReportClicked;
			ybuttonSave.Clicked += (sender, args) => ViewModel.ExportCommand.Execute();

			ConfigureReportTreeView();
		}

		private async void OnYbtnRunReportClicked(object sender, System.EventArgs e)
		{
			ViewModel.GenerateCommand.Execute();

			ytreeviewReport.ItemsDataSource = ViewModel.Report.Rows;
			ytreeviewReport.YTreeModel.EmitModelChanged();
		}

		private void ConfigureReportTreeView()
		{
			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<BulkEmailEventReportRow>.Create()
				.AddColumn("№").AddNumericRenderer(r => ViewModel.Report.Rows.IndexOf(r) + 1)
				.AddColumn("Дата + Время").AddTextRenderer(r => r.ActionDatetimeString)
				.AddColumn("Событие").AddTextRenderer(r => r.BulkEmailEventTypeString)
				.AddColumn("Причина").AddTextRenderer(r => r.FullReasonString).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("Код клиента").AddNumericRenderer(r => r.CounterpartyId)
				.AddColumn("Имя клиента").AddTextRenderer(n => n.CounterpartyName)
				.AddColumn("E-mail").AddTextRenderer(r => r.Email)
				.AddColumn("Телефон").AddTextRenderer(r => r.Phone)
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
