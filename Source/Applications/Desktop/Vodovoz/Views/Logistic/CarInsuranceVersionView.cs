using Gamma.ColumnConfig;
using Gdk;
using QS.Views.GtkUI;
using System.ComponentModel;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Widgets.Cars.Insurance;
namespace Vodovoz.Views.Logistic
{
	[ToolboxItem(true)]
	public partial class CarInsuranceVersionView : WidgetViewBase<CarInsuranceVersionViewModel>
	{
		private static readonly Color _greenColor = GdkColors.SuccessText;
		private static readonly Color _primaryBaseColor = GdkColors.PrimaryBase;

		public CarInsuranceVersionView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			ytreeCarInsuranceVersion.ColumnsConfig = FluentColumnsConfig<CarInsurance>.Create()
				.AddColumn("Код").MinWidth(50).HeaderAlignment(0.5f).AddTextRenderer(x => x.Id == 0 ? "Новая" : x.Id.ToString()).XAlign(0.5f)
					.AddSetter((c, n) => c.BackgroundGdk = n.Id == 0 ? _greenColor : _primaryBaseColor)
				.AddColumn("Начало действия")
					.AddTextRenderer(x => x.StartDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Окончание действия")
					.AddTextRenderer(x => x.EndDate.ToString("g")).XAlign(0.5f)
				.AddColumn("Страховщик")
					.AddTextRenderer(x => x.Insurer.Name).XAlign(0.5f)
				.AddColumn("Номер")
					.AddTextRenderer(x => x.InsuranceNumber).XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeCarInsuranceVersion.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.SelectedCarInsurance, w => w.SelectedRow)
				.AddBinding(vm => vm.Insurances, w => w.ItemsDataSource)
				.AddBinding(vm => vm.IsInsurancesSensitive, w => w.Sensitive)
				.InitializeFromSource();

			yhboxButtons.Binding
				.AddBinding(ViewModel, vm => vm.IsInsurancesSensitive, w => w.Sensitive)
				.InitializeFromSource();

			ycheckbuttonIsNotRelevantForCar.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.CanChangeInsuranceNotRelevantForCar, w => w.Visible)
				.AddBinding(e => e.IsInsuranceNotRelevantForCar, w => w.Active)
				.InitializeFromSource();

			ycheckbuttonIsNotRelevantForCar.Clicked += (sender, e) => ViewModel.ChangeIsKaskoNotRelevantCommand.Execute();

			ybuttonNewVersion.Binding
				.AddBinding(ViewModel, vm => vm.CanAddCarInsurance, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonEditVersion.Binding
				.AddBinding(ViewModel, vm => vm.CanEditCarInsurance, w => w.Sensitive)
				.InitializeFromSource();

			ybuttonNewVersion.BindCommand(ViewModel.AddCarInsuranceCommand);
			ybuttonEditVersion.BindCommand(ViewModel.EditCarInsuranceCommand);
		}

		public override void Destroy()
		{
			ytreeCarInsuranceVersion.Destroy();

			base.Destroy();
		}
	}
}
