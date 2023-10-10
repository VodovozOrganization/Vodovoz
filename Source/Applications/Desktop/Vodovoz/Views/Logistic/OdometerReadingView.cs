using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Utilities;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.Cars;

namespace Vodovoz.Views.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OdometerReadingView : WidgetViewBase<OdometerReadingsViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		public OdometerReadingView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			datepickerDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDate, w => w.DateOrNull)
				.AddFuncBinding(vm => !vm.IsNewCar, w => w.Sensitive)
				.InitializeFromSource();

			ytreeOdometerReading.ColumnsConfig = FluentColumnsConfig<OdometerReading>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
					.AddSetter((c, n) => c.BackgroundGdk = n.Id == 0 ? _greenColor : _primaryBaseColor)
				.AddColumn("Одометр")
					.AddNumericRenderer(x => x.Odometer)
					.Adjustment(new Adjustment(1, 0, 10000000, 1, 100, 100))
					.AddSetter((c, n) => { c.Editable = n.Id == 0 || ViewModel.CanEdit; })
					.XAlign(0.5f)
				.AddColumn("Начало действия")
					.AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "")
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeOdometerReading.ItemsDataSource = ViewModel.Entity.ObservableOdometerReadings;
			ytreeOdometerReading.Binding.AddBinding(ViewModel, vm => vm.SelectedOdometerReading, w => w.SelectedRow).InitializeFromSource();

			buttonNew.Binding.AddBinding(ViewModel, vm => vm.CanAddNewOdometerReading, w => w.Sensitive).InitializeFromSource();
			buttonNew.Clicked += (sender, args) =>
			{
				ViewModel.AddNewOdometerReadingCommand.Execute();
				GtkHelper.WaitRedraw();
				ytreeOdometerReading.Vadjustment.Value = 0;
			};

			buttonChangeDate.Binding.AddBinding(ViewModel, vm => vm.CanChangeOdometerReadingDate, w => w.Sensitive).InitializeFromSource();
			buttonChangeDate.Clicked += (sender, args) => ViewModel.ChangeOdometerReadingStartDateCommand.Execute();

			Visible = ViewModel.CanRead;
		}
	}
}
