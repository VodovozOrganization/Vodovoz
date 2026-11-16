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
	}
}
