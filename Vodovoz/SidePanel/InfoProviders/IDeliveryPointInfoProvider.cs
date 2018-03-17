using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.SidePanel.InfoProviders
{
	public interface IDeliveryPointInfoProvider:IInfoProvider
	{		
		DeliveryPoint DeliveryPoint{get;}
	}
}

