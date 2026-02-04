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
		/// Проверяет, можно ли добавить промежуточный код Честного Знака к строке документа самовывоза
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="stagingTrueMarkCode">Код ЧЗ промежуточного хранения</param>
		/// <param name="nomeclatureId">Номенклатура</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения проверки</returns>
		Task<Result> IsStagingTrueMarkCodeCanBeAddedToItemOfNomenclature(IUnitOfWork uow, StagingTrueMarkCode stagingTrueMarkCode, int nomeclatureId, CancellationToken cancellationToken);
		/// <summary>
		/// Проверяет, что промежуточный код Честного Знака не был ранее использован в других кодах продуктов
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="stagingTrueMarkCode">Код ЧЗ промежуточного хранения</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения проверки</returns>
		Task<Result> IsStagingTrueMarkCodeAlreadyUsedInProductCodes(IUnitOfWork uow, StagingTrueMarkCode stagingTrueMarkCode, CancellationToken cancellationToken);
		/// <summary>
		/// Проверяет, что все коды продуктов Честного Знака добавлены к строке документа самовывоза
		/// </summary>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <returns>Результат выполнения проверки</returns>
		Result IsAllSelfDeliveryDocumentItemTrueMarkProductCodesAdded(SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem);
	}
}
