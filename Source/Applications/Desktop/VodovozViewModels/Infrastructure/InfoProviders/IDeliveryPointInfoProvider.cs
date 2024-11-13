using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.ViewModels.Infrastructure.InfoProviders
{
	public interface IDeliveryPointInfoProvider : IInfoProvider
	{
		DeliveryPoint DeliveryPoint {get;}
		OrderAddressType? TypeOfAddress { get; } 

	}
}
