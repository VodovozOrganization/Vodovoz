using QS.Views.GtkUI;
using System;
using Vodovoz.ViewModels.Widgets.Mango;

namespace Vodovoz.ViewWidgets.Mango
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MangoCallButtonView : WidgetViewBase<MangoCallButtonViewModel>
	{
		public MangoCallButtonView(MangoCallButtonViewModel viewModel) : base(viewModel)
		{
			this.Build();
			Configure();
		}

		private void Configure()
		{
			ybuttonMakeCall.Binding
				.AddBinding(ViewModel, vm => vm.CanMakeCall, w => w.Sensitive)
				.AddBinding(ViewModel, vm => vm.TooltipText, w => w.TooltipText)
				.InitializeFromSource();

			ybuttonMakeCall.Clicked += OnMakeCallButtonClicked;
		}

		private void OnMakeCallButtonClicked(object sender, EventArgs e)
		{
			ViewModel.MakeCallCommand.Execute();
		}
	}
}
