using System;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel.InfoViews
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CarsMonitoringInfoPanelView : Gtk.Bin, IPanelView
	{
		public CarsMonitoringInfoPanelView()
		{
			Build();
		}

		public IInfoProvider InfoProvider { get; set; }

		public bool VisibleOnPanel => true;

		public void OnCurrentObjectChanged(object changedObject) => Refresh();

		public void Refresh()
		{
			return;
		}
	}
}
