using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.Repositories
{
	public interface ITrueMarkCodeRepository
	{
		TrueMarkTransportCode FindParentTransportCode(IUnitOfWork uow, TrueMarkWaterGroupCode code);
		TrueMarkTransportCode FindParentTransportCode(TrueMarkWaterGroupCode code);
		TrueMarkTransportCode FindParentTransportCode(IUnitOfWork uow, TrueMarkWaterIdentificationCode code);
		TrueMarkTransportCode FindParentTransportCode(TrueMarkWaterIdentificationCode code);
		TrueMarkWaterGroupCode GetParentGroupCode(IUnitOfWork uow, int id);
		TrueMarkWaterGroupCode GetParentGroupCode(int id);
	}
}
