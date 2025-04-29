using QS.DomainModel.UoW;
using System.Threading.Tasks;
using System.Threading;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;
using System.Collections.Generic;

namespace Vodovoz.Core.Data.Repositories
{
	public interface ITrueMarkCodeRepository
	{
		Task PreloadCodes(IEnumerable<TrueMarkWaterIdentificationCode> codes, CancellationToken cancellationToken);
		Task<TrueMarkTransportCode> FindParentTransportCode(
			TrueMarkWaterIdentificationCode code,
			CancellationToken cancellationToken
		);
		Task<TrueMarkTransportCode> FindParentTransportCode(
			TrueMarkWaterGroupCode code,
			CancellationToken cancellationToken
		);
		Task<TrueMarkWaterGroupCode> GetGroupCode(int id, CancellationToken cancellationToken);
	}
}
