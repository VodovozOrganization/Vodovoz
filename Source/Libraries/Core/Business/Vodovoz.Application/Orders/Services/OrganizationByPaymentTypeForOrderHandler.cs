using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации по типу оплаты
	/// </summary>
	public class OrganizationByPaymentTypeForOrderHandler : OrganizationForOrderHandler
	{
		private readonly OrganizationForDeliveryOrderByPaymentTypeHandler _organizationForDeliveryOrderByPaymentTypeHandler;
		private readonly OrganizationForSelfDeliveryOrderByPaymentTypeHandler _organizationForSelfDeliveryOrderByPaymentTypeHandler;

		public OrganizationByPaymentTypeForOrderHandler(
			OrganizationForDeliveryOrderByPaymentTypeHandler organizationForDeliveryOrderByPaymentTypeHandler,
			OrganizationForSelfDeliveryOrderByPaymentTypeHandler organizationForSelfDeliveryOrderByPaymentTypeHandler)
		{
			_organizationForDeliveryOrderByPaymentTypeHandler =
				organizationForDeliveryOrderByPaymentTypeHandler
				?? throw new ArgumentNullException(nameof(organizationForDeliveryOrderByPaymentTypeHandler));
			_organizationForSelfDeliveryOrderByPaymentTypeHandler =
				organizationForSelfDeliveryOrderByPaymentTypeHandler
				?? throw new ArgumentNullException(nameof(organizationForSelfDeliveryOrderByPaymentTypeHandler));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="order"></param>
		/// <param name="uow"></param>
		/// <returns></returns>
		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow)
		{
			return new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>
			{
				new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(GetOrganizationForOrder(requestTime, order, uow))
			};
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="order"></param>
		/// <param name="uow"></param>
		/// <returns></returns>
		public Organization GetOrganizationForOrder(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow)
		{
			if(order.SelfDelivery || order.DeliveryPoint is null)
			{
				return _organizationForSelfDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(requestTime, order, uow);
			}

			return _organizationForDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(requestTime, order, uow);
		}
	}
}
