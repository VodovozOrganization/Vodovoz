using System.ComponentModel;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using QS.Views.GtkUI;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.Cars.CarVersions;

namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class CarVersionsView : WidgetViewBase<CarVersionsViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		public CarVersionsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			datepickerVersionDate.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedDate, w => w.DateOrNull)
				.AddFuncBinding(vm => !vm.IsNewCar, w => w.Sensitive)
				.InitializeFromSource();

			ytreeCarVersions.ColumnsConfig = FluentColumnsConfig<CarVersion>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
					.AddSetter((c, n) => c.BackgroundGdk = n.Id == 0 ? _greenColor : _primaryBaseColor)
				.AddColumn("Принадлежность")
					.AddComboRenderer(x => x.CarOwnType)
					.SetDisplayFunc(x => x.GetEnumTitle())
					.XAlign(0.5f)
				.AddColumn("Начало действия").AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия").AddTextRenderer(x => x.EndDate.HasValue ? x.EndDate.Value.ToString("g") : "").XAlign(0.5f)
				.AddColumn("Собственник").AddTextRenderer(x => x.CarOwnerOrganization == null ? "" : x.CarOwnerOrganization.Name).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeCarVersions.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CarVersions, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedCarVersion, w => w.SelectedRow)
				.InitializeFromSource();

			buttonNewVersion.Binding.AddBinding(ViewModel, vm => vm.CanAddNewVersion, w => w.Sensitive).InitializeFromSource();
			buttonNewVersion.BindCommand(ViewModel.AddNewVersionCommand);

			buttonChangeVersionDate.Binding.AddBinding(ViewModel, vm => vm.CanChangeVersionStartDate, w => w.Sensitive).InitializeFromSource();
			buttonChangeVersionDate.BindCommand(ViewModel.ChangeStartDateCommand);

			buttonEditOwner.Binding.AddBinding(ViewModel, vm => vm.CanEditCarOwner, w => w.Sensitive).InitializeFromSource();
			buttonEditOwner.BindCommand(ViewModel.EditCarOwnerCommand);

			Visible = ViewModel.IsWidgetVisible;
		}
	}
}
