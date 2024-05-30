using Gamma.ColumnConfig;
using Gdk;
using QS.Utilities;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.Cars;
namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class FuelCardVersionView : WidgetViewBase<FuelCardVersionViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		public FuelCardVersionView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			entityentryFuelCard.ViewModel = ViewModel.FuelCardEntryViewModel;

			datepickerVersionDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDate, w => w.DateOrNull)
				.AddFuncBinding(vm => !vm.IsNewCar, w => w.Sensitive)
				.InitializeFromSource();

			ytreeFuelCardVersions.ColumnsConfig = FluentColumnsConfig<FuelCardVersion>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
					.AddSetter((c, n) => c.BackgroundGdk = n.Id == 0 ? _greenColor : _primaryBaseColor)
				.AddColumn("Топливная карта")
					.AddTextRenderer(x => x.FuelCard.CardNumber).XAlign(0.5f)
				.AddColumn("Начало действия")
					.AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия")
					.AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "")
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeFuelCardVersions.ItemsDataSource = ViewModel.Entity.ObservableFuelCardVersions;
			ytreeFuelCardVersions.Binding.AddBinding(ViewModel, vm => vm.SelectedVersion, w => w.SelectedRow).InitializeFromSource();

			ybuttonNewVersion.Binding.AddBinding(ViewModel, vm => vm.CanAddNewVersion, w => w.Sensitive).InitializeFromSource();
			ybuttonNewVersion.Clicked += (sender, args) =>
			{
				ViewModel.AddNewVersionCommand.Execute();
				GtkHelper.WaitRedraw();
				ytreeFuelCardVersions.Vadjustment.Value = 0;
			};

			ybuttonChangeVersionDate.Binding.AddBinding(ViewModel, vm => vm.CanChangeVersionStartDate, w => w.Sensitive).InitializeFromSource();
			ybuttonChangeVersionDate.Clicked += (sender, args) => ViewModel.ChangeVersionStartDateCommand.Execute();

			Visible = ViewModel.CanRead;
		}
	}
}
