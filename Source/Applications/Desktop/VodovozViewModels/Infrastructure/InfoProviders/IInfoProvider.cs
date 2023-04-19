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

	public class CurrentObjectChangedArgs : EventArgs
	{
		private static CurrentObjectChangedArgs _s_empty => new CurrentObjectChangedArgs(null);

		public object ChangedObject { get; }

		public CurrentObjectChangedArgs(object obj)
		{
			ChangedObject = obj;
		}

		public static new CurrentObjectChangedArgs Empty => _s_empty;
	}
}

