using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;

namespace Mindbox.Library.Services
{
	/// <summary>
	/// Получение бонусов заказа из Mindbox для GetOrderInfo
	/// </summary>
	public interface IMindboxOrderBonusesService
	{
		/// <summary>
		/// Запрашивает бонусы заказа в Mindbox
		/// </summary>
		/// <param name="onlineOrder">Онлайн-заказ ERP</param>
		/// <param name="lineIds">Идентификаторы позиций части разделённого заказа; null — весь заказ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Бонусы заказа или null, если блок не нужно показывать</returns>
		Task<MindboxOrderBonuses> GetAsync(
			OnlineOrder onlineOrder,
			IReadOnlyCollection<string> lineIds,
			CancellationToken cancellationToken);
	}
}
