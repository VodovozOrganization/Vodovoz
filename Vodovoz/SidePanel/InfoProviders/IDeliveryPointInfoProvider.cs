using System;
using Vodovoz.Domain;

namespace Vodovoz.Panel
{
	public interface IDeliveryPointInfoProvider:IInfoProvider
	{		
		DeliveryPoint DeliveryPoint{get;}
	}
}

