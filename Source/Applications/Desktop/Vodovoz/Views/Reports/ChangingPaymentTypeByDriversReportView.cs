using Gamma.ColumnConfig;
using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Reports.Logistics.ChangingPaymentTypeByDriversReport;
namespace Vodovoz.Views.Reports
{
	public partial class ChangingPaymentTypeByDriversReportView : TabViewBase<ChangingPaymentTypeByDriversReportViewModel>
	{
		public ChangingPaymentTypeByDriversReportView(ChangingPaymentTypeByDriversReportViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			rangeDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.StartDate, w => w.StartDateOrNull)
				.AddBinding(vm => vm.EndDate, w => w.EndDateOrNull)
				.InitializeFromSource();

			ycheckbuttonGroupByDriver.Binding
				.AddBinding(ViewModel, vm => vm.IsGroupByDriver, w => w.Active)
				.InitializeFromSource();

			cmbGeoGroup.ItemsList = ViewModel.AllUsedGeoGroups;
			cmbGeoGroup.Binding
				.AddBinding(ViewModel, vm => vm.SelectedGeoGroup, w => w.SelectedItem)
				.InitializeFromSource();

			ybuttonCreateReport.BindCommand(ViewModel.CreateReportCommand);
			ybuttonCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanGenerateReport, w => w.Visible)
				.InitializeFromSource();

			ybuttonAbortCreateReport.BindCommand(ViewModel.AbortCreateCommand);
			ybuttonAbortCreateReport.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.CanCancelGenerateReport, w => w.Visible)
				.InitializeFromSource();
			ybuttonSave.BindCommand(ViewModel.SaveReportCommand);

			ytreeviewReport.ColumnsConfig = FluentColumnsConfig<ChangingPaymentTypeByDriversReportRow>.Create()
				.AddColumn("Номер заказа").AddTextRenderer(r => r.OrderId)
				.AddColumn("Дата и время").AddTextRenderer(r => r.ChangeDateTime)
				.AddColumn("ФИО водителя").AddTextRenderer(r => r.DriverName)
				.AddSetter((cell, node) =>
				{
					if(node.IsTitle)
					{
						cell.Markup = $"<b>{node.DriverName}</b>";
					}
				})
				.AddColumn("Исходный способ оплаты").AddTextRenderer(r => r.OriginalPaymentTypeDisplayName)
				.AddColumn("Новый способ оплаты").AddTextRenderer(r => r.NewPaymentTypeDisplayName)
				.AddColumn("Сумма заказа").AddTextRenderer(r => r.OrderSum)
				.AddColumn("")
				.Finish();

			ytreeviewReport.Binding.AddBinding(ViewModel, vm => vm.ReportRows, w => w.ItemsDataSource).InitializeFromSource();

			buttonInfo.BindCommand(ViewModel.ShowHelpInfoCommand);

			eventboxArrow.ButtonPressEvent += OnEventboxArrowButtonPressEvent;
		}

		protected void OnEventboxArrowButtonPressEvent(object o, ButtonPressEventArgs args)
		{
			parametersContainer.Visible = !parametersContainer.Visible;
			arrowSlider.ArrowType = parametersContainer.Visible ? ArrowType.Left : ArrowType.Right;
		}

		public override void Dispose()
		{
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;

			base.Dispose();
		}
	}
}
