using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Documents;
using VodovozBusiness.Controllers;

namespace VodovozBusiness.Services.TrueMark
{
	public interface ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService
	{
		/// <summary>
		/// Возвращает промежуточные коды Честного Знака, привязанные к строке документа самовывоза
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="selfDeliveryDocumentItemId">Номер строки документа самовывоза</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Промежуточные коды ЧЗ</returns>
		Task<IEnumerable<StagingTrueMarkCode>> GetStagingTrueMarkCodesBySelfDeliveryDocumentItem(IUnitOfWork uow, int selfDeliveryDocumentItemId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Добавляет коды Честного Знака к строке документа самовывоза
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <param name="stagingCodes">Коды ЧЗ промежуточного хранения</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения операции</returns>
		Task<Result> AddProductCodesToSelfDeliveryDocumentItem(IUnitOfWork uow, SelfDeliveryDocumentItem selfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode> stagingCodes, CancellationToken cancellationToken = default);

		/// <summary>
		/// Создает промежуточный код Честного Знака
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="selfDeliveryDocumentItemId">Номер строки документа самовывоза</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат с соданным промежуточным кодом</returns>
		Task<Result<StagingTrueMarkCode>> CreateStagingTrueMarkCode(IUnitOfWork uow, string scannedCode, int selfDeliveryDocumentItemId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Добавляет любой код Честного Знака к строке документа самовывоза без проверки статуса кода.
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <param name="trueMarkAnyCode">Код Честного Знака</param>
		/// <param name="status">Статус исходного кода продукта</param>
		/// <param name="problem">Проблема с кодом продукта</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		Task AddTrueMarkAnyCodeToSelfDeliveryDocumentItemNoCodeStatusCheck(IUnitOfWork uow, SelfDeliveryDocumentItem selfDeliveryDocumentItem, TrueMarkAnyCode trueMarkAnyCode, SourceProductCodeStatus status, ProductCodeProblem problem, CancellationToken cancellationToken = default);
		/// <summary>
		/// Проверка, что все коды продуктов Честного Знака добавлены для строк документа самовывоза
		/// </summary>
		/// <param name="document">Документ отпуска самовывоза</param>
		/// <returns></returns>
		Result IsAllTrueMarkProductCodesAdded(SelfDeliveryDocument document);
		/// <summary>
		/// Получает промежуточные коды Честного Знака, привязанные к строкам документа самовывоза
		/// </summary>
		/// <param name="document">Документ отпуска самовывоза</param>
		/// <param name="stagingCodes">Промежуточные коды</param>
		/// <returns></returns>
		IDictionary<SelfDeliveryDocumentItem, IEnumerable<StagingTrueMarkCode>> GetSelfDeliveryDocumentItemStagingTrueMarkCodes(SelfDeliveryDocument document, IEnumerable<StagingTrueMarkCode> stagingCodes);
		/// <summary>
		/// Проверка, что все коды продуктов Честного Знака отсканированы для строк документа самовывоза
		/// </summary>
		/// <param name="document">Документ отпуска самовывоза</param>
		/// <param name="stagingCodes">Промежуточные коды</param>
		/// <returns></returns>
		bool IsAllCodesScanned(SelfDeliveryDocument document, IEnumerable<StagingTrueMarkCode> stagingCodes);
		/// <summary>
		/// Проверка, что промежуточный код Честного Знака может быть добавлен к строке документа самовывоза
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="document">Документ отпуска самовывоза</param>
		/// <param name="stagingTrueMarkCode"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<Result> IsStagingTrueMarkCodeCanBeAddedToDocument(IUnitOfWork uow, SelfDeliveryDocument document, StagingTrueMarkCode stagingTrueMarkCode, CancellationToken cancellationToken);
		/// <summary>
		/// Проверка, что для данного документа самовывоза все коды продуктов Честного Знака должны быть добавлены, чтобы завершить документ
		/// </summary>
		/// <param name="document">Документ отпуска самовывоза</param>
		/// <param name="edoAccountController">Контракт контроллера работы с ЭДО аккаунтами</param>
		/// <returns></returns>
		bool IsAllTrueMarkProductCodesMustBeAdded(SelfDeliveryDocument document, ICounterpartyEdoAccountController edoAccountController);
	}
}
