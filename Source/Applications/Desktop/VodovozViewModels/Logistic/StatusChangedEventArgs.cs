using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz
{
	public class StatusChangedEventArgs : EventArgs
	{
		public RouteListItemStatus NewStatus { get; private set; }
		public StatusChangedEventArgs(RouteListItemStatus newStatus) => NewStatus = newStatus;
	}
}
