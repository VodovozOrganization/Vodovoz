using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		IEnumerable<TrueMarkWaterIdentificationCode> LoadWaterCodes(List<int> codeIds);

		ISet<string> GetAllowedCodeOwnersInn();

		ISet<string> GetAllowedCodeOwnersGtins();

		IEnumerable<TrueMarkWaterIdentificationCode> GetTrueMarkCodeDuplicates(IUnitOfWork uow, string gtin, string serialNumber, string checkCode);
	}
}
