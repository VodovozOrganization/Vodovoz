using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		IEnumerable<TrueMarkWaterIdentificationCode> LoadWaterCodes(List<int> codeIds);

		ISet<string> GetAllowedCodeOwnersInn();

		IQueryable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(IUnitOfWork uow, string gtin, string serialNumber, string checkCode);
	}
}
