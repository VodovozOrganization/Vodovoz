using FastPaymentsApi.Contracts.Requests;
using FastPaymentsApi.Contracts.Responses;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;

namespace FastPaymentsAPI.Library.Managers
{
	public interface IOrderRequestManager
	{
		/// <summary>
		/// Получение URL для оплаты онлайн-заказа
		/// </summary>
		/// <param name="fastPaymentGuid">Уникальный идентификатор быстрого платежа</param>
		/// <returns>Полный URL для перенаправления на страницу оплаты</returns>
		string GetVodovozFastPayUrl(Guid fastPaymentGuid);

		/// <summary>
		/// Регистрация обычного заказа в системе эквайринга
		/// </summary>
		/// <param name="order">Заказ из системы ДВ</param>
		/// <param name="fastPaymentGuid">Уникальный идентификатор быстрого платежа</param>
		/// <param name="organization">Организация, от имени которой производится оплата</param>
		/// <param name="phoneNumber">Номер телефона клиента для отправки ссылки на оплату (опционально)</param>
		/// <param name="isQr">True - оплата по QR-коду, False - оплата по ссылке</param>
		/// <returns>Ответ от системы эквайринга с результатами регистрации заказа</returns>
		Task<OrderRegistrationResponseDTO> RegisterOrder(
			Order order,
			Guid fastPaymentGuid,
			Organization organization,
			string phoneNumber = null,
			bool isQr = true);

		/// <summary>
		/// Регистрация онлайн-заказа (с сайта, мобильного приложения и т.д.) в системе эквайринга
		/// </summary>
		/// <param name="registerOnlineOrderDto">DTO с данными онлайн-заказа для регистрации</param>
		/// <param name="organization">Организация, от имени которой производится оплата</param>
		/// <param name="fastPaymentRequestFromType">Тип источника запроса (сайт, мобильное приложение, AI бот)</param>
		/// <returns>Ответ от системы эквайринга с результатами регистрации онлайн-заказа</returns>
		Task<OrderRegistrationResponseDTO> RegisterOnlineOrder(
			RequestRegisterOnlineOrderDTO registerOnlineOrderDto,
			Organization organization,
			FastPaymentRequestFromType fastPaymentRequestFromType);

		/// <summary>
		/// Получение информации о платеже по его тикету/сессии
		/// </summary>
		/// <param name="ticket">Тикет/сессия оплаты, присвоенный системой эквайринга</param>
		/// <param name="organization">Организация, через которую производилась оплата</param>
		/// <returns>Информация о статусе и деталях платежа</returns>
		Task<OrderInfoResponseDTO> GetOrderInfo(string ticket, Organization organization);

		/// <summary>
		/// Отмена платежа (отмена сессии оплаты)
		/// </summary>
		/// <param name="ticket">Тикет/сессия оплаты, которую необходимо отменить</param>
		/// <param name="organization">Организация, через которую производилась оплата</param>
		/// <returns>Результат отмены платежа с кодом ответа</returns>
		Task<CancelPaymentResponseDTO> CancelPayment(string ticket, Organization organization);

		/// <summary>
		/// Создание запроса на возврат денежных средств по платежу
		/// </summary>
		/// <param name="ticket">Тикет/сессия оплаты, по которой производится возврат</param>
		/// <param name="organization">Организация, через которую производилась оплата</param>
		/// <param name="amount">Сумма возврата. Если не указана (null) - возвращается полная сумма платежа</param>
		/// <returns>Результат инициации возврата с кодом и сообщением</returns>
		Task<ReverseOrderResponseDTO> ReverseOrder(string ticket, Organization organization, decimal? amount = null);
	}
}
