using System;
using QSOrmProject;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IInfoProvider
	{
		event EventHandler<CurrentObjectChangedArgs> CurrentObjectChanged;
		PanelViewType[] InfoWidgets{get;}
		IUnitOfWork UoW{get;}
	}

	public class CurrentObjectChangedArgs : EventArgs{
		public object ChangedObject{get;set;}
		public CurrentObjectChangedArgs(object obj){
			this.ChangedObject = obj;
		}
	}
}

