using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Domain.Goods;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		Task<IEnumerable<TrueMarkWaterIdentificationCode>> LoadWaterCodes(List<int> codeIds, CancellationToken cancellationToken);

		ISet<string> GetAllowedCodeOwnersInn();

		ISet<string> GetAllowedCodeOwnersGtins();

		IEnumerable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(IUnitOfWork uow, string gtin, string serialNumber, string checkCode);
		Task<IDictionary<string, List<string>>> GetGtinsNomenclatureData(IUnitOfWork uow, CancellationToken cancellationToken);
		Task<IDictionary<string, int>> GetSoldYesterdayGtinsCount(IUnitOfWork uow, CancellationToken cancellationToken);
	}
}
