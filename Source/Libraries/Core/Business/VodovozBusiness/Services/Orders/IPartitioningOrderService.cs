using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Services.Orders
{
	public interface IPartitioningOrderService
	{
		Result<IEnumerable<int>> CreatePartOrdersAndSave(
			int baseOrderId,
			Employee employee,
			PartitionedOrderByOrganizations partitionedOrderByOrganizations);
	}
}
