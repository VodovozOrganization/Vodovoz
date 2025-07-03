using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Контракт сервиса разбивки заказа на подзаказы
	/// </summary>
	public interface IPartitioningOrderService
	{
		/// <summary>
		/// Разбитие общего заказа на подзаказы
		/// </summary>
		/// <param name="baseOrderId">Id общего заказа</param>
		/// <param name="employee">Сотрудник</param>
		/// <param name="partitionedOrderByOrganizations">Информация для разделения заказа по организациям, в зависимости от наполнения</param>
		/// <returns></returns>
		Result<IEnumerable<int>> CreatePartOrdersAndSave(
			int baseOrderId,
			Employee employee,
			PartitionedOrderByOrganizations partitionedOrderByOrganizations);
	}
}
