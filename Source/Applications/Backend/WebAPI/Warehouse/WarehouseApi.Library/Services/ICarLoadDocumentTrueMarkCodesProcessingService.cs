using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;

namespace WarehouseApi.Library.Services
{
	public interface ICarLoadDocumentTrueMarkCodesProcessingService
	{
		/// <summary>
		/// Добавляем коды ЧЗ из staging таблицы в строки талона погрузки и удаляем их из staging таблицы
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="carLoadDocument">Талон погрузки</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения операции</returns>
		Task<Result> AddProductCodesToCarLoadDocumentAndDeleteStagingCodes(IUnitOfWork uow, CarLoadDocumentEntity carLoadDocument, CancellationToken cancellationToken = default);
		/// <summary>
		/// Добавляем отсканированный код ЧЗ в staging таблицу
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="carLoadDocumentItemId">Идентификатор строки талона погрузки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат выполнения операции</returns>
		Task<Result<StagingTrueMarkCode>> AddStagingTrueMarkCode(IUnitOfWork uow, string scannedCode, int carLoadDocumentItemId, CancellationToken cancellationToken = default);
		/// <summary>
		/// Удаляем отсканированный код ЧЗ из staging таблицы
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="carLoadDocumentItemId">Идентификатор строки талона погрузки</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат выполнения операции</returns>
		Task<Result> RemoveStagingTrueMarkCode(IUnitOfWork uow, string scannedCode, int carLoadDocumentItemId, CancellationToken cancellationToken = default);
	}
}
