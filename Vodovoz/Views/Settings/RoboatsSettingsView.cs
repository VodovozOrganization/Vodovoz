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
	}
}
