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
	}
}
