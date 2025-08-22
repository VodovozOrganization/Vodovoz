using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace VodovozBusiness.Services.TrueMark
{
	public interface ISelfDeliveryDocumentItemTrueMarkProductCodesProcessingService
	{
		/// <summary>
		/// Добавляет коды Честного Знака к строке документа самовывоза и удаляет промежуточные коды.
		/// </summary>
		/// <param name="uow">Единица работы с базой данных</param>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения операции</returns>
		Task<Result> AddProductCodesToSelfDeliveryDocumentItemAndDeleteStagingCodes(IUnitOfWork uow, SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem, CancellationToken cancellationToken = default);

		/// <summary>
		/// Добавляет промежуточный код Честного Знака к строке документа самовывоза.
		/// </summary>
		/// <param name="uow">Единица работы с базой данных</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат с добавленным промежуточным кодом</returns>
		Task<Result<StagingTrueMarkCode>> AddStagingTrueMarkCode(IUnitOfWork uow, string scannedCode, SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem, CancellationToken cancellationToken = default);

		/// <summary>
		/// Добавляет любой код Честного Знака к строке документа самовывоза без проверки статуса кода.
		/// </summary>
		/// <param name="uow">Единица работы с базой данных</param>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <param name="trueMarkAnyCode">Код Честного Знака</param>
		/// <param name="status">Статус исходного кода продукта</param>
		/// <param name="problem">Проблема с кодом продукта</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		Task AddTrueMarkAnyCodeToSelfDeliveryDocumentItemNoCodeStatusCheck(IUnitOfWork uow, SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem, TrueMarkAnyCode trueMarkAnyCode, SourceProductCodeStatus status, ProductCodeProblem problem, CancellationToken cancellationToken = default);

		/// <summary>
		/// Удаляет промежуточный код Честного Знака из строки документа самовывоза.
		/// </summary>
		/// <param name="uow">Единица работы с базой данных</param>
		/// <param name="scannedCode">Отсканированный код</param>
		/// <param name="selfDeliveryDocumentItem">Строка документа самовывоза</param>
		/// <param name="cancellationToken">Токен отмены операции</param>
		/// <returns>Результат выполнения операции</returns>
		Task<Result> RemoveStagingTrueMarkCode(IUnitOfWork uow, string scannedCode, SelfDeliveryDocumentItemEntity selfDeliveryDocumentItem, CancellationToken cancellationToken = default);
	}
}
