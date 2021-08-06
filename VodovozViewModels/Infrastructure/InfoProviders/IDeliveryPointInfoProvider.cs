using Vodovoz.Domain.Client;
using Vodovoz.SidePanel.InfoProviders;

namespace Vodovoz.ViewModels.Infrastructure.InfoProviders
{
	public interface IDeliveryPointInfoProvider : IInfoProvider
	{
		DeliveryPoint DeliveryPoint {get;}
	}
}
