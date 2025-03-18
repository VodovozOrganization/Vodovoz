using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации для доставляемого заказа
	/// </summary>
	public class OrganizationForDeliveryOrderByPaymentTypeHandler : OrganizationForOrderByDelivery, IGetOrganizationForOrder
	{
		public OrganizationForDeliveryOrderByPaymentTypeHandler(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
		: base(organizationSettings, orderSettings, fastPaymentRepository, organizationForOrderFromSet)
		{
		}

		public IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
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
		/// Подбор организации в зависимости от типа оплаты заказа
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="order">Заказ, для которого подбирается организация</param>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="paymentType">Тип оплаты заказа</param>
		/// <returns>Организация</returns>
		/// <exception cref="NotSupportedException">Не поддерживается переданный тип оплаты</exception>
		public Organization GetOrganizationForOrder(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			var onlineOrderId = order.OnlineOrder;
			paymentType = order.PaymentType;

			var organizationsByPaymentType = uow.GetAll<PaymentTypeOrganizationSettings>().ToList();
			var paymentTypeOrganization = organizationsByPaymentType.FirstOrDefault(settings => settings.PaymentType == paymentType);

			if(paymentTypeOrganization is null)
			{
				throw new NotSupportedException(
					$"Невозможно подобрать организацию, так как тип оплаты {paymentType} не поддерживается.");
			}

			if(paymentTypeOrganization.PaymentType == PaymentType.SmsQR
				|| paymentTypeOrganization.PaymentType == PaymentType.DriverApplicationQR
				|| paymentTypeOrganization.PaymentType == PaymentType.PaidOnline)
			{
				var organizationId = GetOrganizationId(
					uow,
					paymentTypeOrganization,
					requestTime,
					onlineOrderId);
				
				return uow.GetById<Organization>(organizationId);
			}

			return OrganizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, paymentTypeOrganization);
		}
	}
}
