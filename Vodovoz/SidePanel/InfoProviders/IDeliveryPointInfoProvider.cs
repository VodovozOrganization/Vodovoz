using System;
using Vodovoz.Domain.Client;

namespace Vodovoz.Panel
{
	public interface IDeliveryPointInfoProvider:IInfoProvider
	{		
		DeliveryPoint DeliveryPoint{get;}
	}
}

