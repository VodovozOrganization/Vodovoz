using System;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.SidePanel
{
	public interface IPanelView
	{
		IInfoProvider InfoProvider{get;set;}
		void Refresh();
		void OnCurrentObjectChanged(object changedObject);
		bool VisibleOnPanel{ get; }
	}
}