using System;
using QS.Views.GtkUI;
using Vodovoz.ViewModels.ViewModels.Settings;

namespace Vodovoz.Views.Settings
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RoboatsSettingsView : WidgetViewBase<RoboatsSettingsViewModel>
	{
		public RoboatsSettingsView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			ycheckbuttonRoboatsEnabled.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.RoboatsServiceEnabled, w => w.Active)
				.AddBinding(vm => vm.CanEdit, w => w.Sensitive)
				.InitializeFromSource();
		}
	}
}
