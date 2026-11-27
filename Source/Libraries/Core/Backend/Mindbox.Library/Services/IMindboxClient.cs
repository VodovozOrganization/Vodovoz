using System;
using System.Threading;
using System.Threading.Tasks;
using Mindbox.Library.Contracts;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Клиент API Mindbox
	/// </summary>
	public interface IMindboxClient
	{
		/// <summary>
		/// Создаёт или обновляет клиента в Mindbox операцией создания/обновления клиента
		/// </summary>
		/// <param name="request">Тело запроса с данными клиента и точкой контакта</param>
		/// <param name="transactionId">Идентификатор транзакции для идемпотентности и корреляции логов</param>
		/// <param name="cancellationToken">Токен отмены запроса</param>
		/// <returns>
		/// Результат вызова Mindbox: признак успеха, категория ошибки, HTTP-код,
		/// длительность и сырой ответ для разбора вызывающим кодом
		/// </returns>
		Task<MindboxOperationResult> CreateCustomerAsync(
			MindboxCustomerRequest request,
			Guid transactionId,
			CancellationToken cancellationToken);

		/// <summary>
		/// Рассчитывает цены и начисляемые бонусы списка товаров операцией CalculateProductList
		/// </summary>
		/// <param name="request">Тело запроса со списком товаров и базовыми ценами</param>
		/// <param name="cancellationToken">Токен отмены запроса</param>
		/// <returns>
		/// Результат вызова Mindbox: признак успеха, категория ошибки, HTTP-код,
		/// длительность и сырой ответ для разбора вызывающим кодом
		/// </returns>
		Task<MindboxOperationResult> CalculateProductListAsync(
			MindboxCalculateProductListRequest request,
			CancellationToken cancellationToken);

		/// <summary>
		/// Рассчитывает баланс, доступное списание и начисление бонусов для заказа
		/// операцией CalculateAuthorizedCart
		/// </summary>
		/// <param name="request">Тело запроса с клиентом, позициями заказа и суммой списания</param>
		/// <param name="cancellationToken">Токен отмены запроса</param>
		/// <returns>
		/// Результат вызова Mindbox: признак успеха, категория ошибки, HTTP-код,
		/// длительность и сырой ответ для разбора вызывающим кодом
		/// </returns>
		Task<MindboxOperationResult> CalculateAuthorizedCartAsync(
			MindboxAuthorizedCartRequest request,
			CancellationToken cancellationToken);

		/// <summary>
		/// Создаёт заказ с бонусами операцией CreateAuthorizedLoyaltyOrder
		/// </summary>
		Task<MindboxOperationResult> CreateAuthorizedLoyaltyOrderAsync(
			MindboxLoyaltyOrderRequest request,
			Guid transactionId,
			CancellationToken cancellationToken);

		/// <summary>
		/// Изменяет статус позиций заказа операцией 118
		/// </summary>
		/// <param name="request">Тело запроса со статусом позиций</param>
		/// <param name="transactionId">Идентификатор транзакции для идемпотентности</param>
		/// <param name="cancellationToken">Токен отмены запроса</param>
		/// <returns>Результат вызова Mindbox</returns>
		Task<MindboxOperationResult> UpdateOrderLinesStatusAsync(
			MindboxOrderLinesStatusRequest request,
			Guid transactionId,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получает информацию о заказе из Mindbox операцией GetOrderInfo
		/// </summary>
		/// <param name="request">Запрос с идентификатором заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат вызова Mindbox</returns>
		Task<MindboxOperationResult> GetOrderInfoAsync(
			MindboxGetOrderInfoRequest request,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получает историю баланса клиента операцией GetBalanceHistory
		/// </summary>
		/// <param name="request">Запрос с идентификатором клиента</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат вызова Mindbox</returns>
		Task<MindboxOperationResult> GetBalanceHistoryAsync(
			MindboxGetBalanceHistoryRequest request,
			CancellationToken cancellationToken);
	}
}
