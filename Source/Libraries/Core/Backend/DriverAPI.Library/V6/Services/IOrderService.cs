using DriverApi.Contracts.V6;
using DriverApi.Contracts.V6.Responses;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors;

namespace DriverAPI.Library.V6.Services
{
	/// <summary>
	/// Интерфейс сервиса заказов
	/// </summary>
	public interface IOrderService
	{
		/// <summary>
		/// Получить заказ по номеру
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Результат с DTO заказа</returns>
		Result<OrderDto> GetOrder(int orderId);

		/// <summary>
		/// Получить заказы по массиву номеров
		/// </summary>
		/// <param name="orderIds">Массив номеров заказов</param>
		/// <returns>Перечисление DTO заказов</returns>
		IEnumerable<OrderDto> Get(int[] orderIds);

		/// <summary>
		/// Изменить тип оплаты заказа
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="paymentType">Тип оплаты</param>
		/// <param name="driver">Водитель</param>
		/// <param name="paymentByTerminalSource">Источник оплаты через терминал</param>
		/// <returns>Результат изменения типа оплаты</returns>
		Result ChangeOrderPaymentType(int orderId, PaymentType paymentType, Employee driver, PaymentByTerminalSource? paymentByTerminalSource);

		/// <summary>
		/// Получить доступные для изменения типы оплаты для заказа
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <returns>Перечисление доступных типов оплаты</returns>
		IEnumerable<PaymentDtoType> GetAvailableToChangePaymentTypes(Order order);

		/// <summary>
		/// Получить доступные для изменения типы оплаты для заказа
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Результат с перечислением доступных типов оплаты</returns>
		Result<IEnumerable<PaymentDtoType>> GetAvailableToChangePaymentTypes(int orderId);

		/// <summary>
		/// Отправить запрос на оплату по QR-коду
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="driverId">Идентификатор водителя</param>
		/// <returns>Задача с результатом запроса на оплату по QR-коду</returns>
		Task<Result<PayByQrResponse>> SendQrPaymentRequestAsync(int orderId, int driverId);

		/// <summary>
		/// Создание рекламации по координатам точки доставки заказа
		/// </summary>
		/// <param name="actionTime">Время действия</param>
		/// <param name="driver">Водитель</param>
		/// <param name="completeOrderInfo">Информация о завершении заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат обновления информации о доставке</returns>
		Task<Result> UpdateOrderShipmentInfoAsync(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo, CancellationToken cancellationToken);

		/// <summary>
		/// Получить дополнительную информацию о заказе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <returns>Результат с дополнительной информацией о заказе</returns>
		Result<OrderAdditionalInfoDto> GetAdditionalInfo(int orderId);

		/// <summary>
		/// Получить дополнительную информацию о заказе
		/// </summary>
		/// <param name="order">Заказ</param>
		/// <returns>Результат с дополнительной информацией о заказе</returns>
		Result<OrderAdditionalInfoDto> GetAdditionalInfo(Order order);

		/// <summary>
		/// Обновить количество бутылей по фактическому наличию на складе
		/// </summary>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="bottlesByStockActualCount">Фактическое количество бутылей на складе</param>
		/// <returns>Результат обновления количества бутылей</returns>
		Result UpdateBottlesByStockActualCount(int orderId, int bottlesByStockActualCount);

		/// <summary>
		/// Завершить доставку заказа
		/// </summary>
		/// <param name="actionTime">Время действия</param>
		/// <param name="driver">Водитель</param>
		/// <param name="completeOrderInfo">Информация о завершении заказа</param>
		/// <param name="driverComplaintInfo">Информация о жалобе водителя</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат завершения доставки заказа</returns>
		Task<Result> CompleteOrderDelivery(DateTime actionTime, Employee driver, IDriverOrderShipmentInfo completeOrderInfo, IDriverComplaintInfo driverComplaintInfo, CancellationToken cancellationToken);

		/// <summary>
		/// Добавить код ЧЗ
		/// </summary>
		/// <param name="actionTime">Время действия</param>
		/// <param name="driver">Водитель</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="orderSaleItemId">Номер строки заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача с результатом обработки кода ЧЗ</returns>
		Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> AddTrueMarkCode(DateTime actionTime, Employee driver, int orderId, int orderSaleItemId, string scannedCode, CancellationToken cancellationToken);

		/// <summary>
		/// Изменить код ЧЗ
		/// </summary>
		/// <param name="actionTime">Время действия</param>
		/// <param name="driver">Водитель</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="orderSaleItemId">Номер строки заказа</param>
		/// <param name="oldScannedCode">Старый сканированный код</param>
		/// <param name="newScannedCode">Новый сканированный код</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача с результатом обработки кода ЧЗ</returns>
		Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> ChangeTrueMarkCode(DateTime actionTime, Employee driver, int orderId, int orderSaleItemId, string oldScannedCode, string newScannedCode, CancellationToken cancellationToken);

		/// <summary>
		/// Удалить код ЧЗ
		/// </summary>
		/// <param name="driver">Водитель</param>
		/// <param name="orderId">Номер заказа</param>
		/// <param name="orderSaleItemId">Номер строки заказа</param>
		/// <param name="scannedCode">Сканированный код</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача с результатом обработки кода ЧЗ</returns>
		Task<RequestProcessingResult<TrueMarkCodeProcessingResultResponse>> RemoveTrueMarkCode(Employee driver, int orderId, int orderSaleItemId, string scannedCode, CancellationToken cancellationToken);

		/// <summary>
		/// Отправить коды ЧЗ
		/// </summary>
		/// <param name="actionTime">Время действия</param>
		/// <param name="driver">Водитель</param>
		/// <param name="orderId">Номер строки заказа</param>
		/// <param name="scannedBottles">Данные по отсканированным бутылкам</param>
		/// <param name="unscannedBottlesReason">Причина несканирования кодов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача с результатом обработки кодов ЧЗ</returns>
		Task<Result> SendTrueMarkCodes(DateTime actionTime, Employee driver, int orderId, IEnumerable<OrderItemScannedBottlesDto> scannedBottles, string unscannedBottlesReason, CancellationToken cancellationToken);

		/// <summary>
		/// Проверить код через API Честного Знака
		/// </summary>
		/// <param name="code">Транспортный код для проверки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Задача с результатом проверки транспортного кода</returns>
		Task<RequestProcessingResult<CheckCodeResultResponse>> CheckCode(string code, CancellationToken cancellationToken);
	}
}
