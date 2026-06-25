using System.Collections.Generic;
using Vodovoz.EntityRepositories.Store;

namespace CustomerAppsApi.Library.Models
{
	public interface IWarehouseModel
	{
		IEnumerable<SelfDeliveryAddressDto> GetSelfDeliveriesAddresses();
	}
}
