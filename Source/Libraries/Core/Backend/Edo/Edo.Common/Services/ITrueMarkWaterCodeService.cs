using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Common.Services
{
	public interface ITrueMarkWaterCodeService
	{
		/// <summary>
		/// Удаляет связанные групповые и транспортные коды, сохраняя экземплярные коды без связи с упаковкой
		/// </summary>
		/// <param name="unitOfWork">Единица работы</param>
		/// <param name="anyCode">Любой код из удаляемой упаковочной иерархии</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task DeleteRelatedGroupAndTransportCodesAsync(
			IUnitOfWork unitOfWork,
			TrueMarkAnyCode anyCode,
			CancellationToken cancellationToken);

		/// <summary>
		/// Дезагрегация связанных кодов (очистка parent кодов)
		/// </summary>
		/// <param name="anyCode">Любой из кодов честного знака</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task DisaggregateRelatedCodesAsync(IUnitOfWork unitOfWork, TrueMarkAnyCode anyCode, CancellationToken cancellationToken);
		TrueMarkAnyCode GetParentGroupCode(IUnitOfWork unitOfWork, TrueMarkAnyCode trueMarkAnyCode);
	}
}
