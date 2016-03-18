using System;

namespace Vodovoz.Panel
{
	public interface IPanelView
	{
		IInfoProvider InfoProvider{get;set;}
		void Refresh();
		void OnCurrentObjectChanged(object changedObject);
	}
}

