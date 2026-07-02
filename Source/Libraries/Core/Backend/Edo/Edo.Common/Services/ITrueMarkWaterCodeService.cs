using QS.DomainModel.UoW;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.TrueMark;

namespace Edo.Common.Services
{
	public interface ITrueMarkWaterCodeService
	{
		/// <summary>
		/// Дезагрегация связанных кодов (очистка parent кодов)
		/// </summary>
		/// <param name="anyCode">Любой из кодов честного знака</param>
		/// <param name="cancellationToken">Токен отмены</param>
		Task DisaggregateRelatedCodesAsync(IUnitOfWork unitOfWork, TrueMarkAnyCode anyCode, CancellationToken cancellationToken);
		TrueMarkAnyCode GetParentGroupCode(IUnitOfWork unitOfWork, TrueMarkAnyCode trueMarkAnyCode);
	}
}
