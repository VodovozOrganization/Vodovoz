using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace VodovozBusiness.Services.TrueMark
{
	/// <summary>
	/// Удаляет принятые сканированием коды ЧЗ из пула, если они там ещё лежат
	/// </summary>
	public interface ITrueMarkCodesPoolCleanupService
	{
		/// <summary>
		/// Удаляет из пула все экземпляры (unit), входящие в staging-код
		/// </summary>
		Task RemoveStagingCodeFromPoolIfPresentAsync(
			StagingTrueMarkCode stagingTrueMarkCode,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Удаляет из пула идентификационные коды ЧЗ по Id
		/// </summary>
		Task RemoveIdentificationCodesFromPoolIfPresentAsync(
			IEnumerable<int> identificationCodeIds,
			CancellationToken cancellationToken = default);

		/// <summary>
		/// Удаляет из пула unit-коды, входящие в TrueMarkAnyCode
		/// </summary>
		Task RemoveTrueMarkAnyCodeFromPoolIfPresentAsync(
			TrueMarkAnyCode trueMarkAnyCode,
			CancellationToken cancellationToken = default);
	}
}
