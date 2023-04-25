using System;

namespace Vodovoz.SidePanel.InfoProviders
{
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

