using SmsPaymentService.DTO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SmsPaymentService
{
    [ServiceContract]
    public interface ISmsPaymentService
    {
        /// <summary>
        /// Меняет статус платежа и время оплаты
        /// </summary>
        [OperationContract, WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        StatusCode ReceivePayment(RequestBody body);

        /// <summary>
        /// Формирует и отправляет платеж, обрабатывает метод GET
        /// </summary>
        /// <param name="orderId">Номер заказа</param>
        /// <param name="phoneNumber">Номер телефона клиента</param>
        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
		PaymentResult SendPayment(int orderId, string phoneNumber);

        /// <summary>
        /// Формирует и отправляет платеж, обрабатывает метод POST
        /// </summary>
        [OperationContract, WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        PaymentResult SendPaymentPost(SendPaymentRequest sendPaymentRequest);

        /// <summary>
        /// Записывает в базу и возвращает актуальный статус платежа
        /// </summary>
        /// <param name="externalId">Id платежа во внешней базе</param>
        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
        PaymentResult RefreshPaymentStatus(int externalId);

		/// <summary>
		/// Возвращает актуальный статус оплаты заказа
		/// </summary>
		/// <param name="orderId">Id заказа</param>
		[OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]
		PaymentResult GetActualPaymentStatus(int orderId);

		/// <summary>
        /// Синхронизирует статусы платежей
        /// </summary>
        [OperationContract]
        void SynchronizePaymentStatuses();

        [OperationContract, WebGet(ResponseFormat = WebMessageFormat.Json)]

        bool ServiceStatus();
    }
}
