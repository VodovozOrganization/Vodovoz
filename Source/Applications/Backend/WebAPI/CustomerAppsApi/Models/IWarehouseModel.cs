using System.Collections.Generic;
using Vodovoz.EntityRepositories.Store;

namespace CustomerAppsApi.Models
{
	public interface IWarehouseModel
	{
		IEnumerable<SelfDeliveryAddressDto> GetSelfDeliveriesAddresses();
	}
}
