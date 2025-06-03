using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Presentation.ViewModels.CommonSettings;

namespace Vodovoz.Presentation.Views.CommonSettings
{
	[ToolboxItem(true)]
	public partial class RecomendationSettingsView : WidgetViewBase<RecomendationSettingsViewModel>
	{
		public RecomendationSettingsView()
		{
			Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ysbOperator.Binding.CleanSources();
			ysbOperator.Binding
				.AddBinding(ViewModel, vm => vm.OperatorCount, w => w.ValueAsInt)
				.InitializeFromSource();

			ysbRobot.Binding.CleanSources();
			ysbRobot.Binding
				.AddBinding(ViewModel, vm => vm.RobotCount, w => w.ValueAsInt)
				.InitializeFromSource();

			ysbIpz.Binding.CleanSources();
			ysbIpz.Binding
				.AddBinding(ViewModel, vm => vm.IpzCount, w => w.ValueAsInt)
				.InitializeFromSource();

			buttonSave.Clicked += OnSaveClicked;
		}

		private void OnSaveClicked(object sender, EventArgs e)
		{
			ViewModel?.SaveCommand.Execute(null);
		}
	}
}
