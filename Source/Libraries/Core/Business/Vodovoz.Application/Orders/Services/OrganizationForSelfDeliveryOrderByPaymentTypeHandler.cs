using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Settings;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации для самовывоза
	/// </summary>
	public class OrganizationForSelfDeliveryOrderByPaymentTypeHandler : OrganizationForOrderByDelivery
	{
		public OrganizationForSelfDeliveryOrderByPaymentTypeHandler(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
			: base(organizationSettings, orderSettings, fastPaymentRepository, organizationForOrderFromSet)
		{
		}

		/// <summary>
		/// Подбор организации в зависимости от типа оплаты заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="paymentType">Тип оплаты заказа</param>
		/// <param name="paymentFrom">Источник оплаты</param>
		/// <param name="onlineOrderNumber">Номер онлайн оплаты</param>
		/// <returns>Организация</returns>
		/// <exception cref="NotSupportedException">Не поддерживается переданный тип оплаты</exception>
		public Organization GetOrganizationForOrder(
			IUnitOfWork uow,
			TimeSpan requestTime,
			PaymentType paymentType,
			PaymentFrom paymentFrom,
			int? onlineOrderNumber)
		{
			PaymentTypeOrganizationSettings paymentTypeOrganization = null;

			if(paymentFrom is null)
			{
				paymentTypeOrganization = uow.GetAll<PaymentTypeOrganizationSettings>()
					.FirstOrDefault(settings => settings.PaymentType == paymentType);
			}
			else
			{
				paymentTypeOrganization = uow.GetAll<OnlinePaymentTypeOrganizationSettings>()
					.FirstOrDefault(settings => settings.PaymentFrom == paymentFrom);
			}

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
					onlineOrderNumber);
				
				return uow.GetById<Organization>(organizationId);
			}

			return OrganizationForOrderFromSet.GetOrganizationForOrderFromSet(requestTime, paymentTypeOrganization);
		}
	}
}
