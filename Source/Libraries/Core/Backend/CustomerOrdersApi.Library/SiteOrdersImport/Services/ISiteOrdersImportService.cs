using CustomerOrdersApi.Library.SiteOrdersImport.Dto;
using System.Threading;
using System.Threading.Tasks;

namespace CustomerOrdersApi.Library.SiteOrdersImport.Services
{
	/// <summary>
	/// Приём и обработка пакета выгрузки заказов и брошенных корзин с сайта (I-5840).
	/// </summary>
	public interface ISiteOrdersImportService
	{
		/// <summary>
		/// Обрабатывает принятый пакет и возвращает результат с разбивкой по успешно принятым
		/// и ошибочным записям.
		/// </summary>
		/// <param name="request">Принятый пакет</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Ответ с тем же batch_id и списками идентификаторов</returns>
		Task<OrdersImportResponse> ImportAsync(OrdersImportRequest request, CancellationToken cancellationToken);
	}
}
