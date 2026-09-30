using QS.Views.GtkUI;
using Vodovoz.ViewModels.Widgets.Mango;

namespace Vodovoz.ViewWidgets.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MangoCallButtonView : WidgetViewBase<MangoCallButtonViewModel>
	{
		public MangoCallButtonView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			if(ViewModel is null)
			{
				return;
			}

			ybuttonMakeCall.HasTooltip = true;

			ybuttonMakeCall.Binding
				.AddBinding(ViewModel, vm => vm.CanMakeCall, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.TooltipText, w => w.TooltipText)
				.InitializeFromSource();

			ybuttonMakeCall.BindCommand(ViewModel.MakeCallCommand);
		}

		protected override void OnDestroyed()
		{
			ViewModel?.Dispose();
			base.OnDestroyed();
		}
	}
}
