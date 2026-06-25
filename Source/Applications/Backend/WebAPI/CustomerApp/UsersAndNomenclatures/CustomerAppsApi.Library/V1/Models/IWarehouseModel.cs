using System.Collections.Generic;
using Vodovoz.EntityRepositories.Store;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface IWarehouseModel
	{
		IEnumerable<SelfDeliveryAddressDto> GetSelfDeliveriesAddresses();
	}
}
