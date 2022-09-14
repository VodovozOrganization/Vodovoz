using System;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Logistic
{
	public class RouteListKeepingNode
	{
		public RouteListItemStatus Status => RouteListItem.Status;

		public RouteListItem RouteListItem { get; set; }

		public string LastUpdate
		{
			get
			{
				var maybeLastUpdate = RouteListItem.StatusLastUpdate;
				
				if(!maybeLastUpdate.HasValue)
				{
					return string.Empty;
				}

				return maybeLastUpdate.Value.Date == DateTime.Today
					? maybeLastUpdate.Value.ToShortTimeString() 
					: maybeLastUpdate.Value.ToString();
			}
		}
	}
}
