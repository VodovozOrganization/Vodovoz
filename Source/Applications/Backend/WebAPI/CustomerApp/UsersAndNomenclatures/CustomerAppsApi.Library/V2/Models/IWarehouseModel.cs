using System.Collections.Generic;
using Vodovoz.EntityRepositories.Store;

namespace CustomerAppsApi.Library.V2.Models
{
	public interface IWarehouseModel
	{
		IEnumerable<SelfDeliveryAddressDto> GetSelfDeliveriesAddresses();
	}
}
