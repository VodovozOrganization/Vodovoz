using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Errors;

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
