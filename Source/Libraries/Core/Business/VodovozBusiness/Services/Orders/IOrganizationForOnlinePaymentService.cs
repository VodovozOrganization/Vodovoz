using System;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Контракт сервиса подбора организации для быстрого платежа
	/// </summary>
	public interface IOrganizationForOnlinePaymentService
	{
		/// <summary>
		/// Получение организации или сообщения об ошибке для быстрого платежа из ИПЗ
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="requestFromType">Источник запроса <see cref="FastPaymentRequestFromType"/></param>
		/// <returns>Результат подбора организации <see cref="Result"/></returns>
		Result<Organization> GetOrganizationForFastPayment(
			IUnitOfWork uow,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType);
		
		/// <summary>
		/// Получение организации или сообщения об ошибке для быстрого платежа
		/// </summary>
		/// <param name="uow">Unit of work</param>
		/// <param name="order">Заказ</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="requestFromType">Источник запроса <see cref="FastPaymentRequestFromType"/></param>
		/// <returns>Результат подбора организации <see cref="Result"/></returns>
		Result<Organization> GetOrganizationForFastPayment(
			IUnitOfWork uow,
			Order order,
			TimeSpan requestTime,
			FastPaymentRequestFromType requestFromType
		);
	}
}
