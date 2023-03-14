using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.TrueMark;

namespace Vodovoz.EntityRepositories.TrueMark
{
	public interface ITrueMarkRepository
	{
		int GetCodeErrorsOrdersCount(IUnitOfWork uow);
		IEnumerable<int> GetReceiptIdsForPrepare();
		IEnumerable<TrueMarkWaterIdentificationCode> LoadWaterCodes(List<int> codeIds);
	}
}
