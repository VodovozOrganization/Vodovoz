using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace VodovozBusiness.Services.TrueMark
{
	public interface ITrueMarkWaterCodeService
	{
		/// <summary>
		/// Статусы кодов ЧЗ, которые были успешно использованы
		/// </summary>
		IList<SourceProductCodeStatus> SuccessfullyUsedProductCodesStatuses { get; }

		Result<TrueMarkAnyCode> TryGetSavedTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode);
		Task<Result<TrueMarkAnyCode>> GetTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode, CancellationToken cancellationToken = default);


		/// <summary>
		/// Проверяет, что все коды ЧЗ в обороте и содержат корректные ИНН владельца
		/// </summary>
		/// <param name="trueMarkWaterIdentificationCodes">Код ЧЗ</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Результат проверки</returns>
		Task<Result> IsAllTrueMarkCodesValid(IEnumerable<TrueMarkWaterIdentificationCode> trueMarkWaterIdentificationCodes, CancellationToken cancellationToken);

		/// <summary>
		/// Проверяет, что код ЧЗ в обороте и содержит корректный ИНН владельца
		/// </summary>
		/// <param name="trueMarkWaterIdentificationCode">Коды ЧЗ</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Результат проверки</returns>
		Task<Result> IsTrueMarkCodeValid(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, CancellationToken cancellationToken);

		/// <summary>
		/// Проверяет, что код ЧЗ не использован
		/// </summary>
		/// <param name="trueMarkWaterIdentificationCode">Код ЧЗ</param>
		/// <param name="exceptProductCodeId">При проверке исключить сохраненную запись кода ЧЗ товара</param>
		/// <returns>Результат проверки</returns>
		Result IsTrueMarkWaterIdentificationCodeNotUsed(TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCode, int exceptProductCodeId = 0);

		/// <summary>
		/// Загружает или создает код ЧЗ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCode">Строка отсканированного кода</param>
		/// <returns>Код ЧЗ</returns>
		[Obsolete("Use TryGetSavedTrueMarkCodeByScannedCode instead")]
		TrueMarkWaterIdentificationCode LoadOrCreateTrueMarkWaterIdentificationCode(IUnitOfWork uow, string scannedCode);
		TrueMarkAnyCode GetParentGroupCode(IUnitOfWork unitOfWork, TrueMarkAnyCode trueMarkAnyCode);

		/// <summary>
		/// Загружает или создает коды ЧЗ. Не подойдет для проверки кодов на перепродажу. Статусы кодов в ЧЗ не проверяются.
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCodes">Строки отсканированных кодов</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат со списком кодов ЧЗ</returns>
		Task<Result<IDictionary<string, TrueMarkAnyCode>>> GetTrueMarkAnyCodesByScannedCodes(IEnumerable<string> scannedCodes, CancellationToken cancellationToken = default);

		/// <summary>
		/// Получает сохраненный код ЧЗ по коду
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="trueMarkAnyCode">Код ЧЗ</param>
		/// <returns>Результат поиска сохраненного кода ЧЗ</returns>
		Result<TrueMarkAnyCode> TryGetSavedTrueMarkAnyCode(IUnitOfWork uow, TrueMarkAnyCode trueMarkAnyCode);

		/// <summary>
		/// Создает код ЧЗ для промежуточного хранения на основе отсканированного кода
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Номер строки заказа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат создания кода</returns>
		Task<Result<StagingTrueMarkCode>> CreateStagingTrueMarkCode(IUnitOfWork uow, string scannedCode, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Проверят, что код ЧЗ для промежуточного хранения уже используется в кодах товаров
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="stagingTrueMarkCode">Код ЧЗ для промежуточного хранения</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат проверки</returns>
		Task<Result> IsStagingTrueMarkCodeAlreadyUsedInProductCodes(IUnitOfWork uow, StagingTrueMarkCode stagingTrueMarkCode, CancellationToken cancellationToken = default);

		/// <summary>
		/// Возвращает все коды Честного Знака для промежуточного хранения, связанные с указанным документом
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список кодов</returns>
		Task<IEnumerable<StagingTrueMarkCode>> GetAllTrueMarkStagingCodesByRelatedDocument(IUnitOfWork uow, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Возвращает сохраненный код Честного Знака для промежуточного хранения по отсканированному коду
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="orderItemId">Id строки заказа</param>
		/// <returns>Результат поиска кода</returns>
		Result<StagingTrueMarkCode> GetSavedStagingTrueMarkCodeByScannedCode(IUnitOfWork uow, string scannedCode, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, int? orderItemId);

		/// <summary>
		/// Создает коды ЧЗ (включая дочерние коды всех уровней вложенности) на основе кодов промежуточного хранения верхнего уровня
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="rootStagingCodes">Коды промежуточного хранения верхнего уровня</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат создания кодов</returns>
		Task<Result<IEnumerable<TrueMarkAnyCode>>> CreateTrueMarkAnyCodesFromStagingCodes(IUnitOfWork uow, IEnumerable<StagingTrueMarkCode> rootStagingCodes, CancellationToken cancellationToken = default);

		/// <summary>
		/// Удаляет все коды Честного Знака для промежуточного хранения, связанные с указанным документом
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="relatedDocumentType">Тип связанного документа</param>
		/// <param name="relatedDocumentId">Id связанного документа</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат удаления кодов</returns>
		Task<Result> DeleteAllTrueMarkStagingCodesByRelatedDocument(IUnitOfWork uow, StagingTrueMarkCodeRelatedDocumentType relatedDocumentType, int relatedDocumentId, CancellationToken cancellationToken = default);
	}
}
