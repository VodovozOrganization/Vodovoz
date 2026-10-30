using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace WarehouseApi.Library.Services
{
	/// <summary>
	/// Предзагрузчик кодов маркировки ЧЗ для операций складского API.
	/// </summary>
	public interface IWarehouseTrueMarkCodePreloader
	{
		/// <summary>
		/// Предзагружает коды маркировки ЧЗ и связанные сущности для строк документа погрузки.
		/// </summary>
		/// <param name="documentItems">Строки документа погрузки.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task PreloadCarLoadDocumentItems(
			IEnumerable<CarLoadDocumentItem> documentItems,
			CancellationToken cancellationToken);

		/// <summary>
		/// Предзагружает коды маркировки ЧЗ и связанные сущности для самовывоза.
		/// </summary>
		/// <param name="order">Заказ самовывоза.</param>
		/// <param name="selfDeliveryDocument">Документ самовывоза.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task PreloadSelfDelivery(
			Order order,
			SelfDeliveryDocument selfDeliveryDocument,
			CancellationToken cancellationToken);

		/// <summary>
		/// Предзагружает коды маркировки ЧЗ, связанные с указанными кодами продукции.
		/// </summary>
		/// <param name="productCodes">Коды продукции, для которых нужно загрузить связанные коды ЧЗ.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task PreloadProductCodes(
			IEnumerable<TrueMarkProductCode> productCodes,
			CancellationToken cancellationToken);

		/// <summary>
		/// Предзагружает иерархию кодов ЧЗ: родительские и дочерние транспортные, групповые и единичные коды.
		/// </summary>
		/// <param name="codes">Единичные коды ЧЗ, для которых нужно загрузить иерархию.</param>
		/// <param name="cancellationToken">Токен отмены.</param>
		Task PreloadCodes(
			IEnumerable<TrueMarkWaterIdentificationCode> codes,
			CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает родительский код ЧЗ (групповой или транспортный) для указанного кода.
		/// </summary>
		/// <param name="trueMarkAnyCode">Код ЧЗ, для которого нужно найти родительский код.</param>
		/// <returns>Родительский код ЧЗ или исходный код, если родителя нет.</returns>
		TrueMarkAnyCode GetParentGroupCode(TrueMarkAnyCode trueMarkAnyCode);

		/// <summary>
		/// Возвращает все коды ЧЗ в иерархии указанного кода, включая сам код.
		/// </summary>
		/// <param name="parentCode">Родительский код ЧЗ.</param>
		/// <returns>Все коды ЧЗ в иерархии указанного кода.</returns>
		IEnumerable<TrueMarkAnyCode> GetAllCodes(TrueMarkAnyCode parentCode);
	}
}