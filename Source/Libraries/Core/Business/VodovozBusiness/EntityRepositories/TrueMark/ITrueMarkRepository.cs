using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		Task<IEnumerable<TrueMarkWaterIdentificationCode>> LoadWaterCodes(List<int> codeIds, CancellationToken cancellationToken);

		ISet<string> GetAllowedCodeOwnersInn();

		ISet<string> GetAllowedCodeOwnersGtins();

		IEnumerable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(IUnitOfWork uow, string gtin, string serialNumber, string checkCode);

		/// <summary>
		/// Получить все значения Gtin в базе с названиями номенклатур
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Словарь. Ключ - gtin, значение - список названий номенклатур с данным gtin</returns>
		Task<IDictionary<string, List<string>>> GetGtinsNomenclatureData(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Получить все значения Gtin товаров, проданных за предыдущий день, и их количество
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Словарь. Ключ - gtin, значение - количество единиц товаров с данным gtin</returns>
		Task<IDictionary<string, int>> GetSoldYesterdayGtinsCount(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Получить значение Gtin и количество кодов, отсутствующих в пуле, необходимых для формирования передаточных документов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Словарь. Ключ - gtin, значение - количество кодов с данным gtin, необходимых для документов</returns>
		Task<IDictionary<string, int>> GetMissingCodesCount(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Проверяет, сохранён ли уже код TrueMark в базе
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="trueMarkAnyCode">Код ЧЗ</param>
		/// <returns>Результат проверки</returns>
		bool IsTrueMarkAnyCodeAlreadySaved(IUnitOfWork uow, TrueMarkAnyCode trueMarkAnyCode);

		/// <summary>
		/// Возвращает транспортные коды маркировки
		/// </summary>
		IEnumerable<TrueMarkTransportCode> GetTransportCodes(IUnitOfWork uow, IEnumerable<int> transportCodeIds);

		/// <summary>
		/// Возвращает групповые коды маркировки
		/// </summary>
		IEnumerable<TrueMarkWaterGroupCode> GetGroupWaterCodes(IUnitOfWork uow, IEnumerable<int> groupCodeIds);

		/// <summary>
		/// Возвращает коды маркировки для заказа,
		/// которые были добавлены складом в документе погрузки автомобиля.
		/// </summary>
		IEnumerable<CarLoadDocumentItemTrueMarkProductCode> GetCodesFromWarehouseByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает коды маркировки для заказа,
		/// которые были добавлены из маршрутного листа водителем.
		/// </summary>
		IEnumerable<RouteListItemTrueMarkProductCode> GetCodesFromDriverByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает коды маркировки для заказа,
		/// которые были добавлены из самовывоза.
		/// </summary>
		IEnumerable<SelfDeliveryDocumentItemTrueMarkProductCode> GetCodesFromSelfdeliveryByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает коды маркировки для заказа, 
		/// которые были добавлены из пула в виду отсутствия 
		/// кодов из других источников (склад, водитель, самовывоз).
		/// </summary>
		IEnumerable<AutoTrueMarkProductCode> GetCodesFromPoolByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает кол-во кодов маркировки требуемое в заказе
		/// </summary>
		int GetCodesRequiredByOrder(IUnitOfWork uow, int orderId);

		/// <summary>
		/// Возвращает использованные коды ЧЗ товара в которых идентификационный код совпадает с добавляемым промежуточным кодов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="stagingTrueMarkCode">Код ЧЗ для промежуточного хранения</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список кодов ЧЗ товаров</returns>
		Task<IEnumerable<TrueMarkProductCode>> GetUsedTrueMarkProductCodeByStagingTrueMarkCode(IUnitOfWork uow, StagingTrueMarkCode stagingTrueMarkCode, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает номер заказа по коду товара Честного Знака
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="trueMarkProductCode">Код ЧЗ товара</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Номер заказа</returns>
		Task<int?> GetOrderIdByTrueMarkProductCode(IUnitOfWork uow, TrueMarkProductCode trueMarkProductCode, CancellationToken cancellationToken);

		/// <summary>
		/// Возвращает все коды ЧЗ для промежуточного хранения, добавленные для заказа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="orderId">Номер заказа</param>
		/// <returns></returns>
		IList<StagingTrueMarkCode> GetAllStagingCodesByOrderId(IUnitOfWork uow, int orderId);
	}
}
