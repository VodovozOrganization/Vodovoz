using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Models.Orders;
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

		public override IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			return new List<PartOrderWithGoods>
			{
				new PartOrderWithGoods(
					GetOrganization(
						uow,
						requestTime,
						organizationChoice))
			};
		}
		
		/// <summary>
		/// Получение организации для заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="organizationChoice">Данные для подбора организации</param>
		/// <returns></returns>
		public Organization GetOrganization(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			return GetOrganization(
				uow,
				requestTime,
				organizationChoice.IsSelfDelivery || organizationChoice.DeliveryPoint == null,
				organizationChoice.PaymentType,
				organizationChoice.PaymentFrom,
				organizationChoice.OnlinePaymentNumber);
		}

		/// <summary>
		/// Получение организации для заказа с заданными параметрами
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="isSelfDelivery">Самовывоз или нет</param>
		/// <param name="paymentType">Тип оплаты заказа</param>
		/// <param name="paymentFrom">Источник оплаты</param>
		/// <param name="onlinePaymentNumber">Номер онлайн оплаты</param>
		/// <returns></returns>
		public Organization GetOrganization(
			IUnitOfWork uow,
			TimeSpan requestTime,
			bool isSelfDelivery,
			PaymentType paymentType,
			PaymentFrom paymentFrom,
			int? onlinePaymentNumber
			)
		{
			if(isSelfDelivery)
			{
				return _organizationForSelfDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(
					uow, requestTime, paymentType, paymentFrom, onlinePaymentNumber);
			}

			return _organizationForDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(
				uow, requestTime, paymentType, paymentFrom, onlinePaymentNumber);
		}
	}
}
