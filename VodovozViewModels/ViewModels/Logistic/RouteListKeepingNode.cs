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
				if(maybeLastUpdate.HasValue)
				{
					if(maybeLastUpdate.Value.Date == DateTime.Today)
					{
						return maybeLastUpdate.Value.ToShortTimeString();
					}

					return maybeLastUpdate.Value.ToString();
				}

				return string.Empty;
			}
		}
	}
}
