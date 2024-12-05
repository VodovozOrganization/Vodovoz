using Gtk;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.Bookkeeping.Reports.OrderChanges;

namespace Vodovoz.Views.Reports
{
	public partial class OrderChangesReportView : TabViewBase<OrderChangesReportViewModel>
	{
		private const int _hpanedDefaultPosition = 480;
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



			ConfigureChangeTypesTreeView();
			ConfigureIssueTypesTreeView();
			ConfigureReportRowsTreeView();
		}

		private void ConfigureChangeTypesTreeView()
		{

		}

		private void ConfigureIssueTypesTreeView()
		{

		}

		private void ConfigureReportRowsTreeView()
		{

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

		public override void Destroy()
		{
			eventboxArrow.ButtonPressEvent -= OnEventboxArrowButtonPressEvent;

			base.Destroy();
		}
	}
}
