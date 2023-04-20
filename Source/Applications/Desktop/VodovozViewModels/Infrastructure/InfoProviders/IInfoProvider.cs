using System;
using QS.DomainModel.UoW;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IInfoProvider
	{
		event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		PanelViewType[] InfoWidgets { get; }
		IUnitOfWork UoW { get; }
	}
}

